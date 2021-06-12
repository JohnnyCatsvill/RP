
using Common;
using Common.Messager;
using Common.Storage;
using NNanomsg.Protocols;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace DisturbedStorage
{
    public class DistirbutedStorage
    {
        private static IStorage _storage;
        private int _raftState = Constants.RAFT_STATE_FOLLOWER;
        private uint _term = 0;
        private bool _isRunning = false;
        private int _disturbedStorageName;

        private List<LogBit> _log = new();
        private bool _logChanged = false;

        IPublisher _publisher;
        ISubscriber _subscriber;

        private List<KeyValuePair<int, int>> votingResults = new();

        public DistirbutedStorage(IStorage storage, int disturbedStorageName, IPublisher publisher, ISubscriber subscriber)  
        {
            _storage = storage;
            _disturbedStorageName = disturbedStorageName;

            _publisher = publisher;
            _subscriber = subscriber;

            _publisher.SetTimeout(Constants.SEND_TIMEOUT);
            _subscriber.SetTimeout(Constants.REBELLING_TIME);

            _subscriber.AddSubscription(Constants.FOLLOWER_SUBSCRIBTION);
            _subscriber.AddSubscription(Constants.LEADER_SUBSCRIBTION);
            _subscriber.AddSubscription(Constants.REBEL_SUBSCRIBTION);
            _subscriber.AddSubscription(Constants.NO_REBEL_SUBSCRIBTION);
        }

        public void Save(int key, string value)
        {
            _log.Add(new LogBit(key, value));
            _storage.Save(key, value);

            if(_raftState == Constants.RAFT_STATE_FOLLOWER)
            {
                FollowerAppendEntries(key, value);
            }
        }
        public static string Load(int key)
        {
            return _storage.Load(key);
        }

        public void Run()
        {
            _isRunning = true;

            var leaderThreadPublisher = new Thread(
                () =>
                {
                    while (_isRunning)
                    {
                        LeaderAppendEntries();
                        Thread.Sleep(Constants.HEARTBEAT_TIME);
                    }
                });

            var leaderThreadSubscriber = new Thread(
                () =>
                {
                    while (_isRunning)
                    {
                        LeaderGetMessage();
                    }
                });


            var followerThreadSubscriber = new Thread(
                () =>
                {
                    while (_isRunning)
                    {
                        if (FollowerGetMessage() == false)
                        {
                            RiseARiot();
                        }
                    }
                });

            clientThread.Start();
        }

        public void Stop()
        {
            _isRunning = false; //thread скорее сделает эту хрень бесполезной
        }

        public void LeaderAppendEntries()
        {
            LeaderNewAppendMessage message = new(_term, _log.Count - 2, _log[_log.Count - 1], _log[_log.Count - 2]);
            
            var text = JsonSerializer.Serialize(message);
            _publisher.Send(Constants.LEADER_SUBSCRIBTION, text);
        }

        public void FollowerAppendEntries(int key, string value)
        { 
            LogBit logBit = new(key, value);
            FollowerNewAppendMessage message = new(_term, logBit); 

            string text = JsonSerializer.Serialize(message);
            _publisher.Send(Constants.FOLLOWER_SUBSCRIBTION, text);
        }

        public void LeaderGetMessage()
        {
            string subscription = "";
            string data = "";

            bool success = _subscriber.Recieve(ref data, ref subscription);

            if (success)
            {
                if (subscription == Constants.FOLLOWER_SUBSCRIBTION)
                {
                    FollowerNewAppendMessage message = JsonSerializer.Deserialize<FollowerNewAppendMessage>(data);
                    _log.Add(message._data);
                }

                else if (subscription == Constants.FOLLOWER_ASK_FOR_DOP_DATA)
                {
                    LeaderSendDopMessage(data);
                }
                else if (subscription == Constants.REBEL_SUBSCRIBTION)
                {
                    _publisher.Send(Constants.NO_REBEL_SUBSCRIBTION, "f u");
                }
            }
        }

        public void LeaderSendDopMessage(string data)
        {
            FollowersAskForMore message = JsonSerializer.Deserialize<FollowersAskForMore>(data);
            int requestedIndex = message._index;
            int asker = message._nameOfAsker;

            DopMessage dopMessage = new(requestedIndex, _log[requestedIndex], asker);

            var text = JsonSerializer.Serialize(dopMessage);
            _publisher.Send(Constants.SEND_MORE_TO_FOLLOWER_SUBSCRIBTION, text);
        }

        public bool FollowerGetMessage()
        {
            string situation = "";
            string data = "";

            bool success = _subscriber.Recieve(ref data, ref situation);

            if (success)
            {
                switch (situation)
                {
                    case Constants.LEADER_SUBSCRIBTION: 
                        FollowersAddNewInstances(data);
                        break;

                    case Constants.REBEL_SUBSCRIBTION: 
                        Vote(data);
                        break;

                    case Constants.REBEL_MESSAGE_VOTE_RESULTS: 
                        CheckVotings(data);
                        break;

                    case Constants.NO_REBEL_SUBSCRIBTION:
                        AnarchyIsFallen(data);
                        break;

                    default:
                        break;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AskMoreData()
        {
            List<LogBit> _newlogs = new();
            bool noConnectionInteruptions = false;

            int index = _log.Count - 2;
            int messageIndex = _log.Count - 1;

            LogBit logBit = _log[index];
            LogBit messageLogBit = new(0, "");

            while ((index != messageIndex || logBit != messageLogBit) && noConnectionInteruptions && index >= 0)
            {
                if (index > messageIndex - 1)
                {
                    index = messageIndex - 1;
                }
                FollowersAskForMore message = new(index, _disturbedStorageName);
                var textToSend = JsonSerializer.Serialize(message);
                noConnectionInteruptions = _publisher.Send(Constants.FOLLOWER_ASK_FOR_DOP_DATA, textToSend);

                if(noConnectionInteruptions)
                {
                    string situation = "";
                    string data = "";
                    noConnectionInteruptions = _subscriber.Recieve(ref data, ref situation);

                    if (noConnectionInteruptions)
                    {
                        if(situation == Constants.SEND_MORE_TO_FOLLOWER_SUBSCRIBTION)
                        {
                            DopMessage dopMessage = JsonSerializer.Deserialize<DopMessage>(data);
                            if(dopMessage._nameOfAsker == _disturbedStorageName)
                            {
                                _newlogs.Insert(0, dopMessage._data);
                                messageIndex = dopMessage._index;
                                messageLogBit = dopMessage._data;
                                logBit = _log[messageIndex];
                                
                            }
                        }
                        else if (situation == Constants.LEADER_SUBSCRIBTION)
                        {
                            LeaderNewAppendMessage dopMessage = JsonSerializer.Deserialize<LeaderNewAppendMessage>(data);
                            _newlogs.Add(dopMessage._curData);
                        }
                    }
                }
            }

            if (noConnectionInteruptions)
            {
                foreach (LogBit elem in _newlogs)
                {
                    if (_log.Count > index)
                    {
                        _log[index] = elem;
                        index++;
                    }
                    else
                    {
                        _log.Add(elem);
                    }
                }
            }

            return noConnectionInteruptions;

        }

        public void RiseARiot()
        {
            //ping leader is dead to all
        }

        public void Vote(string data)
        {
            RebelMessage message = JsonSerializer.Deserialize<RebelMessage>(data);

            if (message._term > _term)
            {
                VoteMessage voteMessage = new(_term, message._sender, _disturbedStorageName);
                string dataToSend = JsonSerializer.Serialize(voteMessage);
                _publisher.Send(Constants.REBEL_MESSAGE_VOTE_SUBSCRIPTION, dataToSend);
            }
        }

        public void FollowersAddNewInstances(string data)
        {
            LeaderNewAppendMessage message = JsonSerializer.Deserialize<LeaderNewAppendMessage>(data);

            uint messageTerm = message._term;

            if (message._term >= _term)
            {
                int currentPrevIndex = _log.Count - 1;
                LogBit prevLogBit = _log[currentPrevIndex];

                if (currentPrevIndex == message._prevIndex && prevLogBit == message._prevData)
                {
                    _log.Add(message._curData);
                }
                else
                {
                    AskMoreData();
                }

                if (message._term > _term)
                {
                    _term = message._term;
                }
            }
        }

        public void AnarchyIsFallen(string data)
        {
            _raftState = Constants.RAFT_STATE_FOLLOWER;
        }
        public void CheckVotings(string data)
        {
            VoteResultsMessage resultsMessage = JsonSerializer.Deserialize<VoteResultsMessage>(data);

            if (resultsMessage._term > _term)
            {
                KeyValuePair<int, int> keyValuePair = new KeyValuePair<int, int>(resultsMessage._candidate, resultsMessage._amountOfVoices);
                votingResults.Add(keyValuePair);
            }
        }
    }
}
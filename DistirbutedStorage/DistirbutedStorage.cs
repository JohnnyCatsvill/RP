
using Common;
using Common.Messager;
using Common.Storage;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Dictionary<int, int> votingResults = new();

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
            _subscriber.AddSubscription(Constants.REBEL_MESSAGE_VOTE_SUBSCRIPTION);
            _subscriber.AddSubscription(Constants.REBEL_MESSAGE_VOTE_RESULTS);
            _subscriber.AddSubscription(Constants.FOLLOWER_ASK_FOR_DOP_DATA);
            _subscriber.AddSubscription(Constants.SEND_MORE_TO_FOLLOWER_SUBSCRIBTION);
        }

        public void Save(int key, string value)
        {
            _log.Add(new LogBit(key, value));
            _storage.Save(key, value);
            _logChanged = true;

            if(_raftState == Constants.RAFT_STATE_FOLLOWER)
            {
                FollowerAppendEntries(key, value);
            }
        }
        public string Load(int key)
        {
            return _storage.Load(key);
        }

        public bool Run()
        {
            _isRunning = true;

            var leaderThreadPublisher = new Thread(
                () =>
                {
                    System.Console.WriteLine("I AM A LEADER");
                    Stopwatch stopwatch = new();
                    
                    while (_isRunning && stopwatch.ElapsedMilliseconds < Constants.REBELLING_TIME + Constants.VOTING_TIME && _raftState == Constants.RAFT_STATE_LEADER)
                    {
                        bool isNetworkFine = LeaderAppendEntries();

                        if (!isNetworkFine && !stopwatch.IsRunning)
                        {
                            stopwatch.Start();
                        }
                        else if (isNetworkFine && stopwatch.IsRunning)
                        {
                            stopwatch.Reset();
                        }

                        Thread.Sleep(Constants.HEARTBEAT_TIME);
                    }
                    _raftState = Constants.RAFT_STATE_FOLLOWER;
                });

            var leaderThreadSubscriber = new Thread(
                () =>
                {
                    while (_isRunning && _raftState == Constants.RAFT_STATE_LEADER)
                    {
                        LeaderGetMessage();
                    }
                });


            var followerThreadSubscriber = new Thread(
                () =>
                {
                    System.Console.WriteLine("I AM A FOLLOWER");
                    while (_isRunning && _raftState == Constants.RAFT_STATE_FOLLOWER)
                    {
                        if (FollowerGetMessage() == false)
                        {
                            RiseARiot();
                        }
                    }
                });

            var candidateThreadSubscriber = new Thread(
                () =>
                {
                    System.Console.WriteLine("I AM A CANDIDATE");
                    Stopwatch stopwatch = new();
                    stopwatch.Start();

                    while (stopwatch.ElapsedMilliseconds < Constants.VOTING_TIME && _raftState == Constants.RAFT_STATE_CANDIDATE)
                    {
                        CandidateGetMessage();
                    }
                });

            var candidateThreadPublisher = new Thread(
                () =>
                {
                    Stopwatch stopwatch = new();
                    stopwatch.Start();

                    while (stopwatch.ElapsedMilliseconds < Constants.VOTING_TIME && _raftState == Constants.RAFT_STATE_CANDIDATE)
                    {
                        CandidateHeartbeat();
                        Thread.Sleep(Constants.HEARTBEAT_TIME);
                    }

                    bool isWeWin = true;

                    foreach(var pair in votingResults)
                    {
                        if(pair.Key != _disturbedStorageName && pair.Value >= votingResults[_disturbedStorageName])
                        {
                            isWeWin = false;
                        }
                    }

                    if(isWeWin)
                    {
                        _raftState = Constants.RAFT_STATE_LEADER;
                    }
                    else
                    {
                        _raftState = Constants.RAFT_STATE_FOLLOWER;
                    }

                });

            var threadController = new Thread(
                () =>
                {
                    System.Console.WriteLine("CHOOSING DESTINY");
                    var prevRaftState = Constants.RAFT_STATE_OFF;
                    while(_isRunning)
                    {
                        if(prevRaftState != _raftState)
                        {
                            if (!candidateThreadSubscriber.IsAlive && 
                                !candidateThreadPublisher.IsAlive && 
                                !leaderThreadPublisher.IsAlive && 
                                !leaderThreadSubscriber.IsAlive && 
                                !followerThreadSubscriber.IsAlive)
                            {
                                if(_raftState == Constants.RAFT_STATE_LEADER)
                                {
                                    leaderThreadPublisher.Start();
                                    leaderThreadSubscriber.Start();
                                }
                                else if (_raftState == Constants.RAFT_STATE_FOLLOWER)
                                {
                                    followerThreadSubscriber.Start();
                                }
                                else if (_raftState == Constants.RAFT_STATE_CANDIDATE)
                                {
                                    candidateThreadSubscriber.Start();
                                    candidateThreadPublisher.Start();
                                }
                                prevRaftState = _raftState;
                            }
                        }
                        Thread.Sleep(Constants.SWAP_ROLE_TIME);
                    }
                });

            threadController.Start();

            return true;
        }

        public void Stop()
        {
            _isRunning = false; //thread скорее сделает эту хрень бесполезной
        }

        public bool LeaderAppendEntries()
        {
            //System.Console.WriteLine("Heartbeat");
            LeaderNewAppendMessage message;

            if (_log.Count == 0)
            {
                message = new(_term, _log.Count - 2, new LogBit(0, "0"), new LogBit(0, "0"), _logChanged);
            }
            else if (_log.Count == 1)
            {
                message = new(_term, _log.Count - 2, _log[_log.Count - 1], new LogBit(0, "0"), _logChanged);
            }
            else
            {
                message = new(_term, _log.Count - 2, _log[_log.Count - 1], _log[_log.Count - 2], _logChanged);
            }
            var text = JsonSerializer.Serialize(message);
            _logChanged = false;
            return _publisher.Send(Constants.LEADER_SUBSCRIBTION, text);
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
                    System.Console.WriteLine("get fol message");
                    FollowerNewAppendMessage message = JsonSerializer.Deserialize<FollowerNewAppendMessage>(data);
                    _log.Add(message._data);
                    _logChanged = true;
                }

                else if (subscription == Constants.FOLLOWER_ASK_FOR_DOP_DATA)
                {
                    System.Console.WriteLine("get fol dop message");
                    LeaderSendDopMessage(data);
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
            //System.Console.WriteLine("GetData" + success);

            if (success)
            {
                switch (situation)
                {
                    case Constants.LEADER_SUBSCRIBTION: //внутри реализовано и получений недостающией инфы
                        FollowersAddNewInstances(data);
                        break;

                    case Constants.REBEL_SUBSCRIBTION: 
                        Vote(data);
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
            _raftState = Constants.RAFT_STATE_CANDIDATE;
            votingResults[_disturbedStorageName] = 1;
            RebelMessage message = new(_term + 1, _disturbedStorageName);

            _publisher.Send(Constants.REBEL_SUBSCRIBTION, JsonSerializer.Serialize(message));
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

            //System.Console.WriteLine("Get Heartbeat " + message._curData._key+ "-" + message._curData._value);

            if (message._term >= _term && message._anyChange)
            {
                if(message._prevIndex == -2)
                {
                    _log.Add(message._curData);
                }
                else if(message._prevIndex >= -1)
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

        public bool CandidateGetMessage()
        {
            string situation = "";
            string data = "";

            bool success = _subscriber.Recieve(ref data, ref situation);

            if (success)
            {
                switch (situation)
                {
                    case Constants.LEADER_SUBSCRIBTION:
                        AnarchyIsFallen(data);
                        break;

                    case Constants.REBEL_MESSAGE_VOTE_SUBSCRIPTION:
                        CheckVote(data);
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

        public void CheckVote(string data)
        {
            VoteMessage voteMessage = JsonSerializer.Deserialize<VoteMessage>(data);
            if(votingResults.ContainsKey(voteMessage._voteFor))
            {
                votingResults[voteMessage._voteFor] += 1;
            }
            else
            {
                votingResults[voteMessage._voteFor] = 1;
            }
        }

        public void CandidateHeartbeat()
        {
            VoteResultsMessage message = new(_term, votingResults[_disturbedStorageName], _disturbedStorageName);

            var text = JsonSerializer.Serialize(message);
            _publisher.Send(Constants.LEADER_SUBSCRIBTION, text);
        }
    }
}
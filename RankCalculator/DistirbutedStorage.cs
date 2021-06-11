
using Common;
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

        PublishSocket _channelPublisher;// = new PublishSocket();


        public DistirbutedStorage(IStorage storage, int disturbedStorageName)  
        {
            _storage = storage;
            _disturbedStorageName = disturbedStorageName;

            _channelPublisher = new PublishSocket();
            _channelPublisher.Bind("tcp://" + Constants.NNMSG_HOST);
            _channelPublisher.Options.SendTimeout = System.TimeSpan.FromMilliseconds(Constants.SEND_TIMEOUT);
        }

        public void Save(int key, string value)
        {
            _log.Add(new LogBit(key, value));
            _storage.Save(key, value);

            if(_raftState == Constants.RAFT_STATE_FOLLOWER)
            {
                var text = Constants.FOLLOWER_SUBSCRIBTION;

                LogBit changes = new(key, value);
                text += JsonSerializer.Serialize(changes);
                
                var data = Encoding.ASCII.GetBytes(text);
                _channelPublisher.Send(data);
            }
        }
        public static string Load(int key)
        {
            return _storage.Load(key);
        }

        public void Run()
        {
            _isRunning = true;

            SubscribeSocket _channelSubscription = new SubscribeSocket();
            

            _channelSubscription.Connect("tcp://" + Constants.NNMSG_HOST);
            _channelSubscription.Subscribe(Constants.FOLLOWER_SUBSCRIBTION);
            _channelSubscription.Subscribe(Constants.LEADER_SUBSCRIBTION);
            _channelSubscription.Subscribe(Constants.REBEL_SUBSCRIBTION);
            _channelSubscription.Subscribe(Constants.NO_REBEL_SUBSCRIBTION);
            _channelSubscription.Options.ReceiveTimeout = System.TimeSpan.FromMilliseconds(Constants.REBELLING_TIME);

            

            var leaderThreadPublisher = new Thread(
                () =>
                {
                    while (_isRunning)
                    {
                        try
                        {
                            var text = Constants.LEADER_SUBSCRIBTION;
                            text += _logChanged ? "1" : "0";

                            if (_logChanged)
                            {
                                Message changes = new(_term, _log.Count - 2, _log[_log.Count - 1], _log[_log.Count - 2]);
                                text += JsonSerializer.Serialize(changes);
                            }
                            
                            var data = Encoding.ASCII.GetBytes(text);
                            _channelPublisher.Send(data);

                            _logChanged = false;

                            Thread.Sleep(Constants.HEARTBEAT_TIME);
                        }
                        catch (NNanomsg.NanomsgException)
                        {
                            //System.Console.WriteLine("can't send");
                        }
                    }
                });

            var leaderThreadSubscriber = new Thread(
                () =>
                {
                    while (_isRunning)
                    {
                        try
                        {
                            byte[] data = _channelSubscription.Receive();
                            string text = data.ToString();

                            string situation = text.Remove(0, 1);

                            if(situation == Constants.FOLLOWER_SUBSCRIBTION)
                            {
                                LogBit logBit = JsonSerializer.Deserialize<LogBit>(text);
                                _log.Add(logBit);
                                _logChanged = true;
                            }
                            else if (situation == Constants.FOLLOWER_NEED_MORE_SUBSCRIBTION)
                            {
                                int requestedIndex = int.Parse(text.Remove(0, text.IndexOf(" ")));
                                int nodeName = int.Parse(text.Substring(1, text.Length));

                                var textToSend = Constants.SEND_MORE_TO_FOLLOWER_SUBSCRIBTION;

                                DopMessage dopMessage = new(requestedIndex, _log[requestedIndex]);
                                textToSend += JsonSerializer.Serialize(dopMessage);

                                var dataToSend = Encoding.ASCII.GetBytes(textToSend);
                                _channelPublisher.Send(dataToSend);
                            }
                            else if (situation == Constants.REBEL_SUBSCRIBTION)
                            {
                                var textToSend = Constants.NO_REBEL_SUBSCRIBTION;
                                var dataToSend = Encoding.ASCII.GetBytes(textToSend);
                                _channelPublisher.Send(dataToSend);
                            }
                        }
                        catch (NNanomsg.NanomsgException)
                        {
                            //System.Console.WriteLine("can't send");
                        }
                    }
                });


            var clientThreadSubscriber = new Thread(
                () =>
                {
                    while (_isRunning)
                    {
                        try
                        {
                            byte[] data = _channelSubscription.Receive();
                            string text = data.ToString();

                            string situation = text.Remove(0, 1);

                            if (situation == Constants.LEADER_SUBSCRIBTION)
                            {
                                string anyChanges = text.Remove(0, 1);

                                if (anyChanges == "1")
                                {
                                    Message message = JsonSerializer.Deserialize<Message>(text);

                                    if(message._term >= _term)
                                    {
                                        if(_log.Count - 1 == message._prevIndex && _log[_log.Count - 1]._key == message._prevData._key && _log[_log.Count - 1]._value == message._prevData._value)
                                        {
                                            _log.Add(message._curData);
                                        }
                                        else
                                        {
                                            //send to leader 'bout shit

                                            //get new message

                                            //repeat until good

                                            //save all to _log

                                        }

                                        if(message._term > _term)
                                        {
                                            _term = message._term;
                                        }
                                    }

                                }
                            }
                        }
                        catch (NNanomsg.NanomsgException)
                        {
                            // bund!!!
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
            //leader's ping to all
        }

        public void FollowerAppendEntries()
        {
            //follower's ping to leader
        }

        public void RequestVote()
        {
            //ping leader is dead to all
        }

        public void Vote()
        {
            //ping leader is dead to all
        }
    }
}
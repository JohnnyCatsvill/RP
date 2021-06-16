using NATS.Client;
using System;
using System.Diagnostics;
using System.Threading;

namespace Common.Messager
{
    public class NatsSubscriber : ISubscriber
    {
        private static IConnection _broker;
        private IAsyncSubscription _loggerSubscription;
        private int _timeout = int.MaxValue;
        private static bool _dataCame = false;
        private static string _data = "";

        public NatsSubscriber(string protocol, string address)
        {
            _broker = new ConnectionFactory().CreateConnection();
        }

        public void SetTimeout(int milliseconds)
        {
            _timeout = milliseconds;
        }
        public void AddSubscription(string channel)
        {
            _loggerSubscription.Connection.SubscribeAsync(channel, Print);
        }
        public bool Recieve(ref string recievingString, ref string recivingChannel)
        {
            _dataCame = false;
            Stopwatch watch = new();
            watch.Start();

            while(watch.ElapsedMilliseconds < _timeout && !_dataCame)
            {
                Thread.Sleep(10);
            }
            if(_dataCame)
            {
                recivingChannel = _data.Substring(0, _data.IndexOf(MessagerConstants.CHANNEL_END));
                recievingString = _data.Substring(_data.IndexOf(MessagerConstants.CHANNEL_END) + MessagerConstants.CHANNEL_END.Length, -1);
                return true;
            }
            else
            {
                return false;
            }
        }

        private EventHandler<MsgHandlerEventArgs> Print = (sender, args) =>
        {
            _data = args.Message.Data.ToString();
            _dataCame = true;
        };
    }
}

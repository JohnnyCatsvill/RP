using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messager
{
     public class NmqPub : IPublisher
    {
        private static PushSocket _channelPublisher = new();// = new PublishSocket();

        public NmqPub(string protocol, string address)
        {
            _channelPublisher.Connect(protocol + "://" + address);
        }

        public void SetTimeout(int milliseconds)
        {
            _channelPublisher.Options.HeartbeatTimeout = System.TimeSpan.FromMilliseconds(milliseconds);
        }

        public bool Send(string channel, string message)
        {
            try
            {
                _channelPublisher.SendFrame(channel + MessagerConstants.CHANNEL_END + message);
                return true;
            }
            catch (NetMQ.NetMQException e)
            {
                return false;
            }
        }
    }
}

using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messager
{
    public class NatsPublisher : IPublisher
    {
        private static IConnection _broker;

        public NatsPublisher(string protocol, string address)
        {
            _broker = new ConnectionFactory().CreateConnection(protocol + "://" + address);
            _broker.Opts.Timeout = -1;
        }

        public void SetTimeout(int milliseconds)
        {
            _broker.Opts.Timeout = milliseconds;
        }

        public bool Send(string channel, string message)
        {
            try
            {
                _broker.Publish(channel, Encoding.UTF8.GetBytes(channel + MessagerConstants.CHANNEL_END + message));
                return true;
            }
            catch (NATS.Client.NATSException)
            {
                return false;
            }
        }
    }
}

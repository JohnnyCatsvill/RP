
using NetMQ;
using NetMQ.Sockets;
using System.Text;

namespace Common.Messager
{
    

    public class NmcPublisher : IPublisher
    {
        private PublisherSocket _channelPublisher;// = new PublishSocket();

        public NmcPublisher(string protocol, string address)
        {
            _channelPublisher = new PublisherSocket();
            _channelPublisher.Bind(protocol + "://" + address);
        }

        public void SetTimeout(int milliseconds)
        {
            _channelPublisher.Options.HeartbeatTimeout = System.TimeSpan.FromMilliseconds(milliseconds);
            //_channelPublisher.Options.SendTimeout = System.TimeSpan.FromMilliseconds(milliseconds);
        }

        public bool Send(string channel, string message)
        {
            try
            {
                //var text = channel + MessagerConstants.CHANNEL_END + message;
                //var data = Encoding.ASCII.GetBytes(text);
                _channelPublisher.SendMoreFrame(channel).SendFrame(message);
                return true;
            }
            catch (NetMQ.NetMQException e)
            {
                return false;
            }
        }
    }
}

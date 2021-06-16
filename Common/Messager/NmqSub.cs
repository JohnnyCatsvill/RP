using NetMQ.Sockets;
using NetMQ;

namespace Common.Messager
{
    public class NmqSub: ISubscriber
    {
        private PullSocket _channelSubscription = new();
        private System.TimeSpan _timeout;

        public NmqSub(string protocol, string address)
        {
            _channelSubscription.Bind(protocol + "://" + address);
        }

        public void SetTimeout(int milliseconds)
        {
            _timeout = System.TimeSpan.FromMilliseconds(milliseconds);
        }
        public void AddSubscription(string channel)
        {
            //not actual sub
        }
        public bool Recieve(ref string recievingString, ref string recivingChannel)
        {
            string data = ""; 
            bool delivered = _channelSubscription.TryReceiveFrameString(_timeout, out data);

            if(delivered)
            {
                recivingChannel = data.Remove(0, data.IndexOf(MessagerConstants.CHANNEL_END));
                recievingString = data.Substring(0, data.Length);
                System.Console.WriteLine("message!");
                return true;
            }
            return false;
        }
    }
}

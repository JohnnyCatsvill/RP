using NetMQ;
using NetMQ.Sockets;

namespace Common.Messager
{

    /*using (var subscriber = new SubscriberSocket())
{
    subscriber.Connect("tcp://127.0.0.1:5556");
    subscriber.Subscribe("A");

    while (true)
    {
        var topic = subscriber.ReceiveFrameString();
        var msg = subscriber.ReceiveFrameString();
        Console.WriteLine("From Publisher: {0} {1}", topic, msg);
    }
}*/

    public class NmcSubscriber : ISubscriber
    {
        private SubscriberSocket _channelSubscription;
        private System.TimeSpan _timeout;

        public NmcSubscriber(string protocol, string address)
        {
            _channelSubscription = new SubscriberSocket();
            _channelSubscription.Bind(protocol + "://" + address);
        }

        public void SetTimeout(int milliseconds)
        {
            _timeout = System.TimeSpan.FromMilliseconds(milliseconds);
        }
        public void AddSubscription(string channel)
        {
            _channelSubscription.Subscribe(channel);
        }
        public bool Recieve(ref string recievingString, ref string recivingChannel)
        {
            
            string topic = "";
            string msg = "";

            bool isTopicCame = _channelSubscription.TryReceiveFrameString(_timeout, out topic);
            bool isMessageCame = _channelSubscription.TryReceiveFrameString(_timeout, out msg);

            if(isTopicCame && isMessageCame)
            {
                recivingChannel = topic;
                recievingString = msg;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

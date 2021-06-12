using NNanomsg.Protocols;

namespace Common.Messager
{
    public class NnSubscriber : ISubscriber
    {
        public SubscribeSocket _channelSubscription;

        public NnSubscriber(string protocol, string address)
        {
            _channelSubscription = new SubscribeSocket();
            _channelSubscription.Connect(protocol + "://" + address);
        }

        public void SetTimeout(int milliseconds)
        {
            _channelSubscription.Options.ReceiveTimeout = System.TimeSpan.FromMilliseconds(milliseconds);
        }
        public void AddSubscription(string channel) 
        {
            _channelSubscription.Subscribe(channel);
        }
        public bool Recieve(ref string recievingString, ref string recivingChannel) 
        {
            try
            {
                byte[] data = _channelSubscription.Receive();
                string text = data.ToString();

                recivingChannel = text.Remove(0, text.IndexOf(MessagerConstants.CHANNEL_END) + MessagerConstants.CHANNEL_END.Length);
                recivingChannel.Substring(MessagerConstants.CHANNEL_START.Length, recivingChannel.Length - MessagerConstants.CHANNEL_END.Length);

                recievingString = text;
                return true;
            }
            catch (NNanomsg.NanomsgException e)
            {
                return false;
            }
        }
    }
}

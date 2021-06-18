
//using System.Text;
//using nng;
//using nng.Native;
//
//namespace Common.Messager
//{
//    public class NnPublisher : IPublisher
//    {
//        NngCollectionFixture Fixture;
//        IAPIFactory<nng_msg> Factory => Fixture.Factory;
//
//        //private Factory.PublisherOpen _channelPublisher;// = new PublishSocket();
//
//        public NnPublisher(string protocol, string address)
//        {
//            _channelPublisher = new PublishSocket();
//            _channelPublisher.Bind(protocol + "://" + address);
//        }
//
//        public void SetTimeout(int milliseconds) 
//        {
//            using (var pubSocket = Factory.PublisherOpen().ThenListenAs(out var listener, url).Unwrap())
//
//                _channelPublisher.Options.SendTimeout = System.TimeSpan.FromMilliseconds(milliseconds);
//        }
//
//        public bool Send(string channel, string message) 
//        {
//            try
//            {
//                var text = MessagerConstants.CHANNEL_START + channel + MessagerConstants.CHANNEL_END + message;
//                var data = Encoding.ASCII.GetBytes(text);
//                _channelPublisher.Send(data);
//                return true;
//            }
//            catch(NNanomsg.NanomsgException e)
//            {
//                return false;
//            }
//        }
//    }
//}

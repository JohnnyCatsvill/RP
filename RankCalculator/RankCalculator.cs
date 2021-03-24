using System;
using System.Text;
using Common;
using Common.Storage;
using NATS.Client;

namespace RankCalculator
{
    public class RankCalculator
    {
        private static IStorage _storage;
        private static IConnection _broker;
        private readonly IAsyncSubscription _subscription; 

        public RankCalculator(IStorage storage)  
        {
            _storage = storage;
            _broker = new ConnectionFactory().CreateConnection();
            _subscription = _broker.SubscribeAsync(Constants.BROKER_CHANNEL, CalculateRank);
        }

        ~RankCalculator()
        {
            _broker.Drain();
            _broker.Close();
        }

        private EventHandler<MsgHandlerEventArgs> CalculateRank = (sender, args) =>
        {
            string id = Encoding.UTF8.GetString(args.Message.Data);
            string text = _storage.Load(Constants.TEXT_NAME + id);
            
            int alphabeticLetters = 0;
            foreach (char ch in text)
            {
                if (Char.IsLetter(ch))
                {
                    alphabeticLetters++;
                }
            }
            double rank = (double)alphabeticLetters / (double)text.Length;

            _storage.Save(Constants.RANK_NAME + id, rank.ToString());
        };
    }
}
using System;
using System.Text;
using System.Text.Json;
using Common;
using Common.Storage;
using Common.Structures;
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
            _subscription = _broker.SubscribeAsync(Constants.BROKER_CHANNEL_FOR_RANK_CALCULATION, "queue", CalculateRank);
        }

        ~RankCalculator()
        {
            _broker.Drain();
            _broker.Close();
        }

        private EventHandler<MsgHandlerEventArgs> CalculateRank = (sender, args) =>
        {
            string id = Encoding.UTF8.GetString(args.Message.Data);
            string text = _storage.Load(Constants.TEXT_NAME, id);
            
            int alphabeticLetters = 0;
            foreach (char ch in text)
            {
                if (Char.IsLetter(ch))
                {
                    alphabeticLetters++;
                }
            }
            double rank = (double)alphabeticLetters / (double)text.Length;

            _storage.Save(Constants.RANK_NAME, id, rank.ToString());

            LoggerData loggerData = new LoggerData("rank_calculated", id, Convert.ToString(rank));
            var dataToSend = JsonSerializer.Serialize(loggerData);
            _broker.Publish(Constants.BROKER_CHANNEL_EVENTS_LOGGER, Encoding.UTF8.GetBytes(dataToSend));
        };
    }
}
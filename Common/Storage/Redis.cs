using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Common.Structures;
using NATS.Client;
using StackExchange.Redis;

namespace Common.Storage
{
    public class Redis : IStorage
    {
        private readonly ConnectionMultiplexer _mainDatabase = ConnectionMultiplexer.Connect(Constants.REDIS_HOST);
        private static IConnection _broker;
        public Redis() 
        {
            _broker = new ConnectionFactory().CreateConnection();
        }
        ~Redis()
        {
            _mainDatabase.Dispose();
            _mainDatabase.Close();
        }
        public void Save(string obj, string id, string value)
        {
            IDatabase mainConn = _mainDatabase.GetDatabase();
            string nameOfSecondaryDB = mainConn.StringGet(id);
            
            ConnectionMultiplexer secondaryDB = ConnectionMultiplexer.Connect(nameOfSecondaryDB);

            IDatabase secondaryConn = secondaryDB.GetDatabase();
            secondaryConn.StringSet(obj + id, value);

            secondaryDB.Dispose();
            secondaryDB.Close();
        }
        public string Load(string obj, string id)
        {
            IDatabase mainConn = _mainDatabase.GetDatabase();
            string nameOfSecondaryDB = mainConn.StringGet(id);

            ConnectionMultiplexer secondaryDB = ConnectionMultiplexer.Connect(nameOfSecondaryDB);

            IDatabase secondaryConn = secondaryDB.GetDatabase();
            string text = secondaryConn.StringGet(obj + id);

            secondaryDB.Dispose();
            secondaryDB.Close();

            LoggerData loggerData = new("LOOKUP", id, Constants.DICT_OF_HOSTS_TO_REGIONS[nameOfSecondaryDB]);
            string dataToSend = JsonSerializer.Serialize(loggerData);

            _broker.Publish(Constants.BROKER_CHANNEL_EVENTS_LOGGER, Encoding.UTF8.GetBytes(dataToSend));

            return text;
        }
        public List<string> GetKeys(string obj)
        {
            //IDatabase mainConn = _mainDatabase.GetDatabase();
            //string nameOfSecondaryDB = Constants.DICT_OF_COUNTRIES_TO_REGIONS[country];
            //
            //ConnectionMultiplexer secondaryDB = ConnectionMultiplexer.Connect(nameOfSecondaryDB);
            //
            var mainConn = _mainDatabase.GetServer(Constants.REDIS_HOST);
            List<string> list = new List<string>();

            var dbList = mainConn.Keys(pattern: "*");

            foreach(var item in dbList)
            {
                list.Add(item.ToString());
                System.Console.WriteLine(item.ToString());
            }
            return list;
        }

        public void SaveIdToRegion(string id, string country)
        {
            IDatabase mainConn = _mainDatabase.GetDatabase();
            mainConn.StringSet(id, Constants.DICT_OF_COUNTRIES_TO_REGIONS[country]);
        }
    }
}

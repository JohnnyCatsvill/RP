using System.Collections.Generic;
using StackExchange.Redis;

namespace Common.Storage
{
    public class Redis : IStorage
    {
        private readonly ConnectionMultiplexer _mainDatabase;
        public Redis() 
        {
            _mainDatabase = ConnectionMultiplexer.Connect(Constants.REDIS_HOST);
        }
        ~Redis()
        {
            _mainDatabase.Dispose();
            _mainDatabase.Close();
        }
        public void Save(int key, string value)
        {
            IDatabase mainConn = _mainDatabase.GetDatabase();
            mainConn.StringSet(key.ToString(), value);
        }
        public string Load(int key)
        {
            IDatabase mainConn = _mainDatabase.GetDatabase();
            string value = mainConn.StringGet(key.ToString());
            return value;
        }
        public List<int> GetKeys()
        {
            var mainConn = _mainDatabase.GetServer(Constants.REDIS_HOST);
            List<int> list = new();

            var dbList = mainConn.Keys(pattern: "*");

            foreach(var item in dbList)
            {
                list.Add(int.Parse(item.ToString()));
            }
            return list;
        }
    }
}

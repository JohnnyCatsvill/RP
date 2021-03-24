using System.Collections.Generic;
using StackExchange.Redis;

namespace Common.Storage
{
    public class Redis : IStorage
    {
        readonly ConnectionMultiplexer _muxer = ConnectionMultiplexer.Connect(Constants.REDIS_HOST);
        public Redis() { }
        public void Save(string key, string value)
        {
            IDatabase conn = _muxer.GetDatabase();
            conn.StringSet(key, value);
        }
        public string Load(string key)
        {
            IDatabase conn = _muxer.GetDatabase();
            return conn.StringGet(key);
        }
        public List<string> GetKeys(string key)
        {
            var db = _muxer.GetServer(Constants.REDIS_HOST, Constants.REDIS_PORT);
            List<string> list = new List<string>();

            var dbList = db.Keys(pattern: "*" + key + "*");

            foreach(var item in dbList)
            {
                list.Add(item.ToString());
            }
            return list;
        }
    }
}

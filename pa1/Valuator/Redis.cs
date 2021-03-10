using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using static Valuator.Constants;

namespace Valuator
{
    public class Redis
    {
        readonly ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect(RedisHost);
        public Redis() { }
        public void Save(string key, string value)
        {
            IDatabase conn = muxer.GetDatabase();
            conn.StringSet(key, value);
        }
        public string Load(string key)
        {
            IDatabase conn = muxer.GetDatabase();
            return conn.StringGet(key);
        }
        public List<string> GetKeys(string key)
        {
            var db = muxer.GetServer(RedisHost, RedisPort);
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

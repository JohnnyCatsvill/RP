using System.Collections.Generic;

namespace Common.Storage
{
    public interface IStorage
    {
        public void Save(string key, string value) { }
        public string Load(string key) { return null; }
        public List<string> GetKeys(string key) { return null; }
    }
}

using System.Collections.Generic;

namespace Common.Storage
{
    public interface IStorage
    {
        public void Save(string obj, string id, string value) { }
        public string Load(string obj, string id) { return null; }
        public List<string> GetKeys(string obj, string id) { return null; }
        public void SaveIdToRegion(string id, string country) { }
    }
}

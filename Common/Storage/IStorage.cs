using System.Collections.Generic;

namespace Common.Storage
{
    public interface IStorage
    {
        public void Save(int key, string value) { }
        public string Load(int key) { return null; }
        public List<int> GetKeys() { return null; }
    }
}

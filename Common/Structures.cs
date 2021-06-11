
namespace Common
{
   
    public struct LogBit
    {
        public int _key { get; }
        public string _value { get; }

        public LogBit(int key, string value)
        {
            _key = key;
            _value = value;
        }
    }

    public struct Message
    {
        public uint _term { get; }
        public int _prevIndex { get; }
        public LogBit _curData { get; }
        public LogBit _prevData { get; }

        public Message(uint term, int previousIndex, LogBit currentData, LogBit previousData)
        {
            _term = term;
            _prevIndex = previousIndex;
            _curData = currentData;
            _prevData = previousData;
        }
    }

    public struct DopMessage
    {
        public int _index { get; }
        public LogBit _data { get; }

        public DopMessage(int index, LogBit data)
        {
            _index = index;
            _data = data;
        }
    }
}

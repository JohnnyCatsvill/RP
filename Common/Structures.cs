
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
        public static bool operator ==(LogBit a, LogBit b)
        => (a._key == b._key && a._value == b._value);
        public static bool operator !=(LogBit a, LogBit b)
        => (a._key != b._key || a._value != b._value);
    }

    public struct LeaderNewAppendMessage
    {
        public uint _term { get; }
        public int _prevIndex { get; }
        public LogBit _curData { get; }
        public LogBit _prevData { get; }

        public LeaderNewAppendMessage(uint term, int previousIndex, LogBit currentData, LogBit previousData)
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
        public int _nameOfAsker { get; }

        public DopMessage(int index, LogBit data, int nameOfAsker)
        {
            _index = index;
            _data = data;
            _nameOfAsker = nameOfAsker;
        }
    }

    public struct FollowersAskForMore
    {
        public int _index { get; }
        public int _nameOfAsker { get; }

        public FollowersAskForMore(int index, int nameOfAsker)
        {
            _index = index;
            _nameOfAsker = nameOfAsker;
        }
    }

    public struct FollowerNewAppendMessage
    {
        public uint _term { get; }
        public LogBit _data { get; }

        public FollowerNewAppendMessage(uint term, LogBit data)
        {
            _term = term;
            _data = data;
        }
    }

    public struct RebelMessage
    {
        public uint _term { get; }
        public int _sender { get; }

        public RebelMessage(uint term, int sender)
        {
            _term = term;
            _sender = sender;
        }
    }

    public struct VoteMessage
    {
        public uint _term { get; }
        public int _voterName { get; }
        public int _voteFor { get; }

        public VoteMessage(uint term, int voteFor, int voterName)
        {
            _term = term;
            _voteFor = voteFor;
            _voterName = voterName;
        }
    }

    public struct VoteResultsMessage
    {
        public uint _term { get; }
        public int _candidate { get; }
        public int _amountOfVoices { get; }

        public VoteResultsMessage(uint term, int amountOfVoices, int candidate)
        {
            _term = term;
            _amountOfVoices = amountOfVoices;
            _candidate = candidate;
        }
    }
}

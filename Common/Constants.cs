
namespace Common
{
    public class Constants
    {
        public const string NNMSG_HOST = "localhost:5999";
        public const string REDIS_HOST = "localhost:5998";

        public const int RAFT_STATE_LEADER = 0;
        public const int RAFT_STATE_FOLLOWER = 1;
        public const int RAFT_STATE_CANDIDATE = 2;

        public const string BROKER_LEADER_CHANNEL = "leader_channel";
        public const string BROKER_FOLLOWER_CHANNEL = "follower_channel";

        public const string LEADER_SUBSCRIBTION = "0";
        public const string FOLLOWER_SUBSCRIBTION = "1";
        public const string REBEL_SUBSCRIBTION = "2";
        public const string NO_REBEL_SUBSCRIBTION = "3";
        public const string FOLLOWER_NEED_MORE_SUBSCRIBTION = "4";
        public const string SEND_MORE_TO_FOLLOWER_SUBSCRIBTION = "5";

        public const int VOTING_TIME = 100;
        public const int REBELLING_TIME = 1000;
        public const int HEARTBEAT_TIME = 30;
        public const int SEND_TIMEOUT = 300;

    }
}

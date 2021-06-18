
namespace Common
{
    public class Constants
    {
        public const string NNMSG_HOST = "localhost:5999";
        public const string REDIS_HOST = "localhost:5998";

        public const int RAFT_STATE_LEADER = 0;
        public const int RAFT_STATE_FOLLOWER = 1;
        public const int RAFT_STATE_CANDIDATE = 2;
        public const int RAFT_STATE_OFF = 3;

        public const string BROKER_LEADER_CHANNEL = "leader_channel";
        public const string BROKER_FOLLOWER_CHANNEL = "follower_channel";

        public const string LEADER_SUBSCRIBTION = "0";
        public const string FOLLOWER_SUBSCRIBTION = "1";
        public const string REBEL_SUBSCRIBTION = "2";
        public const string NO_REBEL_SUBSCRIBTION = "3";
        public const string FOLLOWER_ASK_FOR_DOP_DATA = "4";
        public const string SEND_MORE_TO_FOLLOWER_SUBSCRIBTION = "5";
        public const string REBEL_MESSAGE_VOTE_SUBSCRIPTION = "6";
        public const string REBEL_MESSAGE_VOTE_RESULTS = "7";

        public const int VOTING_TIME = 1000;
        public const int REBELLING_TIME = 5000;
        public const int HEARTBEAT_TIME = 300;
        public const int SEND_TIMEOUT = 1000;

        public const int SWAP_ROLE_TIME = 50;

    }
}

namespace Common
{
    public class Constants
    {
        public const string REDIS_HOST = "localhost";
        public const int REDIS_PORT = 6379;

        public const string RANK_NAME = "RANK-";
        public const string TEXT_NAME = "TEXT-";
        public const string SIMILARITY_NAME = "SIMILARITY-";

        public const string BROKER_CHANNEL_FOR_RANK_CALCULATION = "calculate_rank";
        public const string BROKER_CHANNEL_EVENTS_LOGGER = "events_logger";
    }
}

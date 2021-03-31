using System.Collections.Generic;

namespace Common
{
    public class Constants
    {
        public const string REDIS_HOST = "localhost:6379";

        public const string RANK_NAME = "RANK-";
        public const string TEXT_NAME = "TEXT-";
        public const string SIMILARITY_NAME = "SIMILARITY-";

        public const string BROKER_CHANNEL_FOR_RANK_CALCULATION = "calculate_rank";
        public const string BROKER_CHANNEL_EVENTS_LOGGER = "events_logger";
        public const string BROKER_CHANNEL_RANK_CALCULATED = "rank_calculated";


        public const string DB_RUS = "localhost:6000";
        public const string DB_EU = "localhost:6001";
        public const string DB_OTHER = "localhost:6002";

        public static Dictionary<string, string> DICT_OF_HOSTS_TO_REGIONS = new()
        {
            ["localhost:6000"] = "RUS",
            ["localhost:6001"] = "EU",
            ["localhost:6002"] = "OTHER"
        };

        public static Dictionary<string, string> DICT_OF_COUNTRIES_TO_REGIONS = new()
        {
            ["Russia"] = DB_RUS ,
            ["France"] = DB_EU ,
            ["Germany"] = DB_EU ,
            ["USA"] = DB_OTHER ,
            ["India"] = DB_OTHER 
        };
    }
}

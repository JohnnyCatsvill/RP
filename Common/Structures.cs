using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Structures
{
    public struct LoggerData
    {
        public string eventName { get; set; }
        public string contextId { get; set; }
        public string eventData { get; set; }
        public LoggerData(string nameOfEvent, string idOfContext, string dataOfEvent)
        {
            eventName = nameOfEvent;
            contextId = idOfContext;
            eventData = dataOfEvent;
        }
    }
    
}

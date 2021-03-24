using Common;
using Common.Structures;
using NATS.Client;
using System;
using System.Text.Json;

namespace EventsLogger
{
    public class EventsLogger
    {
        private static IConnection _broker;
        private readonly IAsyncSubscription _loggerSubscription;
        public EventsLogger()
        {
            _broker = new ConnectionFactory().CreateConnection();
            _loggerSubscription = _broker.SubscribeAsync(Constants.BROKER_CHANNEL_EVENTS_LOGGER, Print);
        }

        ~EventsLogger()
        {
            _broker.Drain();
            _broker.Close();
        }

        private EventHandler<MsgHandlerEventArgs> Print = (sender, args) =>
        {
            LoggerData data = JsonSerializer.Deserialize<LoggerData>(args.Message.Data);

            Console.Write("Event Name - " + data.eventName + " /// ");
            Console.Write("Context ID - " + data.contextId + " /// ");
            Console.WriteLine("Value - " + data.eventData);
        };
    }
}
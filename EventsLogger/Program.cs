using System.Threading.Tasks;

namespace EventsLogger
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var eventsLogger = new EventsLogger();
            await Task.Delay(-1);
        }
    }
}
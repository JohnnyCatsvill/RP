using Common.Messager;
using Common.Storage;
using System.Threading.Tasks;

namespace DisturbedStorage
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            System.Console.WriteLine("Enter unique storage name <int>");

            int storageName = int.Parse(System.Console.ReadLine());
            string protocol = "tcp";
            string address = "localhost";


            var calculator = new DistirbutedStorage(new Redis(), storageName, new NnPublisher(protocol, address), new NnSubscriber(protocol, address));
            calculator.Run();
        }
    }
}
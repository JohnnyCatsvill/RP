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

            System.Console.WriteLine("Enter unique pub port name <int>");
            string addressPubs = System.Console.ReadLine();

            System.Console.WriteLine("Enter unique sub port name <int>");
            int addressSubs = int.Parse(System.Console.ReadLine());


            string protocol = "tcp";
            //string address = "localhost";
            //string port = ":5999";

            
            var calculator = new DistirbutedStorage(new Redis(), storageName, new NmcPublisher(protocol, "localhost"+":"+addressPubs), new NmcSubscriber(protocol, "localhost" + ":" + addressSubs));
            calculator.Run();

            while( true)
            {
                string querry = System.Console.ReadLine();

                if (querry.StartsWith("read"))
                {
                    querry = querry[5..];
                    System.Console.WriteLine(calculator.Load(int.Parse(querry)));
                }
                else if (querry.StartsWith("write"))
                {
                    querry = querry[6..];
                    int key = int.Parse(querry[0..querry.IndexOf(" ")]);
                    calculator.Save(key, querry);
                }
            }

        }
    }
}
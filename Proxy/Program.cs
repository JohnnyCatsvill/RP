using NetMQ;
using NetMQ.Sockets;
using System;

namespace nngcat
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.Write("Enter publishers port: ");
            string address_pubs = System.Console.ReadLine();

            System.Console.Write("Enter subscribers port: ");
            string address_subs = System.Console.ReadLine();

            using (var xpubSocket = new XPublisherSocket("@tcp://127.0.0.1:" + address_subs))
            using (var xsubSocket = new XSubscriberSocket("@tcp://127.0.0.1:" + address_pubs))
            {
                Console.WriteLine("Intermediary started, and waiting for messages");
                // proxy messages between frontend / backend
                var proxy = new Proxy(xsubSocket, xpubSocket);
                // blocks indefinitely
                proxy.Start();
            }
        }
    }
}
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chain
{
    class Program
    {
        public static void StartClient(int listeningPort, string nextHost, int nextPort, bool isInit)
        {
            try
            {
                IPAddress prevIpAddress = IPAddress.Any;
                IPAddress nextIpAddress = (nextHost == "localhost") ? IPAddress.Loopback : IPAddress.Parse(nextHost);

                IPEndPoint prevRemoteEP = new IPEndPoint(prevIpAddress, listeningPort);
                IPEndPoint nextRemoteEP = new IPEndPoint(nextIpAddress, nextPort);

                Socket sender = new Socket(
                    nextIpAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                Socket listener = new Socket(
                    prevIpAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                try
                {
                    listener.Bind(prevRemoteEP);
                    listener.Listen(10);

                    sender.Connect(nextRemoteEP);

                    string number = Console.ReadLine();
                    int x = Convert.ToInt32(number);

                    Socket listenerHandler = listener.Accept();

                    if (isInit)
                    {
                        // SEND FIRST TOKEN
                        byte[] msg = Encoding.UTF8.GetBytes(x.ToString());
                        int bytesSent = sender.Send(msg);

                        // RECEIVE RESULT
                        byte[] buf = new byte[1024];
                        int bytesRec = listenerHandler.Receive(buf);
                        string data = Encoding.UTF8.GetString(buf, 0, bytesRec);
                        int y = Int32.Parse(data);

                        x = y;

                        // SEND RESULT
                        msg = Encoding.UTF8.GetBytes(x.ToString());
                        bytesSent = sender.Send(msg);

                        // RECEIVE RESULT TO BE SURE EVERYTHING GOES FINE
                        buf = new byte[1024];
                        bytesRec = listenerHandler.Receive(buf);
                        data = Encoding.UTF8.GetString(buf, 0, bytesRec);
                        x = Int32.Parse(data);

                        Console.Write(x);
                    }
                    else
                    {
                        // RECEIVE NEIGHBOUR NUMBER
                        byte[] buf = new byte[1024];
                        int bytesRec = listenerHandler.Receive(buf);
                        string data = Encoding.UTF8.GetString(buf, 0, bytesRec);
                        int y = Int32.Parse(data);

                        int maxOfXandY = Math.Max(x, y);

                        // SEND NEW NUMBER
                        byte[] msg = Encoding.UTF8.GetBytes(maxOfXandY.ToString());
                        int bytesSent = sender.Send(msg);

                        // RECEIVE RESULT
                        buf = new byte[1024];
                        bytesRec = listenerHandler.Receive(buf);
                        data = Encoding.UTF8.GetString(buf, 0, bytesRec);
                        x = Int32.Parse(data);

                        // SEND RESULT, не написано в кольцевом алгоритме, но ТЗ требует чтобы все процессы вывели максимальное значение
                        msg = Encoding.UTF8.GetBytes(x.ToString());
                        bytesSent = sender.Send(msg);

                        Console.Write(x);
                    }

                    listenerHandler.Shutdown(SocketShutdown.Both);
                    listenerHandler.Close();

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            int argv = args.Length;

            int listeningPort = Int32.Parse(args[0]);
            string nextHost = args[1];
            int nextPort = Int32.Parse(args[2]);
            bool isInit = false;

            if (argv >= 4)
            {
                isInit = bool.Parse(args[3]);
            }

            StartClient(listeningPort, nextHost, nextPort, isInit);
        }
    }
}

using System;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Text;
using ServerCore;
using System.Transactions;

namespace Server
{

    class Program
    {
        static Listener _listener = new Listener();
        static void Main(string[] args)
        {
            // DNS를 사용하도록 한다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("listening...");

            while (true)
            {

            }
        }
    }
}
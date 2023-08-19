using System;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Text;
using ServerCore;

namespace Server
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected  endPoint = " + endPoint);

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to mmorpg server");
            Send(sendBuff);

            Thread.Sleep(1000);

            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("OnDisconnected  endPoint = " + endPoint);
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine("recvData = " + recvData);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("transperedBytes = " + numOfBytes);
        }
    }


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

            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("listening...");

            while (true)
            {

            }
        }
    }
}
using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected  endPoint from server = " + endPoint);

            for (int i = 0; i < 5; ++i)
            {
                // 보낸다. 
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World " + i);
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("OnDisconnected  endPoint  " + endPoint);
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine("recvData from server = " + recvData);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("transperedBytes = " + numOfBytes);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // DNS를 사용하도록 한다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Thread.Sleep(100);
            }
        }
    }
}
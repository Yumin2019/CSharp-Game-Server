using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS를 사용하도록 한다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while(true)
            {
                try
                {
                    // 휴대폰 설정
                    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // 문지기 한테 입장 문의
                    socket.Connect(endPoint);
                    Console.WriteLine("connected to socket " + socket.RemoteEndPoint.ToString());

                    for (int i = 0; i < 5; ++i)
                    {
                        // 보낸다. 
                        byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World " + i);
                        int sendBytes = socket.Send(sendBuff);
                    }

                    // 받는다.
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine("recvData = " + recvData);

                    // 나간다.
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                // 받는다.
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine("recvData = " + recvData);

                // 보낸다. 
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to mmorpg server");
                clientSocket.Send(sendBuff);

                // 쫓아낸다.
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Main(string[] args)
        {

            // DNS를 사용하도록 한다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
           
            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("listening...");

            while (true)
            {


            }




        }
    }
}
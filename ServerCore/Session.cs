using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0;

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();

        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        object _lock = new object();


        public void Start(Socket socket)
        {
            _socket = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(byte[] sendBuff)
        {
            lock(_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                // 처음에 접근한 경우
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }


        #region 네트워크 통신

        void RegisterSend()
        {
            _pendingList.Clear();
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(Object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        Console.WriteLine("transperedBytes = " + _sendArgs.BytesTransferred);
                        if(_sendQueue.Count > 0)
                            RegisterSend();
 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("OnSendCompleted error " + e);
                    }
                }
            }
        }

        void RegisterRecv()
        {
           bool pending = _socket.ReceiveAsync(recvArgs);
            if(pending == false)
                OnRecvCompleted(null, recvArgs);
        }

        void OnRecvCompleted(Object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, 0, args.BytesTransferred);
                    Console.WriteLine("recvData = " + recvData);
                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine("OnReceiveCompleted error " + e);
                }
            }
            else
            {
                Disconnect();
            }
        } 
        #endregion
    }
}

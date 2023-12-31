﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        // [size(2)][packetId 2]...
        public static readonly int HeaderSize = 2;

        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLength = 0;
            int packetCount = 0;

            while(true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인 
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인
               ushort dataSize =  BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔으면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                ++packetCount;

                processLength += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            if(packetCount > 1)
                Console.WriteLine($"패킷 모아 보내기 count {packetCount}");

            return processLength;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
 

    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        object _lock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock(_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }


        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0)
                return;

            lock(_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                    _sendQueue.Enqueue(sendBuff);

                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Send(ArraySegment<byte> sendBuff)
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

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }


        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnected == 1)
                return;

            _pendingList.Clear();
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }

            _sendArgs.BufferList = _pendingList;

            try
            {
                if(_socket == null)
                    return;
                
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch(Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
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

                        OnSend(_sendArgs.BytesTransferred);
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
            if (_disconnected == 1)
                return;
   
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
        }

        void OnRecvCompleted(Object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동
                    if(_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 콘텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLength = OnRecv(_recvBuffer.ReadSegment);
                    if(processLength < 0 || _recvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // read 커서 이동
                    if(_recvBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

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

using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }


    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            count += 2;
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + 2);
            count += 2;

            this.playerId = BitConverter.ToInt16(s.Array, s.Offset + count);
            count += 8;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), playerId);
            count += 8;

            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;

        public override void Read(ArraySegment<byte> s)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Write()
        {



            throw new NotImplementedException();
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2
    }

    class SeverSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            //PlayerInfoReq packet = new PlayerInfoReq() { size = 4, packetId = (ushort)PacketID.PlayerInfoReq, playerId = 1001 };


            //// 보낸다 
            //for (int i = 0; i < 5; i++)
            //{
            //    ArraySegment<byte> s = SendBufferHelper.Open(4096);
            //    //byte[] size = BitConverter.GetBytes(packet.size);
            //    //byte[] packetId = BitConverter.GetBytes(packet.packetId);
            //    //byte[] playerId = BitConverter.GetBytes(packet.playerId);

            //    ushort size = 0;
            //    bool success = true;

            //    size += 2;
            //    success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), packet.packetId);
            //    size += 2;
            //    success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), packet.playerId);
            //    size += 8;
            //    success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), size);

            //    ArraySegment<byte> sendBuff = SendBufferHelper.Close(size);

            //    if (success)
            //        Send(sendBuff);
            //}


            Console.WriteLine("OnConnected  endPoint from server = " + endPoint);
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };

            {
                ArraySegment<byte> s = packet.Write();
                if (s != null)
                    Send(s);
            }

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("OnDisconnected  endPoint  " + endPoint);
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine("recvData from server = " + recvData);

            return buffer.Count();
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("transperedBytes = " + numOfBytes);
        }
    }
}

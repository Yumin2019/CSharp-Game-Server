using ServerCore;

namespace DummyClient.Packet
{
    // Server to Server packet, Client to Server Packet 구분해서 작성해야 한다. 
    class PacketHandler
    {
        internal static void S_ChatHandler(PacketSession session, IPacket packet)
        {
            S_Chat chatPacket = packet as S_Chat;
            ServerSession serverSession = session as ServerSession;

        }
    }
}

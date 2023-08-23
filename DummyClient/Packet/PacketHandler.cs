using ServerCore;

namespace DummyClient.Packet
{
    // Server to Server packet, Client to Server Packet 구분해서 작성해야 한다. 
    class PacketHandler
    {
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            C_PlayerInfoReq p = packet as C_PlayerInfoReq;
            Console.WriteLine($"PlayerInfoReq: {p.playerId} {p.name}");

            foreach (C_PlayerInfoReq.Skill skill in p.skill)
            {
                Console.WriteLine($"Skill {skill.id} {skill.level} {skill.duration}");
            }
        }

        public static void S_TestHandler(PacketSession arg1, IPacket arg2)
        {
            throw new NotImplementedException();
        }
    }
}

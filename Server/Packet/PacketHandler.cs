using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class PacketHandler
    {
        public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            C_PlayerInfoReq p = packet as C_PlayerInfoReq;
            Console.WriteLine($"PlayerInfoReq: {p.playerId} {p.name}");

            foreach (C_PlayerInfoReq.Skill skill in p.skill)
            {
                Console.WriteLine($"Skill {skill.id} {skill.level} {skill.duration}");
            }
        }
    }
}

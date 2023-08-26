using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            lock (_lock)
            {
                foreach (ClientSession session in _sessions)
                    session.Send(_pendingList);

                Console.WriteLine($"Flushed {_pendingList.Count} items");
                _pendingList.Clear();
            }
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionID;
            packet.chat = $"{chat} I am {packet.playerId}";

            ArraySegment<byte> segment = packet.Write();

            lock(_lock)
            {
                _pendingList.Add(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Remove(session);
                session.Room = null;
            }
        }

    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Server
//{
//    interface ITask
//    {
//        void Execute();
//    }

//    class BroadcastTask : ITask
//    {
//        GameRoom _room;
//        ClientSession _session;
//        string _chat;

//        BroadcastTask(GameRoom room, ClientSession session, string chat)
//        {
//            _session = session;
//            _chat = chat;
//            _room = room;
//        }

//        public void Execute()
//        {
//            _room.Broadcast(_session, _chat);
//        }
//    }

//    class TaskQueue
//    {
//        Queue<Task> _queue = new Queue<Task>();
//    }
//}

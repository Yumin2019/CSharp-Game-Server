using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 사용 예제
    //class ThreadProgram
    //{
    //    static volatile int count = 0;
    //    static Lock _lock = new Lock();

    //    static void Main(string[] args)
    //    {
    //        Task t1 = new Task(delegate ()
    //        {
    //            for (int i = 0; i < 10000000; ++i)
    //            {
    //                _lock.WriteLock();
    //                count++;
    //                _lock.WriteUnlock();
    //            }
    //        });
    //        Task t2 = new Task(delegate ()
    //        {
    //            for (int i = 0; i < 10000000; ++i)
    //            {
    //                _lock.WriteLock();
    //                count--;
    //                _lock.WriteUnlock();
    //            }
    //        });

    //        t1.Start();
    //        t2.Start();

    //        Task.WaitAll(t1, t2);
    //        Console.WriteLine("count = " + count);
    //    }
    //}

    // 재귀적 락을 허용할지 WriteLock->WriteLock (OK) WriteLock -> ReadLock OK ReadLock -> WriteLock OK
    // [unused 1] [writeThreadId 15] [ReadCount 16] 
    public class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if(Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                ++_writeCount;
                return;
            }

            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 소유권을 얻는다. 
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; ++i)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if(lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다. 
            while (true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; ++i)
                {
                    // 두 마리가 같이 들어왔을 때, 둘 다 0을 기대하는데 한 쪽에서 값을 올려버린다.
                    // 그러면서 __flag와 expected 값이 일치하지 않아서 다른 놈은 못 들어온다. 
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}

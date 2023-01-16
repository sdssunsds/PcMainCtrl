using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PcMainCtrl.Common
{
    public class ThreadManager
    {
        public static int taskCount = 0;
        public static int threadCount = 0;
        private static CancellationTokenSource cancellation = new CancellationTokenSource();
        private static List<Thread> threads = new List<Thread>();

        /// <summary>
        /// 线程数量变化的事件
        /// </summary>
        public static event Action TastCountChanged;
        /// <summary>
        /// 发生线程等待的事件
        /// </summary>
        public static event Func<int, DateTime> SleepInternal;
        /// <summary>
        /// 线程等待结束的事件
        /// </summary>
        public static event Action<DateTime> SleepEnd;

        public static void TaskRun(Action action)
        {
            Task.Run(() =>
            {
                if (!cancellation.IsCancellationRequested)
                {
                    taskCount++;
                    TastCountChanged?.Invoke();
                    action();
                    taskCount--;
                    TastCountChanged?.Invoke();
                }
            }, cancellation.Token);
        }
        public static void ThreadSleep(int sleep)
        {
            //DateTime dt = DateTime.Now;
            //if (SleepInternal != null)
            //{
            //    dt = SleepInternal.Invoke(sleep); 
            //}
            Thread.Sleep(sleep);
            //SleepEnd?.Invoke(dt);
        }
        public static void ThreadStart(Action action)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                action();
                threadCount--;
                TastCountChanged?.Invoke();
            }));
            threads.Add(thread);
            threadCount++;
            TastCountChanged?.Invoke();
            thread.Start();
        }
        public static void CloseAllThread()
        {
            try
            {
                cancellation.Cancel();
            }
            catch (Exception) { }
            foreach (Thread thread in threads)
            {
                try
                {
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }
                catch (Exception) { }
            }
        }
    }
}

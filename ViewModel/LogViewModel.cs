using PcMainCtrl.Common;
using System;
using System.Collections.Generic;

namespace PcMainCtrl.ViewModel
{
    public class LogViewModel : NotifyBase
    {
        private object logLock = new object();
        private object sleepLock = new object();
        private string log = "";
        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                this.DoNotify();
            }
        }

        private const string otherFormat = "Task数量：{0}\r\nThread数量：{1}\r\nSleep列表\r\n  {2}";
        private object otherLock = new object();
        private string other = "";
        private Dictionary<DateTime, int> sleepList = new Dictionary<DateTime, int>();
        public string Other
        {
            get { return other; }
            set
            {
                other = value;
                this.DoNotify();
            }
        }

        public LogViewModel()
        {
            GlobalValues.AddLogEvent += GlobalValues_AddLogEvent;
            ThreadManager.TastCountChanged += ThreadCountChanged_OtherEvent;
            ThreadManager.SleepInternal += SleepInternal_OtherEvent;
            ThreadManager.SleepEnd += SleepEnd_OtherEvent;
        }

        private void GlobalValues_AddLogEvent(string log, int type)
        {
            lock (logLock)
            {
                Log += log + "\r\n";
            }
        }

        private void ThreadCountChanged_OtherEvent()
        {
            ShowOther();
        }

        private DateTime SleepInternal_OtherEvent(int sleep)
        {
            DateTime dt = DateTime.Now;
        checkTime:
            if (sleepList.ContainsKey(dt))
            {
                dt = dt.AddSeconds(0.0001);
                goto checkTime;
            }
            lock (sleepLock)
            {
                sleepList.Add(dt, sleep); 
            }
            ShowOther();
            return dt;
        }

        private void SleepEnd_OtherEvent(DateTime dt)
        {
            try
            {
                if (sleepList.ContainsKey(dt))
                {
                    lock (sleepLock)
                    {
                        sleepList.Remove(dt); 
                    }
                    ShowOther(); 
                }
            }
            catch (System.Exception) { }
        }

        private void ShowOther()
        {
            lock (otherLock)
            {
                int[] sleeps = new int[sleepList.Count];
                lock (sleepLock)
                {
                    int i = 0;
                    foreach (KeyValuePair<DateTime, int> item in sleepList)
                    {
                        if (i < sleeps.Length)
                        {
                            sleeps[i] = item.Value;
                        }
                        else
                        {
                            break;
                        }
                        i++;
                    } 
                }
                Other = string.Format(otherFormat, ThreadManager.taskCount, ThreadManager.threadCount, string.Join("\r\n  ", sleeps));
            }
        }
    }
}

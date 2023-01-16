using System;
using System.Diagnostics;

namespace PcMainCtrl.Common
{
    /// <summary>
    /// 耗时计算
    /// </summary>
    public class TimeConsumingCalculation
    {
        private static Stopwatch stopWatch = new Stopwatch();
        public static void Start()
        {
            stopWatch.Start();
        }
        public static void Stop()
        {
            stopWatch.Stop();
        }
        public static void ReStart()
        {
            stopWatch.Restart();
        }
        public static TimeSpan GetTime()
        {
            return stopWatch.Elapsed;
        }
    }
}

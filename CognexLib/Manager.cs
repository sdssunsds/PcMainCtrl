using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CognexLib
{
    public class Manager : IDisposable
    {
        private const string keyCogIP = "CogIPOneImageTool1";
        private const string keyLastRun = "LastRun";
        private const string keyName = "JobName";
        private const string keyShowLast = "ShowLastRunRecordForUserQueue";

        public readonly string[] xieyi = { "Length", "Width", "High", "High_3", "High_4", "High_5", "High_6", "High_7", "High_8", "High_9", "High_10" };
        public readonly Dictionary<int, string> xieyiDict = new Dictionary<int, string>()
        {
            { 0, "长度"}, {1, "宽度"}, {2, "高度"}, {3, "高度1"}, {4, "高度2"}, {5, "高度3"},
            {6, "高度4"}, {7, "高度5"}, {8, "高度6"}, {9, "高度7"}, {10, "高度8"}
        };
        private Dictionary<string, Dictionary<int, CogToolBlock>> tbDict = new Dictionary<string, Dictionary<int, CogToolBlock>>();

        private CogJobManager cogJobManager = null;
        private CogToolGroup[] groups = new CogToolGroup[2];

        /// <summary>
        /// 显示控件
        /// </summary>
        public CogRecordDisplay cogRecordDisplay { private get; set; }

        /// <summary>
        /// 获取图片事件
        /// </summary>
        public event Action<Bitmap, JobName> GetImageEvent;
        /// <summary>
        /// 获取结果事件
        /// </summary>
        public event Action<string[], JobName> GetResultEvent;
        /// <summary>
        /// 停止事件
        /// </summary>
        public event Action StopEvent;

        public void Create(string vppPath)
        {
            cogJobManager = CogSerializer.LoadObjectFromFile(vppPath) as CogJobManager;

            cogJobManager.UserQueueFlush();
            cogJobManager.FailureQueueFlush();
            cogJobManager.Stopped += CogJobManager_Stopped;
            cogJobManager.UserResultAvailable += CogJobManager_UserResultAvailable;

            string[] values = Enum.GetNames(typeof(JobName));
            for (int i = 0; i < values.Length; i++)
            {
                CogJob cogJob1 = cogJobManager.Job(i);
                CogJobIndependent cogJobIndependent = cogJob1.OwnedIndependent;
                groups[i] = cogJob1.VisionTool as CogToolGroup;
                cogJob1.ImageQueueFlush();
                cogJobIndependent.RealTimeQueueFlush();

                tbDict.Add(values[i], new Dictionary<int, CogToolBlock>());
                for (int j = 2; j < groups[i].Tools.Count; j++)
                {
                    tbDict[values[i]].Add(j - 2, groups[i].Tools[j] as CogToolBlock);
                }
            }
        }

        /// <summary>
        /// 启动扫描
        /// </summary>
        /// <param name="xieyiIndex">协议索引</param>
        public void Run(int xieyiIndex, int jobIndex)
        {
            Console.WriteLine("3D扫描仪执行第1步");
            groups[jobIndex].DisabledTools.Clear();
            Console.WriteLine("3D扫描仪执行第2步");
            string jobName = ((JobName)jobIndex).ToString();
            Console.WriteLine("3D扫描仪执行第3步——循环");
            for (int j = 0; j < tbDict[jobName].Count; j++)
            {
                if (j != xieyiIndex)
                {
                    groups[jobIndex].DisabledTools.Add(tbDict[jobName][j]);
                }
            }
            Console.WriteLine("3D扫描仪执行第4步");
            cogJobManager?.Job(jobIndex)?.Run();
        }

        /// <summary>
        /// 启动扫描
        /// </summary>
        /// <param name="xieyiIndex">协议索引</param>
        public void Run(int[] xieyiIndex)
        {
            Console.WriteLine("3D扫描仪执行第1步——循环");
            for (int i = 0; i < groups.Length; i++)
            {
                Console.WriteLine("3D扫描仪执行第2步");
                groups[i].DisabledTools.Clear();
                Console.WriteLine("3D扫描仪执行第3步");
                string jobName = ((JobName)i).ToString();
                Console.WriteLine("3D扫描仪执行第4步——循环");
                for (int j = 0; j < tbDict[jobName].Count; j++)
                {
                    if (j != xieyiIndex[i])
                    {
                        groups[i].DisabledTools.Add(tbDict[jobName][j]);
                    }
                }
            }
            Console.WriteLine("3D扫描仪执行第5步");
            cogJobManager?.Run();
        }

        public void Stop()
        {
            cogJobManager?.Stop();
        }

        public void Dispose()
        {
            cogJobManager?.Shutdown();
            cogJobManager.Stopped -= CogJobManager_Stopped;
            cogJobManager.UserResultAvailable -= CogJobManager_UserResultAvailable;
            cogJobManager = null;
        }

        public void ShowQuick()
        {
            FormQuickBuild frmQB = new FormQuickBuild(cogJobManager);
            frmQB.Show();
        }

        private void CogJobManager_UserResultAvailable(object sender, CogJobManagerActionEventArgs e)
        {
            ICogRecord topRecord = cogJobManager.UserResult();
            ICogRecord tmpRecord = topRecord.SubRecords[keyShowLast];
            string name = topRecord.SubRecords[keyName].Content.ToString();
            tmpRecord = tmpRecord.SubRecords[keyLastRun];
            //tmpRecord = tmpRecord.SubRecords[keyCogIP];

            if (cogRecordDisplay != null)
            {
                cogRecordDisplay.Record = tmpRecord;
                cogRecordDisplay.Fit(true); 
            }

            JobName jobName = (JobName)Enum.Parse(typeof(JobName), name);
            CogIPOneImageTool cogIPOneImage = groups[(int)jobName].Tools[1] as CogIPOneImageTool;
            string[] value = new string[xieyi.Length];
            for (int i = 0; i < tbDict[name].Count; i++)
            {
                value[i] = tbDict[name][i].Outputs[xieyi[i]].Value.ToString();
            }

            GetImageEvent?.Invoke(cogIPOneImage?.OutputImage?.ToBitmap(), jobName);
            GetResultEvent?.Invoke(value, jobName);
        }

        private void CogJobManager_Stopped(object sender, CogJobManagerActionEventArgs e)
        {
            StopEvent?.Invoke();
        }
    }

    public enum JobName
    {
        CogJob1 = 0, CogJob2 = 1
    }
}

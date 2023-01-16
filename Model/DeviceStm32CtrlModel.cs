using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    public class DeviceStm32CtrlModel : NotifyBase
    {
        /// <summary>
        /// 控制语言播报信息
        /// </summary>
        public String VoiceMsg { get; set; }

        /// <summary>
        /// 面阵相机光源PWM调节百分比
        /// </summary>
        public int Light1Percentage { get; set; }
        public int Light2Percentage { get; set; }

        /// <summary>
        /// 当前检测车底位置
        /// </summary>
        private String currentDetectSite;
        public String CurrentDetectSite
        {
            get { return currentDetectSite; }
            set
            {
                currentDetectSite = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 当前检测车底高度
        /// </summary>
        private int currentDetectHeight;
        public int CurrentDetectHeight
        {
            get { return currentDetectHeight; }
            set 
            { 
                currentDetectHeight = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 红外第1路信号状态
        /// </summary>
        private int infraRed1_Stat;
        public int InfraRed1_Stat
        {
            get { return infraRed1_Stat; }
            set 
            { 
                infraRed1_Stat = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 红外第2路信号状态
        /// </summary>
        private int infraRed2_Stat;
        public int InfraRed2_Stat
        {
            get { return infraRed2_Stat; }
            set 
            { 
                infraRed2_Stat = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 红外第3路信号状态
        /// </summary>
        private int infraRed3_Stat;
        public int InfraRed3_Stat
        {
            get { return infraRed3_Stat; }
            set 
            { 
                infraRed3_Stat = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 红外第4路信号状态
        /// </summary>
        private int infraRed4_Stat;
        public int InfraRed4_Stat
        {
            get { return infraRed4_Stat; }
            set 
            { 
                infraRed4_Stat = value;
                this.DoNotify();
            }
        }
    }
}

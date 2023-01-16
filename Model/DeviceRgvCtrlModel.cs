using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    public class DeviceRgvCtrlModel : NotifyBase
    {
        /// <summary>
        /// Rgv当前模式：本地模式,远程模式
        /// </summary>
        private String rgvCurrentMode;
        public String RgvCurrentMode
        {
            get { return rgvCurrentMode; }
            set
            {
                rgvCurrentMode = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv当前状态信息
        /// </summary>
        private string rgvCurrentStat;
        public string RgvCurrentStat
        {
            get { return rgvCurrentStat; }
            set
            {
                rgvCurrentStat = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv命令设置状态
        /// </summary>
        private string rgvCurrentCmdSetStat;
        public string RgvCurrentCmdSetStat
        {
            get { return rgvCurrentCmdSetStat; }
            set
            {
                rgvCurrentCmdSetStat = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv参数设置状态
        /// </summary>
        private int rgvCurrentParaSetStat;
        public int RgvCurrentParaSetStat
        {
            get { return rgvCurrentParaSetStat; }
            set
            {
                rgvCurrentParaSetStat = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv小车异常状态：0-设备正常,1-设备异常
        /// </summary>
        private int rgvIsAlarm;
        public int RgvIsAlarm
        {
            get { return rgvIsAlarm; }
            set
            {
                rgvIsAlarm = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv小车运行状态：0-待机停止,1-运动状态
        /// </summary>
        private int rgvIsStandby;
        public int RgvIsStandby
        {
            get { return rgvIsStandby; }
            set
            {
                rgvIsStandby = value;
                this.DoNotify();
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rgv运行速度
        /// </summary>
        private int rgvCurrentRunSpeed;
        public int RgvCurrentRunSpeed
        {
            get { return rgvCurrentRunSpeed; }
            set
            {
                rgvCurrentRunSpeed = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv运行距离
        /// </summary>
        private int rgvCurrentRunDistacnce;
        public int RgvCurrentRunDistacnce
        {
            get { return rgvCurrentRunDistacnce; }
            set 
            { 
                rgvCurrentRunDistacnce = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv当前电池状态
        /// </summary>
        private int rgvCurrentPowerStat;
        public int RgvCurrentPowerStat
        {
            get { return rgvCurrentPowerStat; }
            set
            {
                rgvCurrentPowerStat = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv当前电池电量
        /// </summary>
        private int rgvCurrentPowerElectricity;
        public int RgvCurrentPowerElectricity
        {
            get { return rgvCurrentPowerElectricity; }
            set
            {
                rgvCurrentPowerElectricity = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv当前电池电流
        /// </summary>
        private int rgvCurrentPowerCurrent;
        public int RgvCurrentPowerCurrent
        {
            get { return rgvCurrentPowerCurrent; }
            set 
            {
                rgvCurrentPowerCurrent = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv当前电池温度
        /// </summary>
        private int rgvCurrentPowerTempture;
        public int RgvCurrentPowerTempture
        {
            get { return rgvCurrentPowerTempture; }
            set
            {
                rgvCurrentPowerTempture = value;
                this.DoNotify();
            }
        }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Rgv目标运行速度
        /// </summary>
        private int rgvTargetRunSpeed;
        public int RgvTargetRunSpeed
        {
            get { return rgvTargetRunSpeed; }
            set 
            {
                rgvTargetRunSpeed = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv目标运行距离
        /// </summary>
        private int rgvTargetRunDistance;
        public int RgvTargetRunDistance
        {
            get { return rgvTargetRunDistance; }
            set 
            {
                rgvTargetRunDistance = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Rgv目标轨道长度
        /// </summary>
        private int rgvTrackLength;
        public int RgvTrackLength
        {
            get { return rgvTrackLength; }
            set
            {
                rgvTrackLength = value;
                this.DoNotify();
            }
        }
    }
}

using Basler.Pylon;
using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    namespace PcMainCtrl.Model
    {
        public class TaskXzCameraModel : NotifyBase
        {
            //-----------------------------------------------------------------------------------------------------
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
            /// Rgv目标距离
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
            //-----------------------------------------------------------------------------------------------------
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

            //-----------------------------------------------------------------------------------------------------
            private ICameraInfo xzCamerainfoItem;
            public ICameraInfo XzCamerainfoItem
            {
                get { return xzCamerainfoItem; }
                set
                {
                    xzCamerainfoItem = value;
                    this.DoNotify();
                }
            }
        }
    }
}

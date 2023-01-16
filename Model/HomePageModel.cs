using Basler.Pylon;
using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    public class HomePageModel : NotifyBase
    {
        //-----------------------------------------------------------------------------------------------------
        #region RGV控制
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
        #endregion

        //-----------------------------------------------------------------------------------------------------
        #region 机械臂Robot
        /// <summary>
        /// Ronbot状态消息
        /// </summary>
        private String frontRobotMsg;
        public String FrontRobotMsg
        {
            get { return frontRobotMsg; }
            set
            {
                frontRobotMsg = value;
                this.DoNotify();
            }
        }

        private String frontRobot_J1;
        public String FrontRobot_J1

        {
            get { return frontRobot_J1; }
            set
            {
                frontRobot_J1 = value;
                this.DoNotify();
            }
        }

        private String frontRobot_J2;
        public String FrontRobot_J2
        {
            get { return frontRobot_J2; }
            set
            {
                frontRobot_J2 = value;
                this.DoNotify();
            }
        }

        private String frontRobot_J3;
        public String FrontRobot_J3
        {
            get { return frontRobot_J3; }
            set
            {
                frontRobot_J3 = value;
                this.DoNotify();
            }
        }

        private String frontRobot_J4;
        public String FrontRobot_J4
        {
            get { return frontRobot_J4; }
            set
            {
                frontRobot_J4 = value;
                this.DoNotify();
            }
        }

        private String frontRobot_J5;
        public String FrontRobot_J5
        {
            get { return frontRobot_J5; }
            set
            {
                frontRobot_J5 = value;
                this.DoNotify();
            }
        }

        private String frontRobot_J6;
        public String FrontRobot_J6
        {
            get { return frontRobot_J6; }
            set
            {
                frontRobot_J6 = value;
                this.DoNotify();
            }
        }

        //-----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Robot状态消息
        /// </summary>
        private String backRobotMsg;
        public String BackRobotMsg
        {
            get { return backRobotMsg; }
            set
            {
                backRobotMsg = value;
                this.DoNotify();
            }
        }

        private String backRobot_J1;
        public String BackRobot_J1
        {
            get { return backRobot_J1; }
            set
            {
                backRobot_J1 = value;
                this.DoNotify();
            }
        }

        private String backRobot_J2;
        public String BackRobot_J2
        {
            get { return backRobot_J2; }
            set
            {
                backRobot_J2 = value;
                this.DoNotify();
            }
        }
        private String backRobot_J3;
        public String BackRobot_J3
        {
            get { return backRobot_J3; }
            set
            {
                backRobot_J3 = value;
                this.DoNotify();
            }
        }

        private String backRobot_J4;
        public String BackRobot_J4
        {
            get { return backRobot_J4; }
            set
            {
                backRobot_J4 = value;
                this.DoNotify();
            }
        }

        private String backRobot_J5;
        public String BackRobot_J5
        {
            get { return backRobot_J5; }
            set
            {
                backRobot_J5 = value;
                this.DoNotify();
            }
        }

        private String backRobot_J6;
        public String BackRobot_J6
        {
            get { return backRobot_J6; }
            set
            {
                backRobot_J6 = value;
                this.DoNotify();
            }
        }
        #endregion

        //-----------------------------------------------------------------------------------------------------
        #region 相机管理
        private ICameraInfo frontCamerainfoItem;
        public ICameraInfo FrontCamerainfoItem
        {
            get { return frontCamerainfoItem; }
            set
            {
                frontCamerainfoItem = value;
                this.DoNotify();
            }
        }

        private ICameraInfo backCamerainfoItem;
        public ICameraInfo BackCamerainfoItem
        {
            get { return backCamerainfoItem; }
            set
            {
                backCamerainfoItem = value;
                this.DoNotify();
            }
        }

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
        #endregion

        //-----------------------------------------------------------------------------------------------------
        #region STM32开发板采集信息
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
        #endregion
    }
}
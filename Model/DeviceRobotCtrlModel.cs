using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    public class DeviceRobotCtrlModel : NotifyBase
    {
        /// <summary>
        /// Ronbot状态消息
        /// </summary>
        private String robotMsg;
        public String RobotMsg
        {
            get { return robotMsg; }
            set
            {
                robotMsg = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Robot命令数据格式
        /// </summary>
        private String j1;
        public String J1
        {
            get { return j1; }
            set
            {
                j1 = value;
                this.DoNotify();
            }
        }

        private String j2;
        public String J2
        {
            get { return j2; }
            set
            {
                j2 = value;
                this.DoNotify();
            }
        }

        private String j3;
        public String J3
        {
            get { return j3; }
            set
            {
                j3 = value;
                this.DoNotify();
            }
        }

        private String j4;
        public String J4
        {
            get { return j4; }
            set
            {
                j4 = value;
                this.DoNotify();
            }
        }

        private String j5;
        public String J5
        {
            get { return j5; }
            set
            {
                j5 = value;
                this.DoNotify();
            }
        }

        private String j6;
        public String J6
        {
            get { return j6; }
            set
            {
                j6 = value;
                this.DoNotify();
            }
        }
    }

    public class DeviceRobotDataLineModel : NotifyBase
    {
        /// <summary>
        /// 数据行描述
        /// </summary>
        private int dataLine_Index;
        public int DataLine_Index
        {
            get { return dataLine_Index; }
            set
            {
                dataLine_Index = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 数据行描述
        /// </summary>
        private String dataLine_Discript;
        public String DataLine_Discript
        {
            get { return dataLine_Discript; }
            set
            {
                dataLine_Discript = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Ronbot编号
        /// </summary>
        private String frontRobot_Id;
        public String FrontRobot_Id
        {
            get { return frontRobot_Id; }
            set
            {
                frontRobot_Id = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Ronbot使能
        /// </summary>
        private String frontRobot_Enable;
        public String FrontRobot_Enable
        {
            get { return frontRobot_Enable; }
            set
            {
                frontRobot_Enable = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Robot命令数据格式
        /// </summary>
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

        /// <summary>
        /// Ronbot编号
        /// </summary>
        private String backRobot_Id;
        public String BackRobot_Id
        {
            get { return backRobot_Id; }
            set
            {
                backRobot_Id = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Ronbot使能
        /// </summary>
        private String backRobot_Enable;
        public String BackRobot_Enable
        {
            get { return backRobot_Enable; }
            set
            {
                backRobot_Enable = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// Robot命令数据格式
        /// </summary>
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
    }
}

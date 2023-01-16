using PcMainCtrl.Common;
using System;
using System.Reflection;

namespace PcMainCtrl.Model
{
    public class DataMzCameraModel :  NotifyBase
    {
        //--------------------------------------------------------------------------------------------
        private string trainMode;
        public string TrainMode
        {
            get { return trainMode; }
            set
            {
                trainMode = value;
                this.DoNotify();
            }
        }

        private string trainSn;
        public string TrainSn
        {
            get { return trainSn; }
            set
            {
                trainSn = value;
                this.DoNotify();
            }
        }

        //--------------------------------------------------------------------------------------------
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
        /// Rgv运行距离
        /// </summary>
        private int rgvCurrentHeadDistacnce;
        public int RgvCurrentHeadDistacnce
        {
            get { return rgvCurrentHeadDistacnce; }
            set
            {
                rgvCurrentHeadDistacnce = value;
                this.DoNotify();
            }
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Relay协议信息
        /// </summary>
        private String relayRspMsg;
        public String RelayRspMsg
        {
            get { return relayRspMsg; }
            set
            {
                relayRspMsg = value;
                this.DoNotify();
            }
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Robot状态消息
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

        private int frontRobot_Enable;
        public int FrontRobot_Enable
        {
            get { return frontRobot_Enable; }
            set
            {
                frontRobot_Enable = value;
                this.DoNotify();
            }
        }

        private int frontCamera_Enable;
        public int FrontCamera_Enable
        {
            get { return frontCamera_Enable; }
            set
            {
                frontCamera_Enable = value;
                this.DoNotify();
            }
        }

        private String frontComponentId;
        public String FrontComponentId
        {
            get { return frontComponentId; }
            set
            {
                frontComponentId = value;
                this.DoNotify();
            }
        }

        private String frontComponentType;
        public String FrontComponentType
        {
            get { return frontComponentType; }
            set
            {
                frontComponentType = value;
                this.DoNotify();
            }
        }

        //---------------------------------------------
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

        private int backRobot_Enable;
        public int BackRobot_Enable
        {
            get { return backRobot_Enable; }
            set
            {
                backRobot_Enable = value;
                this.DoNotify();
            }
        }

        private int backCamera_Enable;
        public int BackCamera_Enable
        {
            get { return backCamera_Enable; }
            set
            {
                backCamera_Enable = value;
                this.DoNotify();
            }
        }

        private String backComponentId;
        public String BackComponentId
        {
            get { return backComponentId; }
            set
            {
                backComponentId = value;
                this.DoNotify();
            }
        }

        private String backComponentType;
        public String BackComponentType
        {
            get { return backComponentType; }
            set
            {
                backComponentType = value;
                this.DoNotify();
            }
        }
    }

    public class DataMzCameraLineModel : NotifyBase
    {
        private int dataLine_Index;
        /// <summary>
        /// 数据行编号
        /// </summary>
        public int DataLine_Index
        {
            get { return dataLine_Index; }
            set
            {
                dataLine_Index = value;
                this.DoNotify();
            }
        }

        private string trainModel;
        /// <summary>
        /// 动车组型号
        /// </summary>
        public string TrainModel
        {
            get { return trainModel; }
            set
            {
                trainModel = value;
                this.DoNotify();
            }
        }

        private string trainSn;
        /// <summary>
        /// 动车组序号
        /// </summary>
        public string TrainSn
        {
            get { return trainSn; }
            set
            {
                trainSn = value;
                this.DoNotify();
            }
        }

        private int rgv_Id;
        /// <summary>
        /// RGV编号
        /// </summary>
        public int Rgv_Id
        {
            get { return rgv_Id; }
            set
            {
                rgv_Id = value;
                this.DoNotify();
            }
        }

        private int rgv_Enable;
        public int Rgv_Enable
        {
            get { return rgv_Enable; }
            set
            {
                rgv_Enable = value;
                this.DoNotify();
            }
        }

        private int rgv_Distance;
        public int Rgv_Distance
        {
            get { return rgv_Distance; }
            set
            {
                rgv_Distance = value;
                this.DoNotify();
            }
        }

        private string front_Parts_Id;
        /// <summary>
        /// 前部件编号
        /// </summary>
        public string Front_Parts_Id
        {
            get { return front_Parts_Id; }
            set
            {
                front_Parts_Id = value;
                this.DoNotify();
            }
        }

        private string back_Parts_Id;
        /// <summary>
        /// 后部件编号
        /// </summary>
        public string Back_Parts_Id
        {
            get { return back_Parts_Id; }
            set
            {
                back_Parts_Id = value;
                this.DoNotify();
            }
        }

        private string location;
        /// <summary>
        /// 位置（车厢索引_转向架索引）
        /// </summary>
        public string Location
        {
            get { return location; }
            set
            {
                location = value;
                this.DoNotify();
            }
        }

        private string point;
        /// <summary>
        /// 点位（z1或z2）
        /// </summary>
        public string Point
        {
            get { return point; }
            set
            {
                point = value;
                this.DoNotify();
            }
        }
        
        private int frontRobot_Id;
        /// <summary>
        /// Ronbot编号
        /// </summary>
        public int FrontRobot_Id
        {
            get { return frontRobot_Id; }
            set
            {
                frontRobot_Id = value;
                this.DoNotify();
            }
        }

        private int frontRobot_Enable;
        /// <summary>
        /// Ronbot使能
        /// </summary>
        public int FrontRobot_Enable
        {
            get { return frontRobot_Enable; }
            set
            {
                frontRobot_Enable = value;
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

        private int frontCamera_Id;
        /// <summary>
        /// Camera编号
        /// </summary>
        public int FrontCamera_Id
        {
            get { return frontCamera_Id; }
            set
            {
                frontCamera_Id = value;
                this.DoNotify();
            }
        }

        private int front3d_Id;
        /// <summary>
        /// 3D算法编号
        /// </summary>
        public int Front3d_Id
        {
            get { return front3d_Id; }
            set
            {
                front3d_Id = value;
                this.DoNotify();
            }
        }

        private int frontCamera_Enable;
        /// <summary>
        /// Camera使能
        /// </summary>
        public int FrontCamera_Enable
        {
            get { return frontCamera_Enable; }
            set
            {
                frontCamera_Enable = value;
                this.DoNotify();
            }
        }

        private String frontComponentId;
        /// <summary>
        /// 部件总编号
        /// </summary>
        public String FrontComponentId
        {
            get { return frontComponentId; }
            set
            {
                frontComponentId = value;
                this.DoNotify();
            }
        }

        private String frontComponentType;
        /// <summary>
        /// 相机类别
        /// </summary>
        public String FrontComponentType
        {
            get { return frontComponentType; }
            set
            {
                frontComponentType = value;
                this.DoNotify();
            }
        }

        private int backRobot_Id;
        /// <summary>
        /// Robot编号
        /// </summary>
        public int BackRobot_Id
        {
            get { return backRobot_Id; }
            set
            {
                backRobot_Id = value;
                this.DoNotify();
            }
        }

        private int backRobot_Enable;
        /// <summary>
        /// Ronbot使能
        /// </summary>
        public int BackRobot_Enable
        {
            get { return backRobot_Enable; }
            set
            {
                backRobot_Enable = value;
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

        private int backCamera_Id;
        /// <summary>
        /// Camera编号
        /// </summary>
        public int BackCamera_Id
        {
            get { return backCamera_Id; }
            set
            {
                backCamera_Id = value;
                this.DoNotify();
            }
        }

        private int back3d_Id;
        /// <summary>
        /// 3D算法编号
        /// </summary>
        public int Back3d_Id
        {
            get { return back3d_Id; }
            set
            {
                back3d_Id = value;
                this.DoNotify();
            }
        }

        private int backCamera_Enable;
        /// <summary>
        /// Camera使能
        /// </summary>
        public int BackCamera_Enable
        {
            get { return backCamera_Enable; }
            set
            {
                backCamera_Enable = value;
                this.DoNotify();
            }
        }

        private String backComponentId;
        /// <summary>
        /// 部件总编号
        /// </summary>
        public String BackComponentId
        {
            get { return backComponentId; }
            set
            {
                backComponentId = value;
                this.DoNotify();
            }
        }

        private String backComponentType;
        /// <summary>
        /// 相机类别
        /// </summary>
        public String BackComponentType
        {
            get { return backComponentType; }
            set
            {
                backComponentType = value;
                this.DoNotify();
            }
        }
    }

    public class DataMzCameraLineModelEx : DataMzCameraLineModel
    {
        public bool Enable { get; set; } = true;
        public int sorIndex { get; set; } = 0;
        public int AxisID { get; set; }
        public int Axis_Distance { get; set; }
    }

    public class Axis
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Distance { get; set; }
    }
}

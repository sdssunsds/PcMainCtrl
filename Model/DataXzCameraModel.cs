using PcMainCtrl.Common;

namespace PcMainCtrl.Model
{
    public class DataXzCameraModel : NotifyBase
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
        /// 车厢编号
        /// </summary>
        private int carriageId;
        public int CarriageId
        {
            get { return carriageId; }
            set
            {
                carriageId = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 车厢类别
        /// </summary>
        private int carriageType;
        public int CarriageType
        {
            get { return carriageType; }
            set
            {
                carriageType = value;
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
        /// Rgv实时距离
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

        //--------------------------------------------------------------------------------------------
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

    public class DataXzCameraLineModel : NotifyBase
    {
        private int dataLine_Index;
        /// <summary>
        /// 数据行描述
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

        //-------------------------------------------------------------------------------------------------------------------
        private int carriageId;
        /// <summary>
        /// 车厢编号
        /// </summary>
        public int CarriageId
        {
            get { return carriageId; }
            set
            {
                carriageId = value;
                this.DoNotify();
            }
        }

        private int carriageType;
        /// <summary>
        /// 车厢类别
        /// </summary>
        public int CarriageType
        {
            get { return carriageType; }
            set
            {
                carriageType = value;
                this.DoNotify();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------
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

        private int rgvCheckMinDistacnce;
        /// <summary>
        /// Rgv运行Min距离
        /// </summary>
        public int RgvCheckMinDistacnce
        {
            get { return rgvCheckMinDistacnce; }
            set
            {
                rgvCheckMinDistacnce = value;
                this.DoNotify();
            }
        }

        private int rgvCheckMaxDistacnce;
        /// <summary>
        /// Rgv运行Max距离
        /// </summary>
        public int RgvCheckMaxDistacnce
        {
            get { return rgvCheckMaxDistacnce; }
            set
            {
                rgvCheckMaxDistacnce = value;
                this.DoNotify();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------
        private int currentDetectMinHeight;
        /// <summary>
        /// 当前检测车底Min高度
        /// </summary>
        public int CurrentDetectMinHeight
        {
            get { return currentDetectMinHeight; }
            set
            {
                currentDetectMinHeight = value;
                this.DoNotify();
            }
        }

        private int currentDetectMaxHeight;
        /// <summary>
        /// 当前检测车底Min高度
        /// </summary>
        public int CurrentDetectMaxHeight
        {
            get { return currentDetectMaxHeight; }
            set
            {
                currentDetectMaxHeight = value;
                this.DoNotify();
            }
        }

        private int infraRed1_Stat;
        /// <summary>
        /// 红外第1路信号状态
        /// </summary>
        public int InfraRed1_Stat
        {
            get { return infraRed1_Stat; }
            set
            {
                infraRed1_Stat = value;
                this.DoNotify();
            }
        }

        private int infraRed2_Stat;
        /// <summary>
        /// 红外第2路信号状态
        /// </summary>
        public int InfraRed2_Stat
        {
            get { return infraRed2_Stat; }
            set
            {
                infraRed2_Stat = value;
                this.DoNotify();
            }
        }

        private int infraRed3_Stat;
        /// <summary>
        /// 红外第3路信号状态
        /// </summary>
        public int InfraRed3_Stat
        {
            get { return infraRed3_Stat; }
            set
            {
                infraRed3_Stat = value;
                this.DoNotify();
            }
        }

        private int infraRed4_Stat;
        /// <summary>
        /// 红外第4路信号状态
        /// </summary>
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

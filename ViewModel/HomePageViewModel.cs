using PcMainCtrl.Common;
using PcMainCtrl.DataAccess.DataEntity;
using PcMainCtrl.HardWare;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HttpServer;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.ViewModel
{
    public class MainTaskScheduleHandleData
    {
        //距离是否Check
        public bool rgvDistanceData_IsEnable = true;

        //高度是否Check
        public bool stm32HighData_IsEnable = false;

        //IO是否Check
        public bool stm32IOData_IsEnable = false;
    }

    public class MainTaskScheduleHandle
    {
        public int rgvTaskStat = 1;        //rgv执行过程
        public int stm32TaskStat = 0;     //stm32执行过程
        public int frontRobotTaskStat = 1; //frontRobot执行过程
        public int frontCameraTaskStat = 1;
        public int backRobotTaskStat = 1;  //backRobot执行过程
        public int backCameraTaskStat = 1;
        public int xzCameraTaskStat = 1;   //xzCamera执行过程
        
        public MainTaskScheduleHandleData handleData = new MainTaskScheduleHandleData();

        //面阵---------------------------------------------------------------------
        public DataMzCameraLineModel mz_dataline = new DataMzCameraLineModel();

        //线阵-----------------------------------------------------------------------
        public int xzCameraPicDataListCount = 0;
        public int xzLeftPicDataListCount = 0;
        public int xzRightPicDataListCount = 0;
        public bool xzCameraPicIndexIsValid = false;
        public DataXzCameraLineModel xz_dataline = new DataXzCameraLineModel();
    }

    public partial class HomePageViewModel : NotifyBase
    {
        private const int RgvTrackLength = 250000;

        private bool isWait = false;
        private bool RunStat = false;
        private bool RunMz = false;
        private bool saveSpeed = true;
        private bool TaskMzCameraIsRunning = false;
        private int logCount = 0;
        private int logMaxLine = 1000;
        /// <summary>
        /// 车头位置
        /// </summary>
        private int TrainCurrentHeadDistance = 0;
        private object logLock = new object();
        private object taskLock = new object();

        private string job = "上位机初始化中";
        private string rgvState = "待机中";
        private string rgvSpeed = "0 mm/s";
        private string rgvDistacnce = "5000 mm";
        private string rgvElectricity = "100%";
        private string trainHeadDistance = "0 m";
        private string ipAddr = Properties.Settings.Default.FtpIP;
        private string port = Properties.Settings.Default.FtpPort;
        private string userName = Properties.Settings.Default.FtpUser;
        private string password = Properties.Settings.Default.FtpPwd;

        private AppRemoteCtrl_TrainPara train_para;
        private HomePageModel taskMainCameraModel = new HomePageModel();
        /// <summary>
        /// 任务控制数据
        /// </summary>
        private MainTaskScheduleHandle taskScheduleHandleInfo = new MainTaskScheduleHandle();
        private LightManager light = null;

        private List<int> rgvSpeedList = new List<int>();
        /// <summary>
        /// 面阵表数据
        /// </summary>
        private List<DataMzCameraLineModelEx> mzCameraDataList = new List<DataMzCameraLineModelEx>();
        /// <summary>
        /// 线阵表数据
        /// </summary>
        private List<DataXzCameraLineModel> xzCameraDataList = new List<DataXzCameraLineModel>();

        #region 界面绑定
        private bool isStartEnable = false;
        public bool IsStartEnable
        {
            get { return isStartEnable; }
            set
            {
                isStartEnable = value;
                DoNotify();
            }
        }

        public bool IsEndEnable
        {
            get { return !isStartEnable; }
            set
            {
                isStartEnable = !value;
                DoNotify();
            }
        }

        private bool isRgvMoved = false;
        public bool IsRgvMoved
        {
            get { return isRgvMoved; }
            set
            {
                isRgvMoved = value;
                DoNotify();
            }
        }

        private bool isAlarm = false;
        public bool IsAlarm
        {
            get { return isAlarm; }
            set
            {
                isAlarm = value;
                DoNotify();
            }
        }

        private bool isRelinkShot_Back = false;
        public bool IsBackRelinkShot
        {
            get { return isRelinkShot_Back; }
            set
            {
                isRelinkShot_Back = value;
                DoNotify();
            }
        }

        private bool isRelinkShot_Front = false;
        public bool IsFrontRelinkShot
        {
            get { return isRelinkShot_Front; }
            set
            {
                isRelinkShot_Front = value;
                DoNotify();
            }
        }

        private bool isPLCAlarm_Front = false;
        public bool IsFrontPLCAlarm
        {
            get { return isPLCAlarm_Front; }
            set
            {
                isPLCAlarm_Front = value;
                DoNotify();
            }
        }

        private bool isPLCAlarm_Back = false;
        public bool IsBackPLCAlarm
        {
            get { return isPLCAlarm_Back; }
            set
            {
                isPLCAlarm_Back = value;
                DoNotify();
            }
        }

        public string Job
        {
            get { return job; }
            set
            {
                job = value;
                DoNotify();
            }
        }

        public string RgvState
        {
            get { return rgvState; }
            set
            {
                rgvState = value;
                DoNotify();
            }
        }

        public string RgvSpeed
        {
            get { return rgvSpeed; }
            set
            {
                rgvSpeed = value;
                DoNotify();
            }
        }

        public string RgvDistacnce
        {
            get { return rgvDistacnce; }
            set
            {
                rgvDistacnce = value;
                DoNotify();
            }
        }

        public string RgvElectricity
        {
            get { return rgvElectricity; }
            set
            {
                rgvElectricity = value;
                DoNotify();
            }
        }

        public string TrainHeadDistance
        {
            get { return trainHeadDistance; }
            set
            {
                trainHeadDistance = value;
                DoNotify();
            }
        }

        private string grabImg;
        public string GrabImg
        {
            get { return grabImg; }
            set
            {
                grabImg = value;
                DoNotify();
            }
        }

        private string log;
        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                DoNotify();
            }
        }

        private HardWare.Robot.RobotModCtrlProtocol.RobotDataPack front = new HardWare.Robot.RobotModCtrlProtocol.RobotDataPack();
        public HardWare.Robot.RobotModCtrlProtocol.RobotDataPack Front
        {
            get { return front; }
            set
            {
                front = value;
                DoNotify();
            }
        }

        private HardWare.Robot.RobotModCtrlProtocol.RobotDataPack back = new HardWare.Robot.RobotModCtrlProtocol.RobotDataPack();
        public HardWare.Robot.RobotModCtrlProtocol.RobotDataPack Back
        {
            get { return back; }
            set
            {
                back = value;
                DoNotify();
            }
        }
        #endregion

        public CommandBase OneKeyStartCmd { get; set; }

        public CommandBase OneKeyStopCmd { get; set; }

        public CommandBase RgvForwardMotorCmd { get; set; }

        public CommandBase RgvBackMotorCmd { get; set; }

        public CommandBase RgvNormalStopCmd { get; set; }

        public CommandBase RgvIntelligentChargeCmd { get; set; }

        public CommandBase RgvClearAlarmCmd { get; set; }

        public CommandBase FrontRelinkShotCmd { get; set; }

        public CommandBase BackRelinkShotCmd { get; set; }

        public CommandBase FrontClearPLCAlarmCmd { get; set; }

        public CommandBase BackClearPLCAlarmCmd { get; set; }

        public HomePageViewModel()
        {
            StartApplication();

            this.OneKeyStartCmd = new CommandBase();
            this.OneKeyStartCmd.DoExecute = new Action<object>(DoOneKeyStartCmdHandle);
            this.OneKeyStartCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.OneKeyStopCmd = new CommandBase();
            this.OneKeyStopCmd.DoExecute = new Action<object>(DoOneKeyStopCmdHandle);
            this.OneKeyStopCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvForwardMotorCmd = new CommandBase();
            this.RgvForwardMotorCmd.DoExecute = new Action<object>(DoRgvForwardMotorCmdHandle);
            this.RgvForwardMotorCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvBackMotorCmd = new CommandBase();
            this.RgvBackMotorCmd.DoExecute = new Action<object>(DoRgvBackMotorCmdHandle);
            this.RgvBackMotorCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvNormalStopCmd = new CommandBase();
            this.RgvNormalStopCmd.DoExecute = new Action<object>(DoRgvNormalStopCmdHandle);
            this.RgvNormalStopCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvClearAlarmCmd = new CommandBase();
            this.RgvClearAlarmCmd.DoExecute = new Action<object>(DoRgvClearAlarmCmdHandle);
            this.RgvClearAlarmCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.FrontRelinkShotCmd = new CommandBase();
            this.FrontRelinkShotCmd.DoExecute = new Action<object>((object o) =>
            {
                DoRelinkShot(RobotName.Front);
                IsFrontRelinkShot = false;
            });
            this.FrontRelinkShotCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.BackRelinkShotCmd = new CommandBase();
            this.BackRelinkShotCmd.DoExecute = new Action<object>((object o) =>
            {
                DoRelinkShot(RobotName.Back);
                IsBackRelinkShot = false;
            });
            this.BackRelinkShotCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.FrontClearPLCAlarmCmd = new CommandBase();
            this.FrontClearPLCAlarmCmd.DoExecute = new Action<object>(ClearFrontAlarm);
            this.FrontClearPLCAlarmCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.BackClearPLCAlarmCmd = new CommandBase();
            this.BackClearPLCAlarmCmd.DoExecute = new Action<object>(ClearBackAlarm);
            this.BackClearPLCAlarmCmd.DoCanExecute = new Func<object, bool>((o) => true);

            InitApplication();

            TaskRun(() =>
            {
                while (true)
                {
                    IsStartEnable = !RunStat;
                    IsRgvMoved = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE;
                    ThreadSleep(1000);
                }
            });
        }

        private string GetTimeStamp()
        {
            return "_" + DateTime.Now.ToString("yyMMddHHmmss");
        }
    }
}
//#define test  // 本地测试模式
//#define saveRobotData  // 存储机械臂点位，采点用
#define saveLog  // 保存日志
#define sumJoin  // 按照图片数量拼图，同时用于拆分每节车
#define newBasler  // 新Basler底层驱动
#define plcModbus  // 使用PLC的Modbus协议连接部分硬件设备
#define lidarLocation  // 激光雷达定位
#define lidarCsharp  // 使用C#代码进行雷达定位
//#define lidarOneLoc  // 只进行一次雷达重定位
#define controlServer  // 远程控制服务，PC服务
#define controlSocket  // 远程控制服务，socket模式(192.168.0.101:10010)或(192.168.0.102:10010)
#define socketExe  // 独立进程进行Socket连接
#define ping  // 使用ping检测设备连接
//#define bakImage  // 备份图片
#define shotImageProcess  // 拍照图片流程
#define init3dCamera  // 初始化3D扫描仪
#define initRGV  // 初始化RGV
#define initRobot  // 初始化机械臂
#define initPointCamera  // 初始化点云相机
#define controlLightPower  // 用通断电源的方式控制光源

#if newBasler
using Basler;
#else
using Basler.Pylon;
#endif
using GW.Function.FileFunction;
using GW.Function.Util;
using Newtonsoft.Json;
#if lidarCsharp
using LidarLib;
#endif
using PcMainCtrl.Common;
using PcMainCtrl.DataAccess;
using PcMainCtrl.DataAccess.DataEntity;
using PcMainCtrl.HardWare;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.HardWare.NetworkRelay;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HardWare.Robot;
using PcMainCtrl.HardWare.Stm32;
using PcMainCtrl.HttpServer;
using PcMainCtrl.Model;
using PcMainCtrl.ServiceReference1;
#if saveRobotData
using PcMainCtrl.Form;
using PcMainCtrl.Model;
#endif
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
#if controlSocket
using PcMainCtrl.Network;
#if !socketExe
using static PcMainCtrl.Network.Network; 
#endif
#endif
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.BaslerCamera.CameraDataHelper;
// error i = 8
namespace PcMainCtrl.ViewModel
{
    public partial class HomePageViewModel
    {
        #region 全局变量
        /// <summary>
        /// 车头位置，手动定位车头使用
        /// </summary>
        public static int trainHeadLocation = 0;  //在这修改当前车头距离
        public static bool socketRunStart = false;
        private const int plcSleep = 100;
        private const int lightSleep = 150;
        private const string uploadServerKey = "传图服务器";
        private readonly string iniPath = Application.StartupPath + "/system.ini";
        private bool _3dInitComplete = false;
        private bool axisOutTimeAlarm = false;
        private bool cameraOpened = false;
        private bool isBackProcess = false;
        private bool isContinuousShot = false;
        private bool isInitApplication = false;
        private bool robotAxisError = false;
        private bool showGread = false;
        private bool showRed = false;
        private int axisBackID = 0;
        private int axisFrontID = 0;
        private int back3dID = -1;
        private int backMzIndex = 0;
        private int copyIndex = 0;
        private int front3dID = -1;
        private int frontMzIndex = 0;
        private int mzDistance = 0;
        private int mzIndex = -1;
        private int rgvSpeedStartIndex = 0;
        private int rgvSpeedEndIndex = 0;
        private int rgvUpSpeed = 0;
        private int shotFrontOutTime = 0;
        private int shotBackOutTime = 0;
        private int shotOutTimeMax = 300;
        private int slidingOutTimeMax = 200;
        private int uploadSize = 1024000;
        private int xzOutTimeCount = 0;
        private int xzIndex = -1;
        private int xzLeftOutTimeCount = 0;
        private int xzRightOutTimeCount = 0;
        private int xzStart = -1;
        private double height = 0d;
        private string back_parts_id = "";
        private string front_parts_id = "";
        private string shotBackID = "";
        private string shotBackType = "";
        private string shotFrontID = "";
        private string shotFrontType = "";
        private string uploadID = "";
        private object _3dScanLock = new object();
        private object lockObj = new object();
        private IService picService = new ServiceClient();
        private CognexLib.Manager cognexManager = new CognexLib.Manager();
        private SerialPortManager serialPortManager = null;
        private List<int> rgvMathSpeedList = new List<int>();
        private List<string> copyImages_xz = new List<string>();
        private List<Axis> AxisList = null;
        private List<MzDataExtent> frontData = new List<MzDataExtent>();
        private List<MzDataExtent> backData = new List<MzDataExtent>();
        private Dictionary<int, int> axisDict = new Dictionary<int, int>();

        public const bool JoinPicMode =
#if sumJoin
            true;
#else
            false;
#endif
#if plcModbus
        private bool canMovePlan = true;
        private PLC3DCamera pLC3DCamera = null;
#endif
#if newBasler
        private int xzID, xzLeftID, xzRightID, frontID, backID;
        private CameraManager cameraManager = null;
#else
        private bool isContinuousShot = true;
        private List<string> simulationProcessImages_xz = new List<string>();
#endif
#if lidarLocation
        private const string lidarExeName = "lplocation";
        private const string resultPath = "lidar.txt";
#endif
#if lidarCsharp
        private double upCheckHead = 0;
        private Lidar lidar = null; 
#endif
#if lidarOneLoc
        private bool isFirst = true;
        private int lidarLocation = 0;
        private int lidarMovePoint = 0;
#endif
#if controlServer
#if controlSocket && !socketExe
        private TcpClientSocket client = null; 
#endif
        private List<string> logs = new List<string>();
#endif
#if socketExe
        private bool socketConnect = false;
#endif
#if ping
        private List<int> xzNotLoad = new List<int>();
        private List<string> xzLNotLoad = new List<string>();
        private List<string> xzRNotLoad = new List<string>();
        private List<string> mzNotLoad = new List<string>();
        private Dictionary<string, string> back3dNotLoad = new Dictionary<string, string>();
        private Dictionary<string, string> front3dNotLoad = new Dictionary<string, string>();
        private Dictionary<string, bool> pingResult = new Dictionary<string, bool>();
#endif
        public Action<Image> XzLeftShot { get; set; }
        public Action<Image> XzRightShot { get; set; }
        #endregion

        #region 主业务
        private void StartApplication()
        {
            RunStat = true;
            int logI = 0;
            AddLogEvent += new AddLogs((string log, int type) =>
            {
                if (type >= 0 && type != 4 && type != 5 && type != 7)
                {
                    #region 语音
#if false
                    if (type == 1)
                    {
                        GW.Function.SpeechRecognition.SpeechRecognition.Speak(log, 0);
                    } 
#endif 
                    #endregion
                    lock (logLock)
                    {
                        if (logCount > logMaxLine)
                        {
                            Log = log + "\r\n";
                            logCount = 1;
                        }
                        else
                        {
                            Log = log + "\r\n" + Log;
                            logCount++;
                        }
                    }
                }
#if controlServer
                logs.Add(log);
#endif
#if ping
                if (pingResult.ContainsKey(uploadServerKey) && pingResult[uploadServerKey])
                {
                    ThreadStart(() =>
                    {
                        try
                        {
                            picService.AddLog(log, type);
                        }
                        catch (Exception e)
                        {
                            pingResult[uploadServerKey] = false;
                            AddLog(e.Message, -1);
                        }
                    });
                }
#endif
#if saveLog
                string logPath = Application.StartupPath + @"\log";
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                try
                {
                    string time = DateTime.Now.ToString("MM-dd HH:mm:ss");
                    if (type != 4 && type != 5)
                    {
                        StreamWriter writer = new StreamWriter(logPath + @"\log.txt", true);
                        writer.WriteLine(time + "\t" + (type < 0 ? "ERROR【" + logI + "】" : "") + log);
                        writer.Close(); 
                    }
                    if (type < 0)
                    {
                        StreamWriter sw = new StreamWriter(logPath + @"\error.txt", true);
                        sw.WriteLine("ERROR【" + logI + "】 " + time + "\t" + log);
                        sw.Close();
                        logI++;
                    }
                    else if (type == 1)
                    {
                        StreamWriter sw = new StreamWriter(logPath + @"\frontLog.txt", true);
                        sw.WriteLine(time + "\t" + log);
                        sw.Close();
                    }
                    else if (type == 2)
                    {
                        StreamWriter sw = new StreamWriter(logPath + @"\backLog.txt", true);
                        sw.WriteLine(time + "\t" + log);
                        sw.Close();
                    }
                    else if (type == 3)
                    {
                        StreamWriter sw = new StreamWriter(logPath + @"\lidarLog.txt", true);
                        sw.WriteLine(time + "\t" + log);
                        sw.Close();
                    }
                    else if (type == 4)
                    {
                        StreamWriter sw = new StreamWriter(logPath + @"\rgvLog.txt", true);
                        sw.WriteLine(time + "\t" + log);
                        sw.Close();
                    }
                    else if (type == 5)
                    {
                        StreamWriter sw = new StreamWriter(logPath + @"\depthLog.txt", true);
                        sw.WriteLine(time + "\t" + log);
                        sw.Close();
                    }
                    else if (type == 7)
                    {
                        StreamWriter sw = new StreamWriter(logPath + @"\lightLog.txt", true);
                        sw.WriteLine(time + "\t" + log);
                        sw.Close();
                    }
                }
                catch (Exception) { }
#endif
            });

            serialPortManager = new SerialPortManager(Properties.Settings.Default.LaserPositioning)
            {
                ReadValue = (string val) =>
                {
                    string[] vs = val.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (vs.Length > 0)
                    {
                        if (!double.TryParse(vs[0], out height))
                        {
                            if (vs.Length > 1)
                            {
                                double.TryParse(vs[1], out height);
                            }
                        }
                    }
                }
            };
#if controlSocket
#if socketExe
            string socketPath = Application.StartupPath + "\\socket";
            if (!Directory.Exists(socketPath))
            {
                Directory.CreateDirectory(socketPath);
            }
            MyRgvModInfoEvent(RgvModCtrlHelper.GetInstance().myRgvGlobalInfo);
            Process[] processes = Process.GetProcessesByName("SocketControl");
            if (processes.Length > 0)
            {
                foreach (Process item in processes)
                {
                    item.Kill();
                }
            }
            Process.Start(Application.StartupPath + "\\SocketControl.exe");
            TaskRun(() =>
            {
                do
                {
                    string[] files = Directory.GetFiles(socketPath, "*.c");
                    if (files != null)
                    {
                        foreach (string file in files)
                        {
                            string cmd = file.Substring(file.LastIndexOf("\\") + 1).Replace(".c", "");
                            string par = "";
                        read:
                            try
                            {
                                using (StreamReader sr = new StreamReader(file))
                                {
                                    par = sr.ReadToEnd();
                                }
                            }
                            catch (Exception)
                            {
                                ThreadSleep(10);
                                goto read;
                            }
                        delete:
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception)
                            {
                                ThreadSleep(10);
                                goto delete;
                            }
                            try
                            {
                                if (cmd == "link")
                                {
                                    socketConnect = bool.Parse(par);
                                }
                                if (cmd != "robot_online" && cmd != "robot_state")
                                {
                                    SocketCmd socketCmd = (SocketCmd)Enum.Parse(typeof(SocketCmd), cmd);
                                    try
                                    {
                                        bool exp = SocketCmdFunc(socketCmd, "", "", par);
                                        SokcetExeWrite(socketCmd, exp.ToString());
                                    }
                                    catch (Exception e)
                                    {
                                        SokcetExeWrite(socketCmd, "error:" + e.Message);
                                    }
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    ThreadSleep(1000);
                } while (true);
            });
#else
            string[] cmdArray = Enum.GetNames(typeof(SocketCmd));
            client = new TcpClientSocket(Properties.Settings.Default.ServerIP, Properties.Settings.Default.ServerPort);
            client.recvMessageEvent = (string msg) =>
            {
                foreach (string cmd in cmdArray)
                {
                    try
                    {
                        if (msg.Contains("/" + cmd + "/"))
                        {
                            string[] vs = msg.Split('/');
                            SocketCmd socketCmd = (SocketCmd)Enum.Parse(typeof(SocketCmd), cmd);
                            SocketCmdFunc(socketCmd, vs);
                            client.SendAsync("START/" + cmd + "/true/END");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        client.SendAsync("START/" + cmd + "/error:" + e.Message + "/END");
                    }
                }
            };
            TaskRun(() =>
            {
                try
                {
                    do
                    {
                        if (client.connectSocket == null || !client.connectSocket.Connected)
                        {
                            while (!client.Start(0))
                            {
                                ThreadSleep(3000);
                            }
                            client.SendAsync("START/robot_online/" + Properties.Settings.Default.RobotID + "/END"); 
                        }
                        ThreadSleep(Properties.Settings.Default.ServerHardInternal);
                        if (client.connectSocket != null && client.connectSocket.Connected)
                        {
                            string[] vs = logs.ToArray();
                            logs.Clear();
                            string log = "";
                            if (vs != null)
                            {
                                foreach (string item in vs)
                                {
                                    log += item + "\r\n";
                                }
                            }
                            SocketRgvInfo rgv = new SocketRgvInfo(RgvModCtrlHelper.GetInstance().myRgvGlobalInfo,
                                xzIndex > -1 ? xzCameraDataList[xzIndex] : new DataXzCameraLineModel() { DataLine_Index = -1 },
                                mzIndex > -1 ? mzCameraDataList[mzIndex] : new DataMzCameraLineModelEx() { Rgv_Distance = 0 });
#if controlServer
                            rgv.TrainMode = Properties.Settings.Default.TrainMode;
                            rgv.TrainSn = Properties.Settings.Default.TrainSn;
                            rgv.TrainCurrentHeadDistance = TrainCurrentHeadDistance;
                            rgv.Log = log;
                            rgv.Job = Job; 
#endif
                            client.SendAsync("START/robot_state/" + JsonConvert.SerializeObject(rgv) + "/END"); 
                        }
                    } while (true);
                }
                catch (Exception ex)
                {
                    AddLog(ex.Message);
                }
            });
#endif
#endif
            TaskRun(() =>
            {
                testForm = new Form.TestForm()
                {
                    InitAct = () =>
                    {
                        light = null;
                        pLC3DCamera = null;
                        RgvModCtrlHelper.GetInstance().RgvModInfoEvent -= MyRgvModInfoEvent;
                        Stm32ModCtrlHelper.GetInstance().Stm32ModInfoEvent -= MyStm32ModInfoEvent;
                        RobotModCtrlHelper.GetInstance().RobotModInfoEvent -= MyRobotModInfoEvent;
#if !plcModbus
                        RobotModCtrlHelper.GetInstance().RobotModInfoEvent -= MyRobotModInfoEvent;
                        NetworkRelayCtrlHelper.GetInstance().NetworkRelayModInfoEvent -= MyNetworkRelayModInfoEvent;
#endif
                        Init();
                    },
                    SpeedCorrect = RgvSpeedCorrect,
                    Run = DoOneKeyStartCmdHandle,
                    Stop = DoOneKeyStopCmdHandle,
                    Inspect = this.Inspect,
                    GetSpeed = () => { return rgvUpSpeed; },
                    GetState = (int i) =>
                    {
                        switch (i)
                        {
                            case 0:
                                return RgvModCtrlHelper.GetInstance().IsLink;
                            case 1:
                                return Stm32ModCtrlHelper.GetInstance().IsLink;
                            case 2:
                                return ModbusTCP.IsLink;
                            case 3:
                                return RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotConnStat;
                            case 4:
                                return RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotConnStat;
                            case 5:
#if controlSocket
                                return socketConnect; 
#else
                                return true;
#endif
                            case 6:
#if ping
                                return pingResult.ContainsKey(uploadServerKey) ? pingResult[uploadServerKey] : true;
#else
                                return true;
#endif
                            case 7:
#if ping
                                return pingResult.ContainsKey(Properties.Settings.Default.UploadDataServer) ? pingResult[Properties.Settings.Default.UploadDataServer] : true;
#else
                                return true;
#endif
                            case 8:
                                return xzID >= 0 && cameraManager != null && cameraManager.Cameras != null && cameraManager.Cameras.Count > xzID && cameraManager.Cameras[xzID].Opened;
                            case 9:
                                return frontID >= 0 && cameraManager != null && cameraManager.Cameras != null && cameraManager.Cameras.Count > frontID && cameraManager.Cameras[frontID].Opened;
                            case 10:
                                return backID >= 0 && cameraManager != null && cameraManager.Cameras != null && cameraManager.Cameras.Count > backID && cameraManager.Cameras[backID].Opened;
                            case 11:
                                return _3dInitComplete;
                            case 12:
#if lidarCsharp
                                if (lidar == null)
                                {
                                    LidarType lidarType = LidarType.A2;
                                    try
                                    {
                                        lidarType = (LidarType)Enum.Parse(typeof(LidarType), Properties.Settings.Default.LidarType);
                                        lidar = Lidar.Instance(lidarType, logInit: AddLog);
                                    }
                                    catch (Exception e)
                                    {
                                        AddLog("雷达类型异常：" + e.Message, -1);
                                    }
                                }
                                return lidar?.IsLink ?? false;
#else
                                return false;
#endif
                            case 13:
                                return xzLeftID >= 0 && cameraManager != null && cameraManager.Cameras != null && cameraManager.Cameras.Count > xzLeftID && cameraManager.Cameras[xzLeftID].Opened;
                            case 14:
                                return xzRightID >= 0 && cameraManager != null && cameraManager.Cameras != null && cameraManager.Cameras.Count > xzRightID && cameraManager.Cameras[xzRightID].Opened;
                            default:
                                return false;
                        }
                    }
                };
                testForm.Shown += (object sender, EventArgs e) =>
                {
                    if (trainHeadLocation > 0)
                    {
                        testForm.SetHead(trainHeadLocation);
                    }
                };
                testForm.SetConfig();
            });
            rgvUpSpeed = int.Parse(FileSystem.ReadIniFile("RGV", "rgvSpeed", "800", iniPath));
        }

        private void InitApplication()
        {
            var mzList = LocalDataBase.GetInstance().MzCameraDataListQurey();
            AddLog("缓存面阵数据");
            mzCameraDataList = mzList;
            AddLog(string.Format("面阵数据数量: {0}", mzCameraDataList.Count.ToString()));

            var xzList = LocalDataBase.GetInstance().XzCameraDataListQurey();
            AddLog("缓存线阵数据");
            xzCameraDataList = xzList;
            AddLog(string.Format("线阵数据数量: {0}", xzCameraDataList.Count.ToString()));

#if test && false
            TaskRun(() =>
            {
                #region 初始化3D扫描仪
#if init3dCamera
                AddLog("初始化3D扫描仪...");
                try
                {
                    cognexManager.Create(Properties.Settings.Default.VppPath);
                    cognexManager.GetResultEvent += (string[] value, CognexLib.JobName jobName) =>
                    {
                        int logType = 0;
                        if (jobName == CognexLib.JobName.CogJob1)
                        {
                            logType = 1;
                            AddLog(JsonConvert.SerializeObject(value), logType);
                            AddLog($"前3D算法编号: {front3dID} 部件编号: {shotFrontID}", logType);
                            if (front3dID > -1)
                            {
                                AddLog($"前3D数据上传参数：{value[front3dID]}, {shotFrontID}, 0", logType);
                            }
#if shotImageProcess
                            shotFrontID = "";
                            shotFrontOutTime = -10000;
#endif
                        }
                        else
                        {
                            logType = 2;
                            AddLog(JsonConvert.SerializeObject(value), logType);
                            AddLog($"后3D算法编号: {back3dID} 部件编号: {shotBackID}", logType);
                            if (back3dID > -1)
                            {
                                AddLog($"后3D数据上传参数：{value[back3dID]}, {shotBackID}, 1", logType);
                            }
#if shotImageProcess
                            shotBackID = "";
                            shotBackOutTime = -10000;
#endif
                        }
                    };
                    _3dInitComplete = true;
                    AddLog("3D扫描仪初始化完成");
                }
                catch (Exception e)
                {
                    AddLog("3D扫描仪初始化失败：" + e.Message);
                }
#endif
                #endregion
                RgvState = "待机中";
                RgvSpeed = "0 mm/s";
                RgvDistacnce = "5000 mm";
                RgvElectricity = "99%";
                TrainHeadDistance = "0 mm";

                UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;
                testForm?.SetConfig();
                RunStat = false;
                AddLog("设备状态：" + UserInfo.myDeviceStat);
                Schedule.ScheduleManager.RemoveEvent();
                Job = "上位机初始化完成，等待任务中";
                testForm?.SetEnable(isInitApplication = true);
                ModbusTCP.SetAddress13(Address13Type.Green, 1);
            });
            return;
#endif
#if !controlServer
            AddLog("初始化HttpServer...");
            HttpServerHelper.GetInstance().CreatMyHttpserver();
            HttpServerHelper.GetInstance().httpServer.HttpServerModInfoEvent += MyHttpServerModInfoEvent;
            AddLog("HttpServer初始化完成");
#endif
            TaskRun(() =>
            {
#if ping
                Ping1();
                Ping2();
#endif
                Init();

                //设备状态
                UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;
                RunStat = false;
                AddLog("设备状态：" + UserInfo.myDeviceStat);
                Schedule.ScheduleManager.RemoveEvent();
                Job = "上位机初始化完成，等待任务中";
                testForm?.SetEnable(isInitApplication = true);
            });
#if plcModbus
            TaskRun(() =>
            {
                while (RunStat)
                {
                    ThreadSleep(1000);
                }
                do
                {
                    try
                    {
                        if (testForm.GetEnable(Form.EnableEnum.PLC))
                        {
                            if (isWait || IsBackRelinkShot || IsFrontRelinkShot || IsFrontPLCAlarm || IsBackPLCAlarm)
                            {
                                ThreadSleep(1000);
                                if (showGread)
                                {
                                    AddLog("报警灯：关闭绿灯", 7);
                                    ModbusTCP.SetAddress13(Address13Type.Green, 0);
                                    ThreadSleep(500);
                                    showGread = false;
                                }
                                if (!showRed)
                                {
                                    AddLog("报警灯：打开红灯", 7);
                                    ModbusTCP.SetAddress13(Address13Type.Red, 1);
                                    showRed = true; 
                                }
                            }
                            else if (RunStat || RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce > 5010)
                            {
                                if (showGread)
                                {
                                    AddLog("任务灯：关闭绿灯", 7);
                                    ModbusTCP.SetAddress13(Address13Type.Green, 0);
                                    ThreadSleep(500);
                                    showGread = false;
                                }
                                if (showRed)
                                {
                                    AddLog("任务灯：关闭红灯", 7);
                                    ModbusTCP.SetAddress13(Address13Type.Red, 0);
                                    ThreadSleep(500);
                                    showRed = false;
                                }
                                ThreadSleep(500);
                                AddLog("任务灯：打开黄灯", 7);
                                ModbusTCP.SetAddress13(Address13Type.Yellow, 1);
                                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed > 0)
                                {
                                    AddLog("任务灯：打开蜂鸣", 7);
                                    ModbusTCP.SetAddress13(Address13Type.Buzzer, 1); 
                                }
                                ThreadSleep(500);
                                AddLog("任务灯：关闭黄灯", 7);
                                ModbusTCP.SetAddress13(Address13Type.Yellow, 0);
                                AddLog("任务灯：关闭蜂鸣", 7);
                                ModbusTCP.SetAddress13(Address13Type.Buzzer, 0);
                            }
                            else if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce > 4990 && RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce < 5010)
                            {
                                ThreadSleep(1000);
                                if (showRed)
                                {
                                    AddLog("等待灯：关闭红灯", 7);
                                    ModbusTCP.SetAddress13(Address13Type.Red, 0);
                                    showRed = false;
                                }
                                if (!showGread)
                                {
                                    AddLog("等待灯：打开绿灯", 7);
                                    ModbusTCP.SetAddress13(Address13Type.Green, 1);
                                    showGread = true; 
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("PLC警示灯异常：" + e.Message, -1);
                        ThreadSleep(1000);
                    }
                } while (true);
            }); 
#endif
        }

        private void Init()
        {
#if plcModbus
            if (testForm.GetEnable(Form.EnableEnum.PLC))
            {
                ModbusTCP.SetAddress13(Address13Type.Green, 0);
                PowerOn(Address12Type.RobotFrontMzPower,
                        Address12Type.RobotBackMzPower,
                        Address12Type.RobotMzLedPower,
                        Address12Type.RobotXZPower
                );
                ModbusTCP.SetAddress13(Address13Type._3D, 1);
                AddLog("上电等待中...");
                ThreadSleep(15000);
                AddLog("电源上电完成");
                pLC3DCamera = new PLC3DCamera();
                pLC3DCamera.SetAlarmEvent(() =>
                {
                    if (pLC3DCamera.GetDriverAlarm(RobotName.Front))
                    {
                        AddLog("前滑台报警", 1);
#if controlSocket
                        PLCAlarm(RobotName.Front, SocketCmd.robot_driver_alarm);
#endif
                    }
                    if (pLC3DCamera.GetPLCAlarm(RobotName.Front))
                    {
                        AddLog("前PLC报警", 1);
#if controlSocket
                        PLCAlarm(RobotName.Front, SocketCmd.robot_plc_alarm);
#endif
                    }
                }, () =>
                {
                    if (pLC3DCamera.GetDriverAlarm(RobotName.Back))
                    {
                        AddLog("后滑台报警", 2);
#if controlSocket
                        PLCAlarm(RobotName.Back, SocketCmd.robot_driver_alarm);
#endif
                    }
                    if (pLC3DCamera.GetPLCAlarm(RobotName.Back))
                    {
                        AddLog("后PLC报警", 2);
#if controlSocket
                        PLCAlarm(RobotName.Back, SocketCmd.robot_plc_alarm);
#endif
                    }
                });
            }
#endif
            AddLog("初始化相机...");
            do
            {
#if newBasler
                if (cameraManager != null)
                {
                    for (int i = 0; i < cameraManager.Cameras.Count; i++)
                    {
                        cameraManager.Cameras[i].Close();
                    }
                    cameraManager.Close();
                }
#endif
                InitCameraMod();
                ThreadSleep(1000);
#if newBasler
            } while (GetCameraState());
#else
                AddLog("相机数量: " + CameraCtrlHelper.GetInstance().myCameraList.Count);
            } while (CameraCtrlHelper.GetInstance().myCameraList.Count < 3);
            CameraCtrlHelper.GetInstance().CameraImageEvent += Camera_CameraImageEvent;
            CameraCtrlHelper.GetInstance().CameraErrorEvent += HomePageViewModel_CameraErrorEvent; 
            Schedule.ScheduleManager.CameraTest(taskMainCameraModel.XzCamerainfoItem,
                taskMainCameraModel.FrontCamerainfoItem, taskMainCameraModel.BackCamerainfoItem);
            ThreadSleep(2000); 
#endif
            CameraOpen();
            AddLog("相机初始化完成");

            AddLog("初始化光源...");
            light = new LightManager(Properties.Settings.Default.Light)
            {
                LinkLight = () =>
                {
                    return testForm.GetEnable(Form.EnableEnum.光源);
                }
            };
#if controlLightPower
            ThreadSleep(500);
            light.LightOn(Properties.Settings.Default.LightFrontHigh, true);
            ThreadSleep(500);
            light.LightOn(Properties.Settings.Default.LightFrontHigh, false);
#endif
#if plcModbus
            if (testForm.GetEnable(Form.EnableEnum.PLC))
            {
                light.PowerOn();
            }
#endif
            AddLog("光源初始化完成");
#if !test
#if initRGV
            if (testForm.GetEnable(Form.EnableEnum.RGV))
            {
                AddLog("初始化Rgv...");
                RgvModCtrlHelper.GetInstance().RgvModInfoEvent += MyRgvModInfoEvent;
                InitRgvMod();
                AddLog("Rgv初始化完成");
            }
#endif
            if (testForm.GetEnable(Form.EnableEnum.Stm32))
            {
                AddLog("初始化Stm32...");
                Stm32ModCtrlHelper.GetInstance().Stm32ModInfoEvent += MyStm32ModInfoEvent;
                InitStm32Mod();
                AddLog("Stm32初始化完成");
            }
#endif
            if (!RobotModCtrlHelper.isServer)
            {
                AddLog("初始化机械臂...");
#if plcModbus
                if (testForm.GetEnable(Form.EnableEnum.PLC))
                {
                    ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 0);
                    ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 0);
                    ThreadSleep(3000);
                    ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 1);
                    ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 1);
                    ThreadSleep(3000);
                    ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 0);
                    ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 0);
                }
#else
                RobotModCtrlHelper.GetInstance().RobotModInfoEvent += MyRobotModInfoEvent;
                InitRobotMod(); 
#endif
                AddLog("机械臂初始化完成");
            }
#if initRobot
            AddLog("初始化继电器...");
#if plcModbus
            if (testForm.GetEnable(Form.EnableEnum.PLC))
            {
                ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 0);
                ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 0);
                ThreadSleep(3000);
                ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 1);
                ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 1);
                ThreadSleep(3000);
                ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 0);
                ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 0);
            }
#else
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayModInfoEvent += MyNetworkRelayModInfoEvent;
            InitRelayMod();
#endif
            AddLog("继电器初始化完成");

            if (RobotModCtrlHelper.isServer)
            {
                AddLog("初始化机械臂...");
                RobotModCtrlHelper.GetInstance().RobotModInfoEvent += MyRobotModInfoEvent;
                InitRobotMod();
                AddLog("机械臂初始化完成");
            }
            while (true)
            {
                if ((testForm.GetEnable(Form.EnableEnum.前机械臂) ? RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotConnStat : true) &&
                    (testForm.GetEnable(Form.EnableEnum.后机械臂) ? RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotConnStat : true))
                {
                    AddLog("延迟等到机械臂复位动作完成，等待3秒...");
                    ThreadSleep(3000);

                    DoRobotCmdBackZeroHandle("1");
                    DoRobotCmdBackZeroHandle("2");
                    break;
                }

                AddLog("等待机械臂复位【前臂：" + RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotConnStat +
                       "】【后臂：" + RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotConnStat + "】");
                ThreadSleep(1000);
            }

            WaitRobotEnding();
#endif
#if init3dCamera
            if (testForm.GetEnable(Form.EnableEnum.三维相机))
            {
                AddLog("初始化3D扫描仪...");
                try
                {
                    cognexManager.Create(Properties.Settings.Default.VppPath);
                    cognexManager.GetResultEvent = (string[] value, CognexLib.JobName jobName) =>
                    {
                        int logType = 0;
                        if (jobName == CognexLib.JobName.CogJob1)
                        {
                            logType = 1;
                            AddLog(JsonConvert.SerializeObject(value), logType);
                            AddLog($"前3D算法编号: {front3dID} 部件编号: {shotFrontID}", logType);
                            if (front3dID > -1)
                            {
                                AddLog($"前3D数据上传参数：{value[front3dID]}, {shotFrontID}, 0", logType);
                                UploadData(value[front3dID],
                                    testForm.GetEnable(Form.EnableEnum.异步) ?
#if shotImageProcess
                                    shotFrontID
#else
                                    frontData[frontMzIndex].OnlyID
#endif
                                    : mzCameraDataList[mzIndex].FrontComponentId
                                    , 0);
                            }
#if shotImageProcess
                            shotFrontID = "";
                            shotFrontOutTime = -10000;
#endif
                        }
                        else
                        {
                            logType = 2;
                            AddLog(JsonConvert.SerializeObject(value), logType);
                            AddLog($"后3D算法编号: {back3dID} 部件编号: {shotBackID}", logType);
                            if (back3dID > -1)
                            {
                                AddLog($"后3D数据上传参数：{value[back3dID]}, {shotBackID}, 1", logType);
                                UploadData(value[back3dID],
                                    testForm.GetEnable(Form.EnableEnum.异步) ?
#if shotImageProcess
                                    shotBackID
#else
                                    backData[backMzIndex].OnlyID
#endif
                                    : mzCameraDataList[mzIndex].BackComponentId
                                    , 1);
                            }
#if shotImageProcess
                            shotBackID = "";
                            shotBackOutTime = -10000;
#endif
                        }
                    };
                    _3dInitComplete = true;
                    AddLog("3D扫描仪初始化完成");
                }
                catch (Exception e)
                {
                    AddLog("3D扫描仪初始化失败：" + e.Message, -1);
                }
            }
#endif
#if initPointCamera
            try
            {
                AddLog("初始化点云相机");
                DkamHelper.Instance.Addlog = AddLog;
                DkamHelper.Instance.ReLinkCameraFaid = DepthRelinkFaid;
            InitDeptCamera:
                DkamHelper.Instance.FindCamera();
                if (!DkamHelper.Instance.Init(Properties.Settings.Default.PointCamera, true, true))
                {
                    ThreadSleep(1000);
                    AddLog("重新初始化点云相机");
                    goto InitDeptCamera;
                }
                AddLog("初始化点云相机完成");
            }
            catch (Exception e)
            {
                AddLog("初始化点云相机失败: " + e.Message, -1);
            }
#endif
        }

        /// <summary>
        /// 任务开始
        /// </summary>
        private void DoOneKeyStartCmdHandle(object obj)
        {
            socketRunStart = false;
            testForm?.SetEnable(IsStartEnable);
            DoOneKeyStartCmdHandle(obj, null);
        }

        /// <summary>
        /// 任务开始
        /// </summary>
        private void DoOneKeyStartCmdHandle(object obj, Action endAct)
        {
#if test
            TestStart(obj);
            return;
#endif
#if saveRobotData
            SaveRobotPointForm form = new SaveRobotPointForm();
            form.PLC3DCamera = pLC3DCamera;
            form.Light = light;
            RobotModCtrlHelper.GetInstance().RobotModInfoEvent += form.RobotModInfoEvent;
            if (trainHeadLocation == 0)
            {
                TaskRun(() =>
                {
                    AddLog("开始车头检测...");
#if lidarLocation
                    TrainCurrentHeadDistance = (int)(GetLidarLoc(Properties.Settings.Default.TrainMode + "_" + Properties.Settings.Default.TrainSn, "", "") * 1000);
                    if (TrainCurrentHeadDistance < 0)
                    {
                        AddLog("获取雷达定位车头失败：" + TrainCurrentHeadDistance);
                        return;
                    }
                    AddLog("检测到车头位置：" + (TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ)); ;
#else
                    while (true)
                    {
                        if (height > 0.6 && height < 3)
                        {
                            TrainCurrentHeadDistance = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
                            AddLog("记录车头位置" + TrainCurrentHeadDistance);
                            break;
                        }
                        ThreadSleep(10);
                    }
                    AddLog("检测到车头位置：" + TrainCurrentHeadDistance);
#endif
                    form.HeadLocation = TrainCurrentHeadDistance;
                });
            }
            else
            {
                form.HeadLocation = trainHeadLocation;
            }
            form.Show();

            CameraForm.cameraManager = cameraManager;
            CameraForm frontCamera = new CameraForm() { IsFrontCamaer = true };
            CameraForm backCamera = new CameraForm() { IsFrontCamaer = false };
            frontCamera.Show();
            backCamera.Show();
            return;
#endif
            if (!isInitApplication)
            {
                return;
            }
            if (RunStat)
            {
                return;
            }
            RunStat = true;
            foreach (ICamera item in cameraManager.Cameras)
            {
                if (!item.Opened)
                {
                    item.Open();
                }
            }
            TaskRun(() =>
            {
                lock (taskLock)
                {
                    #region 任务开始
                    if (!RunStat)
                    {
                        return;
                    }
                    AddLog("<---任务开始--->");
                    Job = "任务准备中";
                    RunMz = true;
                    isWait = false;
                    GrabImg = "";
                    xzIndex = -1;
                    xzOutTimeCount = xzLeftOutTimeCount = xzRightOutTimeCount = 0;
                    if (!testForm.GetEnable(Form.EnableEnum.异步))
                    {
                        mzIndex = -1;
                    }
                    height = 0d;
                    isBackProcess = false;
#if lidarOneLoc && lidarLocation
                    isFirst = true; 
#endif
#if ping
                    pingResult[uploadServerKey] = true;
#endif
                    train_para = new AppRemoteCtrl_TrainPara();
                    if (obj != null)
                    {
                        train_para = obj as AppRemoteCtrl_TrainPara;
                        Properties.Settings.Default.TrainMode = train_para.train_mode;
                        Properties.Settings.Default.TrainSn = train_para.train_sn;
                        LocalDataBase.GetInstance().jsonPath = Application.StartupPath + "/json/" + train_para.train_mode + "_" + train_para.train_sn + "/";
                    }
                    mzCameraDataList = LocalDataBase.GetInstance().MzCameraDataListQurey();
                    xzCameraDataList = LocalDataBase.GetInstance().XzCameraDataListQurey();
                    AxisList = LocalDataBase.GetInstance().AxesDataListQurey();
                    axisDict.Clear();
                    foreach (Axis item in AxisList)
                    {
                        axisDict.Add(item.ID, 0);
                    }
                    AddLog("动车参数：" + JsonConvert.SerializeObject(train_para));
                    uploadID = Properties.Settings.Default.TrainMode + "_" + Properties.Settings.Default.TrainSn + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
                    #endregion

                    #region 清理缓存
                    if (!RunStat)
                    {
                        AddLog("任务强制中止！");
                        return;
                    }
                    AddLog("开始清理缓存图片");
                    string task = Application.StartupPath + "/task_data";
                    string test = Application.StartupPath + "/test_data";
                    string work = Application.StartupPath + "/work_data";
                    string orc = Application.StartupPath + "/orc";
                    string _3dLoc = Application.StartupPath + @"\task_data\3d.data";
                    var deleteFiles = new Action<DirectoryInfo>((DirectoryInfo dir) => { });
                    deleteFiles = new Action<DirectoryInfo>((DirectoryInfo dir) =>
                    {
                        foreach (FileInfo item in dir.GetFiles())
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                try
                                {
                                    File.Delete(item.FullName);
                                    break;
                                }
                                catch (Exception)
                                {
                                    ThreadSleep(10);
                                } 
                            }
                        }
                        foreach (DirectoryInfo item in dir.GetDirectories())
                        {
                            deleteFiles(item);
                        }
                    });
                    var deleteFile = new Action<string>((string path) =>
                    {
                        DirectoryInfo directory = new DirectoryInfo(path);
                        deleteFiles(directory);
                    });
                    if (testForm.GetEnable(Form.EnableEnum.清理日志))
                    {
                        DateTime now = DateTime.Now;
                        if (now.Hour > 5)
                        {
                            DateTime date = DateTime.Parse(FileSystem.ReadIniFile("日志", "最后一次记录日期", now.ToString("yyyy-MM-dd"), iniPath));
                            if (date.Day < now.Day || date.Month < now.Month || date.Year < now.Year)
                            {
                                string[] logs = Directory.GetFiles(Application.StartupPath + "\\log");
                                foreach (string log in logs)
                                {
                                    try
                                    {
                                        File.Delete(log);
                                    }
                                    catch (Exception) { }
                                }
                            }
                            FileSystem.WriteIniFile("日志", "最后一次记录日期", now.ToString("yyyy-MM-dd"), iniPath);
                        }
                    }
                    if (testForm.GetEnable(Form.EnableEnum.清理Task))
                    {
                        AddLog(">>清理task_data文件夹下的缓存图片");
                        deleteFile(task); 
                    }
                    if (testForm.GetEnable(Form.EnableEnum.清理Test))
                    {
                        AddLog(">>清理test_data文件夹下的缓存图片");
                        deleteFile(test);
                    }
                    if (testForm.GetEnable(Form.EnableEnum.清理Work))
                    {
                        AddLog(">>清理work_data文件夹下的缓存图片");
                        deleteFile(work);
                    }
                    if (testForm.GetEnable(Form.EnableEnum.清理Orc))
                    {
                        AddLog(">>清理orc文件夹下的缓存图片");
                        deleteFile(orc);
                    }
                    if (testForm.GetEnable(Form.EnableEnum.清理3D))
                    {
                        AddLog(">>清理往次未上传3D数据");
                        try
                        {
                            File.Delete(_3dLoc);
                        }
                        catch (Exception) { }
                    }
#if ping
                    xzNotLoad.Clear();
                    xzLNotLoad.Clear();
                    xzRNotLoad.Clear();
                    mzNotLoad.Clear();
                    back3dNotLoad.Clear();
                    front3dNotLoad.Clear();
#endif
                    #endregion

                    #region 准备阶段
                    //切换到Busy状态
                    UserInfo.myDeviceStat = UserEntity.key_DEVICE_BUSY;
                    AddLog("设备状态：" + UserInfo.myDeviceStat);
                    DoRgvStopIntelligentChargeCmdHandle();
                    AddLog("停止充电");
                    WaitCmdHandle();
                    bool mzDataComplete = false;
                    List<DataMzCameraLineModelEx> mzList = null;
                    if (testForm.GetEnable(Form.EnableEnum.异步))
                    {
                        frontMzIndex = backMzIndex = mzDistance = 0;
                        frontData.Clear();
                        backData.Clear();
                        ThreadStart(() =>
                        {
                            if (testForm.GetEnable(Form.EnableEnum.完整面阵))
                            {
                                for (int i = 0; i < mzCameraDataList.Count; i++)
                                {
                                    if (!RunStat)
                                    {
                                        return;
                                    }
                                    DataMzCameraLineModel model = mzCameraDataList[i];
                                    MzDataExtent front = new MzDataExtent(model, RobotName.Front);
                                    MzDataExtent back = new MzDataExtent(model, RobotName.Back);
                                    frontData.Add(front);
                                    backData.Add(back);
                                } 
                            }
                            else
                            {
                                mzList = testForm.GetMzData(mzCameraDataList);
                                for (int i = 0; i < mzList.Count; i++)
                                {
                                    if (!RunStat)
                                    {
                                        return;
                                    }
                                    DataMzCameraLineModel model = mzList[i];
                                    MzDataExtent front = new MzDataExtent(model, RobotName.Front);
                                    MzDataExtent back = new MzDataExtent(model, RobotName.Back);
                                    frontData.Add(front);
                                    backData.Add(back);
                                }
                            }
                            mzDataComplete = true;
                        });
                    }
                    else
                    {
                        if (testForm.GetEnable(Form.EnableEnum.部分面阵))
                        {
                            ThreadStart(() =>
                            {
                                mzList = testForm.GetMzData(mzCameraDataList);
                                mzDataComplete = true;
                            });
                        }
                        else
                        {
                            mzDataComplete = true;
                        }
                    }
#if lidarCsharp
                    upCheckHead = 0;
#endif
                    #endregion

                    #region  线阵流程
                    if (testForm.GetEnable(Form.EnableEnum.执行线阵))
                    {
                        Job = "线阵任务执行中";
                        AddLog("开始线阵流程");
                        AddLog("执行Rgv任务...");
                        RunMz = false;
                        AddLog("Rgv运动到指定位置...");
                        
                        if (trainHeadLocation > 0)
                        {
                            TrainCurrentHeadDistance = trainHeadLocation;
                            AddLog("记录车头位置：" + TrainCurrentHeadDistance);
                            RgvSetSpeed();
                            DoRgvSetTargetDistanceCmdHandle(RgvTrackLength);
                            while (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce < trainHeadLocation - 1500)
                            {
                                if (!RunStat)
                                {
                                    AddLog("任务强制中止！");
                                    return;
                                }
                                ThreadSleep(50);
                            }
                            AddLog("开始相机线扫***");
                            taskScheduleHandleInfo.xzCameraPicDataListCount = taskScheduleHandleInfo.xzLeftPicDataListCount = taskScheduleHandleInfo.xzRightPicDataListCount = 0;
#if plcModbus
                            ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 1);
#endif
                            if (!testForm.GetEnable(Form.EnableEnum.部分线阵))
                            {
#if newBasler
                                if (testForm.GetEnable(Form.EnableEnum.单线阵))
                                {
                                    DoCameraCmdHandle_ContinuousShot(xzID);
                                }
                                if (testForm.GetEnable(Form.EnableEnum.双线阵) && xzLeftID > -1 && xzRightID > -1)
                                {
                                    DoCameraCmdHandle_ContinuousShot(xzLeftID);
                                    DoCameraCmdHandle_ContinuousShot(xzRightID);
                                }
#else
                                DoCameraCmdHandle_ContinuousShot(taskMainCameraModel.XzCamerainfoItem, Application.StartupPath + @"\task_data\xz_camera\10000" + GetExtend());
#endif 
                            }
                        }
                        else
                        {
                            if (testForm.GetEnable(Form.EnableEnum.车头检测))
                            {
                                if (!CheckHeadLocation(true))
                                {
                                    return;
                                }
                            }
                        }

                        int joinIndex = 0;
                        int joinComplete = 1;
                        if (testForm.GetEnable(Form.EnableEnum.完整线阵))
                        {
                            for (int i = 0; i < xzCameraDataList.Count; i++)
                            {
                                xzIndex = i;
                                if (!RunStat)
                                {
                                    AddLog("任务强制中止！");
                                    return;
                                }

                                DataXzCameraLineModel item = xzCameraDataList[i];
#if sumJoin
                                while (taskScheduleHandleInfo.xzCameraPicDataListCount < item.RgvCheckMinDistacnce &&
                                       taskScheduleHandleInfo.xzLeftPicDataListCount < item.RgvCheckMinDistacnce &&
                                       taskScheduleHandleInfo.xzRightPicDataListCount < item.RgvCheckMinDistacnce)
#else
                                while (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce - TrainCurrentHeadDistance < item.RgvCheckMinDistacnce)
#endif
                                {
                                    if (!RunStat)
                                    {
                                        AddLog("任务强制中止！");
                                        return;
                                    }
                                    ThreadSleep(100);
                                }
                                joinIndex = JoinImg(i, joinIndex, ref joinComplete);
                                AddLog("已完成第" + (i + 1) + "节");
                            } 
                        }
                        else if (testForm.GetEnable(Form.EnableEnum.部分线阵))
                        {
                            bool isContinuousShot = false;
                            xzStart = testForm.GetXzLocationStart();
                            List<Form.TrainLocation> xzData = testForm.GetXzLocation();
                            AddLog("数据数量：" + xzData.Count);
                            for (int i = 0; i < xzData.Count; i++)
                            {
                                Form.TrainLocation item = xzData[i];
                                int start = trainHeadLocation > 0 ? trainHeadLocation + item.Start : TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ + item.Start;
                                int end = trainHeadLocation > 0 ? trainHeadLocation + item.End : TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ + item.End;
                                AddLog("起始位置：" + start + " 结束位置：" + end);
                            waitShot:
                                if (!RunStat)
                                {
                                    AddLog("任务强制中止！");
                                    return;
                                }
                                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce >= start &&
                                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce <= end)
                                {
                                    if (!isContinuousShot)
                                    {
                                        AddLog("开始采图");
#if newBasler
                                        if (testForm.GetEnable(Form.EnableEnum.单线阵))
                                        {
                                            DoCameraCmdHandle_ContinuousShot(xzID);
                                        }
                                        if (testForm.GetEnable(Form.EnableEnum.双线阵) && xzLeftID > -1 && xzRightID > -1)
                                        {
                                            DoCameraCmdHandle_ContinuousShot(xzLeftID);
                                            DoCameraCmdHandle_ContinuousShot(xzRightID);
                                        }
#else
                                        DoCameraCmdHandle_ContinuousShot(taskMainCameraModel.XzCamerainfoItem, Application.StartupPath + @"\task_data\xz_camera\10000" + GetExtend());
#endif 
                                        isContinuousShot = true;
                                    }
                                    ThreadSleep(50);
                                    goto waitShot;
                                }
                                else
                                {
                                    if (isContinuousShot)
                                    {
                                        AddLog("停止采图");
                                        DoCameraCmdHandle_Stop();
                                        isContinuousShot = false;
                                    }
                                    else
                                    {
                                        ThreadSleep(50);
                                        goto waitShot;
                                    }
                                }
                                joinIndex = JoinImg(i, joinIndex, ref joinComplete);
                            }
                        }
                        AddLog("停止采图");
                        DoCameraCmdHandle_Stop();
                        Parameter parameter = new Parameter();
                        do
                        {
                            if (!RunStat)
                            {
                                AddLog("任务强制中止！");
                                return;
                            }
                            AddLog("发送停车信号: " + RgvModCtrlHelper.GetInstance().IsEnable);
                            DoRgvNormalStopCmdHandle(parameter);
                            ThreadSleep(1000);
                        } while (!parameter.Complete);
#if plcModbus
                        ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 0);
#endif
                        AddLog("扫图结束");
                        UploadComplete();
                        AddLog("等待RGV停止...");
                        while (true)
                        {
                            if (!RunStat)
                            {
                                AddLog("任务强制中止！");
                                return;
                            }
                            if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                            {
                                break;
                            }
                        }
                        AddLog("线阵结束，RGV已停止，当前位置：" + RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce);
                    }
                    #endregion

                    #region 车头检测
                    else if (testForm.GetEnable(Form.EnableEnum.车头检测))
                    {
                        if (!CheckHeadLocation())
                        {
                            return;
                        }
                        DoRgvNormalStopCmdHandle(null);
                        ThreadSleep(5000);
                        while (true)
                        {
                            if (!RunStat)
                            {
                                AddLog("任务强制中止！");
                                return;
                            }
                            if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor == eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP)
                            {
                                AddLog("Rgv停止");
                                break;
                            }
                            ThreadSleep(50);
                        } 
                    }
                    #endregion

                    if (!testForm.GetEnable(Form.EnableEnum.返回线阵))
                    {
                        #region 面阵流程
#if newBasler
                        frontShotCount = backShotCount = frontPicCount = backPicCount = 0;
#endif
                        Job = "面阵任务执行中";
                        RunMz = TaskMzCameraIsRunning = true;
                        int last_rgv_distance = 0;
                        AddLog("开始面阵流程，数据数量：" + mzCameraDataList.Count);
                        #region 等待数据准备
                        while (!mzDataComplete)
                        {
                            if (!RunStat)
                            {
                                AddLog("任务强制中止！");
                                return;
                            }
                            ThreadSleep(100);
                        }
                        #endregion
                        #region 执行面阵流程
                        if (testForm.GetEnable(Form.EnableEnum.异步))
                        {
                            MzRun(testForm.GetEnable(Form.EnableEnum.RGV));
                        }
                        else if (testForm.GetEnable(Form.EnableEnum.同步))
                        {
                            if (testForm.GetEnable(Form.EnableEnum.完整面阵))
                            {
                                for (int i = 0; i < mzCameraDataList.Count; ++i)
                                {
                                    if (!RunStat)
                                    {
                                        AddLog("任务强制中止！");
                                        return;
                                    }
                                    mzIndex = i;
                                    DataMzCameraLineModel model = mzCameraDataList[i];
                                    MzRun(model, ref last_rgv_distance);
                                }
                            }
                            else if (testForm.GetEnable(Form.EnableEnum.部分面阵))
                            {
                                for (int i = 0; i < mzList.Count; ++i)
                                {
                                    if (!RunStat)
                                    {
                                        AddLog("任务强制中止！");
                                        return;
                                    }
                                    mzIndex = i;
                                    DataMzCameraLineModel model = mzList[i];
                                    MzRun(model, ref last_rgv_distance);
                                }
                            }
                        }
                        else
                        {
                            AddLog("跳过面阵流程");
                        } 
                        #endregion
                        //复位状态
                        TaskMzCameraIsRunning = false;
                        if (!testForm.GetEnable(Form.EnableEnum.异步))
                        {
                            last_rgv_distance = 0;
                        }
#if newBasler
                        AddLog(string.Format("前相机共拍照{0}次，收到照片{1}张", frontShotCount, frontPicCount));
                        AddLog(string.Format("后相机共拍照{0}次，收到照片{1}张", backShotCount, backPicCount));
#endif
                        AddLog("面阵流程结束");
                        #endregion

                        #region 重传
#if ping
                        ThreadStart(() =>
                        {
                            if (xzNotLoad.Count > 0)
                            {
                                AddLog("有未能上传的线阵图片(" + xzNotLoad.Count + "张)");
                                GrabImg = "";
                                int completeCount = 0;
                                for (int i = 0; i < xzNotLoad.Count; i++)
                                {
                                    pingResult[uploadServerKey] = true;
                                    Image img = Image.FromFile(Application.StartupPath + @"\task_data\xz_camera\" + xzNotLoad[i] + GetExtend());
                                    if (UploadImage(img, xzNotLoad[i]))
                                    {
                                        completeCount++;
                                    }
                                }
#if controlSocket
                                SokcetExeWrite(SocketCmd.robot_upload_error, $"{Properties.Settings.Default.RobotID},0,{xzNotLoad.Count},{completeCount}"); 
#endif
                                AddLog($"重新上传线阵图片完成，上传成功【{completeCount}】张，失败【{(xzNotLoad.Count - completeCount)}】张");
                            }
                            if (xzLNotLoad.Count > 0)
                            {
                                AddLog("有未能上传的左线阵图片(" + xzLNotLoad.Count + "张)");
                                GrabImg = "";
                                int completeCount = 0;
                                for (int i = 0; i < xzLNotLoad.Count; i++)
                                {
                                    pingResult[uploadServerKey] = true;
                                    Image img = Image.FromFile(Application.StartupPath + @"\task_data\xz_camera\" + xzLNotLoad[i] + GetExtend());
                                    if (UploadImage(img, xzLNotLoad[i], true))
                                    {
                                        completeCount++;
                                    }
                                }
#if controlSocket
                                SokcetExeWrite(SocketCmd.robot_upload_error, $"{Properties.Settings.Default.RobotID},1,{xzLNotLoad.Count},{completeCount}"); 
#endif
                                AddLog($"重新上传左线阵图片完成，上传成功【{completeCount}】张，失败【{(xzLNotLoad.Count - completeCount)}】张");
                            }
                            if (xzRNotLoad.Count > 0)
                            {
                                AddLog("有未能上传的右线阵图片(" + xzRNotLoad.Count + "张)");
                                GrabImg = "";
                                int completeCount = 0;
                                for (int i = 0; i < xzRNotLoad.Count; i++)
                                {
                                    pingResult[uploadServerKey] = true;
                                    Image img = Image.FromFile(Application.StartupPath + @"\task_data\xz_camera\" + xzRNotLoad[i] + GetExtend());
                                    if (UploadImage(img, xzRNotLoad[i], true))
                                    {
                                        completeCount++;
                                    }
                                    AddLog($"重新上传右线阵图片完成，上传成功【{completeCount}】张，失败【{(xzRNotLoad.Count - completeCount)}】张");
                                }
#if controlSocket
                                SokcetExeWrite(SocketCmd.robot_upload_error, $"{Properties.Settings.Default.RobotID},2,{xzRNotLoad.Count},{completeCount}"); 
#endif
                                AddLog("重新上传右线阵图片完成");
                            }
                            if (mzNotLoad.Count > 0)
                            {
                                AddLog("有未能上传的面阵图片(" + mzNotLoad.Count + "张)");
                                int completeCount = 0;
                                for (int i = 0; i < mzNotLoad.Count; i++)
                                {
                                    pingResult[uploadServerKey] = true;
                                    string file = Application.StartupPath + @"\task_data\front_camera\" + mzNotLoad[i] + GetExtend();
                                    if (File.Exists(file))
                                    {
                                        Image img = Image.FromFile(file);
                                        if (UploadImage(img, mzNotLoad[i], 0))
                                        {
                                            completeCount++;
                                        }
                                    }
                                    else
                                    {
                                        file = Application.StartupPath + @"\task_data\back_camera\" + mzNotLoad[i] + GetExtend();
                                        if (File.Exists(file))
                                        {
                                            Image img = Image.FromFile(file);
                                            if (UploadImage(img, mzNotLoad[i], 1))
                                            {
                                                completeCount++;
                                            }
                                        }
                                        else
                                        {
                                            AddLog("本地未发现图片: " + mzNotLoad[i]);
                                        }
                                    }
                                }
#if controlSocket
                                SokcetExeWrite(SocketCmd.robot_upload_error, $"{Properties.Settings.Default.RobotID},3,{mzNotLoad.Count},{completeCount}"); 
#endif
                                AddLog($"重新上传面阵图片完成，上传成功【{completeCount}】张，失败【{(mzNotLoad.Count - completeCount)}】张");
                            }
                            Dictionary<string, string> faild = new Dictionary<string, string>();
                            if (back3dNotLoad.Count > 0)
                            {
                                AddLog("有未能上传的后3D数据(" + back3dNotLoad.Count + "个)");
                                int completeCount = 0;
                                foreach (KeyValuePair<string, string> item in back3dNotLoad)
                                {
                                    pingResult[Properties.Settings.Default.UploadDataServer] = pingResult[uploadServerKey] = true;
                                    if (UploadData(item.Value, item.Key, 1))
                                    {
                                        completeCount++;
                                    }
                                    else
                                    {
                                        if (!faild.ContainsKey(item.Key))
                                        {
                                            faild.Add(item.Key, item.Value); 
                                        }
                                    }
                                }
#if controlSocket
                                SokcetExeWrite(SocketCmd.robot_upload_error, $"{Properties.Settings.Default.RobotID},5,{back3dNotLoad.Count},{completeCount}"); 
#endif
                                AddLog($"重新上传后3D数据完成，上传成功【{completeCount}】，失败【{(back3dNotLoad.Count - completeCount)}】", 2);
                            }
                            if (front3dNotLoad.Count > 0)
                            {
                                AddLog("有未能上传的前3D数据(" + front3dNotLoad.Count + "个)");
                                int completeCount = 0;
                                foreach (KeyValuePair<string, string> item in front3dNotLoad)
                                {
                                    pingResult[Properties.Settings.Default.UploadDataServer] = pingResult[uploadServerKey] = true;
                                    if (UploadData(item.Value, item.Key, 0))
                                    {
                                        completeCount++;
                                    }
                                    else
                                    {
                                        if (!faild.ContainsKey(item.Key))
                                        {
                                            faild.Add(item.Key, item.Value);
                                        }
                                    }
                                }
#if controlSocket
                                SokcetExeWrite(SocketCmd.robot_upload_error, $"{Properties.Settings.Default.RobotID},4,{front3dNotLoad.Count},{completeCount}"); 
#endif
                                AddLog($"重新上传前3D数据完成，上传成功【{completeCount}】，失败【{(front3dNotLoad.Count - completeCount)}】", 1);
                                if (faild.Count > 0)
                                {
                                    using (StreamWriter sw = new StreamWriter(_3dLoc, true))
                                    {
                                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                        sw.WriteLine(JsonConvert.SerializeObject(faild));
                                    }
                                }
                            }
                        });
#endif
                        #endregion

                        #region 流程结束-自动充电
                        //机械臂归零
                        DoRobotCmdBackZeroHandle("1");
                        AddLog("前机械臂回归原点");
                        DoRobotCmdBackZeroHandle("2");
                        AddLog("后机械臂回归原点");
                        WaitRobotEnding();

                    //自动充电
                    goback:
                        WaitCmdHandle("结束准备充电");
                        DoRgvStartIntelligentChargeCmdHandle();
                        AddLog("开始充电");
                        while (true)
                        {
                            if (!RunStat)
                            {
                                AddLog("任务强制中止！");
                                return;
                            }
                            if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                            {
                                break;
                            }
                            ThreadSleep(1000);
                        }
                        if (isWait)
                        {
                            goto goback;
                        }
                        #endregion 
                    }
                    else
                    {
                        #region 返回时采图流程
                        job = "返回线阵任务执行中";
                        isBackProcess = true;
                        taskScheduleHandleInfo.xzCameraPicDataListCount = taskScheduleHandleInfo.xzLeftPicDataListCount = taskScheduleHandleInfo.xzRightPicDataListCount = 0;
                        uploadID = Properties.Settings.Default.TrainMode + "_" + Properties.Settings.Default.TrainSn + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
                        DoRgvStartIntelligentChargeCmdHandle();
                        AddLog("开始充电");
                        ThreadSleep(2000);
                        if (!RunStat)
                        {
                            AddLog("任务强制中止！");
                            return;
                        }
                        if (!Directory.Exists(Application.StartupPath + @"\task_data\xz_back_camera\"))
                        {
                            Directory.CreateDirectory(Application.StartupPath + @"\task_data\xz_back_camera\");
                        }
                        AddLog("开始采图");
                        if (testForm.GetEnable(Form.EnableEnum.完整线阵))
                        {
#if newBasler
                            if (testForm.GetEnable(Form.EnableEnum.单线阵))
                            {
                                DoCameraCmdHandle_ContinuousShot(xzID);
                            }
                            if (testForm.GetEnable(Form.EnableEnum.双线阵) && xzLeftID > -1 && xzRightID > -1)
                            {
                                DoCameraCmdHandle_ContinuousShot(xzLeftID);
                                DoCameraCmdHandle_ContinuousShot(xzRightID);
                            }
#else
                            DoCameraCmdHandle_ContinuousShot(taskMainCameraModel.XzCamerainfoItem, Application.StartupPath + @"\task_data\xz_back_camera\10000" + GetExtend());
#endif
                            while (true)
                            {
                                if (!RunStat)
                                {
                                    AddLog("任务强制中止！");
                                    return;
                                }
                                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce < TrainCurrentHeadDistance)
                                {
                                    AddLog("停止采图");
                                    DoCameraCmdHandle_Stop();
                                    break;
                                }
                                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                                {
                                    AddLog("停止采图");
                                    DoCameraCmdHandle_Stop();
                                    break;
                                }
                                ThreadSleep(500);
                            }
                        }
                        else if (testForm.GetEnable(Form.EnableEnum.部分线阵))
                        {
                            bool isContinuousShot = true;
                            List<Form.TrainLocation> xzData = testForm.GetXzLocation();
                            for (int i = 0; i < xzData.Count; i++)
                            {
                                Form.TrainLocation item = xzData[i];
                            waitShot:
                                if (!RunStat)
                                {
                                    AddLog("任务强制中止！");
                                    return;
                                }
                                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce >= TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ + item.Start &&
                                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce <= TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ + item.End)
                                {
                                    if (!isContinuousShot)
                                    {
                                        AddLog("开始采图");
#if newBasler
                                        if (testForm.GetEnable(Form.EnableEnum.单线阵))
                                        {
                                            DoCameraCmdHandle_ContinuousShot(xzID);
                                        }
                                        if (testForm.GetEnable(Form.EnableEnum.双线阵) && xzLeftID > -1 && xzRightID > -1)
                                        {
                                            DoCameraCmdHandle_ContinuousShot(xzLeftID);
                                            DoCameraCmdHandle_ContinuousShot(xzRightID);
                                        }
#else
                                        DoCameraCmdHandle_ContinuousShot(taskMainCameraModel.XzCamerainfoItem, Application.StartupPath + @"\task_data\xz_camera\10000" + GetExtend());
#endif 
                                        isContinuousShot = true;
                                    }
                                    ThreadManager.ThreadSleep(50);
                                    goto waitShot;
                                }
                                else
                                {
                                    if (isContinuousShot)
                                    {
                                        AddLog("停止采图");
                                        DoCameraCmdHandle_Stop();
                                        isContinuousShot = false;
                                    }
                                }
                            }
                            AddLog("停止采图");
                            DoCameraCmdHandle_Stop();
                        }
                        UploadComplete();
                        #endregion
                    }

                    #region 任务结束
                    UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;
                    RunStat = false;
                    AddLog("<---任务结束--->");
                    Job = "上位机等待任务中";
                    endAct?.Invoke();
                    #endregion
                }
            });
        }

        /// <summary>
        /// 强制终止
        /// </summary>
        private void DoOneKeyStopCmdHandle(object obj)
        {
            TaskRun(() =>
            {
                lock (lockObj)
                {
                    DoOneKeyStopCmdHandle();
                }
            });
        }

        /// <summary>
        /// 强制终止
        /// </summary>
        private void DoOneKeyStopCmdHandle()
        {
            if (!isInitApplication)
            {
                return;
            }
            AddLog("正在终止任务...");
            if (RunStat)
            {
                RunStat = false;

                try
                {
                    AddLog("停止拍照");
                    DoCameraCmdHandle_Stop();
                }
                catch (Exception) { }
                if (testForm.GetEnable(Form.EnableEnum.三维相机))
                {
                    AddLog("停止3D相机");
                    cognexManager.Stop();
                }
#if plcModbus
                if (testForm.GetEnable(Form.EnableEnum.PLC))
                {
                    AddLog("关闭线阵光源");
                    ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 0);
                    ThreadSleep(500);
                    AddLog("关闭激光");
                    ModbusTCP.SetAddress13(Address13Type.Laser, 0);
                }
#endif
                try
                {
                    AddLog("关闭前面阵光源");
                    LightOff(RobotName.Front);
                }
                catch (Exception) { }
                try
                {
                    AddLog("关闭后面阵光源");
                    LightOff(RobotName.Back);
                }
                catch (Exception) { }

                DoRgvNormalStopCmdHandle(null);
                while (true)
                {
                    if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                    {
                        break;
                    }
                    ThreadSleep(1000);
                    DoRgvNormalStopCmdHandle(null);
                }
                RgvSetSpeed();

                AddLog("前臂回原点");
                DoRobotCmdBackZeroHandle("1");
                AddLog("后臂回原点");
                DoRobotCmdBackZeroHandle("2");
                WaitRobotEnding();
                ThreadSleep(1000);
                WaitRobotEnding();
                Job = "任务终止中";
            }
            else
            {
                DoRgvNormalStopCmdHandle(null);
                while (true)
                {
                    if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                    {
                        break;
                    }
                    ThreadSleep(1000);
                    DoRgvNormalStopCmdHandle(null);
                }
                WaitRobotEnding();
                ThreadSleep(1000);
                WaitRobotEnding();
                DoRgvStartIntelligentChargeCmdHandle();
                AddLog("开始充电");
                while (true)
                {
                    if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                    {
                        break;
                    }
                    ThreadSleep(1000);
                }
            }
            //设备状态
            UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;
            AddLog("任务已终止");
            Job = "上位机等待任务中";
            testForm?.SetOnOff(true);
        }
        #endregion

        #region 底层逻辑
        #region 相机
#if newBasler
        private int frontShotCount = 0;
        private int backShotCount = 0;
        private int frontPicCount = 0;
        private int backPicCount = 0;

        private void Item_ImageReady(object sender, CameraEventArgs e)
        {
            int tryIndex = 0;
            try
            {
                BaslerCamera camera = sender as BaslerCamera;
                if (e.Image.Bitmap == null)
                {
                    return;
                }
                Bitmap bitmap = e.Image.Bitmap;
                #region 线阵图片保存及上传
                if (testForm.GetEnable(Form.EnableEnum.双线阵) ? camera.Name.ToLower() == "camera_xz" : camera.Name.ToLower().Contains("camera_xz"))
                {
                    if (xzOutTimeCount >= 0)
                    {
                        xzOutTimeCount = 0;
                    }
                    else
                    {
                        AddLog("TimeOut: 线阵超时后收到图片");
                    }
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    string fileIndex = taskScheduleHandleInfo.xzCameraPicDataListCount.ToString("10000");
                    string strfile = Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + GetExtend();
                    tryIndex = 1;
                    if (testForm.GetEnable(Form.EnableEnum.返回线阵))
                    {
                        if (isBackProcess)
                        {
                            strfile = Application.StartupPath + @"\task_data\xz_back_camera\" + fileIndex + GetExtend();
                        }
                    }
                    if (testForm.GetEnable(Form.EnableEnum.模拟流程))
                    {
                        string imgPath = string.Format(@"{0}\test_data\xz_camera\{1}{2}", Application.StartupPath, fileIndex, GetExtend());
                        if (File.Exists(imgPath))
                        {
                            AddLog("加载线阵图片：" + imgPath);
                            bitmap = (Bitmap)Bitmap.FromFile(imgPath); 
                        }
                    }
                    if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
                    {
                        rgvSpeedEndIndex = rgvSpeedList.Count - 1;
                    }
                    tryIndex = 2;
                    UploadImage(bitmap, 10000 + taskScheduleHandleInfo.xzCameraPicDataListCount);
                    tryIndex = 3;
                    if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
                    {
                        SaveBitmapIntoFileFromSpeed(bitmap, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion, rgvSpeedStartIndex, rgvSpeedEndIndex, rgvSpeedList);
                        rgvSpeedStartIndex = rgvSpeedEndIndex + 1;
                    }
                    else
                    {
                        SaveBitmapIntoFile(bitmap, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion);
                    }
                    AddLog("保存线阵图片：" + strfile);
#if bakImage
                    SaveBitmapIntoFile(bitmap, Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + ".bmp", JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion); 
                    AddLog("备份线阵图片：" + Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + ".bmp");
#endif
                    taskScheduleHandleInfo.xzCameraPicDataListCount++;
                    ThreadSleep(1500);
                    GrabImg = strfile;
                }
                #endregion
                #region 左线阵图片保存及上传
                else if (camera.Name.ToLower().Contains("camera_xz_left"))
                {
                    if (xzLeftOutTimeCount >= 0)
                    {
                        xzLeftOutTimeCount = 0; 
                    }
                    else
                    {
                        AddLog("TimeOut: 左线阵超时后收到图片");
                    }
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzLeftCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    string fileIndex = taskScheduleHandleInfo.xzLeftPicDataListCount.ToString("10000") + "L";
                    string strfile = Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + GetExtend();
                    tryIndex = 11;
                    if (testForm.GetEnable(Form.EnableEnum.返回线阵))
                    {
                        if (isBackProcess)
                        {
                            strfile = Application.StartupPath + @"\task_data\xz_back_camera\" + fileIndex + GetExtend();
                        }
                    }
                    if (testForm.GetEnable(Form.EnableEnum.模拟流程))
                    {
                        string imgPath = string.Format(@"{0}\test_data\xz_camera\{1}{2}", Application.StartupPath, fileIndex, GetExtend());
                        if (File.Exists(imgPath))
                        {
                            AddLog("加载左线阵图片：" + imgPath);
                            bitmap = (Bitmap)Bitmap.FromFile(imgPath);
                        }
                    }
                    if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
                    {
                        rgvSpeedEndIndex = rgvSpeedList.Count - 1;
                    }
                    tryIndex = 12;
                    UploadImage(bitmap, fileIndex, true);
                    tryIndex = 13;
                    if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
                    {
                        SaveBitmapIntoFileFromSpeed(bitmap, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion, rgvSpeedStartIndex, rgvSpeedEndIndex, rgvSpeedList);
                        rgvSpeedStartIndex = rgvSpeedEndIndex + 1;
                    }
                    else
                    {
                        SaveBitmapIntoFile(bitmap, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion);
                    }
                    AddLog("保存左线阵图片：" + strfile);
#if bakImage
                    SaveBitmapIntoFile(bitmap, Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + ".bmp", JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion); 
                    AddLog("备份线阵图片：" + Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + ".bmp");
#endif
                    taskScheduleHandleInfo.xzLeftPicDataListCount++;
                    ThreadSleep(1500);
                    XzLeftShot?.Invoke(bitmap);
                }
                #endregion
                #region 右线阵图片保存及上传
                else if (camera.Name.ToLower().Contains("camera_xz_right"))
                {
                    if (xzRightOutTimeCount >= 0)
                    {
                        xzRightOutTimeCount = 0;
                    }
                    else
                    {
                        AddLog("TimeOut: 右线阵超时后收到图片");
                    }
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzRightCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    string fileIndex = taskScheduleHandleInfo.xzRightPicDataListCount.ToString("10000") + "R";
                    string strfile = Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + GetExtend();
                    tryIndex = 14;
                    if (testForm.GetEnable(Form.EnableEnum.返回线阵))
                    {
                        if (isBackProcess)
                        {
                            strfile = Application.StartupPath + @"\task_data\xz_back_camera\" + fileIndex + GetExtend();
                        }
                    }
                    if (testForm.GetEnable(Form.EnableEnum.模拟流程))
                    {
                        string imgPath = string.Format(@"{0}\test_data\xz_camera\{1}{2}", Application.StartupPath, fileIndex, GetExtend());
                        if (File.Exists(imgPath))
                        {
                            AddLog("加载右线阵图片：" + imgPath);
                            bitmap = (Bitmap)Bitmap.FromFile(imgPath);
                        }
                    }
                    if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
                    {
                        rgvSpeedEndIndex = rgvSpeedList.Count - 1;
                    }
                    tryIndex = 15;
                    UploadImage(bitmap, fileIndex, false);
                    tryIndex = 16;
                    if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
                    {
                        SaveBitmapIntoFileFromSpeed(bitmap, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion, rgvSpeedStartIndex, rgvSpeedEndIndex, rgvSpeedList);
                        rgvSpeedStartIndex = rgvSpeedEndIndex + 1;
                    }
                    else
                    {
                        SaveBitmapIntoFile(bitmap, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion);
                    }
                    AddLog("保存右线阵图片：" + strfile);
#if bakImage
                    SaveBitmapIntoFile(bitmap, Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + ".bmp", JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion); 
                    AddLog("备份线阵图片：" + Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + ".bmp");
#endif
                    taskScheduleHandleInfo.xzRightPicDataListCount++;
                    ThreadSleep(1500);
                    XzRightShot?.Invoke(bitmap);
                }
                #endregion
                #region 前面阵图片保存及上传
                else if (camera.Name.ToLower().Contains("camera_front"))
                {
                    frontPicCount++;
                    shotFrontOutTime = -10000;
                getFrontID:
                    string id =
                        testForm.GetEnable(Form.EnableEnum.异步) ?
#if shotImageProcess
                        shotFrontID
#else
                        frontData[frontMzIndex].OnlyID
#endif
                        : mzCameraDataList[mzIndex].FrontComponentId;
                    if (string.IsNullOrEmpty(id))
                    {
                        ThreadSleep(50);
                        goto getFrontID;
                    }
                    AddLog($"待接收前相机图片: {id}", 1);
                    tryIndex = 4;
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    string strfile = Application.StartupPath + @"\task_data\front_camera\" + id + GetExtend();
                    if (testForm.GetEnable(Form.EnableEnum.模拟流程))
                    {
                        string imgPath = string.Format(@"{0}\test_data\front_camera\{1}{2}", Application.StartupPath, id, GetExtend());
                        if (File.Exists(imgPath))
                        {
                            AddLog("加载前臂图片：" + imgPath);
                            bitmap = (Bitmap)Bitmap.FromFile(imgPath); 
                        }
                    }
                    tryIndex = 5;
                    UploadImage(bitmap, id, 0);
                    tryIndex = 6;
                    SaveBitmapIntoFile(bitmap, strfile);
                    AddLog("保存前相机图片：" + strfile, 1);
#if bakImage
                    SaveBitmapIntoFile(bitmap, Application.StartupPath + @"\task_data\front_camera\" + id + ".bmp", JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion);
                    AddLog("备份前相机图片：" + Application.StartupPath + @"\task_data\front_camera\" + id + ".bmp");
#endif
#if shotImageProcess
                    shotFrontID = "";
                    AddLog("完成接收前相机图片", 1);
#endif
                }
                #endregion
                #region 后面阵图片保存及上传
                else if (camera.Name.ToLower().Contains("camera_back"))
                {
                    backPicCount++;
                    shotBackOutTime = -10000;
                getBackID:
                    string id =
                        testForm.GetEnable(Form.EnableEnum.异步) ?
#if shotImageProcess
                        shotBackID
#else
                        backData[backMzIndex].OnlyID
#endif
                        : mzCameraDataList[mzIndex].BackComponentId;
                    if (string.IsNullOrEmpty(id))
                    {
                        ThreadSleep(50);
                        goto getBackID;
                    }
                    AddLog($"待接收后相机图片: {id}", 2);
                    tryIndex = 7;
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    string strfile = Application.StartupPath + @"\task_data\back_camera\" + id
                        + GetExtend();
                    if (testForm.GetEnable(Form.EnableEnum.模拟流程))
                    {
                        string imgPath = string.Format(@"{0}\test_data\back_camera\{1}{2}", Application.StartupPath, id, GetExtend());
                        if (File.Exists(imgPath))
                        {
                            AddLog("加载后臂图片：" + imgPath);
                            bitmap = (Bitmap)Bitmap.FromFile(imgPath); 
                        }
                    }
                    tryIndex = 8;
                    UploadImage(bitmap, id, 1);
                    tryIndex = 9;
                    SaveBitmapIntoFile(bitmap, strfile);
                    AddLog("保存后相机图片：" + strfile, 2);
#if bakImage
                    SaveBitmapIntoFile(bitmap, Application.StartupPath + @"\task_data\back_camera\" + id + ".bmp", JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion);
                    AddLog("备份后相机图片：" + Application.StartupPath + @"\task_data\front_camera\" + id + ".bmp");
#endif
#if shotImageProcess
                    shotBackID = "";
                    AddLog("完成接收后相机图片", 2);
#endif
                }
                #endregion
                tryIndex = 10;
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                AddLog($"[{tryIndex}]{ex.Message}", -1);
            }
        }

        private void Item_CameraStatusChanged(object sender, CameraStatusEventArgs e)
        {
            BaslerCamera camera = sender as BaslerCamera;
            switch (e.Status)
            {
                case CameraStatusEvent.DEVICE_OPENED:
                    if (testForm.GetEnable(Form.EnableEnum.双线阵) ? camera.Name.ToLower() == "camera_xz" : camera.Name.ToLower().Contains("camera_xz"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                    }
                    else if (camera.Name.ToLower().Contains("camera_xz_left"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzLeftCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                    }
                    else if (camera.Name.ToLower().Contains("camera_xz_right"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzRightCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                    }
                    else if (camera.Name.ToLower().Contains("camera_front"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                    }
                    else if (camera.Name.ToLower().Contains("camera_back"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                    }
                    break;
                case CameraStatusEvent.DEVICE_CLOSING:
                    if (isContinuousShot)
                    {
                        if (testForm.GetEnable(Form.EnableEnum.单线阵))
                        {
                            if (testForm.GetEnable(Form.EnableEnum.双线阵) ? camera.Name.ToLower() == "camera_xz" : camera.Name.ToLower().Contains("camera_xz"))
                            {
                                if (!RunMz)
                                {
                                    RunMz = false;
                                    AddLog("camera_xz相机正在关闭，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                        }
                        if (testForm.GetEnable(Form.EnableEnum.双线阵))
                        {
                            if (camera.Name.ToLower().Contains("camera_xz_left"))
                            {
                                if (!RunMz)
                                {
                                    RunMz = false;
                                    AddLog("camera_xz_left相机正在关闭，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                            else if (camera.Name.ToLower().Contains("camera_xz_right"))
                            {
                                if (!RunMz)
                                {
                                    RunMz = false;
                                    AddLog("camera_xz_right相机正在关闭，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                        } 
                    }
#if controlSocket
                    SokcetExeWrite(SocketCmd.robot_camera_alarm, $"{Properties.Settings.Default.RobotID},2,{camera.Name}"); 
#endif
                    AddLog(camera.Name + "相机状态：" + e.Status.ToString());
                    break;
                case CameraStatusEvent.DEVICE_CLOSED:
                    if (isContinuousShot)
                    {
                        if (testForm.GetEnable(Form.EnableEnum.单线阵))
                        {
                            if (testForm.GetEnable(Form.EnableEnum.双线阵) ? camera.Name.ToLower() == "camera_xz" : camera.Name.ToLower().Contains("camera_xz"))
                            {
                                if (!RunMz)
                                {
                                    RunMz = false;
                                    AddLog("camera_xz相机已关闭，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                        }
                        if (testForm.GetEnable(Form.EnableEnum.双线阵))
                        {
                            if (camera.Name.ToLower().Contains("camera_xz_left"))
                            {
                                if (!RunMz)
                                {
                                    RunMz = false;
                                    AddLog("camera_xz_left相机已关闭，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                            else if (camera.Name.ToLower().Contains("camera_xz_right"))
                            {
                                if (!RunMz)
                                {
                                    RunMz = false;
                                    AddLog("camera_xz_right相机已关闭，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                        }
                    }
#if controlSocket
                    SokcetExeWrite(SocketCmd.robot_camera_alarm, $"{Properties.Settings.Default.RobotID},3,{camera.Name}"); 
#endif
                    AddLog(camera.Name + "相机状态：" + e.Status.ToString());
                    break;
                case CameraStatusEvent.GRABBING_STARTED:
                    if (camera.Name.ToLower().Contains("camera_xz"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_xz_left"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzLeftCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_xz_right"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzRightCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_front"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_back"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    }
                    break;
                case CameraStatusEvent.GRABBING_STOPPED:
                    if (camera.Name.ToLower().Contains("camera_xz"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_xz_left"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzLeftCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_xz_right"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzRightCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_front"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    }
                    else if (camera.Name.ToLower().Contains("camera_back"))
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                    }
                    break;
                case CameraStatusEvent.DEVICE_REMOVED:
                    if (isContinuousShot)
                    {
                        if (testForm.GetEnable(Form.EnableEnum.单线阵))
                        {
                            if (testForm.GetEnable(Form.EnableEnum.双线阵) ? camera.Name.ToLower() == "camera_xz" : camera.Name.ToLower().Contains("camera_xz"))
                            {
                                if (!RunMz)
                                {
                                    AddLog("camera_xz相机掉线，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                        }
                        if (testForm.GetEnable(Form.EnableEnum.双线阵))
                        {
                            if (camera.Name.ToLower().Contains("camera_xz_left"))
                            {
                                if (!RunMz)
                                {
                                    AddLog("camera_xz_left相机掉线，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                            else if (camera.Name.ToLower().Contains("camera_xz_right"))
                            {
                                if (!RunMz)
                                {
                                    AddLog("camera_xz_right相机掉线，停止线阵流程");
                                    DoOneKeyStopCmdHandle();
                                }
                            }
                        }
                    }
#if controlSocket
                    SokcetExeWrite(SocketCmd.robot_camera_alarm, $"{Properties.Settings.Default.RobotID},1,{camera.Name}"); 
#endif
                    AddLog(camera.Name + "相机状态：" + e.Status.ToString());
                    break;
            }
        }

        private void Item_CameraError(object sender, CameraErrorEventArgs e)
        {
            if (!e.Message.Contains("正在中止线程"))
            {
                if (isContinuousShot)
                {
                    DoOneKeyStopCmdHandle();
                }
#if controlSocket
                SokcetExeWrite(SocketCmd.robot_camera_alarm, $"{Properties.Settings.Default.RobotID},0,{(sender as ICamera).Name}");
#endif
                AddLog(">> [6] 相机异常：" + e.Message, -1); 
            }
        }
#else
        private void Camera_CameraImageEvent(Camera camera, Bitmap bmp)
        {
            if (camera != null && bmp == null)
            {
                CameraData data = camera as CameraData;
                switch (data.Name)
                {
                    case "front":
                        GrabImg1 = data.Path;
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                        break;
                    case "back":
                        GrabImg2 = data.Path;
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                        break;
                    case "xz":
                        GrabImg3 = data.Path;
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                        break;
                }
            }
            else
            {
                //保存图片到本地
                string strheigth = bmp.Height.ToString();

                try
                {
                    bool isFront = false;
                    bool isBack = false;
                    bool isXz = false;
                    if (testForm.GetEnable(Form.EnableEnum.模拟流程))
	                {
		                CameraData data = camera as CameraData;
                        isFront = data.Name == "camera_front";
                        isBack = data.Name ==  "camera_back";
                        isXz = data.Name == "camera_xz";
	                }
                    else
                    {
                        ICameraInfo camerainfo = camera.CameraInfo;
                        isFront = camerainfo[CameraInfoKey.FriendlyName].Contains("camera_front");
                        isBack = camerainfo[CameraInfoKey.FriendlyName].Contains("camera_back");
                        isXz = camerainfo[CameraInfoKey.FriendlyName].Contains("camera_xz");
                    }
                    if (isFront)
                    {
                        //启动图片传输任务
                        string strfile = Application.StartupPath + @"\task_data\front_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + strheigth + "H" + GetExtend();
                        SaveBitmapIntoFile(bmp, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion);

                        ThreadSleep(100);
                        light?.LightOff(true);
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                        ThreadSleep(500);
                        GrabImg1 = strfile;
                    } 
                    else if (isBack)
                    {
                        //保存图片到本地
                        string strfile = Application.StartupPath + @"\task_data\back_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + strheigth + "H" + GetExtend();
                        SaveBitmapIntoFile(bmp, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion);

                        ThreadSleep(100);
                        light?.LightOff(false);
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                        ThreadSleep(500);
                        GrabImg2 = strfile;
                    }
                    else if (isXz)
                    {
                        if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
	                    {
                            rgvSpeedEndIndex = rgvSpeedList.Count - 1; 
	                    }
                        string fileIndex = taskScheduleHandleInfo.xzCameraPicDataListCount.ToString("10000");
                        string strfile = Application.StartupPath + @"\task_data\xz_camera\" + fileIndex + GetExtend();
                        if (testForm.GetEnable(Form.EnableEnum.返回线阵))
	                    {
		                    if (isBackProcess)
                            {
                                strfile = Application.StartupPath + @"\task_data\xz_back_camera\" + fileIndex + GetExtend();
                            } 
	                    }
                        if (testForm.GetEnable(Form.EnableEnum.拉伸速度图片))
	                    {
                            SaveBitmapIntoFileFromSpeed(bmp, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion, rgvSpeedStartIndex, rgvSpeedEndIndex, rgvSpeedList);
                            rgvSpeedStartIndex = rgvSpeedEndIndex + 1; 
                        }
                        else
	                    {
	                        SaveBitmapIntoFile(bmp, strfile, JoinMode.Vertical, Properties.Settings.Default.OrcImageProportion); 
	                    }
                        //拍完一张图片
                        taskScheduleHandleInfo.xzCameraPicDataListCount++;
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                        ThreadSleep(1500);
                        GrabImg3 = strfile;
                    }
                }
                catch (Exception e)
                {
                    AddLog("保存图片异常：" + e.StackTrace, -1);
                }
            }
        }

        private void HomePageViewModel_CameraErrorEvent(Exception obj)
        {
            DoOneKeyStopCmdHandle(null);
            AddLog("Camera Error：" + obj.Message);
        }
#endif
        private void InitCameraMod()
        {
#if newBasler
            cameraManager = new CameraManager();
            cameraManager.Initialise();
            xzID = xzLeftID = xzRightID = frontID = backID = -1;
            int i = 0;
            foreach (ICamera item in cameraManager.Cameras)
            {
                item.CameraError += Item_CameraError;
                item.CameraStatusChanged += Item_CameraStatusChanged;
                item.ImageReady += Item_ImageReady;
                if (testForm.GetEnable(Form.EnableEnum.双线阵) ? item.Name.ToLower() == "camera_xz" : item.Name.ToLower().Contains("camera_xz"))
                {
                    xzID = i;
                    item.Format = "Mono8";
                }
                else if (item.Name.ToLower().Contains("camera_xz_left"))
                {
                    xzLeftID = i;
                    item.Format = "Mono8";
                }
                else if (item.Name.ToLower().Contains("camera_xz_right"))
                {
                    xzRightID = i;
                    item.Format = "Mono8";
                }
                else if (item.Name.ToLower().Contains("camera_front"))
                {
                    frontID = i;
                    item.Format = "";
                }
                else if (item.Name.ToLower().Contains("camera_back"))
                {
                    backID = i;
                    item.Format = "";
                }
                i++;
            }
#else
            try
            {
                //搜索可用相机
                CameraCtrlHelper.GetInstance().CameraConnection();

                //管理相机设备
                foreach (ICameraInfo item in CameraCtrlHelper.GetInstance().myCameraList)
                {
                    //找出具体对应的相机
                    if (item[CameraInfoKey.FriendlyName].Contains(@"camera_front"))
                    {
                        taskMainCameraModel.FrontCamerainfoItem = item;
                    }
                    else if (item[CameraInfoKey.FriendlyName].Contains(@"camera_back"))
                    {
                        taskMainCameraModel.BackCamerainfoItem = item;
                    }
                    //找出具体对应的相机
                    else if (item[CameraInfoKey.FriendlyName].Contains(@"camera_xz"))
                    {
                        taskMainCameraModel.XzCamerainfoItem = item;
                    }
                }
            }
            catch (Exception e)
            {
                AddLog("初始化相机异常：" + e.StackTrace, -1);
            }
#endif
        }

        private void CameraOpen()
        {
#if newBasler
            foreach (ICamera item in cameraManager.Cameras)
            {
                item.Initialise();
                item.Open();
            }
#endif
#if shotImageProcess
            cameraOpened = true;
#endif
        }
#if newBasler
        private void DoCameraCmdHandle_OneShot(int id)
        {
#if shotImageProcess
            while (!cameraOpened)
            {
                ThreadSleep(50);
            }
#endif
            if (!cameraManager.Cameras[id].Opened)
            {
                cameraManager.Cameras[id].Open();
            }
            if (cameraManager.Cameras[id].Opened)
            {
                if (id == xzID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                }
                else if (id == xzLeftID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzLeftCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                }
                else if (id == xzRightID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzRightCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                }
                else if (id == frontID)
                {
                    frontShotCount++;
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
#if bakImage
                    cameraManager.Cameras[id].BackImagePath = Application.StartupPath + @"\task_data\front_camera\" + frontShotCount;
#endif
                }
                else if (id == backID)
                {
                    backShotCount++;
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
#if bakImage
                    cameraManager.Cameras[id].BackImagePath = Application.StartupPath + @"\task_data\back_camera\" + backShotCount;
#endif
                }
                try
                {
                    cameraManager.Cameras[id].OneShot();
                }
                catch (Exception e)
                {
                    AddLog(e.Message, -1);
                }
            }
        }
#else
        private void DoCameraCmdHandle_OneShot(ICameraInfo item, string name, string path)
        {
            if (testForm.GetEnable(Form.EnableEnum.正常流程))
	        {
		        //默认选中挂接到第1个相机
                CameraCtrlHelper.GetInstance().CameraChangeItem(item);
                CameraCtrlHelper.GetInstance().CameraOneShot(name, path);
	        }
        }
#endif
#if newBasler
        private void DoCameraCmdHandle_ContinuousShot(int id)
        {
#if shotImageProcess
            while (!cameraOpened)
            {
                ThreadSleep(50);
            }
#endif
            ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 1);
            if (!cameraManager.Cameras[id].Opened)
            {
                cameraManager.Cameras[id].Open();
            }
            if (cameraManager.Cameras[id].Opened)
            {
                if (id == xzID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    XzShotOutTime(true);
                }
                else if (id == xzLeftID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzLeftCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    XzShotOutTime(leftXz: true);
                }
                else if (id == xzRightID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzRightCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                    XzShotOutTime(rightXz: true);
                }
                else if (id == frontID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                }
                else if (id == backID)
                {
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                }
                if (testForm.GetEnable(Form.EnableEnum.记录速度))
                {
                    saveSpeed = true;
                    TaskRun(AddSpeed);
                }
                isContinuousShot = true;
                cameraManager.Cameras[id].ContinuousShot();
            }
        }
#else
        private void DoCameraCmdHandle_ContinuousShot(ICameraInfo item, string path)
        {
            ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 1);
            if (testForm.GetEnable(Form.EnableEnum.模拟流程))
	        {
                string test = Application.StartupPath + "/work_data/xz_camera";
                DirectoryInfo directoryInfo = new DirectoryInfo(test);
                FileInfo[] fileInfos = directoryInfo.GetFiles();
                simulationProcessImages_xz.Clear();
                foreach (FileInfo file in fileInfos)
                {
                    simulationProcessImages_xz.Add(file.FullName);
                }
                isContinuousShot = true;
                TaskRun(() =>
                {
                    int i = 0;
                    while (isContinuousShot)
                    {
                        ThreadSleep(3000);
                        if (i >= simulationProcessImages_xz.Count)
                        {
                            i = 0;
                        }
                        Image image = Image.FromFile(simulationProcessImages_xz[i]);
                        Camera_CameraImageEvent(new CameraData()
                        {
                            Name = "camera_xz"
                        }, image as Bitmap);
                        i++;
                    }
                });
	        }
            else
            {
                //默认选中挂接到第1个相机
                try
                {
                    CameraCtrlHelper.GetInstance().CameraChangeItem(item);

                    try
                    {
                        //设置相机参数
                        CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].SetValue(Properties.Settings.Default.CameraWidth, IntegerValueCorrection.Nearest);
                        CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].SetValue(Properties.Settings.Default.CameraHeight, IntegerValueCorrection.Nearest);
                        CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(Properties.Settings.Default.CameraExposureTime);
                    }
                    catch (Exception e)
                    {
                        AddLog("设置相机参数失败");
                        AddLog(e.StackTrace, -1);
                    }

                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                    CameraCtrlHelper.GetInstance().CameraContinuousShot(path);
                }
                catch (Exception e)
                {
                    AddLog(e.StackTrace, -1);
                }
            }
            if (testForm.GetEnable(Form.EnableEnum.记录速度))
	        {
                saveSpeed = true;
                TaskRun(AddSpeed);
	        }
        }
#endif
        private void DoCameraCmdHandle_Stop()
        {
#if newBasler
            try
            {
                isContinuousShot = false;
                xzOutTimeCount = xzLeftOutTimeCount = xzRightOutTimeCount = -10;
                if (cameraManager?.Cameras != null)
                {
                    foreach (ICamera item in cameraManager?.Cameras)
                    {
                        if (item.Opened)
                        {
                            item.Stop();
                        }
                    }
                    ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 0); 
                }
            }
            catch (Exception e)
            {
                AddLog(e.Message, -1);
            }
#else
            if (testForm.GetEnable(Form.EnableEnum.模拟流程))
	        {
		        isContinuousShot = false; 
	        }
            else if (CameraCtrlHelper.GetInstance() != null)
            {
                CameraCtrlHelper.GetInstance().CameraStop();
                CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP; 
            }
#endif
            if (testForm.GetEnable(Form.EnableEnum.记录速度))
            {
                saveSpeed = false;
                SaveSpeed();
            }
        }

        private void DoRelinkShot(RobotName robotName)
        {
#if shotImageProcess
            switch (robotName)
            {
                case RobotName.Front:
                    RelinkCameraShot(frontID, shotFrontType, "前");
                    break;
                case RobotName.Back:
                    RelinkCameraShot(backID, shotBackType, "后");
                    break;
            } 
#endif
        }

        /// <summary>
        /// 部分图片线阵拼图
        /// </summary>
        private void DoCameraCmdHandle_Partial_Join(int obj1, int obj2, string obj3)
        {
            if (testForm.GetEnable(Form.EnableEnum.模拟流程))
	        {
		        if (copyImages_xz.Count == 0)
                {
                    string test = Application.StartupPath + "/work_data/xz_join";
                    DirectoryInfo directoryInfo = new DirectoryInfo(test);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo file in fileInfos)
                    {
                        copyImages_xz.Add(file.Name);
                    }
                }
                if (copyIndex < copyImages_xz.Count)
                {
                    File.Copy(Application.StartupPath + "/work_data/xz_join/" + copyImages_xz[copyIndex],
                              Application.StartupPath + "/task_data/xz_join/" + copyImages_xz[copyIndex]);
                    copyIndex++; 
                }
                else
                {
                    AddLog("缓存图片不足 " + copyIndex + "/" + copyImages_xz.Count);
                } 
	        }
            else
            {
                int pic_index1 = obj1 + 10000;
                int pic_index2 = obj2 + 10000;
                AddLog(string.Format("部分图片拼图: {0}至{1}, 路径 {2}", pic_index1, pic_index2, obj3));

                string join_file_name = obj3;
                List<Image> img_list = new List<Image>();

                //1.首先读取本地的所有图片文件
                DirectoryInfo dir = new DirectoryInfo(Application.StartupPath + @"\task_data\xz_camera\");
                FileInfo[] inf = dir.GetFiles();
                foreach (FileInfo finf in inf)
                {
                    //如果扩展名为“.bmp”
                    if (finf.Extension.Equals(GetExtend()))
                    {
                        //表示取到文件
                        int filename_num = -1;
                        string filename = Path.GetFileNameWithoutExtension(finf.Name);     //返回不带扩展名的文件名 
                        int.TryParse(filename, out filename_num);

                        if (filename_num != -1 && (filename_num >= pic_index1 && filename_num <= pic_index2))
                        {
                            Image newImage = Image.FromFile(finf.FullName);
                            img_list.Add(newImage);
                        }
                    }
                }

                //2.然后进行拼接
                //图片规则是：380AL - 2637 - 1 - 00000001.jpg.
                //车型号 - 车编号 - 车厢号 - 机器人编号
                AddLog(img_list.Count + "|" + pic_index1 + "-" + pic_index2);
                Image img = JoinImage(img_list, JoinMode.Vertical);
                AddLog(img == null ? "Image: Null" : img.Width + "|" + img.Height);
                String strfile = Application.StartupPath + @"\task_data\xz_join\" + join_file_name + GetExtend();
                SaveBitmapIntoFile(img, strfile, JoinMode.Vertical); 
            }
        }

        private void RelinkCameraShot(int cameraID, string type, string name)
        {
#if shotImageProcess
            if (type == "Mz" || type == "Ywc")
            {
                cameraOpened = false;
                do
                {
                    if (cameraManager != null)
                    {
                        AddLog("关闭相机，准备重连");
                        for (int i = 0; i < cameraManager.Cameras.Count; i++)
                        {
                            cameraManager.Cameras[i].Close();
                        }
                        cameraManager.Close();
                    }
                    InitCameraMod();
                    ThreadSleep(1000);
#if newBasler
                } while (GetCameraState());
#else
                    AddLog("相机数量: " + CameraCtrlHelper.GetInstance().myCameraList.Count);
                } while (CameraCtrlHelper.GetInstance().myCameraList.Count < 3);
                CameraCtrlHelper.GetInstance().CameraImageEvent += Camera_CameraImageEvent;
                CameraCtrlHelper.GetInstance().CameraErrorEvent += HomePageViewModel_CameraErrorEvent; 
                Schedule.ScheduleManager.CameraTest(taskMainCameraModel.XzCamerainfoItem,
                    taskMainCameraModel.FrontCamerainfoItem, taskMainCameraModel.BackCamerainfoItem);
                ThreadSleep(2000); 
#endif
                AddLog("打开相机");
                CameraOpen();

            }
#endif
        }

        private bool GetCameraState()
        {
            AddLog("相机数量: " + cameraManager.Cameras.Count + $" 线阵{xzID},左线阵{xzLeftID},右线阵{xzRightID},前面阵{frontID},后面阵{backID}");
            return
                (testForm.GetEnable(Form.EnableEnum.执行线阵) ? xzID < 0 : false) ||
                (testForm.GetEnable(Form.EnableEnum.前相机) ? frontID < 0 : false) ||
                (testForm.GetEnable(Form.EnableEnum.后相机) ? backID < 0 : false) ||
                (testForm.GetEnable(Form.EnableEnum.双线阵) ? xzLeftID < 0 : false) ||
                (testForm.GetEnable(Form.EnableEnum.双线阵) ? xzRightID < 0 : false);
        }

        private string GetExtend()
        {
            try
            {
                string extend = Properties.Settings.Default.SaveImageType.Split('/')[1];
                if (extend == "jpeg")
                {
                    return ".jpg";
                }
                else if (extend == "bmp")
                {
                    return ".bmp";
                }
                else if (extend == "png")
                {
                    return ".png";
                }
                else
                {
                    return ".jpg";
                }
            }
            catch (Exception)
            {
                return ".jpg";
            }
        }
        #endregion

        #region Ftp机制
        /// <summary>
        /// 列出目录
        /// </summary>
        private void ListDirectory()
        {
            bool isOk = false;
            if (string.IsNullOrEmpty(ipAddr) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                AddLog("请输入登录信息");
                return;
            }

            FtpClient.FtpHelper ftpHelper = FtpClient.FtpHelper.GetInstance(ipAddr, port, userName, password);
            string[] arrAccept = ftpHelper.ListDirectory(out isOk);//调用Ftp目录显示功能
            if (!isOk)
            {
                ftpHelper.SetPrePath();
            }
        }

        /// <summary>
        /// Ftp登录
        /// </summary>
        private void DoFtpLoginCmdHandle()
        {
            try
            {
                ListDirectory();
            }
            catch (Exception e)
            {
                AddLog("Ftp登录异常：" + e.Message, -1);
            }
        }

        /// <summary>
        /// Ftp直接上传
        /// </summary>
        private void DoFtpDirectUploadCmdHandle(string path)
        {
            FtpClient.FtpHelper ftpHelper = FtpClient.FtpHelper.GetInstance(ipAddr, port, userName, password);
            if (File.Exists(path))
            {
                ftpHelper.RelatePath = string.Format("{0}/{1}", ftpHelper.RelatePath, Path.GetFileName(path));

                bool isOk = false;
                ftpHelper.UpLoad(path, out isOk);
                ftpHelper.SetPrePath();
                if (isOk)
                {
                    ListDirectory();
                }
            }
        }

        /// <summary>
        /// 线阵压缩
        /// </summary>
        private void DoCameraCmdHandle_Compress1(string name)
        {
            if (testForm.GetEnable(Form.EnableEnum.压缩))
            {
                List<Image> img_list = new List<Image>();
                img_list.Clear();

                //读取本地的所有图片文件
                string src_dir = Application.StartupPath + @"\task_data\xz_join\";
                string dest_dir = Application.StartupPath + @"\task_data\xz_compress\" + name + ".zip";

                CreateZipFile(src_dir, dest_dir); 
            }
        }

        /// <summary>
        /// Ftp压缩后上传
        /// </summary>
        private void DoFtpZipUploadCmdHandle(string path)
        {
            //压缩打包
            DoCameraCmdHandle_Compress1(path);

            //找到压缩文件
            string dest_path = Application.StartupPath + @"\task_data\xz_compress\" + path + GetTimeStamp() + ".zip";
            AddLog("压缩到文件：" + dest_path);
            DoFtpDirectUploadCmdHandle(dest_path);
        }
        #endregion

        #region HttpServer
        private void MyHttpServerModInfoEvent(AppRemoteCtrl_DeviceFrame frame)
        {
            try
            {
                //开始任务
                if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_StartWork))
                {
                    //传递para
                    DoOneKeyStartCmdHandle(frame.para);
                }
                //结束任务
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_StopWork))
                {
                    //传递para
                    DoOneKeyStopCmdHandle(null);
                }
                //紧急停止
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceStop))
                {
                    //传递para
                    DoRgvEmergeStopCmdHandle();
                }
                //清除报警(车的报警)
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceClearAlarm))
                {
                    //传递para
                    DoRgvClearAlarmCmdHandle(null);
                }
                //自动充电
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DevicePowerCharge))
                {
                    DoRgvStartIntelligentChargeCmdHandle();
                }
                //回归原点
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceRZSite))
                {
                    DoRobotCmdBackZeroHandle("1");
                    DoRobotCmdBackZeroHandle("2");
                }
                //设备自检
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceCheckSelf))
                {
                    if (Schedule.ScheduleManager.PowerSelfTest(taskMainCameraModel.XzCamerainfoItem, taskMainCameraModel.FrontCamerainfoItem, taskMainCameraModel.BackCamerainfoItem, mzCameraDataList, taskScheduleHandleInfo))
                    {
                        UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;
                    }
                    else
                    {
                        UserInfo.myDeviceStat = UserEntity.key_DEVICE_INVALID;
                    }
                }
                //设备关机
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DevicePowerOff))
                {
                    System.Diagnostics.Process.Start("shutdown.exe", "-s");
                }
            }
            catch (Exception e)
            {
                AddLog("Http事件异常：" + e.Message, -1);
            }
        }
        private void UploadMzImage(string parsID, int robot, string path2D)
        {
            if (!testForm.GetEnable(Form.EnableEnum.业务接口))
            {
                return;
            }
            string strURL = Properties.Settings.Default.UploadDataServer + "/planMalfunctionManagement/auth/add" +
                string.Format("?abnormalPhoto={0}&uniqueNumber={1}&motorCarModel={2}&motorCarNumber={3}&componentNumber={4}",
                path2D, Properties.Settings.Default.RobotID, Properties.Settings.Default.TrainMode, Properties.Settings.Default.TrainSn, parsID);
            int logType = robot + 1;
            AddLog(string.Format("HTTP请求：{0}", strURL), logType);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
            // 编码格式
            byte[] payload = System.Text.Encoding.UTF8.GetBytes("");
            request.ContentLength = payload.Length;
            Stream writer;
            try
            {
                writer = request.GetRequestStream();
            }
            catch (Exception e)
            {
                writer = null;
                AddLog("连接服务器失败[HTTP上传面阵图片]: " + e.Message, -1);
                return;
            }
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }
            Stream s = response.GetResponseStream();
            StreamReader sRead = new StreamReader(s);
            string postContent = sRead.ReadToEnd();
            sRead.Close();
            AddLog("HTTP返回：" + postContent, logType);
        }
        private bool Upload3dData(string parsID, int robot, string value)
        {
            if (!testForm.GetEnable(Form.EnableEnum.业务接口))
            {
                return false;
            }
            int logType = robot + 1;
#if ping
            if (!pingResult[Properties.Settings.Default.UploadDataServer])
            {
                if (robot == 0)
                {
                    if (!front3dNotLoad.ContainsKey(parsID))
                    {
                        front3dNotLoad.Add(parsID, value); 
                    }
                }
                else
                {
                    if (!back3dNotLoad.ContainsKey(parsID))
                    {
                        back3dNotLoad.Add(parsID, value); 
                    }
                }
                AddLog("3D数据未上传", logType);
                return false;
            }
#endif
            string strURL = Properties.Settings.Default.UploadDataServer + "/overhaulPlanMeasuringValueManagement/auth/addOverhaulPlanMeasuringValue" +
                string.Format("?uniqueNumber={0}&totalCode={1}&measuring={2}&measuringValue={3}&status={4}&photo2d={5}&photo3d={6}",
                Properties.Settings.Default.RobotID, parsID, robot, value, "正常", "", value);
            AddLog(string.Format("HTTP请求：{0}", strURL), logType);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
            // 编码格式
            byte[] payload = System.Text.Encoding.UTF8.GetBytes("");
            request.ContentLength = payload.Length;
            Stream writer;
            try
            {
                writer = request.GetRequestStream();
            }
            catch (Exception e)
            {
                writer = null;
                AddLog("连接服务器失败[HTTP上传3D数据]: " + e.Message, -1);
#if ping
                pingResult[Properties.Settings.Default.UploadDataServer] = false;
#endif
                return false;
            }
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
                AddLog(">> [5] " + ex.Message, -1);
            }
            try
            {
                Stream s = response.GetResponseStream();
                StreamReader sRead = new StreamReader(s);
                string postContent = sRead.ReadToEnd();
                sRead.Close();
                AddLog("HTTP返回：" + postContent, logType);
            }
            catch (Exception ex)
            {
                AddLog(">> [4] " + ex.Message, -1);
                return false;
            }
            return true;
        }
        #endregion

        #region Relay继电器
        /// <summary>
        /// NetworkRelay继电器信息事件
        /// </summary>
        private void MyNetworkRelayModInfoEvent(NetworkRelayGlobalInfo relayinfo)
        {
            try
            {
                //数据协议信息
                //taskMainCameraModel. = relayinfo.NetworkRelayRspMsg;
            }
            catch (Exception e)
            {
                AddLog("继电器事件异常：" + e.Message, -1);
            }
        }

        private void InitRelayMod()
        {
            if (testForm.GetEnable(Form.EnableEnum.正常流程))
            {
                //继电器操作
                DoRelayCreatServer_CmdHandle();
                while (true)
                {
                    if (NetworkRelayCtrlHelper.GetInstance().myNetworkRelayGlobalInfo.NetworkRelayConnStat)
                    {
                        break;
                    }
                    ThreadSleep(100);
                }
                DoAllRobotResetCtrl_CmdHandle();
                Schedule.ScheduleManager.RelayComplete = true; 
            }
        }

        private void DoRelayCreatServer_CmdHandle()
        {
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayServerCreat();
        }

        private void DoAllRobotResetCtrl_CmdHandle()
        {
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO1Ctrl_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO5Ctrl_Cmd);
            ThreadSleep(Properties.Settings.Default.RobotIO1_Wait);

            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO2Ctrl_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO6Ctrl_Cmd);
            ThreadSleep(Properties.Settings.Default.RobotIO2_Wait);

            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO3Ctrl_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO7Ctrl_Cmd);
            ThreadSleep(Properties.Settings.Default.RobotIO3_Wait);

            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO4Ctrl_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO8Ctrl_Cmd);
            ThreadSleep(Properties.Settings.Default.RobotIO4_Wait);

            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO1Stop_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO2Stop_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO3Stop_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO4Stop_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO5Stop_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO6Stop_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO7Stop_Cmd);
            NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO8Stop_Cmd);
        }
        #endregion

        #region Rgv控制
        private void MyRgvModInfoEvent(RgvGlobalInfo rgvinfo)
        {
            if (testForm != null && !testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }
            try
            {
                if (rgvinfo.RgvRunStatMonitor == eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE && rgvinfo.RgvCurrentRunSpeed == 0)
                {
                    rgvinfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                }
                if (rgvinfo.RgvRunStatMonitor == eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP)
                {
                    rgvinfo.RgvIsStandby = 1;
                }
                //RGV发生报警
                if (rgvinfo.RgvIsAlarm != 0)
                {
                    UserInfo.myDeviceStat = UserEntity.key_DEVICE_ERR_RGV_ERR;
                    UserInfo.myDeviceValue = rgvinfo.RgvCurrentStat;
                    RgvAlarm(rgvinfo);
                    IsAlarm = true;
                }
                else
                {
                    IsAlarm = false;
                    UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;

                    //数据协议信息
                    taskMainCameraModel.RgvCurrentStat = rgvinfo.RgvCurrentStat; //默认小车中间状态Msg信息
                    taskMainCameraModel.RgvCurrentMode = rgvinfo.RgvCurrentMode; //默认远程模式(自动)； 本地模式(手动)
                    taskMainCameraModel.RgvCurrentRunSpeed = rgvinfo.RgvCurrentRunSpeed;
                    taskMainCameraModel.RgvCurrentRunDistacnce = rgvinfo.RgvCurrentRunDistacnce;
                    taskMainCameraModel.RgvCurrentPowerElectricity = rgvinfo.RgvCurrentPowerElectricity;

                    if (isWait)
                    {
                        isWait = false;
                        if (!RunStat)
                        {
                            UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;
                            DoOneKeyStopCmdHandle(null);
                        }
                    }
                }

                RgvState = rgvinfo.RgvIsStandby == 0 ? "运动中" : "待机中";
                RgvSpeed = rgvinfo.RgvCurrentRunSpeed + " mm/s";
                RgvDistacnce = rgvinfo.RgvCurrentRunDistacnce + " mm";
                RgvElectricity = rgvinfo.RgvCurrentPowerElectricity + "%";
#if socketExe
                TaskRun(() =>
                {
#if controlSocket
                    SocketRgvInfo socketRgvInfo = new SocketRgvInfo(rgvinfo,
                        xzIndex > -1 ? xzCameraDataList[xzIndex] : new DataXzCameraLineModel() { DataLine_Index = -1 },
                        testForm != null && testForm.GetEnable(Form.EnableEnum.异步) ?
                        new DataMzCameraLineModelEx() { Rgv_Distance = mzDistance } :
                        mzIndex > -1 ? mzCameraDataList[mzIndex] : new DataMzCameraLineModelEx() { Rgv_Distance = 0 });
#else
                    object socketRgvInfo = null;
#endif
#if controlServer
                    string log = "";
                    lock (logLock)
                    {
                        string[] vs = logs.ToArray();
                        logs.Clear();
                        if (vs != null)
                        {
                            foreach (string item in vs)
                            {
                                log += item + "\r\n";
                            }
                        }
                    }
#if controlSocket
                    socketRgvInfo.TrainMode = Properties.Settings.Default.TrainMode;
                    socketRgvInfo.TrainSn = Properties.Settings.Default.TrainSn;
                    socketRgvInfo.TrainCurrentHeadDistance = TrainCurrentHeadDistance;
                    socketRgvInfo.Log = log;
                    socketRgvInfo.Job = Job; 
#endif
#endif
                    string json = JsonConvert.SerializeObject(socketRgvInfo);
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\socket\\SocketRgvInfo.json"))
                            {
                                sw.Write(json);
                            }
                            break;
                        }
                        catch (Exception)
                        {
                            ThreadSleep(10);
                        } 
                    }
                });
#endif
            }
            catch (Exception e)
            {
                AddLog("Rgv事件异常：" + e.Message, -1);
            }
        }

        private void InitRgvMod()
        {
            try
            {
                RgvModCtrlHelper.GetInstance().RgvModConnect(() => { return testForm.GetEnable(Form.EnableEnum.RGV); });
            }
            catch (Exception e)
            {
                AddLog("Rgv初始化异常: " + e.Message, -1);
            }
        }

        private void DoRgvForwardMotorCmdHandle(object obj)
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }

            //判断机械臂是否繁忙
            if (TaskMzCameraIsRunning)
            {
                AddLog("面阵采集流程正在工作,请稍后操作");
                return;
            }

            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_FORWARDMOTOR);
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
            }
        }

        private void DoRgvBackMotorCmdHandle(object obj)
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }

            //判断机械臂是否繁忙
            if (TaskMzCameraIsRunning)
            {
                AddLog("面阵采集流程正在工作,请稍后操作");
                return;
            }

            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_BACKWARDMOTOR);
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
            }
        }

        /// <summary>
        /// 停止RGV
        /// </summary>
        private void DoRgvNormalStopCmdHandle(object obj)
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }

            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_NORMALSTOP);
                AddLog("正常停车");
            }

            if (obj != null && obj is Parameter)
            {
                (obj as Parameter).Complete = true;
            }
        }

        /// <summary>
        /// 紧急停止
        /// </summary>
        private void DoRgvEmergeStopCmdHandle()
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }

            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_NORMALSTOP);
            }

            //得到坐标
            RobotModCtrlProtocol.RobotDataPack robotDataPack = new RobotModCtrlProtocol.RobotDataPack();
            robotDataPack.j1 = @"0.00";
            robotDataPack.j2 = @"0.00";
            robotDataPack.j3 = @"0.00";
            robotDataPack.j4 = @"0.00";
            robotDataPack.j5 = @"0.00";
            robotDataPack.j6 = @"0.00";
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID, RobotModCtrlProtocol.RobotData.RobotBackZeroFunCode, robotDataPack);

            //得到坐标
            robotDataPack.j1 = @"0.00";
            robotDataPack.j2 = @"0.00";
            robotDataPack.j3 = @"0.00";
            robotDataPack.j4 = @"0.00";
            robotDataPack.j5 = @"0.00";
            robotDataPack.j6 = @"0.00";
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID, RobotModCtrlProtocol.RobotData.RobotBackZeroFunCode, robotDataPack);
        }

        private void DoRgvClearAlarmCmdHandle(object obj)
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_CLEARALARM);
            }
        }

        private void DoRgvStartIntelligentChargeCmdHandle()
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_STARTPOWERCHARGE);
            }
        }

        private void DoRgvStopIntelligentChargeCmdHandle()
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_STOPPOWERCHARGE);
            }
        }

        /// <summary>
        /// RgvCmd-设定目标运行位置
        /// </summary>
        private void DoRgvSetTargetDistanceCmdHandle(int distance)
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }
            if (distance > RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTrackLength || distance < 0)
            {
                AddLog("Rgv运行目标距离,长度设置非法:超过轨道长度 " + distance + "/" + RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTrackLength);
            }
            else
            {
                if (RgvModCtrlHelper.GetInstance().IsEnable)
                {
                    DoRobotCmdBackZeroHandle("1");
                    DoRobotCmdBackZeroHandle("2");
                    WaitRobotEnding();

                    //设置距离
                    taskMainCameraModel.RgvTargetRunDistance = distance;
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunDistance = taskMainCameraModel.RgvTargetRunDistance;
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETDISATNCE);
                    ThreadSleep(100);

                    //运动到指定目标位置
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_RUNAPPOINTDISTANCE);
                }
            }
        }

        private void RgvSetSpeed(int speed = 0)
        {
            if (!testForm.GetEnable(Form.EnableEnum.RGV))
            {
                return;
            }
            if (speed == 0)
            {
                speed = rgvUpSpeed;
            }
            if (speed == 0)
            {
                speed = 800;
            }
            RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunSpeed = speed;
            RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETSPEED);
            ThreadSleep(100);
        }

        private void RgvAlarm(RgvGlobalInfo rgvinfo)
        {
            if (!isWait)
            {
                isWait = true;
                if (!RunMz)
                {
                    DoOneKeyStopCmdHandle(null);
                }
                AddLog("Rgv报警【" + rgvinfo.RgvIsAlarm + "】：" + rgvinfo.RgvCurrentStat);
            }
        }

        /// <summary>
        /// 等待报警结束
        /// </summary>
        private void WaitCmdHandle(string msg = "", int type = 0)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                AddLog(msg, type);
            }
            if (isWait && RunStat)
            {
                AddLog(">>>>>>>>>>>报警等待");
                while (isWait && RunStat)
                {
                    ThreadSleep(50);
                }
                AddLog("<<<<<<<<<<<报警结束");
            }
        }

        private void AddSpeed()
        {
            rgvSpeedList.Clear();
            rgvSpeedStartIndex = 0;
            while (saveSpeed)
            {
                rgvSpeedList.Add(taskMainCameraModel.RgvCurrentRunSpeed);
                ThreadSleep(50);
            }
        }

        private void SaveSpeed()
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "/json/" + Properties.Settings.Default.TrainMode + "_" + Properties.Settings.Default.TrainSn + "/RgvSpeed.json", false);
            sw.Write(JsonConvert.SerializeObject(rgvSpeedList));
            sw.Close();
        }

        private void RgvSpeedCorrect()
        {
            TaskRun(() =>
            {
                RunStat = true;
                int moveDistacnce = 20000;
                DoRgvStopIntelligentChargeCmdHandle();
                AddLog("停止充电");
                ThreadSleep(3000);
            checkSpeed:
                RgvSetSpeed();
                ThreadSleep(50);
                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + moveDistacnce < 5000)
                {
                    moveDistacnce = 0 - moveDistacnce;
                }
                int location = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + moveDistacnce;
                AddLog("移动到指定位置：" + location);
                DoRgvSetTargetDistanceCmdHandle(location);
                while (Math.Abs(RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed - RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunSpeed) > 10)
                {
                    if (!RunStat)
                    {
                        AddLog("任务强制中止！");
                        return;
                    }
                    ThreadSleep(10);
                }
                AddLog("开始测速");
                int speed = MathSpeed(10); if (!RunStat)
                {
                    return;
                }
                AddLog("结束测速，测量的平均速度: " + speed);
                int tolerance = Math.Abs(800 - speed);
                if (tolerance > Properties.Settings.Default.RgvSpeedTolerance)
                {
                    rgvUpSpeed += 800 - speed;
                    moveDistacnce = 0 - moveDistacnce;
                    while (true)
                    {
                        if (!RunStat)
                        {
                            AddLog("任务强制中止！");
                            return;
                        }
                        if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor == eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP)
                        {
                            break;
                        }
                        ThreadSleep(50);
                    }
                    goto checkSpeed;
                }
                else
                {
                    FileSystem.WriteIniFile("RGV", "rgvSpeed", rgvUpSpeed.ToString(), iniPath);
                }
                RunStat = false;
            });
        }

        private int MathSpeed(int listCount)
        {
            rgvMathSpeedList.Clear();
            int rgvUpLocation = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
            do
            {
                if (!RunStat)
                {
                    AddLog("任务强制中止！");
                    return 0;
                }
                ThreadSleep(1000);
                int location = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
                int rgvMathSpeed = Math.Abs(location - rgvUpLocation);
                rgvUpLocation = location;
                rgvMathSpeedList.Add(rgvMathSpeed);
            } while (rgvMathSpeedList.Count < listCount);
            DoRgvNormalStopCmdHandle(null);
            AddLog("测量的速度集合：" + JsonConvert.SerializeObject(rgvMathSpeedList));
            int speedCount = 0;
            foreach (int item in rgvMathSpeedList)
            {
                speedCount += item;
            }
            return speedCount / rgvMathSpeedList.Count;
        }
        #endregion

        #region Robot机械臂
        /// <summary>
        /// Robot信息事件
        /// </summary>
        private void MyRobotModInfoEvent(RobotGlobalInfo robotinfo, bool isFront)
        {
            try
            {
                //数据协议信息
                taskMainCameraModel.FrontRobotMsg = robotinfo.FrontRobotRspMsg;
                taskMainCameraModel.BackRobotMsg = robotinfo.BackRobotRspMsg;
                if (isFront)
                {
                    Front = robotinfo.FrontRobotSiteData;
                }
                else
                {
                    Back = robotinfo.BackRobotSiteData;
                }
            }
            catch (Exception e)
            {
                AddLog("Robot事件异常：" + e.Message, -1);
            }
        }

        private void InitRobotMod()
        {
            try
            {
                RobotModCtrlHelper.GetInstance().RobotServerCreat((int id) =>
                {
                    switch (id)
                    {
                        case 1:
                            return !testForm.GetEnable(Form.EnableEnum.前机械臂);
                        case 2:
                            return !testForm.GetEnable(Form.EnableEnum.后机械臂);
                        default:
                            return true;
                    }
                });
            }
            catch (Exception e)
            {
                AddLog("Robot初始化异常: " + e.Message, -1);
            }
        }

        /// <summary>
        /// 控制Robot运动
        /// </summary>
        private void DoRobotCmdSetDataHandle(string devid_str, RobotModCtrlProtocol.RobotDataPack robotdata)
        {
        CMD:
            if (!IsAlarm)
            {
                byte devid = 0x00;
                try
                {
                    devid = Convert.ToByte(devid_str);
                }
                catch { }

                if (RobotModCtrlHelper.isServer && !RobotModCtrlHelper.GetInstance()[devid])
                {
                    AddLog("机械臂掉线：" + devid_str);
                    return;
                }

                //发送事件
                RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(devid, RobotModCtrlProtocol.RobotData.RobotSetCtrlFunCode, robotdata);
                RefreshState(devid);
            }
            else
            {
                while (IsAlarm)
                {
                    if (!RunStat)
                    {
                        return;
                    }
                    ThreadSleep(1000);
                }
                goto CMD;
            }
        }

        /// <summary>
        /// 控制Robot回归原点
        /// </summary>
        private void DoRobotCmdBackZeroHandle(string devid_str)
        {
            byte devid = 0x00;
            try
            {
                devid = Convert.ToByte(devid_str);
            }
            catch { }
            string name = devid_str == "1" ? "前" : "后";

            if ((!testForm.GetEnable(Form.EnableEnum.前机械臂) && devid_str == "1") ||
                (!testForm.GetEnable(Form.EnableEnum.后机械臂) && devid_str == "2"))
            {
                AddLog("跳过" + name + "机械臂回归原点");
                return;
            }

            if (RobotModCtrlHelper.isServer && !RobotModCtrlHelper.GetInstance()[devid])
            {
                AddLog(name + "机械臂掉线");
                return;
            }

            RobotModCtrlProtocol.RobotDataPack robotDataPack = new RobotModCtrlProtocol.RobotDataPack();
            if (RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID == devid || RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID == devid)
            {
                //得到坐标
                robotDataPack.j1 = @"0.00";
                robotDataPack.j2 = @"0.00";
                robotDataPack.j3 = @"0.00";
                robotDataPack.j4 = @"0.00";
                robotDataPack.j5 = @"0.00";
                robotDataPack.j6 = @"0.00";
            }
            else
            {
                return;
            }

            if (devid_str == "1")
            {
                WaitRobotEnding(back: false);
            }
            else
            {
                WaitRobotEnding(front: false);
            }
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(devid, RobotModCtrlProtocol.RobotData.RobotBackZeroFunCode, robotDataPack);
            RefreshState(devid);
            AddLog(name + "机械臂回归原点");
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        private void RefreshState(byte devid)
        {
            //更新状态
            if (devid == 0x01)
            {
                RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE;
                Front = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotSiteData;
            }
            else if (devid == 0x02)
            {
                RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE;
                Back = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotSiteData;
            }
        }

        /// <summary>
        /// 等待机械臂停止
        /// </summary>
        private void WaitRobotEnding(bool front = true, bool back = true)
        {
            int count = 0;
            if (front && testForm.GetEnable(Form.EnableEnum.前机械臂))
            {
                AddLog("等待前机械臂停止运动。");
                while (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                {
                    if (RobotModCtrlHelper.isServer && !RobotModCtrlHelper.GetInstance()[RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID])
                    {
                        AddLog("前机械臂掉线");
                        break;
                    }
                    if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP ||
                        RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY)
                    {
                        AddLog("前机械臂停止");
                        RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
                        break;
                    }
                    ThreadSleep(50);
                    count++;
                }
                AddLog("前机械臂等待时间：" + count * 50 + "ms"); 
            }
            if (back && testForm.GetEnable(Form.EnableEnum.后机械臂))
            {
                count = 0;
                AddLog("等待后机械臂停止运动。");
                while (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                {
                    if (RobotModCtrlHelper.isServer && !RobotModCtrlHelper.GetInstance()[RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID])
                    {
                        AddLog("后机械臂掉线");
                        break;
                    }
                    if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP ||
                        RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY)
                    {
                        AddLog("后机械臂停止");
                        RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
                        break;
                    }
                    ThreadSleep(50);
                    count++;
                }
                AddLog("后机械臂等待时间：" + count * 50 + "ms"); 
            }
        }
        #endregion

        #region Stm32 IO检测
        /// <summary>
        /// Stm32继电器信息事件
        /// </summary>
        private void MyStm32ModInfoEvent(Stm32GlobalInfo stm32info)
        {
            if (testForm != null && !testForm.GetEnable(Form.EnableEnum.Stm32))
            {
                return;
            }
            try
            {
                //数据协议信息
                taskMainCameraModel.CurrentDetectHeight = stm32info.CurrentDetectHeight;
                taskMainCameraModel.InfraRed1_Stat = stm32info.InfraRed1_Stat;
                taskMainCameraModel.InfraRed2_Stat = stm32info.InfraRed1_Stat;
                taskMainCameraModel.InfraRed3_Stat = stm32info.InfraRed3_Stat;
                taskMainCameraModel.InfraRed4_Stat = stm32info.InfraRed4_Stat;
            }
            catch (Exception e)
            {
                AddLog("Stm32事件异常：" + e.Message, -1);
            }
        }

        private void InitStm32Mod()
        {
            try
            {
                Stm32ModCtrlHelper.GetInstance().Stm32ModConnect();
            }
            catch (Exception e)
            {
                AddLog("Stm32初始化异常: " + e.Message, -1);
            }
        }
        #endregion

        #region 光源
        private void LightOn(RobotName robot)
        {
#if controlLightPower
            ModbusTCP.SetAddress13(robot == RobotName.Front ? Address13Type.Front_Robot_Mz_LED : Address13Type.Back_Robot_Mz_LED, 1);
#else
            light?.LightOn(Properties.Settings.Default.LightFrontHigh, robot == RobotName.Front);
#endif
            ThreadSleep(lightSleep);
        }

        private void LightOff(RobotName robot)
        {
            ThreadSleep(lightSleep);
#if controlLightPower
            ModbusTCP.SetAddress13(robot == RobotName.Front ? Address13Type.Front_Robot_Mz_LED : Address13Type.Back_Robot_Mz_LED, 0);
#else
            light?.LightOff(robot == RobotName.Front);
#endif
        }
        #endregion
        #endregion

        #region 其它
        class Parameter
        {
            public bool Complete { get; set; } = false;
        }

        private bool CheckHeadLocation(bool isXz = false)
        {
            if ((isXz ? true : taskScheduleHandleInfo.rgvTaskStat == 1) && trainHeadLocation == 0)
            {
                if (testForm.GetEnable(Form.EnableEnum.雷达测车头))
                {
                    RgvSetSpeed();
                CheckHead:
                    TrainCurrentHeadDistance = (int)(GetLidarLoc(Properties.Settings.Default.TrainMode + "_" + Properties.Settings.Default.TrainSn, "", "")
#if !lidarCsharp
                        * 1000 
#endif
                    );
                    if (TrainCurrentHeadDistance <= 0)
                    {
                        AddLog("获取雷达定位车头失败：" + TrainCurrentHeadDistance);
                        return false;
                    }
                    TrainHeadDistance = (TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ) + " mm";
                    if (Properties.Settings.Default.ReCheckHead > 0)
                    {
                        int len = TrainCurrentHeadDistance - Properties.Settings.Default.ReCheckHead;
                        if (len > 0)
                        {
                            AddLog("检测的车头位置[" + TrainCurrentHeadDistance + "]远于设定的精度范围[" + Properties.Settings.Default.ReCheckHead + "]，需再次检测。");
                            int merchant = (len / 1000 + 1) * 1000;
                            int moveLocation = merchant + RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
                            DoRgvSetTargetDistanceCmdHandle(moveLocation);
                            while (true)
                            {
                                ThreadSleep(1000);
                                if (!RunStat)
                                {
                                    AddLog("任务强制中止！");
                                    return false;
                                }
                                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE &&
                                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed == 0 &&
                                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce > moveLocation - 100)
                                {
                                    AddLog("RGV停止");
                                    break;
                                }
                                else if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE &&
                                         RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed == 0)
                                {
                                    AddLog("RGV停止，且位置移动不足" + moveLocation);
                                    break;
                                }
                            }
                            ThreadSleep(1000);
                            goto CheckHead;
                        }
                    }
                    TrainCurrentHeadDistance += (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce - 5000);
                }
                AddLog("设置RGV速度");
                RgvSetSpeed(testForm.GetEnable(Form.EnableEnum.激光慢速测车头) ? Properties.Settings.Default.CheckingSpeed : 0);
                AddLog("设置RGV运行：" + RgvTrackLength);
                DoRgvSetTargetDistanceCmdHandle(RgvTrackLength);
            }

            AddLog("开始车头检测...");
            while (trainHeadLocation == 0)
            {
                if (!RunStat)
                {
                    AddLog("任务强制中止！");
                    return false;
                }
                bool isHead = false;
                if (testForm.GetEnable(Form.EnableEnum.激光测车头))
                {
                    isHead = height > 0.6 && height < 3;
                }
                else if (testForm.GetEnable(Form.EnableEnum.激光慢速测车头))
                {
                    isHead = height > 0.6 && height < 3;
                }
                else if (testForm.GetEnable(Form.EnableEnum.传感器测车头))
                {
                    isHead = ModbusTCP.GetIO(15);
                }
                else if (testForm.GetEnable(Form.EnableEnum.雷达测车头))
                {
                    isHead = TrainCurrentHeadDistance + 5000 < RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
                }
                testForm.SetHead(TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ);
                if (isHead)
                {
                    if (testForm.GetEnable(Form.EnableEnum.激光慢速测车头))
                    {
                        DoRgvNormalStopCmdHandle(null);
                        while (true)
                        {
                            if (!RunStat)
                            {
                                AddLog("任务强制中止！");
                                return false;
                            }
                            if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor == eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP)
                            {
                                break;
                            }
                            ThreadSleep(500);
                            DoRgvNormalStopCmdHandle(null);
                        }
                        RgvSetSpeed();
                        DoRgvSetTargetDistanceCmdHandle(RgvTrackLength);
                    }
                    if (!testForm.GetEnable(Form.EnableEnum.雷达测车头))
                    {
                        TrainCurrentHeadDistance = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
                    }
                    TrainHeadDistance = (TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ) + " mm";
                    AddLog("记录车头位置：" + (TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ));
                    if (testForm.GetEnable(Form.EnableEnum.执行线阵))
                    {
                        AddLog("开始相机线扫***");
#if plcModbus
                        ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 1);
#endif
                        taskScheduleHandleInfo.xzCameraPicDataListCount = taskScheduleHandleInfo.xzLeftPicDataListCount = taskScheduleHandleInfo.xzRightPicDataListCount = 0;
                        if (!testForm.GetEnable(Form.EnableEnum.部分线阵))
                        {
#if newBasler
                            if (testForm.GetEnable(Form.EnableEnum.单线阵))
                            {
                                DoCameraCmdHandle_ContinuousShot(xzID);
                            }
                            if (testForm.GetEnable(Form.EnableEnum.双线阵) && xzLeftID > -1 && xzRightID > -1)
                            {
                                DoCameraCmdHandle_ContinuousShot(xzLeftID);
                                DoCameraCmdHandle_ContinuousShot(xzRightID);
                            }
#else
                            DoCameraCmdHandle_ContinuousShot(taskMainCameraModel.XzCamerainfoItem, Application.StartupPath + @"\task_data\xz_camera\10000" + GetExtend());
#endif  
                        }
                    }
                    break;
                }
                else if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed == 0)
                {
                    AddLog("RGV未运行，等待3秒后重新启动");
                    ThreadSleep(3000);
                    if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunSpeed != (testForm.GetEnable(Form.EnableEnum.激光慢速测车头) ? Properties.Settings.Default.CheckingSpeed : (rgvUpSpeed == 0 ? 800 : rgvUpSpeed)))
                    {
                        AddLog("设置RGV速度");
                        RgvSetSpeed(testForm.GetEnable(Form.EnableEnum.激光慢速测车头) ? Properties.Settings.Default.CheckingSpeed : 0);
                    }
                    AddLog("RGV未启动，重新设置RGV运行：" + RgvTrackLength);
                    DoRgvSetTargetDistanceCmdHandle(RgvTrackLength);
                }
                ThreadSleep(50);
            }

            if (!isXz)
            {
                if (trainHeadLocation > 0)
                {
                    RgvSetSpeed();
                    TrainCurrentHeadDistance = trainHeadLocation;
                    AddLog("记录车头位置：" + TrainCurrentHeadDistance);
                } 
            }

            return true;
        }

        private byte[] ImageToBytes(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private bool Inspect()
        {
#if newBasler
            if (cameraManager.Cameras.Count < 3)
#else
            if (CameraCtrlHelper.GetInstance().myCameraList.Count < 3)
#endif
            {
#if newBasler
                AddLog("相机丢失，目前连接的相机数量：" + cameraManager.Cameras.Count);
#else
                AddLog("相机丢失，目前连接的相机数量：" + CameraCtrlHelper.GetInstance().myCameraList.Count);
#endif
                return false;
            }
            if (!RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotConnStat)
            {
                AddLog("前机械臂未完成连接");
                return false;
            }
            if (!RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotConnStat)
            {
                AddLog("后机械臂未完成连接");
                return false;
            }
            if (!RgvModCtrlHelper.GetInstance().IsEnable)
            {
                AddLog("RGV未完成连接");
                return false;
            }
            if (!ModbusTCP.IsLink)
            {
                AddLog("PLC未成功连接");
                return false;
            }
            if (light == null || !light.IsLink)
            {
                AddLog("光源未连接");
                return false;
            }
            AddLog("自检完成");
            return true;
        }

        private int JoinImg(int i, int joinIndex, ref int joinComplete)
        {
            string join_filename;
            if (testForm.GetEnable(Form.EnableEnum.拼图))
            {
                int count = taskScheduleHandleInfo.xzCameraPicDataListCount;
                if (joinIndex < count)
                {
                    int i1 = joinIndex, i2 = count - 1;
                    joinComplete++;
                    int jc = joinComplete;
                    TaskRun(() =>
                    {
                        join_filename = train_para.train_mode + "_" + train_para.train_sn + "_" + i + "_" + train_para.robot_id;
                        DoCameraCmdHandle_Partial_Join(i1, i2, join_filename);
                        if (jc == 8)
                        {
                            AddLog("结束任务流程时的动作,启动Ftp上传流程");
                            for (int j = 0; j < 60; j++)
                            {
                                if (jc < xzCameraDataList.Count)
                                {
                                    ThreadSleep(1000);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            DoFtpLoginCmdHandle();
                            string filename = train_para.train_mode + @"_" + train_para.train_sn;
                            DoFtpZipUploadCmdHandle(filename);
                        }
                    });
                }
                return count;
            }
            return 0;
        }
        #endregion

        #region 线阵与面阵相关
        private void XzRun(int start, int end)
        {
            TaskRun(() =>
            {
                if (RunStat)
                {
                    return;
                }
                RunStat = true;
                if (!CheckHeadLocation())
                {
                    return;
                }
                RgvSetSpeed();
                int startLength = TrainCurrentHeadDistance
#if lidarLocation
                    + Properties.Settings.Default.RobotPointXZ
#endif
                    + start;
                int endLength = TrainCurrentHeadDistance
#if lidarLocation
                    + Properties.Settings.Default.RobotPointXZ
#endif
                    + end;

                AddLog("运动到指定位置：" + endLength);
                DoRgvSetTargetDistanceCmdHandle(endLength);
                while (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + 1000 < startLength)
                {
                    if (!RunStat)
                    {
                        AddLog("任务强制中止！");
                        return;
                    }
                    ThreadSleep(500);
                }
                AddLog("开始相机线扫***");
#if plcModbus
                ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 1);
#endif
                taskScheduleHandleInfo.xzCameraPicDataListCount = taskScheduleHandleInfo.xzLeftPicDataListCount = taskScheduleHandleInfo.xzRightPicDataListCount = xzIndex = 0;
#if newBasler
                if (testForm.GetEnable(Form.EnableEnum.单线阵))
                {
                    DoCameraCmdHandle_ContinuousShot(xzID);
                }
                if (testForm.GetEnable(Form.EnableEnum.双线阵) && xzLeftID > -1 && xzRightID > -1)
                {
                    DoCameraCmdHandle_ContinuousShot(xzLeftID);
                    DoCameraCmdHandle_ContinuousShot(xzRightID);
                }
#endif
            });
        }

        private void MzRun(string json)
        {
            TaskRun(() =>
            {
                if (RunStat)
                {
                    return;
                }
                RunStat = true;
                if (!CheckHeadLocation())
                {
                    return;
                }
#if controlSocket
                List<DataMzCameraLineModelEx> list = GetMzDataList(json);
                if (testForm.GetEnable(Form.EnableEnum.异步))
                {
                    frontMzIndex = backMzIndex = mzDistance = 0;
                    frontData.Clear();
                    backData.Clear();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!RunStat)
                        {
                            return;
                        }
                        DataMzCameraLineModel model = list[i];
                        MzDataExtent front = new MzDataExtent(model, RobotName.Front);
                        MzDataExtent back = new MzDataExtent(model, RobotName.Back);
                        frontData.Add(front);
                        backData.Add(back);
                    }
                    MzRun();
                }
                else
                {
                    int last_rgv_distance = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!RunStat)
                        {
                            AddLog("任务强制中止！");
                            return;
                        }
                        DataMzCameraLineModelEx item = list[i];
                        mzIndex = i;
                        MzRun(item, ref last_rgv_distance);
                    }
                }
#endif
            });
        }

        private void MzRun(DataMzCameraLineModel model, ref int last_rgv_distance)
        {
            try
            {
                #region 执行开始
                if (!RunStat)
                {
                    DoRobotCmdBackZeroHandle("1");
                    DoRobotCmdBackZeroHandle("2");
                    WaitRobotEnding();
                    AddLog("任务强制中止！");
                    return;
                }
                WaitCmdHandle("面阵流程 -> 准备阶段");
                mzDistance = model.Rgv_Distance;
                AddLog(JsonConvert.SerializeObject(model));

                if (last_rgv_distance != GetDistance(model))
                {
                    AddLog("收回机械臂");
                    DoRobotCmdBackZeroHandle("1");
                    DoRobotCmdBackZeroHandle("2");
                    WaitRobotEnding();
#if plcModbus
                    if (testForm.GetEnable(Form.EnableEnum.PLC))
                    {
                        try
                        {
                            if (canMovePlan)
                            {
                                pLC3DCamera.FrontModbus.Home_Complete = false;
                                pLC3DCamera.SetZero(RobotName.Front);
                                AddLog("前臂滑台归原点");
                                ThreadSleep(50);
                                pLC3DCamera.BackModbus.Home_Complete = false;
                                pLC3DCamera.SetZero(RobotName.Back);
                                AddLog("后臂滑台归原点");
                                do
                                {
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Front);
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Back);
                                } while (!pLC3DCamera.BackModbus.Home_Complete && !pLC3DCamera.FrontModbus.Home_Complete);
                                canMovePlan = false;
                            }
                        }
                        catch (Exception e)
                        {
                            AddLog("PLC滑台控制异常：" + e.Message, -1);
                        }
                    }
#endif
                }
                #endregion
                #region 运动控制
                WaitCmdHandle("面阵流程 -> 指示RGV运动");
                int loctaion = trainHeadLocation > 0 ? GetDistance(model) + trainHeadLocation : GetDistance(model) + TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ;
            move:
                if (GetDistance(model) != last_rgv_distance)
                {
                    if (!RunStat)
                    {
                        AddLog("任务强制中止！");
                        return;
                    }
                    //Rgv运动到指定位置
                    last_rgv_distance = GetDistance(model);
                    DoRgvSetTargetDistanceCmdHandle(loctaion);
                    AddLog(string.Format("Rgv运动到指定位置: {0}", loctaion));

                    //判别Rgv是否已经运动到指定位置
                    while (true)
                    {
                        if (!RunStat)
                        {
                            AddLog("任务强制中止！");
                            return;
                        }
                        if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE &&
                            RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed == 0)
                        {
                            WaitCmdHandle("Rgv停止");
                            break;
                        }
                        ThreadSleep(50);
                    }
                }
                else
                {
                    WaitCmdHandle("Rgv已停止");
                }
                #endregion
                #region 移动不足，定位补偿
#if plcModbus
                if (testForm.GetEnable(Form.EnableEnum.PLC))
                {
                    ModbusTCP.SetAddress13(Address13Type.Laser, 1);
                }
#endif
                int c = GetPositionDifference(model: model);
                if (!RunStat)
                {
                    AddLog("任务强制中止！");
                    return;
                }
                if (c > 10 || c < -10)
                {
                    AddLog("位置差：" + c);
                    loctaion = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + c;
                    last_rgv_distance = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce - TrainCurrentHeadDistance;
                    goto move;
                }
#if plcModbus
                if (testForm.GetEnable(Form.EnableEnum.PLC))
                {
                    ModbusTCP.SetAddress13(Address13Type.Laser, 0);
                }
#endif
                #endregion
                #region 激光雷达二次定位
#if lidarLocation && !lidarCsharp
#if lidarOneLoc
                if (isFirst)
                {
                    if (lidarLocation == 0)
                    {
                        lidarLocation = (int)(GetLidarLoc(model.TrainModel + "_" + model.TrainSn, model.Location, model.Point) * 1000);
                        AddLog("激光雷达定位：" + lidarLocation);
                    }
                    if (lidarLocation != 0)
                    {
                        int movePoint = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + lidarLocation;
                        if (lidarLocation > 10)
                        {
                            AddLog("位置差：" + lidarLocation);
                            DoRgvSetTargetDistanceCmdHandle(movePoint);
                            AddLog("Rgv移动到：" + movePoint);
                            while (true)
                            {
                                if (!RunStat)
                                {
                                    AddLog("任务强制中止！");
                                    return;
                                }
                                if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                                {
                                    WaitCmdHandle("Rgv停止");
                                    break;
                                }
                                ThreadSleep(50);
                            }
                            WaitCmdHandle();
                        }
                    }
                    isFirst = false;
                }
#endif
#endif
                #endregion
                #region 开关控制
                bool frontMove = true, backMove = true, frontSi = true, backSi = true, front3d = true, back3d = true, front2d = true, back2d = true, frontYwc = true, backYwc = true, frontShot = true, backShot = true;
                if (!testForm.GetEnable(Form.EnableEnum.面阵相机))
                {
                    if (model.FrontComponentType == "Mz")
                    {
                        frontMove = false;
                        frontSi = false;
                        front2d = false;
                    }
                    if (model.BackComponentType == "Mz")
                    {
                        backMove = false;
                        backSi = false;
                        back2d = false;
                    }
                }
                if (!testForm.GetEnable(Form.EnableEnum.三维相机))
                {
                    if (model.FrontComponentType == "3d")
                    {
                        frontMove = false;
                        frontSi = false;
                        front3d = false;
                    }
                    if (model.BackComponentType == "3d")
                    {
                        backMove = false;
                        backSi = false;
                        back3d = false;
                    }
                }
                if (!testForm.GetEnable(Form.EnableEnum.油位窗))
                {
                    if (model.FrontComponentType == "Ywc")
                    {
                        frontMove = false;
                        frontSi = false;
                        frontYwc = false;
                    }
                    if (model.BackComponentType == "Ywc")
                    {
                        backMove = false;
                        backSi = false;
                        backYwc = false;
                    }
                }
                if (frontMove)
                {
                    frontMove = testForm.GetEnable(Form.EnableEnum.前机械臂);
                }
                if (backMove)
                {
                    backMove = testForm.GetEnable(Form.EnableEnum.后机械臂);
                }
                if (frontSi)
                {
                    frontSi = testForm.GetEnable(Form.EnableEnum.前滑台);
                }
                if (backSi)
                {
                    backSi = testForm.GetEnable(Form.EnableEnum.后滑台);
                }
                if (frontShot)
                {
                    frontShot = testForm.GetEnable(Form.EnableEnum.前相机);
                }
                if (backShot)
                {
                    backShot = testForm.GetEnable(Form.EnableEnum.后相机);
                }
                #endregion

                WaitCmdHandle("面阵流程 -> 指示机械臂运动");
                RobotModCtrlProtocol.RobotDataPack robotDataPack = new RobotModCtrlProtocol.RobotDataPack();
                #region 前机械臂运动
                if (frontMove)
                {
                    try
                    {
                        robotDataPack.j1 = model.FrontRobot_J1;
                        robotDataPack.j2 = model.FrontRobot_J2;
                        robotDataPack.j3 = model.FrontRobot_J3;
                        robotDataPack.j4 = model.FrontRobot_J4;
                        robotDataPack.j5 = model.FrontRobot_J5;
                        robotDataPack.j6 = model.FrontRobot_J6;
                        DoRobotCmdSetDataHandle("1", robotDataPack);

                        taskMainCameraModel.FrontRobot_J1 = robotDataPack.j1;
                        taskMainCameraModel.FrontRobot_J2 = robotDataPack.j2;
                        taskMainCameraModel.FrontRobot_J3 = robotDataPack.j3;
                        taskMainCameraModel.FrontRobot_J4 = robotDataPack.j4;
                        taskMainCameraModel.FrontRobot_J5 = robotDataPack.j5;
                        taskMainCameraModel.FrontRobot_J6 = robotDataPack.j6;
                        AddLog("前臂：" + JsonConvert.SerializeObject(robotDataPack));
                    }
                    catch (Exception e)
                    {
                        AddLog("前臂异常：" + e.Message, -1);
                    } 
                }
                #endregion
                robotDataPack = new RobotModCtrlProtocol.RobotDataPack();
                #region 后机械臂运动
                if (backMove)
                {
                    try
                    {
                        robotDataPack.j1 = model.BackRobot_J1;
                        robotDataPack.j2 = model.BackRobot_J2;
                        robotDataPack.j3 = model.BackRobot_J3;
                        robotDataPack.j4 = model.BackRobot_J4;
                        robotDataPack.j5 = model.BackRobot_J5;
                        robotDataPack.j6 = model.BackRobot_J6;
                        DoRobotCmdSetDataHandle("2", robotDataPack);

                        taskMainCameraModel.BackRobot_J1 = robotDataPack.j1;
                        taskMainCameraModel.BackRobot_J2 = robotDataPack.j2;
                        taskMainCameraModel.BackRobot_J3 = robotDataPack.j3;
                        taskMainCameraModel.BackRobot_J4 = robotDataPack.j4;
                        taskMainCameraModel.BackRobot_J5 = robotDataPack.j5;
                        taskMainCameraModel.BackRobot_J6 = robotDataPack.j6;
                        AddLog("后臂：" + JsonConvert.SerializeObject(robotDataPack));
                    }
                    catch (Exception e)
                    {
                        AddLog("后臂异常：" + e.Message, -1);
                    } 
                }
                #endregion
                WaitRobotEnding();
                #region 拍照预定义
                bool frontSort = false, backSort = false;
                bool isOut = false;
                int outTime = 100;
                bool _3DRun = false;
                front3dID = model.Front3d_Id;
                back3dID = model.Back3d_Id;
                #endregion
                #region 等待拍照完成
#if shotImageProcess
                if (!string.IsNullOrEmpty(shotFrontID))
                {
                    AddLog("前臂相机照片未收到");
                }
                if (!string.IsNullOrEmpty(shotBackID))
                {
                    AddLog("后臂相机照片未收到");
                }
                int shotSleepCount = 0;
                while (!string.IsNullOrEmpty(shotBackID) || !string.IsNullOrEmpty(shotFrontID))
                {
                    if (!RunStat)
                    {
                        AddLog("任务强制中止！");
                        return;
                    }
                    ThreadSleep(100);
                    shotSleepCount++;
                }
                if (shotSleepCount > 0)
                {
                    AddLog($"等待照片时间：{(shotSleepCount * 100)}毫秒");
                }
                if (model.FrontComponentType == "Mz" || model.FrontComponentType == "Ywc")
                {
                    shotFrontID = model.FrontComponentId;
                }
                if (model.BackComponentType == "Mz" || model.BackComponentType == "Ywc")
                {
                    shotBackID = model.BackComponentId;
                }
#endif 
                #endregion
                #region 前机械臂相机
                TaskRun(() =>
                {
                    WaitCmdHandle("前臂相机开始拍照");
                    try
                    {
                        #region 3D扫描仪
                        if (model.FrontComponentType == "3d" && front3d)
                        {
                            outTime = 300;
                            front_parts_id = model.Front_Parts_Id;
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && frontSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetForward(RobotName.Front);
                                WaitCmdHandle("前臂滑台前进");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Front);
                                } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);
                            }
#endif
                            if (testForm.GetEnable(Form.EnableEnum.三维相机) && frontShot)
                            {
                                if (!_3DRun)
                                {
                                    _3DRun = true;
                                    cognexManager.Run(new int[] { front3dID, back3dID });
                                }
                                WaitCmdHandle("前3D扫描仪启动");
                            }
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && frontSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetBackoff(RobotName.Front);
                                WaitCmdHandle("前臂滑台返回");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Front);
                                } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);
                            }
                            canMovePlan = true;
#endif
                        }
                        #endregion
                        #region 油位窗
                        else if (model.FrontComponentType == "Ywc" && frontYwc)
                        {
                            outTime = 300;
                            front_parts_id = model.Front_Parts_Id;
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && frontSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetForward(RobotName.Front);
                                WaitCmdHandle("前臂滑台前进");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Front);
                                } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);
                            }
#endif
                            LightOn(RobotName.Front);
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            if (frontShot)
                            {
#if newBasler
                                DoCameraCmdHandle_OneShot(frontID);
#else
                                DoCameraCmdHandle_OneShot(taskMainCameraModel.FrontCamerainfoItem, "front", Application.StartupPath + @"\task_data\front_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + "H" + GetExtend());
#endif 
                            }
                            LightOff(RobotName.Front);
                            WaitCmdHandle("前臂相机拍照完成");
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && frontSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetBackoff(RobotName.Front);
                                WaitCmdHandle("前臂滑台返回");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Front);
                                } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);
                            }
                            canMovePlan = true;
#endif
                        }
                        #endregion
                        #region 面阵相机
                        else if (model.FrontComponentType == "Mz" && front2d)
                        {
                            WaitCmdHandle("前臂光源开启");
                            LightOn(RobotName.Front);
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            if (frontShot)
                            {
#if newBasler
                                DoCameraCmdHandle_OneShot(frontID);
#else
                                DoCameraCmdHandle_OneShot(taskMainCameraModel.FrontCamerainfoItem, "front", Application.StartupPath + @"\task_data\front_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + "H" + GetExtend());
#endif 
                            }
                            LightOff(RobotName.Front);
                            AddLog("前臂相机拍照完成");
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        AddLog("前臂拍照异常：" + e.Message, -1);
                    }
                    frontSort = true;
                });
                #endregion
                #region 后机械臂相机
                TaskRun(() =>
                {
                    WaitCmdHandle("后臂相机开始拍照");
                    try
                    {
                        #region 3D扫描仪
                        if (model.BackComponentType == "3d" && back3d)
                        {
                            outTime = 300;
                            back_parts_id = model.Back_Parts_Id;
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && backSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetForward(RobotName.Back);
                                WaitCmdHandle("后臂滑台前进");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Back);
                                } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                            }
#endif
                            if (testForm.GetEnable(Form.EnableEnum.三维相机) && backShot)
                            {
                                if (!_3DRun)
                                {
                                    _3DRun = true;
                                    cognexManager.Run(new int[] { front3dID, back3dID });
                                }
                                WaitCmdHandle("后3D扫描仪启动");
                            }
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && backSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetBackoff(RobotName.Back);
                                WaitCmdHandle("后臂滑台返回");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Back);
                                } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                            }
                            canMovePlan = true;
#endif
                        }
                        #endregion
                        #region 油位窗
                        else if (model.BackComponentType == "Ywc" && backYwc)
                        {
                            outTime = 300;
                            front_parts_id = model.Front_Parts_Id;
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && backSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetForward(RobotName.Back);
                                WaitCmdHandle("后臂滑台前进");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Back);
                                } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                            }
#endif
                            LightOn(RobotName.Back);
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            if (backShot)
                            {
#if newBasler
                                DoCameraCmdHandle_OneShot(backID);
#else
                                DoCameraCmdHandle_OneShot(taskMainCameraModel.BackCamerainfoItem, "back", Application.StartupPath + @"\task_data\back_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + "H" + GetExtend());
#endif 
                            }
                            LightOff(RobotName.Back);
                            WaitCmdHandle("后臂相机拍照完成");
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC) && backSi)
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.SetBackoff(RobotName.Back);
                                WaitCmdHandle("后臂滑台返回");
                                do
                                {
                                    if (isOut || !RunStat)
                                    {
                                        return;
                                    }
                                    ThreadSleep(plcSleep);
                                    pLC3DCamera.GetState(RobotName.Back);
                                } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                            }
                            canMovePlan = true;
#endif
                        }
                        #endregion
                        #region 面阵相机
                        else if (model.BackComponentType == "Mz" && back2d)
                        {
                            WaitCmdHandle("后臂光源开启");
                            LightOn(RobotName.Back);
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            if (backShot)
                            {
#if newBasler
                                DoCameraCmdHandle_OneShot(backID);
#else
                                DoCameraCmdHandle_OneShot(taskMainCameraModel.BackCamerainfoItem, "back", Application.StartupPath + @"\task_data\back_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + "H" + GetExtend());
#endif 
                            }
                            LightOff(RobotName.Back);
                            AddLog("后臂相机拍照完成");
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        AddLog("后臂拍照异常：" + e.Message, -1);
                    }
                    backSort = true;
                });
                #endregion
                #region 拍照等待
                if (testForm.GetEnable(Form.EnableEnum.超时))
                {
                    for (int j = 0; j < outTime; j++)
                    {
                        if (frontSort && backSort)
                        {
                            break;
                        }
                        if (!RunStat)
                        {
                            break;
                        }
                        ThreadSleep(100);
                        if (j == outTime - 1)
                        {
                            isOut = true;
                            if (!frontSort)
                            {
                                AddLog("前臂光源关闭");
                                LightOff(RobotName.Front);
                            }
                            if (!backSort)
                            {
                                AddLog("后臂光源关闭");
                                LightOff(RobotName.Back);
                            }
                            AddLog("拍照等待超时。" + (!frontSort ? "前机械臂相机未完成拍照！" : "") + (!backSort ? "后机械臂相机未完成拍照！" : ""));
                        }
                    } 
                }
                #endregion
                if (testForm.GetEnable(Form.EnableEnum.三维相机))
                {
                    AddLog("关闭3D扫描仪");
                    cognexManager.Stop(); 
                }
                ThreadSleep(100);
            }
            catch (Exception e)
            {
                AddLog("面阵点位异常：" + e.Message, -1);
            }
        }

        #region 异步控制
        private void MzRun(bool isRgvMove = true)
        {
            #region 局部变量
            bool frontRobotMove = false, backRobotMove = false;
            bool isFrontComplete = false, isBackComplete = false;
            bool frontComplete = false, backComplete = false;
            int frontAxis = 0, backAxis = 0;
            int frontDistance = 0, backDistance = 0;
            #endregion
            #region 前臂控制线程
            ThreadStart(() =>
            {
                if (frontData.Count == 0)
                {
                    return;
                }
#if shotImageProcess
                shotFrontID = "";
#endif
                axisFrontID = frontData[0].Axis_ID;
                frontAxis = frontData[0].Axis_Distance;
                frontDistance = frontData[0].Distance;
                for (int i = 0; i < frontData.Count; i++)
                {
                    if (!RunStat)
                    {
                        return;
                    }
                    MzDataExtent model = frontData[i];
#if shotImageProcess
                    while (!string.IsNullOrEmpty(shotFrontID) && shotFrontID != "0")
                    {
                        if (!RunStat)
                        {
                            return;
                        }
                        ThreadSleep(50);
                    }
                    if (model.CameraType == "3d" || model.CameraType == "Mz" || model.CameraType == "Ywc")  // 过滤过度点位
                    {
                        shotFrontID = model.OnlyID;
                    }
                    else
                    {
                        AddLog("前臂过渡点不记录总编号", 1);
                    }
                    shotFrontOutTime = 0;
                    shotFrontType = model.CameraType;
#endif
                    front3dID = model.ID_3D;
                    WaitCmdHandle(JsonConvert.SerializeObject(model), 1);
                    if (frontDistance > model.Distance)
                    {
                        frontRobotMove = false;
                        frontAxis = 0;
                        if (testForm.GetEnable(Form.EnableEnum.轴定位))
                        {
                            axisFrontID = model.Axis_ID;
                            Axis axis = AxisList?.Find(a => a.ID == model.Axis_ID);
                            if (axis != null)
                            {
                                frontAxis = model.Axis_Distance;
                                frontDistance = axis.Distance + model.Axis_Distance;
                            }
                            else
                            {
                                frontDistance = model.Distance;
                            }
                        }
                        else
                        {
                            frontDistance = model.Distance;
                        }
                        isFrontComplete = true;
                    }
                    while (!frontRobotMove)
                    {
                        ThreadSleep(1000);
                    }
                    AddLog($"当前RGV位置：{RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce} 前点位位置：{frontDistance}", 1);
                    bool robotMove = true, _2d = true, _3d = true, ywc = true, si = true, shot = true;
                    if (!testForm.GetEnable(Form.EnableEnum.面阵相机))
                    {
                        if (model.CameraType == "Mz")
                        {
                            robotMove = false;
                            si = false;
                            _2d = false;
                            AddLog("前臂：不移动机械臂，不移动滑台，不进行拍照", 1);
                        }
                    }
                    if (!testForm.GetEnable(Form.EnableEnum.三维相机))
                    {
                        if (model.CameraType == "3d")
                        {
                            robotMove = false;
                            si = false;
                            _3d = false;
                            AddLog("前臂：不移动机械臂，不移动滑台，不打开3D", 1);
                        }
                    }
                    if (!testForm.GetEnable(Form.EnableEnum.油位窗))
                    {
                        if (model.CameraType == "Ywc")
                        {
                            robotMove = false;
                            si = false;
                            ywc = false;
                            AddLog("前臂：不移动机械臂，不移动滑台，不执行油位窗", 1);
                        }
                    }
                    if (robotMove)
                    {
                        robotMove = testForm.GetEnable(Form.EnableEnum.前机械臂);
                        if (!robotMove)
                        {
                            AddLog("前臂：不移动机械臂", 1);
                        }
                    }
                    if (si)
                    {
                        si = testForm.GetEnable(Form.EnableEnum.前滑台);
                        if (!robotMove)
                        {
                            AddLog("前臂：不移动滑台", 1);
                        }
                    }
                    if (shot)
                    {
                        shot = testForm.GetEnable(Form.EnableEnum.前相机);
                        if (!robotMove)
                        {
                            AddLog("前臂：跳过所有类型拍照", 1);
                        }
                    }
                    MzRun(model, robotMove, _3d, ywc, _2d, si, shot);
                    frontMzIndex = i;
                }
                frontComplete = true;
            });
            #endregion
            #region 后臂控制线程
            ThreadStart(() =>
            {
                if (backData.Count == 0)
                {
                    return;
                }
#if shotImageProcess
                shotBackID = "";
#endif
                axisBackID = backData[0].Axis_ID;
                backAxis = backData[0].Axis_Distance;
                backDistance = backData[0].Distance;
                for (int i = 0; i < backData.Count; i++)
                {
                    if (!RunStat)
                    {
                        return;
                    }
                    MzDataExtent model = backData[i];
#if shotImageProcess
                    while (!string.IsNullOrEmpty(shotBackID) && shotBackID != "0")
                    {
                        if (!RunStat)
                        {
                            return;
                        }
                        ThreadSleep(50);
                    }
                    if (model.CameraType == "3d" || model.CameraType == "Mz" || model.CameraType == "Ywc")  // 过滤过度点位
                    {
                        shotBackID = model.OnlyID;
                    }
                    else
                    {
                        AddLog("后臂过渡点不记录总编号", 2);
                    }
                    shotBackOutTime = 0;
                    shotBackType = model.CameraType;
#endif
                    back3dID = model.ID_3D;
                    WaitCmdHandle(JsonConvert.SerializeObject(model), 2);
                    if (backDistance > model.Distance)
                    {
                        backRobotMove = false;
                        backAxis = 0;
                        if (testForm.GetEnable(Form.EnableEnum.轴定位))
                        {
                            axisBackID = model.Axis_ID;
                            Axis axis = AxisList?.Find(a => a.ID == model.Axis_ID);
                            if (axis != null)
                            {
                                backAxis = model.Axis_Distance;
                                backDistance = axis.Distance + model.Axis_Distance;
                            }
                            else
                            {
                                backDistance = model.Distance;
                            }
                        }
                        else
                        {
                            backDistance = model.Distance;
                        }
                        isBackComplete = true;
                    }
                    while (!backRobotMove)
                    {
                        ThreadSleep(1000);
                    }
                    AddLog($"当前RGV位置：{RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce} 后点位位置：{backDistance}",2);
                    bool robotMove = true, _2d = true, _3d = true, ywc = true, si = true, shot = true;
                    if (!testForm.GetEnable(Form.EnableEnum.面阵相机))
                    {
                        if (model.CameraType == "Mz")
                        {
                            robotMove = false;
                            si = false;
                            _2d = false;
                            AddLog("后臂：不移动机械臂，不移动滑台，不进行拍照", 2);
                        }
                    }
                    if (!testForm.GetEnable(Form.EnableEnum.三维相机))
                    {
                        if (model.CameraType == "3d")
                        {
                            robotMove = false;
                            si = false;
                            _3d = false;
                            AddLog("后臂：不移动机械臂，不移动滑台，不打开3D", 2);
                        }
                    }
                    if (!testForm.GetEnable(Form.EnableEnum.油位窗))
                    {
                        if (model.CameraType == "Ywc")
                        {
                            robotMove = false;
                            si = false;
                            ywc = false;
                            AddLog("后臂：不移动机械臂，不移动滑台，不执行油位窗", 2);
                        }
                    }
                    if (robotMove)
                    {
                        robotMove = testForm.GetEnable(Form.EnableEnum.后机械臂);
                        if (!robotMove)
                        {
                            AddLog("后臂：不移动机械臂", 2);
                        }
                    }
                    if (si)
                    {
                        si = testForm.GetEnable(Form.EnableEnum.后滑台);
                        if (!robotMove)
                        {
                            AddLog("后臂：不移动滑台", 2);
                        }
                    }
                    if (shot)
                    {
                        shot = testForm.GetEnable(Form.EnableEnum.后相机);
                        if (!robotMove)
                        {
                            AddLog("后臂：跳过所有类型拍照", 2);
                        }
                    }
                    MzRun(model, robotMove, _3d, ywc, _2d, si, shot);
                    backMzIndex = i;
                }
                backComplete = true;
            });
            #endregion
            #region rgv控制
            if (isRgvMove)
            {
                do
                {
                    WaitCmdHandle();
                    if (!RunStat)
                    {
                        AddLog("任务强制中止！");
                        return;
                    }

                    int axis = 0;
                    if (frontDistance > backDistance)
                    {
                        axis = frontAxis;
                        axisBackID = 0;
                        mzDistance = frontDistance;
                    }
                    else
                    {
                        axis = backAxis;
                        axisFrontID = 0;
                        mzDistance = backDistance;
                    }
                    if (mzDistance > 0)
                    {
                        if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP && RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed == 0)
                        {
                            AddLog("RGV状态异常：RGV速度为0，但状态仍在运动");
                        }
                        else if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor == eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP && RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed == 0)
                        {
                            AddLog("RGV已停车");
#if plcModbus
                            if (testForm.GetEnable(Form.EnableEnum.PLC))
                            {
                                ModbusTCP.SetAddress13(Address13Type.Laser, 1); 
                            }
#endif
                        Location:
                            bool reLocation = true;
                            int c = 0;
                            if (testForm.GetEnable(Form.EnableEnum.轴定位))
                            {
                                if (axisFrontID > 0 && axisDict.ContainsKey(axisFrontID))
                                {
                                    if (axisDict[axisFrontID] > 0)
                                    {
                                        c = axisDict[axisFrontID] + TrainCurrentHeadDistance + axis - RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
                                        if (trainHeadLocation == 0)
                                        {
                                            c += Properties.Settings.Default.RobotPointXZ;
                                        }
                                        reLocation = false;
                                    }
                                }
                                else if (axisBackID > 0 && axisDict.ContainsKey(axisBackID))
                                {
                                    if (axisDict[axisBackID] > 0)
                                    {
                                        c = axisDict[axisBackID] + TrainCurrentHeadDistance + axis - RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
                                        if (trainHeadLocation == 0)
                                        {
                                            c += Properties.Settings.Default.RobotPointXZ;
                                        }
                                        reLocation = false;
                                    }
                                }
                            }
                            if (reLocation)
                            {
                                c = GetPositionDifference();
                            }
                            if (!RunStat)
                            {
                                AddLog("任务强制中止！");
                                return;
                            }
                            if (c > 10 || c < -10)
                            {
                                AddLog("位置差：" + c);
                                int loctaion = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + c;
                                DoRgvSetTargetDistanceCmdHandle(loctaion);
                                WaitCmdHandle(string.Format("Rgv运动到指定位置: {0}", loctaion));
#if plcModbus
                                if (testForm.GetEnable(Form.EnableEnum.PLC))
                                {
                                    pLC3DCamera.SetZero(RobotName.Front);
                                    AddLog("前臂滑台归原点");
                                    ThreadSleep(50);
                                    pLC3DCamera.SetZero(RobotName.Back);
                                    AddLog("后臂滑台归原点");
                                }
#endif
                            }
                            else
                            {
                                if (testForm.GetEnable(Form.EnableEnum.轴定位))
                                {
                                    if (axisFrontID > 0 && axisDict.ContainsKey(axisFrontID))
                                    {
                                        if (axisDict[axisFrontID] == 0)
                                        {
                                            axisDict[axisFrontID] = AxisList.Find(a => a.ID == axisFrontID).Distance;
                                            axisDict[axisFrontID] += GetPositionDifference();
                                            goto Location;
                                        }
                                    }
                                    else if (axisBackID > 0 && axisDict.ContainsKey(axisBackID))
                                    {
                                        if (axisDict[axisBackID] == 0)
                                        {
                                            axisDict[axisBackID] = AxisList.Find(a => a.ID == axisBackID).Distance;
                                            axisDict[axisBackID] += GetPositionDifference();
                                            goto Location;
                                        }
                                    }
                                }

#if plcModbus
                                if (testForm.GetEnable(Form.EnableEnum.PLC))
                                {
                                    ModbusTCP.SetAddress13(Address13Type.Laser, 0);
                                }
#endif
                                frontRobotMove = backRobotMove = true;
                                isFrontComplete = isBackComplete = false;
                                while (!isFrontComplete || !isBackComplete)
                                {
                                    if (!RunStat)
                                    {
                                        AddLog("任务强制中止！");
                                        return;
                                    }
                                    if (frontComplete && backComplete)
                                    {
                                        AddLog("所有点位已经完成");
                                        break;
                                    }
                                    ThreadSleep(1000);
                                }
                                AddLog("前后点位已经完成，位置：" + mzDistance);
                            }
                        }
                    }
                    ThreadSleep(1000);
                } while (!frontComplete || !backComplete);
            }
            else
            {
                do
                {
                    if (!RunStat)
                    {
                        return;
                    }
                    frontRobotMove = backRobotMove = true;
                    ThreadSleep(1000);
                } while (!frontComplete || !backComplete);
            }
            #endregion
        }
        private void MzRun(MzDataExtent model, bool robotMove, bool _3dScan, bool ywc, bool _2d, bool si, bool shot)
        {
            #region 机械臂运动
            int logType = 0;
            if (robotMove)
            {
                switch (model.Robot)
                {
                    case RobotName.Front:
                        logType = 1;
                        DoRobotCmdSetDataHandle("1", model.RobotData);
                        taskMainCameraModel.FrontRobot_J1 = model.RobotData.j1;
                        taskMainCameraModel.FrontRobot_J2 = model.RobotData.j2;
                        taskMainCameraModel.FrontRobot_J3 = model.RobotData.j3;
                        taskMainCameraModel.FrontRobot_J4 = model.RobotData.j4;
                        taskMainCameraModel.FrontRobot_J5 = model.RobotData.j5;
                        taskMainCameraModel.FrontRobot_J6 = model.RobotData.j6;
                        AddLog("前臂：" + JsonConvert.SerializeObject(model.RobotData), logType);
                        break;
                    case RobotName.Back:
                        logType = 2;
                        DoRobotCmdSetDataHandle("2", model.RobotData);
                        taskMainCameraModel.BackRobot_J1 = model.RobotData.j1;
                        taskMainCameraModel.BackRobot_J2 = model.RobotData.j2;
                        taskMainCameraModel.BackRobot_J3 = model.RobotData.j3;
                        taskMainCameraModel.BackRobot_J4 = model.RobotData.j4;
                        taskMainCameraModel.BackRobot_J5 = model.RobotData.j5;
                        taskMainCameraModel.BackRobot_J6 = model.RobotData.j6;
                        AddLog("后臂：" + JsonConvert.SerializeObject(model.RobotData), logType);
                        break;
                }
                WaitRobotEnding(model.Robot); 
            }
            #endregion
            string s = model.Robot == RobotName.Front ? "前" : "后";
            if (model.Robot == RobotName.Front ? string.IsNullOrEmpty(shotFrontID) : string.IsNullOrEmpty(shotBackID))
            {
                AddLog(s + "臂过渡点跳过拍照", logType);
                return;  // 跳过过渡点
            }
            WaitCmdHandle(s + "臂相机开始拍照", logType);
            try
            {
                #region 3D扫描仪
                if (model.CameraType == "3d" && _3dScan)
                {
                _3dShot:
                    while (model.Robot == RobotName.Front ? isRelinkShot_Front : isRelinkShot_Back)
                    {
                        ThreadSleep(500);
                    }
                    switch (model.Robot)
                    {
                        case RobotName.Front:
                            shotFrontID = model.OnlyID;
                            break;
                        case RobotName.Back:
                            shotBackID = model.OnlyID;
                            break;
                    }
                    if (!si)
                    {
                        AddLog("跳过" + s + "滑台，并同时跳过3D扫描仪[" + model.OnlyID + "]", logType);
                        return;
                    }
#if plcModbus
                    if (testForm.GetEnable(Form.EnableEnum.PLC))
                    {
                        SetOutTimeZero(model.Robot);
                        do
                        {
                            ThreadSleep(plcSleep);
                            pLC3DCamera.GetState(model.Robot);
                            if (SlidingOutTime(model.Robot))
                            {
                                AddLog(s + "滑台归零超时", (int)model.Robot);
                                goto _3dShot;
                            }
                        } while (model.Robot == RobotName.Back ?
                                !pLC3DCamera.BackModbus.Home_Complete :
                                !pLC3DCamera.FrontModbus.Home_Complete);
                    }
#endif
                    switch (model.Robot)
                    {
                        case RobotName.Front:
                            front_parts_id = model.ID;
                            shotFrontOutTime = 0;
                            break;
                        case RobotName.Back:
                            back_parts_id = model.ID;
                            shotBackOutTime = 0;
                            break;
                    }
#if plcModbus
                    if (testForm.GetEnable(Form.EnableEnum.PLC))
                    {
                        pLC3DCamera.SetForward(model.Robot);
                        WaitCmdHandle(s + "臂滑台前进", logType);
                        do
                        {
                            if (!RunStat)
                            {
                                return;
                            }
                            ThreadSleep(plcSleep);
                            pLC3DCamera.GetState(model.Robot);
                            if (SlidingOutTime(model.Robot))
                            {
                                AddLog(s + "滑台前进超时", (int)model.Robot);
                                goto _3dShot;
                            }
                        } while (model.Robot == RobotName.Front ?
                                !pLC3DCamera.FrontModbus.MoveAbsolute_Complete :
                                !pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                    }
#endif
                    SetOutTimeZero(model.Robot);
                    lock (_3dScanLock)
                    {
                        if (testForm.GetEnable(Form.EnableEnum.三维相机) && shot)
                        {
                            WaitCmdHandle(s + "3D扫描仪启动", logType);
                            cognexManager.Run(model.ID_3D, (int)model.Robot);
                            AddLog(s + "3D扫描仪已启动", logType);
                        }
#if plcModbus
                        if (testForm.GetEnable(Form.EnableEnum.PLC))
                        {
                            pLC3DCamera.SetBackoff(model.Robot);
                            WaitCmdHandle(s + "臂滑台返回", logType);
                            do
                            {
                                if (!RunStat)
                                {
                                    return;
                                }
                                ThreadSleep(plcSleep);
                                pLC3DCamera.GetState(model.Robot);
                                if (SlidingOutTime(model.Robot))
                                {
                                    AddLog(s + "滑台返回超时", (int)model.Robot);
                                    goto _3dShot;
                                }
                            } while (model.Robot == RobotName.Front ?
                                    !pLC3DCamera.FrontModbus.MoveAbsolute_Complete :
                                    !pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                        }
#endif
                        if (testForm.GetEnable(Form.EnableEnum.三维相机))
                        {
                            if (ShotOutTime(model.Robot, 300))
                            {
                                AddLog(s + "3D扫描仪超时", logType);
                                cognexManager.Stop();
                                goto _3dShot;
                            }
                            AddLog("关闭" + s + "3D扫描仪", logType);
                            cognexManager.Stop(); 
                        }
                        else
                        {
                            switch (model.Robot)
                            {
                                case RobotName.Front:
                                    shotFrontID = "";
                                    break;
                                case RobotName.Back:
                                    shotBackID = "";
                                    break;
                            } 
                        }
                    }
                }
                #endregion
                #region 油位窗
                else if (model.CameraType == "Ywc" && ywc)
                {
#if plcModbus
                    if (testForm.GetEnable(Form.EnableEnum.PLC) && si)
                    {
                        SetOutTimeZero(model.Robot);
                        do
                        {
                            ThreadSleep(plcSleep);
                            pLC3DCamera.GetState(model.Robot);
                            if (SlidingOutTime(model.Robot))
                            {
                                AddLog(s + "滑台归零超时", (int)model.Robot);
                                break;
                            }
                        } while (model.Robot == RobotName.Back ?
                                !pLC3DCamera.BackModbus.Home_Complete :
                                !pLC3DCamera.FrontModbus.Home_Complete);
                    }
#endif
                    switch (model.Robot)
                    {
                        case RobotName.Front:
                            front_parts_id = model.ID;
                            shotFrontOutTime = 0;
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            break;
                        case RobotName.Back:
                            back_parts_id = model.ID;
                            shotBackOutTime = 0;
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            break;
                    }
#if plcModbus
                Front:
                    while (model.Robot == RobotName.Front ? isRelinkShot_Front : isRelinkShot_Back)
                    {
                        ThreadSleep(500);
                    }
                    if (testForm.GetEnable(Form.EnableEnum.PLC) && si)
                    {
                        SetOutTimeZero(model.Robot);
                        pLC3DCamera.SetForward(model.Robot);
                        WaitCmdHandle(s + "臂滑台前进", logType);
                        do
                        {
                            if (!RunStat)
                            {
                                return;
                            }
                            ThreadSleep(plcSleep);
                            pLC3DCamera.GetState(model.Robot);
                            if (SlidingOutTime(model.Robot))
                            {
                                AddLog(s + "滑台前进超时", (int)model.Robot);
                                goto Front;
                            }
                        } while (model.Robot == RobotName.Front ?
                                !pLC3DCamera.FrontModbus.MoveAbsolute_Complete :
                                !pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                    }
#endif
                YwcShot:
                    while (model.Robot == RobotName.Front ? isRelinkShot_Front : isRelinkShot_Back)
                    {
                        ThreadSleep(500);
                    }
                    switch (model.Robot)
                    {
                        case RobotName.Front:
                            shotFrontID = model.OnlyID;
                            break;
                        case RobotName.Back:
                            shotBackID = model.OnlyID;
                            break;
                    }
                    ThreadSleep(1000);
                    SetOutTimeZero(model.Robot);
                    LightOn(model.Robot);
                    if (shot)
                    {
#if newBasler
                        DoCameraCmdHandle_OneShot(model.Robot == RobotName.Front ? frontID : backID);
#endif 
                    }
                    if (ShotOutTime(model.Robot))
                    {
                        AddLog(s + "相机拍照超时", logType);
                        goto YwcShot;
                    }
                    AddLog(s + "臂光源关闭", logType);
                    LightOff(model.Robot);
                    WaitCmdHandle(s + "臂相机拍照完成", logType);
#if plcModbus
                Back:
                    while (model.Robot == RobotName.Front ? isRelinkShot_Front : isRelinkShot_Back)
                    {
                        ThreadSleep(500);
                    }
                    if (testForm.GetEnable(Form.EnableEnum.PLC) && si)
                    {
                        SetOutTimeZero(model.Robot);
                        pLC3DCamera.SetBackoff(model.Robot);
                        WaitCmdHandle(s + "臂滑台返回", logType);
                        do
                        {
                            if (!RunStat)
                            {
                                return;
                            }
                            ThreadSleep(plcSleep);
                            pLC3DCamera.GetState(model.Robot);
                            if (SlidingOutTime(model.Robot))
                            {
                                AddLog(s + "滑台返回超时", (int)model.Robot);
                                goto Back;
                            }
                        } while (model.Robot == RobotName.Front ?
                                !pLC3DCamera.FrontModbus.MoveAbsolute_Complete :
                                !pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                    }
                    AddLog(s + "臂相机油位窗拍照完成", logType);
#endif
                }
                #endregion
                #region 面阵相机
                else if (model.CameraType == "Mz" && _2d)
                {
                MzShot:
                    while (model.Robot == RobotName.Front ? isRelinkShot_Front : isRelinkShot_Back)
                    {
                        ThreadSleep(500);
                    }
                    switch (model.Robot)
                    {
                        case RobotName.Front:
                            shotFrontID = model.OnlyID;
                            break;
                        case RobotName.Back:
                            shotBackID = model.OnlyID;
                            break;
                    }
                    ThreadSleep(1000);
                    WaitCmdHandle(s + "臂光源开启", logType);
                    LightOn(model.Robot);
                    switch (model.Robot)
                    {
                        case RobotName.Front:
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            break;
                        case RobotName.Back:
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                            break;
                    }
                    if (shot)
                    {
#if newBasler
                        DoCameraCmdHandle_OneShot(model.Robot == RobotName.Front ? frontID : backID);
#endif 
                    }
                    if (ShotOutTime(model.Robot))
                    {
                        AddLog(s + "相机拍照超时", logType);
                        goto MzShot;
                    }
                    AddLog(s + "臂光源关闭", logType);
                    LightOff(model.Robot);
                    AddLog(s + "臂相机拍照完成", logType);
                }
                #endregion
                else
                {
                    switch (model.Robot)
                    {
                        case RobotName.Front:
                            shotFrontID = front_parts_id = "";
                            break;
                        case RobotName.Back:
                            shotBackID = back_parts_id = "";
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                AddLog(s + "臂拍照异常：" + e.Message, -1);
            }
        }
        private void WaitRobotEnding(RobotName name)
        {
            int count = 0;
            switch (name)
            {
                case RobotName.Front:
                    AddLog("等待前机械臂停止运动。", 1);
                    while (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                    {
                        if (RobotModCtrlHelper.isServer && !RobotModCtrlHelper.GetInstance()[RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID])
                        {
                            AddLog("前机械臂掉线", 1);
                            break;
                        }
                        if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP ||
                            RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY)
                        {
                            AddLog("前机械臂停止", 1);
                            RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
                            break;
                        }
                        ThreadSleep(50);
                        count++;
                    }
                    AddLog("前机械臂等待时间：" + count * 50 + "ms", 1);
                    break;
                case RobotName.Back:
                    AddLog("等待后机械臂停止运动。", 2);
                    while (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                    {
                        if (RobotModCtrlHelper.isServer && !RobotModCtrlHelper.GetInstance()[RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID])
                        {
                            AddLog("后机械臂掉线", 2);
                            break;
                        }
                        if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP ||
                            RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY)
                        {
                            AddLog("后机械臂停止", 2);
                            RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
                            break;
                        }
                        ThreadSleep(50);
                        count++;
                    }
                    AddLog("后机械臂等待时间：" + count * 50 + "ms", 2);
                    break;
            }
        }
        #endregion

        private int GetDistance(DataMzCameraLineModel model)
        {
            if (testForm.GetEnable(Form.EnableEnum.轴定位))
            {
                DataMzCameraLineModelEx item = model as DataMzCameraLineModelEx;
                int id = item.AxisID;
                Axis axis = AxisList?.Find(a => a.ID == id);
                if (axis != null)
                {
                    return axis.Distance + item.Axis_Distance;
                }
                return 0;
            }
            else
            {
                return model.Rgv_Distance;
            }
        }

        private int GetPositionDifference(bool isMove = true, DataMzCameraLineModel model = null, params int[] state)
        {
            if (!isMove)
            {
                return 0;
            }
            bool pingServer = true;
#if ping
            pingServer = pingResult[uploadServerKey];
#endif
            if (testForm.GetEnable(Form.EnableEnum.轴定位))
            {
                if (!pingServer && testForm.GetEnable(Form.EnableEnum.传图服务))
                {
                    if (socketRunStart)
                    {
                        robotAxisError = true;
#if controlSocket
                        SokcetExeWrite(SocketCmd.robot_axis_error, $"{Properties.Settings.Default.RobotID},-10001");
                        do
                        {
                            if (!RunStat)
                            {
                                return 0;
                            }
                            ThreadSleep(1000);
                        } while (robotAxisError);
                        AddLog("Socket继续定轴（忽略服务异常）...");
#endif 
                    }
                    else
                    {
                        if (MessageBox.Show("定轴服务异常，是否继续？", "重定位", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            AddLog("继续定轴...");
                        }
                        else
                        {
                            AddLog("定轴失败，终止任务");
                            DoOneKeyStopCmdHandle();
                            return 0;
                        }
                    }
                }
                if (axisFrontID > 0 && axisDict.ContainsKey(axisFrontID))
                {
                    if (axisDict[axisFrontID] == 0)
                    {
                        int loc = AxisList.Find(a => a.ID == axisFrontID).Distance - RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + TrainCurrentHeadDistance;
                        if (trainHeadLocation == 0)
                        {
                            loc += Properties.Settings.Default.RobotPointXZ;
                        }
                        return loc;
                    }
                }
                else if (axisBackID > 0 && axisDict.ContainsKey(axisBackID))
                {
                    if (axisDict[axisBackID] == 0)
                    {
                        int loc = AxisList.Find(a => a.ID == axisBackID).Distance - RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + TrainCurrentHeadDistance;
                        if (trainHeadLocation == 0)
                        {
                            loc += Properties.Settings.Default.RobotPointXZ;
                        }
                        return loc;
                    }
                }

            Location:
                string savePath, name1, name2;
                if (PointShot(out savePath, out name1, out name2))
                {
                    try
                    {
                        picService.UploadParameter(DkamHelper.Instance.Getkc(), DkamHelper.Instance.Getkk(), Properties.Settings.Default.RobotID);
                        AddLog("上传点云相机内参成功");
                    }
                    catch (Exception)
                    {
                        AddLog("上传点云相机内参失败");
#if ping
                        pingResult[uploadServerKey] = false;
#endif
                    }
                    if (!CheckServer())
                    {
                        return 0;
                    }
                    if (testForm.GetEnable(Form.EnableEnum.模拟流程))
                    {
                        savePath = string.Format(@"{0}\test_data\", Application.StartupPath);
                        bool setName1 = false, setName2 = false;
                        string[] files = Directory.GetFiles(savePath);
                        foreach (string file in files)
                        {
                            if (file.Contains("red") && file.Contains("bmp"))
                            {
                                name1 = file.Substring(file.IndexOf("red"));
                                setName1 = true;
                            }
                            if (file.Contains("depth") && file.Contains("png"))
                            {
                                name2 = file.Substring(file.IndexOf("depth"));
                                setName2 = true;
                            }
                            if (setName1 && setName2)
                            {
                                break;
                            }
                        }
                        if (!setName1 || !setName2)
                        {
                            return 0;
                        }
                    }
                    using (FileStream file = new FileStream(savePath + name1, FileMode.Open))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, bytes.Length);
                        UploadImage(bytes, name1);
                        AddLog("上传红外图片完成");
                    }
                    if (!CheckServer())
                    {
                        return 0;
                    }
                    using (FileStream file = new FileStream(savePath + name2, FileMode.Open))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, bytes.Length);
                        UploadImage(bytes, name2);
                        AddLog("上传深度图片完成");
                    }
                    if (!CheckServer())
                    {
                        return 0;
                    }
                    try
                    {
                        int loc = 0;
                        if (state == null || state.Length == 0)
                        {
                            loc = GetLocation(name1, name2, "");
                            AddLog("得到轴位置偏差：" + loc + "(" + (loc + Properties.Settings.Default.PointCameraXZ) + ")"); 
                        }
                        else
                        {
                            foreach (int item in state)
                            {
                                loc = GetLocation(name1, name2, "", item);
                                AddLog("得到轴位置偏差：" + loc + "(" + (loc + Properties.Settings.Default.PointCameraXZ) + ")");
                            }
                        }
                        if (loc == -10000)
                        {
                            if (socketRunStart)
                            {
                                robotAxisError = true;
#if controlSocket
                                SokcetExeWrite(SocketCmd.robot_axis_error, $"{Properties.Settings.Default.RobotID},-10000");
                                do
                                {
                                    if (!RunStat)
                                    {
                                        return 0;
                                    }
                                    ThreadSleep(1000);
                                } while (robotAxisError);
                                AddLog("Socket重新定轴...");
                                goto Location;
#endif
                            }
                            else
                            {
                                if (MessageBox.Show("是否重新定轴？", "重定位", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    AddLog("重新定轴...");
                                    goto Location;
                                }
                                else
                                {
                                    AddLog("定轴失败，终止任务");
                                    DoOneKeyStopCmdHandle();
                                    return 0;
                                } 
                            }
                        }
                        loc += Properties.Settings.Default.PointCameraXZ;
                        if (model != null)
                        {
                            DataMzCameraLineModelEx mz = model as DataMzCameraLineModelEx;
                            int distance = mz.Axis_Distance;
                            if (Math.Abs(loc - distance) < 10)
                            {
                                return 0;
                            }
                        }
                        return loc;
                    }
                    catch (Exception e)
                    {
                        AddLog(">> [8] 定轴异常：" + e.Message, -1);
                        if (socketRunStart)
                        {
                            robotAxisError = true;
#if controlSocket
                            SokcetExeWrite(SocketCmd.robot_axis_error, $"{Properties.Settings.Default.RobotID},-10000");
                            do
                            {
                                if (!RunStat)
                                {
                                    return 0;
                                }
                                ThreadSleep(1000);
                            } while (robotAxisError);
                            AddLog("Socket重新定轴...");
                            goto Location;
#endif
                        }
                        else
                        {
                            if (MessageBox.Show("是否重新定轴？", "重定位", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                AddLog("重新定轴...");
                                goto Location;
                            }
                            else
                            {
                                AddLog("定轴失败，终止任务");
                                DoOneKeyStopCmdHandle();
                                return 0;
                            }
                        }
                    }
                }
                else
                {
                    AddLog("点云相机拍照失败");
                    DepthReStart();
                    goto Location;
                }
            }
            else if (testForm.GetEnable(Form.EnableEnum.点云图))
            {
                PointShot(out _, out _, out _);
            }
            if (trainHeadLocation == 0)
            {
                return mzDistance + TrainCurrentHeadDistance + Properties.Settings.Default.RobotPointXZ - RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
            }
            else
            {
                return mzDistance + trainHeadLocation - RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;
            }
        }

        private bool CheckServer()
        {
#if ping
            if (!pingResult[uploadServerKey])
            {
                if (socketRunStart)
                {
                    robotAxisError = true;
#if controlSocket
                    SokcetExeWrite(SocketCmd.robot_axis_error, $"{Properties.Settings.Default.RobotID},-10001");
                    do
                    {
                        if (!RunStat)
                        {
                            return false;
                        }
                        ThreadSleep(1000);
                    } while (robotAxisError);
                    AddLog("Socket继续定轴（忽略服务异常）...");
#endif
                }
                else
                {
                    if (MessageBox.Show("定轴服务异常，是否继续？", "重定位", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        AddLog("继续定轴...");
                    }
                    else
                    {
                        AddLog("定轴失败，终止任务");
                        DoOneKeyStopCmdHandle();
                        return false;
                    }
                }
            }
#endif
            return true;
        }

        private bool PointShot(out string savePath, out string name1, out string name2)
        {
            if (!DkamHelper.Instance.IsInit)
            {
#if plcModbus
                if (testForm.GetEnable(Form.EnableEnum.PLC))
                {
                    ModbusTCP.SetAddress13(Address13Type._3D, 1);
                    AddLog("点云相机上电中，等待10秒");
                    ThreadSleep(10000);  
                }
#endif
                DkamHelper.Instance.Addlog = AddLog;
                DkamHelper.Instance.ReLinkCameraFaid = DepthRelinkFaid;
                DkamHelper.Instance.FindCamera();
                DkamHelper.Instance.Init(Properties.Settings.Default.PointCamera, true, true);
            }
            if (DkamHelper.Instance.IsInit)
            {
                int id = axisFrontID > axisBackID ? axisFrontID : axisBackID;
                DkamHelper.Instance.RefreshInfraRed();
                DkamHelper.Instance.RefreshPointCloud();
                savePath = Application.StartupPath + @"\task_data\";
                name1 = "red" + id + ".bmp";
                name2 = "depth" + id + ".png";
                bool exp = DkamHelper.Instance.InfraRedShot(savePath + name1) &&
                           DkamHelper.Instance.PointCloudShot(savePath + "point" + id + ".pcd", savePath + name2);
                ThreadSleep(100);
                return exp;
            }
            savePath = name1 = name2 = "";
            return false;
        }
        #endregion

        #region 超时相关
        private bool DepthRelinkFaid()
        {
            axisOutTimeAlarm = true;
#if controlSocket
            SokcetExeWrite(SocketCmd.robot_axis_outtime_alarm, Properties.Settings.Default.RobotID);
#endif
            if (socketRunStart)
            {
                while (axisOutTimeAlarm)
                {
                    if (!RunStat)
                    {
                        return false;
                    }
                    ThreadSleep(1000);
                }
                AddLog("业务端清除报警，自动重连点云相机并拍照");
                DepthReStart();
                return true;
            }
            else
            {
                if (MessageBox.Show("点云相机重连超时，是否重新连接点云相机？","询问", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    AddLog("手动清除报警，自动重连点云相机并拍照");
                    DepthReStart();
                    return true;
                }
            }
            return false;
        }

        private void DepthReStart()
        {
#if plcModbus
            if (testForm.GetEnable(Form.EnableEnum.PLC))
            {
                ModbusTCP.SetAddress13(Address13Type._3D, 0);
                AddLog("点云相机断电中，等待3秒");
                ThreadSleep(3000);
                ModbusTCP.SetAddress13(Address13Type._3D, 1);
                AddLog("点云相机上电中，等待10秒");
                ThreadSleep(10000);
                DkamHelper.Instance.Close();
                DkamHelper.Instance.FindCamera();
                DkamHelper.Instance.Init(Properties.Settings.Default.PointCamera, true, true);
            } 
#endif
        }

        private bool ShotOutTime(RobotName name, int addOutTime = 0)
        {
            if (!testForm.GetEnable(Form.EnableEnum.超时))
            {
                return false;
            }
            int max = shotOutTimeMax + addOutTime;
            while ((name == RobotName.Back ? shotBackOutTime : shotFrontOutTime) < max &&
                   (name == RobotName.Back ? shotBackOutTime : shotFrontOutTime) >= 0)
            {
                if (!RunStat)
                {
                    return false;
                }
                switch (name)
                {
                    case RobotName.Front:
                        shotFrontOutTime++;
                        break;
                    case RobotName.Back:
                        shotBackOutTime++;
                        break;
                }
                ThreadSleep(50);
            }
            if ((name == RobotName.Back ? shotBackOutTime : shotFrontOutTime) < 0)
            {
                return false;
            }
            switch (name)
            {
                case RobotName.Front:
                    IsFrontRelinkShot = true;
                    shotFrontOutTime = 0;
                    break;
                case RobotName.Back:
                    IsBackRelinkShot = true;
                    shotBackOutTime = 0;
                    break;
            }
#if controlSocket
            string part = name == RobotName.Front ? front_parts_id : back_parts_id;
            string only = name == RobotName.Front ? shotFrontID : shotBackID;
            SokcetExeWrite(SocketCmd.robot_outtime_alarm, $"{Properties.Settings.Default.RobotID},{part},{only},{(int)name}");
#endif
            return true;
        }

        private bool SlidingOutTime(RobotName name)
        {
            if (!testForm.GetEnable(Form.EnableEnum.超时))
            {
                return false;
            }
            if ((name == RobotName.Back ? shotBackOutTime : shotFrontOutTime) < slidingOutTimeMax &&
                (name == RobotName.Back ? shotBackOutTime : shotFrontOutTime) >= 0)
            {
                switch (name)
                {
                    case RobotName.Front:
                        shotFrontOutTime++;
                        return false;
                    case RobotName.Back:
                        shotBackOutTime++;
                        return false;
                }
            }
            if ((name == RobotName.Back ? shotBackOutTime : shotFrontOutTime) < 0)
            {
                return false;
            }
            switch (name)
            {
                case RobotName.Front:
                    IsFrontRelinkShot = true;
                    break;
                case RobotName.Back:
                    IsBackRelinkShot = true;
                    break;
            }
#if controlSocket
            string part = name == RobotName.Front ? front_parts_id : back_parts_id;
            string only = name == RobotName.Front ? shotFrontID : shotBackID;
            SokcetExeWrite(SocketCmd.robot_outtime_alarm, $"{Properties.Settings.Default.RobotID},{part},{only},{(int)name}");
#endif
            return true;
        }

        private void SetOutTimeZero(RobotName name)
        {
            if (!testForm.GetEnable(Form.EnableEnum.超时))
            {
                return;
            }
            switch (name)
            {
                case RobotName.Front:
                    shotFrontOutTime = 0;
                    break;
                case RobotName.Back:
                    shotBackOutTime = 0;
                    break;
            }
        }

        private void XzShotOutTime(bool xz = false, bool leftXz = false, bool rightXz = false)
        {
            if (testForm.GetEnable(Form.EnableEnum.超时))
            {
                Action action = () =>
                {
                    DoOneKeyStopCmdHandle();
                    ThreadSleep(1000);
                    DoOneKeyStopCmdHandle();
                    ThreadSleep(5000);
#if newBasler
                    AddLog("相机重新初始化");
                    do
                    {
                        if (cameraManager != null)
                        {
                            for (int i = 0; i < cameraManager.Cameras.Count; i++)
                            {
                                cameraManager.Cameras[i].Close();
                            }
                            cameraManager.Close();
                        }
                        InitCameraMod();
                        ThreadSleep(1000);
                    } while (GetCameraState());
                    CameraOpen();
#endif
                };
                ThreadStart(() =>
                {
                    while (RunStat && !RunMz && (
                            (xz ? xzOutTimeCount >= 0 && xzOutTimeCount < 50 : false) ||
                            (leftXz ? xzLeftOutTimeCount >= 0 && xzLeftOutTimeCount < 50 : false) ||
                            (rightXz ? xzRightOutTimeCount >= 0 && xzRightOutTimeCount < 50 : false)))
                    {
                        if (xz)
                        {
                            xzOutTimeCount++;
                            if (xzOutTimeCount >= 50)
                            {
                                AddLog("线阵拍照超时");
                                action();
                                break;
                            }
                        }
                        else if (leftXz)
                        {
                            xzLeftOutTimeCount++;
                            if (xzLeftOutTimeCount >= 50)
                            {
                                AddLog("左线阵拍照超时");
                                action();
                                break;
                            }
                        }
                        else if (rightXz)
                        {
                            xzRightOutTimeCount++;
                            if (xzRightOutTimeCount >= 50)
                            {
                                AddLog("右线阵拍照超时");
                                action();
                                break;
                            }
                        }
                        ThreadSleep(100);
                    }
                }); 
            }
        }
        #endregion

        #region 雷达相关
        private double GetLidarLoc(string train_type, string id, string map)
        {
            double loc = 0d;
#if lidarCsharp
            LidarType lidarType = LidarType.A2;
            try
            {
                lidarType = (LidarType)Enum.Parse(typeof(LidarType), Properties.Settings.Default.LidarType);
            }
            catch (Exception e)
            {
                AddLog("雷达类型异常：" + e.Message, -1);
                return loc;
            }
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    lidar = Lidar.Instance(lidarType, logInit: AddLog);
                    lidar.Start();
                    ThreadSleep(5000);
                    lidar.End();
                    loc = lidar.GetTrainHead();
                }
                catch (Exception e)
                {
                    AddLog("雷达异常：" + e.Message, -1);
                    LidarControl(i + 1);
                    continue;
                }
                if (loc == 0)
                {
                    ThreadSleep(500);
                    AddLog("车头检测位置为0，重新检测", 3);
                    LidarControl(i + 1);
                    continue;
                }
                else if (Math.Abs(upCheckHead - loc) < 100)
                {
                    ThreadSleep(500);
                    AddLog("车头检测位置与上次检测位置较近，重新检测", 3);
                    LidarControl(i + 1);
                    continue;
                }
                else
                {
                    upCheckHead = loc;
                }
                break;
            }
#else
#if lidarLocation
            string path = Application.StartupPath + "\\" + resultPath;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (Process.GetProcessesByName(lidarExeName).Length == 0)
            {
                Process.Start(Application.StartupPath + @"\" + lidarExeName + ".exe", "-t " + resultPath);
            }
            while (!File.Exists(path))
            {
                ThreadSleep(100);
            }
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        loc = double.Parse(sr.ReadLine());
                    }
                    File.Delete(path);
                    break;
                }
                catch (Exception e)
                {
                    AddLog(e.Message, -1);
                    ThreadSleep(1000);
                }
            } 
#endif
#endif
            return loc;
        }

        private void LidarControl(int i)
        {
#if plcModbus
            if (testForm.GetEnable(Form.EnableEnum.PLC))
            {
                AddLog("尝试第" + i + "次重启雷达", 3);
                ModbusTCP.SetAddress13(Address13Type.Custom, 1);
                AddLog("雷达断电，等待3秒", 3);
                ThreadSleep(3000);
                ModbusTCP.SetAddress13(Address13Type.Custom, 0);
                AddLog("雷达上电，等待10秒", 3);
                ThreadSleep(10000);
            }
#endif
        }
        #endregion

        #region 上传相关
        private bool UploadImage(Image image, int picIndex)
        {
            bool exp = false;
            if (!testForm.GetEnable(Form.EnableEnum.传图服务))
            {
                return exp;
            }
#if ping
            if (!pingResult[uploadServerKey])
            {
                xzNotLoad.Add(picIndex);
                AddLog("未上传线阵图片：" + picIndex);
                return exp;
            }
#endif
            if (!testForm.GetEnable(Form.EnableEnum.拼图))
            {
                byte[] bytes = ImageToBytes(image);
                TaskRun(() =>
                {
                    try
                    {
                        int remainder = bytes.Length % uploadSize;
                        int length = bytes.Length / uploadSize + (remainder > 0 ? 1 : 0);
                        for (int i = 0; i < length; i++)
                        {
                            List<byte> rom = new List<byte>();
                            for (int j = 0; j < uploadSize; j++)
                            {
                                int index = i * uploadSize + j;
                                if (index < bytes.Length)
                                {
                                    rom.Add(bytes[index]);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            try
                            {
                                picService.UploadImage(picIndex, i, length, uploadID, rom.ToArray(), Properties.Settings.Default.RobotID);
                                exp = true;
                            }
                            catch (Exception e)
                            {
                                AddLog(e.Message, -1);
#if ping
                                pingResult[uploadServerKey] = false;
                                xzNotLoad.Add(picIndex);
                                AddLog("未上传线阵图片：" + picIndex);
                                break;
#endif
                            }
                        }
                        AddLog("上传完成: " + picIndex);
                    }
                    catch (Exception e)
                    {
                        AddLog(">> [0] " + e.Message, -1);
                    }
                }); 
            }

            return exp;
        }

        private bool UploadImage(Image image, string picIndex, bool isLeft)
        {
            bool exp = false;
            if (!testForm.GetEnable(Form.EnableEnum.传图服务))
            {
                return exp;
            }
#if ping
            if (!pingResult[uploadServerKey])
            {
                if (isLeft)
                {
                    xzLNotLoad.Add(picIndex); 
                }
                else
                {
                    xzRNotLoad.Add(picIndex);
                }
                AddLog("未上传" + (isLeft ? "左" : "右") + "线阵图片：" + picIndex);
                return exp;
            }
#endif
            if (!testForm.GetEnable(Form.EnableEnum.拼图))
            {
                byte[] bytes = ImageToBytes(image);
                TaskRun(() =>
                {
                    try
                    {
                        int remainder = bytes.Length % uploadSize;
                        int length = bytes.Length / uploadSize + (remainder > 0 ? 1 : 0);
                        for (int i = 0; i < length; i++)
                        {
                            List<byte> rom = new List<byte>();
                            for (int j = 0; j < uploadSize; j++)
                            {
                                int index = i * uploadSize + j;
                                if (index < bytes.Length)
                                {
                                    rom.Add(bytes[index]);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            try
                            {
                                picService.UploadImage2(picIndex, i, length, uploadID, rom.ToArray(), Properties.Settings.Default.RobotID);
                                exp = true;
                            }
                            catch (Exception e)
                            {
                                AddLog(e.Message, -1);
#if ping
                                pingResult[uploadServerKey] = false;
                                if (isLeft)
                                {
                                    xzLNotLoad.Add(picIndex);
                                }
                                else
                                {
                                    xzRNotLoad.Add(picIndex);
                                }
                                AddLog("未上传" + (isLeft ? "左" : "右") + "线阵图片：" + picIndex);
                                break;
#endif
                            }
                        }
                        AddLog("上次完成: " + picIndex);
                    }
                    catch (Exception e)
                    {
                        AddLog(">> [2] " + e.Message, -1);
                    }
                });
            }

            return exp;
        }

        private void UploadComplete()
        {
            if (!testForm.GetEnable(Form.EnableEnum.传图服务))
            {
                return;
            }
#if ping
            if (!pingResult[uploadServerKey])
            {
                return;
            }
#endif
            if (!testForm.GetEnable(Form.EnableEnum.拼图))
            {
                try
                {
                    picService.UploadComplete(uploadID, Properties.Settings.Default.RobotID, xzStart);
                }
                catch (Exception e)
                {
                    AddLog(e.Message, -1);
#if ping
                    pingResult[uploadServerKey] = false;
#endif
                } 
            }
        }

        private bool UploadImage(Image image, string parsIndex, int robot)
        {
            bool exp = false;
            if (!testForm.GetEnable(Form.EnableEnum.传图服务))
            {
                return exp;
            }
            int logType = robot + 1;
#if ping
            if (!pingResult[uploadServerKey])
            {
                mzNotLoad.Add(parsIndex);
                AddLog("未上传面阵图片：" + parsIndex, logType);
                return exp;
            }
#endif
            if (!testForm.GetEnable(Form.EnableEnum.拼图))
            {
                byte[] bytes = ImageToBytes(image);
                TaskRun(() =>
                {
                    try
                    {
                        int remainder = bytes.Length % uploadSize;
                        int length = bytes.Length / uploadSize + (remainder > 0 ? 1 : 0);
                        string path = "";
                        for (int i = 0; i < length; i++)
                        {
                            List<byte> rom = new List<byte>();
                            for (int j = 0; j < uploadSize; j++)
                            {
                                int index = i * uploadSize + j;
                                if (index < bytes.Length)
                                {
                                    rom.Add(bytes[index]);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            try
                            {
                                path = picService.UploadPictrue(parsIndex, robot, i, length, uploadID, rom.ToArray(), Properties.Settings.Default.RobotID);
                                exp = true;
                            }
                            catch (Exception e)
                            {
                                AddLog(e.Message, -1);
#if ping
                            pingResult[uploadServerKey] = false;
                            mzNotLoad.Add(parsIndex);
                            AddLog("未上传面阵图片：" + parsIndex, logType);
                            break;
#endif
                        }
                            AddLog("上传后保存的路径：" + path, logType);
                        }
                        if (!string.IsNullOrEmpty(path))
                        {
                            path = "http://192.168.0.102:20001/img/Upload/" + uploadID + "_" + Properties.Settings.Default.RobotID + "/" + (robot == 0 ? "Front" : "Back") + "/" + parsIndex + ".jpg";
                            AddLog("代理路径：" + path, logType);
                            UploadMzImage(parsIndex, robot, path);
                        }
                }
                    catch (Exception e)
                    {
                        AddLog(">> [3] " + e.Message, -1);
                    }
                }); 
            }

            return exp;
        }

        private bool UploadData(string data, string parsIndex, int robot)
        {
            if (!testForm.GetEnable(Form.EnableEnum.传图服务))
            {
                return false;
            }
            if (!testForm.GetEnable(Form.EnableEnum.拼图))
            {
#if ping
                if (pingResult[uploadServerKey])
                { 
#endif
                    try
                    {
                            picService.Upload3DData(parsIndex, robot, data, uploadID, Properties.Settings.Default.RobotID);
                        }
                        catch (Exception e)
                        {
                            AddLog(">> [1] " + e.Message, -1);
                        }
#if ping
                } 
#endif
            }
            return Upload3dData(parsIndex, robot, data);
        } 

        private void UploadImage(byte[] bytes, string picName)
        {
            try
            {
                int remainder = bytes.Length % uploadSize;
                int length = bytes.Length / uploadSize + (remainder > 0 ? 1 : 0);
                for (int i = 0; i < length; i++)
                {
                    List<byte> rom = new List<byte>();
                    for (int j = 0; j < uploadSize; j++)
                    {
                        int index = i * uploadSize + j;
                        if (index < bytes.Length)
                        {
                            rom.Add(bytes[index]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    try
                    {
                        picService.UploadImage3(picName, i, length, uploadID, rom.ToArray(), Properties.Settings.Default.RobotID);
                    }
                    catch (Exception e)
                    {
                        AddLog(e.Message, -1);
#if ping
                        pingResult[uploadServerKey] = false;
                        break;
#endif
                    }
                }
            }
            catch (Exception e)
            {
                AddLog(">> [7] " + e.Message, -1);
            }
        }

        private int GetLocation(string name1, string name2, string name3, int? s = null)
        {
            try
            {
                int state = s ?? (name1[3] == '1' ? 0 : 1);
                AddLog(state == 0 ? "= 根据轴定位 =" : "·根据电机定位·");
                return picService.GetLocation(uploadID, name1, name2, name3, Properties.Settings.Default.RobotID, state);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Ping相关
        private Ping ping = new Ping();
        private void Ping1()
        {
            Dictionary<string, string> hostDict = new Dictionary<string, string>()
            {
                { "RGV", Properties.Settings.Default.RgvIP },
#if plcModbus
		        { "PLC", Properties.Settings.Default.ModbusTcp },  
#endif
		        { "前3D扫描仪", "192.168.1.101" },
                { "后3D扫描仪", "192.168.1.102" },
                { "Socket服务", Properties.Settings.Default.ServerIP },
                { uploadServerKey, Properties.Settings.Default.FtpIP }
            };
#if ping
            pingResult.Add(uploadServerKey, false);
            pingResult.Add(Properties.Settings.Default.UploadDataServer, true);
#endif
            Pings(hostDict);
        }

        private void Ping2()
        {
            Dictionary<string, string> hostDict = new Dictionary<string, string>()
            {
                { "前机械臂", Properties.Settings.Default.RobotFrontIP },
                { "后机械臂", Properties.Settings.Default.RobotBackIP },
                { "线阵相机", "192.168.2.210" },
                { "前臂面阵相机", "192.168.2.180" },
                { "后臂面阵相机", "192.168.2.181" }
            };
            Pings(hostDict);
        }

        private void Pings(Dictionary<string, string> hostDict)
        {
            PingResultModel model = null;
            foreach (KeyValuePair<string, string> item in hostDict)
            {
                do
                {
                    if (!Pings(item.Key))
                    {
                        AddLog("跳过" + item.Key + "的连接验证");
                        break;
                    }
                    model = ping.PingHost(item.Value);
                    switch (model.PingResult)
                    {
                        case PingResult.OK:
                            AddLog(item.Key + ">> 来自 " + model.Host + " 的回复: 字节=" + model.ByteEndPoint + " 时间=" + model.Reply + "ms");
#if ping
                            if (pingResult.ContainsKey(item.Key))
                            {
                                pingResult[item.Key] = true;
                            }
                            else if (Properties.Settings.Default.UploadDataServer.Contains(item.Key))
                            {
                                pingResult[Properties.Settings.Default.UploadDataServer] = true;
                            } 
#endif
                            break;
                        case PingResult.ErrorPacket:
                            AddLog(item.Key + ">> 异常");
                            break;
                        case PingResult.ErrorSocket:
                            AddLog(item.Key + ">> 异常");
                            break;
                        case PingResult.NoResponse:
                            AddLog(item.Key + ">> 无响应");
                            break;
                        case PingResult.OutTime:
                            AddLog(item.Key + ">> 超时");
                            break;
                        case PingResult.NoHost:
                            AddLog(item.Key + ">> 未找到主机");
                            break;
                    }
                    ThreadSleep(1000);
                } while (model.PingResult != PingResult.OK);
            }
        }

        private bool Pings(string key)
        {
            switch (key)
            {
                case "RGV":
                    if (testForm.GetEnable(Form.EnableEnum.RGV))
                    {
                        return true;
                    }
                    return false;
                case "PLC":
                    if (testForm.GetEnable(Form.EnableEnum.PLC))
                    {
                        return true;
                    }
                    return false;
                case "前3D扫描仪":
                    if (testForm.GetEnable(Form.EnableEnum.三维相机) && testForm.GetEnable(Form.EnableEnum.前滑台))
                    {
                        return true;
                    }
                    return false;
                case "后3D扫描仪":
                    if (testForm.GetEnable(Form.EnableEnum.三维相机) && testForm.GetEnable(Form.EnableEnum.后滑台))
                    {
                        return true;
                    }
                    return false;
                case "Socket服务":
                    if (testForm.GetEnable(Form.EnableEnum.Socket))
                    {
                        return true;
                    }
                    return false;
                case uploadServerKey:
                    if (testForm.GetEnable(Form.EnableEnum.传图服务))
                    {
                        return true;
                    }
                    return false;
                case "前机械臂":
                    if (testForm.GetEnable(Form.EnableEnum.前机械臂))
                    {
                        return true;
                    }
                    return false;
                case "后机械臂":
                    if (testForm.GetEnable(Form.EnableEnum.后机械臂))
                    {
                        return true;
                    }
                    return false;
                case "线阵相机":
                    if (testForm.GetEnable(Form.EnableEnum.执行线阵))
                    {
                        return true;
                    }
                    return false;
                case "前臂面阵相机":
                    if (testForm.GetEnable(Form.EnableEnum.前相机))
                    {
                        return true;
                    }
                    return false;
                case "后臂面阵相机":
                    if (testForm.GetEnable(Form.EnableEnum.后相机))
                    {
                        return true;
                    }
                    return false;
                default:
                    return true;
            }
        }
        #endregion

        #region PLC相关
        private void ClearFrontAlarm(object obj)
        {
            if (!testForm.GetEnable(Form.EnableEnum.PLC))
            {
                return;
            }
            IsFrontPLCAlarm = false;
            ClearPLCAlarm(RobotName.Front);
        }

        private void ClearBackAlarm(object obj)
        {
            if (!testForm.GetEnable(Form.EnableEnum.PLC))
            {
                return;
            }
            IsBackPLCAlarm = false;
            ClearPLCAlarm(RobotName.Back);
        }

        private void ClearPLCAlarm(RobotName name)
        {
            if (!testForm.GetEnable(Form.EnableEnum.PLC))
            {
                return;
            }
#if plcModbus
            if (pLC3DCamera.GetDriverAlarm(name))
            {
                pLC3DCamera.ClearAlarm(name);
            }
            if (pLC3DCamera.GetPLCAlarm(name))
            {
                pLC3DCamera.SetRst(name);
            } 
#endif
        }

        private void PowerOn(params Address12Type[] address)
        {
            foreach (Address12Type item in address)
            {
                if (!testForm.GetEnable(Form.EnableEnum.PLC))
                {
                    return;
                }
                ModbusTCP.SetAddress12(item, 1);
                AddLog(item.ToString() + "上电");
            }
        }

        private void PowerOff(params Address12Type[] address)
        {
            if (!testForm.GetEnable(Form.EnableEnum.PLC))
            {
                return;
            }
            foreach (Address12Type item in address)
            {
                ModbusTCP.SetAddress12(item, 0);
                AddLog(item.ToString() + "断电");
            }
        }

#if controlSocket
        private void PLCAlarm(RobotName name, SocketCmd cmd)
        {
            if (!testForm.GetEnable(Form.EnableEnum.PLC))
            {
                return;
            }
            switch (name)
            {
                case RobotName.Front:
                    IsFrontPLCAlarm = true;
                    break;
                case RobotName.Back:
                    IsBackPLCAlarm = true;
                    break;
            }

            string part = name == RobotName.Front ? front_parts_id : back_parts_id;
            string only = name == RobotName.Front ? shotFrontID : shotBackID;
            SokcetExeWrite(cmd, $"{Properties.Settings.Default.RobotID},{part},{only},{(int)name}");
        } 
#endif
        #endregion

        #region socket相关
#if controlSocket
        private bool SocketCmdFunc(SocketCmd socketCmd, params string[] vs)
        {
            if (testForm == null || !testForm.GetEnable(Form.EnableEnum.Socket))
            {
                return false;
            }
            switch (socketCmd)
            {
                #region 开始作业
                case SocketCmd.start_work:
                    try
                    {
                        string[] pars = vs[2].Split(',');
                        AppRemoteCtrl_TrainPara para = new AppRemoteCtrl_TrainPara();
                        para.robot_id = pars[0];
                        para.train_mode = pars[1];
                        para.train_sn = pars[2];
                        if (string.IsNullOrEmpty(pars[0]))
                        {
                            AddLog("Socket开始作业异常：缺失机器人编号", -1);
                        }
                        else if (string.IsNullOrEmpty(pars[1]))
                        {
                            AddLog("Socket开始作业异常：缺失车型", -1);
                        }
                        else if (string.IsNullOrEmpty(pars[2]))
                        {
                            AddLog("Socket开始作业异常：缺失车号", -1);
                        }
                        else
                        {
                            socketRunStart = true;
                            DoOneKeyStartCmdHandle(para, null); 
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("Socket开始作业异常：" + e.Message, -1);
                    }
                    return RunStat;
                #endregion
                #region 停止作业
                case SocketCmd.stop_work:
                    DoOneKeyStopCmdHandle();
                    return !RunStat;
                #endregion
                #region 停止作业并充电
                case SocketCmd.home_work:
                    DoOneKeyStopCmdHandle();
                    DoOneKeyStopCmdHandle();
                    return !RunStat;
                #endregion
                #region 部分作业
                case SocketCmd.forward_start:
                    string[] se = vs[2].Split(',');
                    int start = int.Parse(se[0]);
                    xzStart = testForm.GetXzLocationStart(start);
                    XzRun(start, int.Parse(se[1]));
                    return true;
                case SocketCmd.backward_start:
                    MzRun(vs[2]);
                    return true;
                #endregion
                #region 充电
                case SocketCmd.powercharge:
                    DoRgvNormalStopCmdHandle(null);
                    while (true)
                    {
                        if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                        {
                            break;
                        }
                        ThreadSleep(1000);
                        DoRgvNormalStopCmdHandle(null);
                    }
                    WaitRobotEnding();
                    ThreadSleep(1000);
                    WaitRobotEnding();
                    DoRgvStartIntelligentChargeCmdHandle();
                    AddLog("开始充电");
                    while (true)
                    {
                        if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                        {
                            break;
                        }
                        ThreadSleep(1000);
                    }
                    return true;
                case SocketCmd.powerstop:
                    DoRgvStopIntelligentChargeCmdHandle();
                    AddLog("停止充电");
                    return true;
                #endregion
                #region 清除报警
                case SocketCmd.clear_alarm:
                    AddLog("业务端清除报警", 4);
                    axisOutTimeAlarm = robotAxisError = false;
                    DoRgvClearAlarmCmdHandle(null);
                    if (IsFrontRelinkShot)
                    {
                        RelinkCameraShot(frontID, shotFrontType, "前");
                        IsFrontRelinkShot = false;
                    }
                    if (IsBackRelinkShot)
                    {
                        RelinkCameraShot(backID, shotBackType, "后");
                        IsBackRelinkShot = false;
                    }
                    if (IsFrontPLCAlarm)
                    {
                        ClearFrontAlarm(null);
                    }
                    if (IsBackPLCAlarm)
                    {
                        ClearBackAlarm(null);
                    }
                    return !IsAlarm;
                #endregion
                #region 机械臂回归原点
                case SocketCmd.robot_zero:
                    try
                    {
                        DoRobotCmdBackZeroHandle("1");
                        DoRobotCmdBackZeroHandle("2");
                        WaitRobotEnding();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    return true;
                #endregion
                #region 设备自检
                case SocketCmd.robot_check:
                    return Inspect();
                #endregion
                #region 设备控制
                case SocketCmd.forward:
                    DoRgvForwardMotorCmdHandle(null);
                    return true;
                case SocketCmd.backward:
                    DoRgvBackMotorCmdHandle(null);
                    return true;
                case SocketCmd.rgv_run:
                    DoRgvSetTargetDistanceCmdHandle(RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunDistance);
                    return true;
                case SocketCmd.rgv_stop:
                    DoRgvNormalStopCmdHandle(null);
                    return true;
                case SocketCmd.led_power_on:
                    PowerOn(Address12Type.RobotMzLedPower);
                    return true;
                case SocketCmd.led_power_off:
                    PowerOff(Address12Type.RobotMzLedPower);
                    return true;
                case SocketCmd.front_camera_power_on:
                    PowerOn(Address12Type.RobotFrontMzPower);
                    return true;
                case SocketCmd.front_camera_power_off:
                    PowerOff(Address12Type.RobotFrontMzPower);
                    return true;
                case SocketCmd.front_sliding_power_on:
                    PowerOn(Address12Type.FrontRobotStepMotorPower);
                    return true;
                case SocketCmd.front_sliding_power_off:
                    PowerOff(Address12Type.FrontRobotStepMotorPower);
                    return true;
                case SocketCmd.back_camera_power_on:
                    PowerOn(Address12Type.RobotBackMzPower);
                    return true;
                case SocketCmd.back_camera_power_off:
                    PowerOff(Address12Type.RobotBackMzPower);
                    return true;
                case SocketCmd.back_sliding_power_on:
                    PowerOn(Address12Type.BackRobotStepMotorPower);
                    return true;
                case SocketCmd.back_sliding_power_off:
                    PowerOff(Address12Type.BackRobotStepMotorPower);
                    return true;
                case SocketCmd.line_camera_power_on:
                    PowerOn(Address12Type.RobotXZPower);
                    return true;
                case SocketCmd.line_camera_power_off:
                    PowerOff(Address12Type.RobotXZPower);
                    return true;
                case SocketCmd.powerdown:
                    Process.Start("shutdown.exe", "-s");
                    return true;
                case SocketCmd.set_speed:
                    RgvSetSpeed(int.Parse(vs[2]));
                    return true;
                case SocketCmd.set_distance:
                    DoRgvSetTargetDistanceCmdHandle(int.Parse(vs[2]));
                    return true;
                case SocketCmd.set_length:
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTrackLength = int.Parse(vs[2]);
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTRACKLENGTH);
                    return true;
                #endregion
                #region 接收数据
                case SocketCmd.robot_axis:
                    try
                    {
                        string axisjson = vs[2];
                        List<Axis> axisList = JsonConvert.DeserializeObject<List<Axis>>(axisjson);
                        AxisList = axisList;
                        if (axisList.Count > 0)
                        {
                            LocalDataBase.GetInstance().jsonPath = Application.StartupPath + "/json/" + mzCameraDataList[0].TrainModel + "_" + mzCameraDataList[0].TrainSn + "/";
                            LocalDataBase.GetInstance().AxesDataListSave(axisList);
                        }
                        AddLog("收到轴数据：" + axisList.Count);
                    }
                    catch (Exception e)
                    {
                        AddLog(e.Message, -1);
                        return false;
                    }
                    return true;
                case SocketCmd.robot_linedata:
                    try
                    {
                        string xzjson = vs[2];
                        List<SocketXz> xzlist = JsonConvert.DeserializeObject<List<SocketXz>>(xzjson);
                        xzCameraDataList = new List<DataXzCameraLineModel>();
                        for (int i = 0; i < xzlist.Count; i++)
                        {
                            SocketXz item = xzlist[i];
                            DataXzCameraLineModel model = new DataXzCameraLineModel();
#if sumJoin
                            model.RgvCheckMinDistacnce = int.Parse(item.ShotCount);
                            model.RgvCheckMaxDistacnce = int.Parse(item.ShotCount);
#else
                            model.RgvCheckMinDistacnce = int.Parse(item.CarriageLength);
                            model.RgvCheckMaxDistacnce = int.Parse(item.CarriageLength);
#endif
                            model.DataLine_Index = i;
                            model.TrainModel = item.Mode;
                            model.TrainSn = item.Sn;
                            model.Rgv_Id = 0;
                            model.Rgv_Enable = 1;
                            xzCameraDataList.Add(model);
                        }
                        LocalDataBase.GetInstance().jsonPath = Application.StartupPath + "/json/" + xzlist[0].Mode + "_" + xzlist[0].Sn + "/";
                        LocalDataBase.GetInstance().XzCameraDataListSave(xzCameraDataList);
                        AddLog("收到线阵数据：" + xzCameraDataList.Count);
                    }
                    catch (Exception e)
                    {
                        AddLog(e.Message, -1);
                        return false;
                    }
                    return true;
                case SocketCmd.robot_backdata:
                    try
                    {
                        mzCameraDataList = new List<DataMzCameraLineModelEx>();
                        mzCameraDataList.AddRange(GetMzDataList(vs[2]));
                        if (mzCameraDataList.Count > 0)
                        {
                            LocalDataBase.GetInstance().jsonPath = Application.StartupPath + "/json/" + mzCameraDataList[0].TrainModel + "_" + mzCameraDataList[0].TrainSn + "/";
                            LocalDataBase.GetInstance().MzCameraDataListSave(mzCameraDataList);
                        }
                        AddLog("收到面阵数据：" + mzCameraDataList.Count);
                    }
                    catch (Exception e)
                    {
                        AddLog(e.Message, -1);
                    }
                    return true;
        #endregion
            }
            return false;
        }

        private void SokcetExeWrite(SocketCmd socketCmd, string value)
        {
            if (!testForm.GetEnable(Form.EnableEnum.Socket))
            {
                return;
            }
            string path = Application.StartupPath + "\\socket\\" + socketCmd.ToString() + ".r";
            if (!File.Exists(path))
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(path))
                    {
                        sw.Write(value);
                    }
                }
                catch (Exception) { }
            }
        } 

        private List<DataMzCameraLineModelEx> GetMzDataList(string json)
        {
            List<DataMzCameraLineModelEx> list = new List<DataMzCameraLineModelEx>();
            string mzjson = json;
            List<SocketMz> mzlist = JsonConvert.DeserializeObject<List<SocketMz>>(mzjson);
            List<int> distanceList = new List<int>();
            Dictionary<int, List<DataMzCameraLineModelEx>> front = new Dictionary<int, List<DataMzCameraLineModelEx>>();
            Dictionary<int, List<DataMzCameraLineModelEx>> back = new Dictionary<int, List<DataMzCameraLineModelEx>>();
            for (int i = 0; i < mzlist.Count; i++)
            {
                SocketMz item = mzlist[i];
                DataMzCameraLineModelEx model = new DataMzCameraLineModelEx();
                model.DataLine_Index = i;
                model.TrainModel = item.Mode;
                model.TrainSn = item.Sn;
                model.Rgv_Id = 0;
                model.Rgv_Enable = 1;
                model.Rgv_Distance = int.Parse(item.RgvRunDistacnce);
                model.Location = item.Location;
                model.Point = item.Point;
                if (!distanceList.Contains(model.Rgv_Distance))
                {
                    distanceList.Add(model.Rgv_Distance);
                }
                if (item.RobotName == "0")
                {
                    model.Front3d_Id = int.Parse(item.Camera3d_Id);
                    model.FrontCamera_Enable = 1;
                    model.FrontCamera_Id = 3;
                    model.FrontComponentId = item.PartsTypeId;
                    model.Front_Parts_Id = item.PartsTypeId;
                    switch (item.CameraTypeId)
                    {
                        case "1":
                            model.FrontComponentType = "Mz";
                            break;
                        case "2":
                            model.FrontComponentType = "3d";
                            break;
                        case "3":
                            model.FrontComponentType = "Ywc";
                            break;
                    }
                    model.FrontRobot_Enable = item.CanSort ? 1 : 0;
                    model.FrontRobot_Id = int.Parse(item.GroupID);
                    model.FrontRobot_J1 = item.J1;
                    model.FrontRobot_J2 = item.J2;
                    model.FrontRobot_J3 = item.J3;
                    model.FrontRobot_J4 = item.J4;
                    model.FrontRobot_J5 = item.J5;
                    model.FrontRobot_J6 = item.J6;

                    model.Back_Parts_Id = "0";
                    model.BackComponentType = "0";
                    model.BackComponentId = "0";
                    model.BackRobot_J1 = "0.00";
                    model.BackRobot_J2 = "0.00";
                    model.BackRobot_J3 = "0.00";
                    model.BackRobot_J4 = "0.00";
                    model.BackRobot_J5 = "0.00";
                    model.BackRobot_J6 = "0.00";
                    if (!front.ContainsKey(model.Rgv_Distance))
                    {
                        front.Add(model.Rgv_Distance, new List<DataMzCameraLineModelEx>());
                    }
                    front[model.Rgv_Distance].Add(model);
                }
                else
                {
                    model.Back3d_Id = int.Parse(item.Camera3d_Id);
                    model.BackCamera_Enable = 1;
                    model.BackCamera_Id = 4;
                    model.BackComponentId = item.PartsTypeId;
                    model.Back_Parts_Id = item.PartsTypeId;
                    switch (item.CameraTypeId)
                    {
                        case "1":
                            model.BackComponentType = "Mz";
                            break;
                        case "2":
                            model.BackComponentType = "3d";
                            break;
                        case "3":
                            model.BackComponentType = "Ywc";
                            break;
                    }
                    model.BackRobot_Enable = item.CanSort ? 1 : 0;
                    model.BackRobot_Id = int.Parse(item.GroupID);
                    model.BackRobot_J1 = item.J1;
                    model.BackRobot_J2 = item.J2;
                    model.BackRobot_J3 = item.J3;
                    model.BackRobot_J4 = item.J4;
                    model.BackRobot_J5 = item.J5;
                    model.BackRobot_J6 = item.J6;

                    model.Front_Parts_Id = "0";
                    model.FrontComponentType = "0";
                    model.FrontComponentId = "0";
                    model.FrontRobot_J1 = "0.00";
                    model.FrontRobot_J2 = "0.00";
                    model.FrontRobot_J3 = "0.00";
                    model.FrontRobot_J4 = "0.00";
                    model.FrontRobot_J5 = "0.00";
                    model.FrontRobot_J6 = "0.00";
                    if (!back.ContainsKey(model.Rgv_Distance))
                    {
                        back.Add(model.Rgv_Distance, new List<DataMzCameraLineModelEx>());
                    }
                    back[model.Rgv_Distance].Add(model);
                }
            }
            foreach (int distance in distanceList)
            {
                List<DataMzCameraLineModelEx> frontItem = null;
                if (front.ContainsKey(distance))
                {
                    frontItem = front[distance];
                    frontItem.Sort((x, y) => x.sorIndex - y.sorIndex);
                }

                List<DataMzCameraLineModelEx> backItem = null;
                if (back.ContainsKey(distance))
                {
                    backItem = back[distance];
                    backItem.Sort((x, y) => x.sorIndex - y.sorIndex);
                }

                if (frontItem != null && backItem != null)
                {
                    int length = backItem.Count > frontItem.Count ? backItem.Count : frontItem.Count;
                    for (int i = 0; i < length; i++)
                    {
                        if (i < frontItem.Count && i < backItem.Count)
                        {
                            frontItem[i].Back_Parts_Id = backItem[i].Back_Parts_Id;
                            frontItem[i].BackRobot_Id = backItem[i].BackRobot_Id;
                            frontItem[i].BackComponentType = backItem[i].BackComponentType;
                            frontItem[i].BackComponentId = backItem[i].BackComponentId;
                            frontItem[i].Back3d_Id = backItem[i].Back3d_Id;
                            frontItem[i].BackRobot_J1 = backItem[i].BackRobot_J1;
                            frontItem[i].BackRobot_J2 = backItem[i].BackRobot_J2;
                            frontItem[i].BackRobot_J3 = backItem[i].BackRobot_J3;
                            frontItem[i].BackRobot_J4 = backItem[i].BackRobot_J4;
                            frontItem[i].BackRobot_J5 = backItem[i].BackRobot_J5;
                            frontItem[i].BackRobot_J6 = backItem[i].BackRobot_J6;
                            list.Add(frontItem[i]);
                        }
                        else if (i < frontItem.Count)
                        {
                            list.Add(frontItem[i]);
                        }
                        else if (i < backItem.Count)
                        {
                            list.Add(backItem[i]);
                        }
                    }
                }
                else if (frontItem != null)
                {
                    list.AddRange(frontItem);
                }
                else if (backItem != null)
                {
                    list.AddRange(backItem);
                }
            }
            return list;
        }
#endif
        #endregion

        #region 测试
        const bool Test_Dir = false;
        const bool Test_File = false;
        private void TestStart(object obj)
        {
            #region 相机初始化测试
#if false
            do
            {
                if (cameraManager != null)
                {
                    for (int i = 0; i < cameraManager.Cameras.Count; i++)
                    {
                        cameraManager.Cameras[i].Close();
                    }
                    cameraManager.Close();
                }
                InitCameraMod();
                ThreadSleep(1000);
#if newBasler
                AddLog("相机数量: " + cameraManager.Cameras.Count);
            } while (cameraManager.Cameras.Count < 1);
#else
                AddLog("相机数量: " + CameraCtrlHelper.GetInstance().myCameraList.Count);
            } while (CameraCtrlHelper.GetInstance().myCameraList.Count < 3);
            CameraCtrlHelper.GetInstance().CameraImageEvent += Camera_CameraImageEvent;
            CameraCtrlHelper.GetInstance().CameraErrorEvent += HomePageViewModel_CameraErrorEvent; 
            Schedule.ScheduleManager.CameraTest(taskMainCameraModel.XzCamerainfoItem,
                taskMainCameraModel.FrontCamerainfoItem, taskMainCameraModel.BackCamerainfoItem);
            ThreadSleep(2000); 
#endif
            CameraOpen();
            DoCameraCmdHandle_OneShot(frontID);
            DoCameraCmdHandle_OneShot(backID);
            return;
#endif
            #endregion
#if ping
            pingResult.Add(uploadServerKey, false);
            pingResult.Add(Properties.Settings.Default.UploadDataServer, true);
#endif
            if (Test_Dir)
            {
                #region 核对图片与数据数量，查找丢图
#if true
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    string backPath = path + "\\Back";
                    string frontPath = path + "\\Front";
                    string[] backFiles = Directory.GetFiles(backPath, "*.jpg");
                    string[] frontFiles = Directory.GetFiles(frontPath, "*.jpg");
                    List<Model.DataMzCameraLineModelEx> list = null;
                    using (StreamReader sr = new StreamReader(path + "\\DataMzCameraLineModel.json"))
                    {
                        string json = sr.ReadToEnd();
                        list = JsonConvert.DeserializeObject<List<Model.DataMzCameraLineModelEx>>(json);
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        Model.DataMzCameraLineModelEx item = list[i];
                        if (item.FrontComponentType != "0" && item.FrontComponentType != "3d")
                        {
                            int index = Array.IndexOf(frontFiles, frontPath + "\\" + item.FrontComponentId + ".jpg");
                            if (index < 0)
                            {
                                AddLog("Front: " + item.FrontComponentId + " " + JsonConvert.SerializeObject(new string[] { item.FrontRobot_J1, item.FrontRobot_J2, item.FrontRobot_J3, item.FrontRobot_J4, item.FrontRobot_J5, item.FrontRobot_J6 }));
                            }
                        }
                        if (item.BackComponentType != "0" && item.BackComponentType != "3d")
                        {
                            int index = Array.IndexOf(backFiles, backPath + "\\" + item.BackComponentId + ".jpg");
                            if (index < 0)
                            {
                                AddLog("Back: " + item.BackComponentId + " " + JsonConvert.SerializeObject(new string[] { item.BackRobot_J1, item.BackRobot_J2, item.BackRobot_J3, item.BackRobot_J4, item.BackRobot_J5, item.BackRobot_J6 }));
                            }
                        }
                    }
                }
                return;
#endif 
                #endregion
#if !lidarLocation
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = folderBrowserDialog.SelectedPath;
                    #region 拼图测试
                    var puzzle = new Action(() =>
                    {
#if false
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        int fileCount = directoryInfo.GetFiles("*.jpg").Length;
                        var Join = new Action<int, int, string>((int i1, int i2, string name) =>
                        {
                            int pic_index1 = i1 + 10000;
                            int pic_index2 = i2 + 10000;
                            AddLog(string.Format("部分图片拼图: {0}至{1}, 路径 {2}", pic_index1, pic_index2, name));

                            string join_file_name = name;
                            List<Image> img_list = new List<Image>();

                            DirectoryInfo dir1 = new DirectoryInfo(path);
                            FileInfo[] inf1 = dir1.GetFiles();
                            foreach (FileInfo finf in inf1)
                            {
                                if (finf.Extension.Equals(GetExtend()))
                                {
                                    int filename_num = -1;
                                    string filename = Path.GetFileNameWithoutExtension(finf.Name);     //返回不带扩展名的文件名 
                                    int.TryParse(filename, out filename_num);

                                    if (filename_num != -1 && (filename_num >= pic_index1 && filename_num <= pic_index2))
                                    {
                                        Image newImage = Image.FromFile(finf.FullName);
                                        img_list.Add(newImage);
                                    }
                                }
                            }

                            Image img = JoinImage(img_list, JoinMode.Vertical);
                            string strfile = path.Substring(0, path.LastIndexOf(@"\")) + @"\xz_join\" + join_file_name + GetExtend();
                            SaveBitmapIntoFile(img, strfile, JoinMode.Vertical);
                        });
                        int count = fileCount / 8;
                        int i = fileCount % 8;
                        int len = count + i;
                        Join(0, len, "000001");
                        Join(len + 1, len += count, "000002");
                        Join(len + 1, len += count, "000003");
                        Join(len + 1, len += count, "000004");
                        Join(len + 1, len += count, "000005");
                        Join(len + 1, len += count, "000006");
                        Join(len + 1, len += count, "000007");
                        Join(len + 1, len += count, "000008");
                        AddLog("END");
#endif
                    });
                    #endregion
                    TaskRun(() =>
                    {
                        puzzle();
                        #region PLC连接测试
#if false
                        ModbusTCP.SetAddress12(Address12Type.FrontRobotPower, 1);
                        ModbusTCP.SetAddress12(Address12Type.BackRobotPower, 1);
                        ModbusTCP.SetAddress12(Address12Type.RobotMzLedPower, 1);
                        ModbusTCP.SetAddress12(Address12Type.RobotXzLedPower, 1);

                        ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 0);
                        ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 0);
                        ModbusTCP.SetAddress12(Address12Type.FrontRobotStart, 1);
                        ModbusTCP.SetAddress12(Address12Type.BackRobotStart, 1);
                        ModbusTCP.SetAddress13(Address13Type.Music_Robot_Power_on, 1);
                        AddLog("初始化机械臂...", 1);
                        RobotModCtrlHelper.GetInstance().RobotModInfoEvent += MyRobotModInfoEvent;
                        InitRobotMod();
                        AddLog("机械臂初始化完成", 1); while (true)
                        {
                            if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotConnStat &&
                                RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotConnStat)
                            {
                                AddLog("延迟等到机械臂复位动作完成，等待3秒...");
                                ThreadSleep(3000);

                                DoRobotCmdBackZeroHandle("1");
                                DoRobotCmdBackZeroHandle("2");
                                break;
                            }

                            AddLog("等待机械臂复位【前臂：" + RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotConnStat +
                                   "】【后臂：" + RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotConnStat + "】");
                            ThreadSleep(1000);
                        } 
#endif
                        #endregion
                    });

                    uploadID = "380AL_2572_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
                    DirectoryInfo dir = new DirectoryInfo(path);
                    FileInfo[] inf = dir.GetFiles();
                    for (int i = 0; i < inf.Length; i++)
                    {
                        UploadImage(Image.FromFile(inf[i].FullName), 10000 + i);
                        ThreadSleep(1000);
                    }
                    if (MessageBox.Show("是否执行拼图？", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        picService.UploadComplete(uploadID, Properties.Settings.Default.RobotID);
                    }
                    DirectoryInfo[] dirs = dir.GetDirectories();
                    if (dirs != null)
                    {
                        int robot = 0;
                        foreach (DirectoryInfo item in dirs)
                        {
                            FileInfo[] files = item.GetFiles();
                            for (int i = 0; i < files.Length; i++)
                            {
                                UploadImage(Image.FromFile(files[i].FullName), files[i].Name.Replace(".jpg", ""), robot);
                                ThreadSleep(1000);
                            }
                            robot++;
                        }
                    }
                }
#endif
            }

            if (Test_File)
            {
#if false
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] path = openFileDialog.FileNames;
                    string[] logs = null;
                    List<Model.DataMzCameraLineModelEx> list = null;
                    foreach (string item in path)
                    {
                        using (StreamReader sr = new StreamReader(item))
                        {
                            if (item.Contains(".json"))
                            {
                                list = JsonConvert.DeserializeObject<List<Model.DataMzCameraLineModelEx>>(sr.ReadToEnd());
                            }
                            else
                            {
                                logs = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            }
                        }
                    }

                    int min = -1;
                    List<string> frontResult = new List<string>();
                    List<string> backResult = new List<string>();
                    Dictionary<int, List<string>> minDict = new Dictionary<int, List<string>>();
                    foreach (Model.DataMzCameraLineModelEx item in list)
                    {
                        string frontID = "", backID = "";
                        if (item.FrontComponentType == "Mz" || item.FrontComponentType == "Ywc")
                        {
                            frontID = item.FrontComponentId;
                        }
                        if (item.BackComponentType == "Mz" || item.BackComponentType == "Ywc")
                        {
                            backID = item.BackComponentId;
                        }

                        int frontCount = 0, backCount = 0;
                        foreach (string log in logs)
                        {
                            if (!string.IsNullOrEmpty(frontID))
                            {
                                if (log.Contains(frontID))
                                {
                                    frontCount++;
                                }
                            }
                            if (!string.IsNullOrEmpty(backID))
                            {
                                if (log.Contains(backID))
                                {
                                    backCount++;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(frontID))
                        {
                            frontResult.Add($"前 {frontID} : {frontCount}");
                            if (min < 0)
                            {
                                min = frontCount;
                            }
                            if (min > frontCount)
                            {
                                minDict.Remove(min);
                                min = frontCount;
                            }
                            if (!minDict.ContainsKey(min))
                            {
                                minDict.Add(min, new List<string>());
                            }
                            if (minDict.ContainsKey(frontCount))
                            {
                                minDict[frontCount].Add($"前 {frontID} : {frontCount}");
                            }
                        }
                        if (!string.IsNullOrEmpty(backID))
                        {
                            backResult.Add($"后 {backID} : {backCount}");
                            if (min < 0)
                            {
                                min = backCount;
                            }
                            if (min > backCount)
                            {
                                minDict.Remove(min);
                                min = backCount;
                            }
                            if (!minDict.ContainsKey(min))
                            {
                                minDict.Add(min, new List<string>());
                            }
                            if (minDict.ContainsKey(backCount))
                            {
                                minDict[backCount].Add($"后 {backID} : {backCount}");
                            }
                        }
                    }

                    foreach (KeyValuePair<int, List<string>> item in minDict)
                    {
                        foreach (string s in item.Value)
                        {
                            AddLog(s);
                        }
                    }
                    foreach (string s in frontResult)
                    {
                        AddLog(s);
                    }
                    foreach (string s in backResult)
                    {
                        AddLog(s);
                    }
                }
                return;
#endif
#if false
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "图片文件|*.jpg;*.png;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = openFileDialog.FileName;
                    string id = path.Substring(path.LastIndexOf("\\") + 1).Replace(".jpg", "");
                    UploadImage(Image.FromFile(path), id, 0);
                    UploadImage(Image.FromFile(path), id, 1);
                    Upload3dData(id, 0, "22");
                } 
#endif
#if false
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] names = openFileDialog.FileNames;
                    foreach (string name in names)
                    {
                        string _name = name.Substring(name.LastIndexOf('\\') + 1);
                        using (FileStream file = new FileStream(name, FileMode.Open))
                        {
                            byte[] bytes = new byte[file.Length];
                            file.Read(bytes, 0, bytes.Length);
                            UploadImage(bytes, _name);
                        }
                    }
                }
                return;
#endif
            }

            TaskRun(() =>
            {
                #region 日志清理测试
#if false
                if (testForm.GetEnable(Form.EnableEnum.清理日志))
                {
                    DateTime now = DateTime.Now;
                    if (now.Hour > 5)
                    {
                        DateTime date = DateTime.Parse(FileSystem.ReadIniFile("日志", "最后一次记录日期", now.ToString("yyyy-MM-dd"), iniPath));
                        if (date.Day < now.Day || date.Month < now.Month || date.Year < now.Year)
                        {
                            string[] logs = Directory.GetFiles(Application.StartupPath + "\\log");
                            foreach (string log in logs)
                            {
                                try
                                {
                                    File.Delete(log);
                                }
                                catch (Exception) { }
                            }
                        }
                        FileSystem.WriteIniFile("日志", "最后一次记录日期", now.ToString("yyyy-MM-dd"), iniPath);
                    }
                }
#endif
                #endregion
                #region 仅滑台测试
#if false
                pLC3DCamera.SetZero(RobotName.Front);
                AddLog("滑台归原点");
                do
                {
                    ThreadSleep(plcSleep);
                    pLC3DCamera.GetState(RobotName.Front);
                    if (SlidingOutTime(RobotName.Front))
                    {
                        AddLog("滑台超时", (int)RobotName.Front);
                        return;
                    }
                } while (!pLC3DCamera.FrontModbus.Home_Complete);

                pLC3DCamera.SetForward(RobotName.Front);
                AddLog("滑台前进");
                do
                {
                    ThreadSleep(plcSleep);
                    pLC3DCamera.GetState(RobotName.Front);
                    if (SlidingOutTime(RobotName.Front))
                    {
                        AddLog("滑台超时", (int)RobotName.Front);
                        return;
                    }
                } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);

                pLC3DCamera.SetBackoff(RobotName.Front);
                AddLog("滑台返回");
                do
                {
                    ThreadSleep(plcSleep);
                    pLC3DCamera.GetState(RobotName.Front);
                    if (SlidingOutTime(RobotName.Front))
                    {
                        AddLog("滑台超时", (int)RobotName.Front);
                        return;
                    }
                } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);
                AddLog("滑台完成");

                pLC3DCamera.SetZero(RobotName.Back);
                AddLog("滑台归原点");
                do
                {
                    ThreadSleep(plcSleep);
                    pLC3DCamera.GetState(RobotName.Back);
                    if (SlidingOutTime(RobotName.Back))
                    {
                        AddLog("滑台超时", (int)RobotName.Back);
                        return;
                    }
                } while (!pLC3DCamera.BackModbus.Home_Complete);

                pLC3DCamera.SetForward(RobotName.Back);
                AddLog("滑台前进");
                do
                {
                    ThreadSleep(plcSleep);
                    pLC3DCamera.GetState(RobotName.Back);
                    if (SlidingOutTime(RobotName.Back))
                    {
                        AddLog("滑台超时", (int)RobotName.Back);
                        return;
                    }
                } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);

                pLC3DCamera.SetBackoff(RobotName.Back);
                AddLog("滑台返回");
                do
                {
                    ThreadSleep(plcSleep);
                    pLC3DCamera.GetState(RobotName.Back);
                    if (SlidingOutTime(RobotName.Back))
                    {
                        AddLog("滑台超时", (int)RobotName.Back);
                        return;
                    }
                } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                AddLog("滑台完成");
                return;
#endif
                #endregion
                #region 仅光源测试
#if false
                AddLog("初始化光源...");
                light = new LightManager(Properties.Settings.Default.Light)
                {
                    LinkLight = () =>
                    {
                        return true;
                    }
                };
#if controlLightPower
                ThreadSleep(500);
                light?.LightOn(Properties.Settings.Default.LightFrontHigh, true);
                ThreadSleep(500);
                light?.LightOn(Properties.Settings.Default.LightFrontHigh, false);
#endif
                Random ram = new Random();
                TaskRun(() =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        AddLog("打开前光源", 1);
                        light.LightOn(Properties.Settings.Default.LightFrontHigh, true);
                        ThreadSleep(ram.Next(100, 1000));
                        AddLog("关闭前光源", 1);
                        light.LightOff(true);
                    }
                });
                TaskRun(() =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        AddLog("打开后光源", 2);
                        light.LightOn(Properties.Settings.Default.LightFrontHigh, false);
                        ThreadSleep(ram.Next(100, 1000));
                        AddLog("关闭后光源", 2);
                        light.LightOff(false);
                    }
                });
#endif
                #endregion
                #region 3D扫描仪与滑台测试
#if false
                pLC3DCamera = new PLC3DCamera();
                for (int i = 0; i < 2; i++)
                {
                    bool _3DRun = false;
                    bool[] bs = new bool[2];
                    if (i == 0 || i == 2)
                    {
                        TaskRun(() =>
                        {
#if true
                            pLC3DCamera.FrontModbus.Home_Complete = false;
                            pLC3DCamera.SetZero(RobotName.Front);
                            AddLog("滑台归原点");
                            do
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.GetState(RobotName.Front);
                            } while (!pLC3DCamera.FrontModbus.Home_Complete);

                            pLC3DCamera.FrontModbus.MoveAbsolute_Complete = false;
                            pLC3DCamera.SetForward(RobotName.Front);
                            AddLog("滑台前进");
                            do
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.GetState(RobotName.Front);
                            } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);
                            if (!_3DRun)
                            {
                                _3DRun = true;
                                AddLog("3D扫描仪启动");
                                cognexManager.Run(i, (int)RobotName.Front);
                            }
                            pLC3DCamera.FrontModbus.MoveAbsolute_Complete = false;
                            pLC3DCamera.SetBackoff(RobotName.Front);
                            AddLog("滑台返回");
                            do
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.GetState(RobotName.Front);
                            } while (!pLC3DCamera.FrontModbus.MoveAbsolute_Complete);
                            AddLog("前滑台完成"); 
#endif
                            bs[0] = true;
                        });
                    }
                    else
                    {
                        bs[0] = true;
                    }

                    if (i > 0)
                    {
                        TaskRun(() =>
                        {
#if false
                            pLC3DCamera.BackModbus.Home_Complete = false;
                            pLC3DCamera.SetZero(RobotName.Back);
                            AddLog("滑台归原点");
                            do
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.GetState(RobotName.Back);
                            } while (!pLC3DCamera.BackModbus.Home_Complete);

                            pLC3DCamera.BackModbus.MoveAbsolute_Complete = false;
                            pLC3DCamera.SetForward(RobotName.Back);
                            AddLog("滑台前进");
                            do
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.GetState(RobotName.Back);
                            } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                            if (!_3DRun)
                            {
                                _3DRun = true;
                                AddLog("3D扫描仪启动");
                                cognexManager.Run(new int[] { 6, 6 });
                            }
                            pLC3DCamera.BackModbus.MoveAbsolute_Complete = false;
                            pLC3DCamera.SetBackoff(RobotName.Back);
                            AddLog("滑台返回");
                            do
                            {
                                ThreadSleep(plcSleep);
                                pLC3DCamera.GetState(RobotName.Back);
                            } while (!pLC3DCamera.BackModbus.MoveAbsolute_Complete);
                            AddLog("后滑台完成"); 
#endif
                            bs[1] = true;
                        });
                    }
                    else
                    {
                        bs[1] = true;
                    }
                    while (!bs[0] || !bs[1])
                    {
                        ThreadSleep(50);
                    }
                    AddLog("拍照完成");
                    cognexManager.Stop();
                }
#endif
                #endregion
                #region 异步原地控制流程
#if false
                if (obj != null)
                {
                    train_para = obj as AppRemoteCtrl_TrainPara;
                    Properties.Settings.Default.TrainMode = train_para.train_mode;
                    Properties.Settings.Default.TrainSn = train_para.train_sn;
                    LocalDataBase.GetInstance().jsonPath = Application.StartupPath + "/json/" + train_para.train_mode + "_" + train_para.train_sn + "/";
                    mzCameraDataList = LocalDataBase.GetInstance().MzCameraDataListQurey();
                    xzCameraDataList = LocalDataBase.GetInstance().XzCameraDataListQurey();
                }
                AddLog("动车参数：" + JsonConvert.SerializeObject(train_para));
                RunStat = true;
                frontMzIndex = backMzIndex = mzDistance = 0;
                frontData.Clear();
                backData.Clear();
                for (int i = 0; i < mzCameraDataList.Count; i++)
                {
                    Model.DataMzCameraLineModel model = mzCameraDataList[i];
                    MzDataExtent front = new MzDataExtent(model, RobotName.Front);
                    MzDataExtent back = new MzDataExtent(model, RobotName.Back);
                    frontData.Add(front);
                    backData.Add(back);
                }
                MzRun(false);
                return;
#endif
                #endregion
                #region 激光雷达定位测试
#if false
#if lidarLocation
                int moveLocation = (int)(GetLidarLoc("380AL_2572", "0_1", "z1") * 1000);
#if lidarCsharp
                moveLocation /= 1000;
#endif
                if (moveLocation != 0)
                {
                    int movePoint = Properties.Settings.Default.RobotPointXZ + moveLocation;
                    DoRgvSetTargetDistanceCmdHandle(movePoint);
                    AddLog("Rgv运动到指定位置: " + movePoint);
                    while (true)
                    {
                        if (!RunStat)
                        {
                            AddLog("任务强制中止！");
                            return;
                        }
                        if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                        {
                            AddLog("Rgv停止");
                            break;
                        }
                        ThreadSleep(50);
                    }
                }
                Console.WriteLine();
                return;
#endif
#endif
                #endregion
                #region 轴定位测试
#if false
                GetPositionDifference(state: new int[] { 0, 1 });
#endif
#if false
                AddLog("初始化Rgv...");
                RgvModCtrlHelper.GetInstance().RgvModInfoEvent += MyRgvModInfoEvent;
                InitRgvMod();
                AddLog("Rgv初始化完成");
                DoRgvStopIntelligentChargeCmdHandle();
                ThreadSleep(5000);
                LocalDataBase.GetInstance().jsonPath = Application.StartupPath + "/json/380AL_2589/";
                AxisList = LocalDataBase.GetInstance().AxesDataListQurey();
                uploadID = "testDepth" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm");
                axisFrontID = axisBackID = 0;
                for (int i = 4; i < AxisList.Count; i++)
                {
                    int c = -200;
                    for (int j = 0; j < 21; j++)
                    {
                        c += 20;
                        axisFrontID = AxisList[i].ID * 100 + j;
                        int location = AxisList[i].Distance + c + trainHeadLocation;
                        AddLog("RGV运动到" + location);
                        DoRgvSetTargetDistanceCmdHandle(location);
                        do
                        {
                            ThreadSleep(1000);
                        } while (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP || RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed != 0);

                        AddLog("点云相机拍照");
                        string name1, name2, path;
                        if (PointShot(out path, out name1, out name2))
                        {
                            try
                            {
                                using (FileStream file = new FileStream(path + name1, FileMode.Open))
                                {
                                    byte[] bytes = new byte[file.Length];
                                    file.Read(bytes, 0, bytes.Length);
                                    UploadImage(bytes, name1);
                                    AddLog("上传红外图片完成");
                                }
                                using (FileStream file = new FileStream(path + name2, FileMode.Open))
                                {
                                    byte[] bytes = new byte[file.Length];
                                    file.Read(bytes, 0, bytes.Length);
                                    UploadImage(bytes, name2);
                                    AddLog("上传深度图片完成");
                                }
                            }
                            catch (Exception e)
                            {
                                AddLog(e.Message, -1);
                                return;
                            }
                        }
                    }
                }
#endif
                #endregion
                #region Socket伪流程
#if false
                //RgvState = "运行中";
                //RgvElectricity = "99%";
                //TrainHeadDistance = "12168 mm";
                //GrabImg = @"F:\task_data\xz_camera\10016.jpg";
                AddLog("<---任务开始--->");
                RunStat = true;
                IsEndEnable = true;
                Random ram = new Random();
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentPowerTempture = 17;
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentPowerElectricity = 99;
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentPowerCurrent = 40;
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce = 15000;
                Job = "任务准备中";
                RunMz = true;
                isWait = false;
                xzIndex = -1;
                train_para = new AppRemoteCtrl_TrainPara();
                if (obj != null)
                {
                    train_para = obj as AppRemoteCtrl_TrainPara;
                    Properties.Settings.Default.TrainMode = train_para.train_mode;
                    Properties.Settings.Default.TrainSn = train_para.train_sn;
                    LocalDataBase.GetInstance().jsonPath = Application.StartupPath + "/json/" + train_para.train_mode + "_" + train_para.train_sn + "/";
                    mzCameraDataList = LocalDataBase.GetInstance().MzCameraDataListQurey();
                    xzCameraDataList = LocalDataBase.GetInstance().XzCameraDataListQurey();
                }
                AddLog("动车参数：" + JsonConvert.SerializeObject(train_para));
                uploadID = Properties.Settings.Default.TrainMode + "_" + Properties.Settings.Default.TrainSn + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTrackLength = 300000;

                AddLog("开始清理缓存图片");
                string task = Application.StartupPath + "/task_data";
                string test = Application.StartupPath + "/test_data";
                string work = Application.StartupPath + "/work_data";
                var deleteFiles = new Action<DirectoryInfo>((DirectoryInfo dir) => { });
                deleteFiles = new Action<DirectoryInfo>((DirectoryInfo dir) =>
                {
                    foreach (FileInfo item in dir.GetFiles())
                    {
                        try
                        {
                            File.Delete(item.FullName);
                        }
                        catch (Exception) { }
                    }
                    foreach (DirectoryInfo item in dir.GetDirectories())
                    {
                        deleteFiles(item);
                    }
                });
                var deleteFile = new Action<string>((string path) =>
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    deleteFiles(directory);
                });
                AddLog(">>清理task_data文件夹下的缓存图片");
                deleteFile(task);

                UserInfo.myDeviceStat = UserEntity.key_DEVICE_BUSY;
                AddLog("设备状态：" + UserInfo.myDeviceStat);
                AddLog("停止充电");

                TaskRun(() =>
                {
                    do
                    {
                        RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed = ram.Next(790, 810);
                        RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce += RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed;
                        RgvSpeed = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed + " mm/s";
                        RgvDistacnce = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce + " mm";

                        //if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce > 30000)
                        //{
                        //    IsFrontPLCAlarm = IsBackPLCAlarm = true;
                        //    break;
                        //}
                        ThreadSleep(1000);
                    } while (true);
                });
#if controlSocket
#if socketExe
                Job = "线阵任务执行中";
                AddLog("开始线阵流程");
                AddLog("执行Rgv任务...");
                RunMz = false;
                AddLog("Rgv运动到指定位置...");
                AddLog("开始车头检测...");
                AddLog("记录车头位置：" + (TrainCurrentHeadDistance));
                AddLog("开始相机线扫***");
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE;
                for (int i = 0; i < xzCameraDataList.Count; i++)
                {
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentPowerElectricity--;
                    xzIndex = i;
                    Model.DataXzCameraLineModel item = xzCameraDataList[i];
                    for (int j = 0; j < item.CurrentDetectMaxHeight; j++)
                    {
                        RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed = ram.Next(790, 811);
                        string fileIndex = taskScheduleHandleInfo.xzCameraPicDataListCount.ToString("10000");
                        string imgPath = string.Format(@"{0}\test_data\xz_camera\{1}{2}", Application.StartupPath, fileIndex, GetExtend());
                        Bitmap bitmap;
                        if (File.Exists(imgPath))
                        {
                            AddLog("加载线阵图片：" + imgPath);
                            bitmap = (Bitmap)Bitmap.FromFile(imgPath);
                            RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce += 1000;
                            AddLog("当前车辆位置：" + RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce);
                            UploadImage(bitmap, 10000 + taskScheduleHandleInfo.xzCameraPicDataListCount);
                            taskScheduleHandleInfo.xzCameraPicDataListCount++;
                            ThreadSleep(1000);
                            GrabImg = imgPath;
                        }
                    }
                    AddLog("已完成第" + (i + 1) + "节");
                }

                AddLog("扫图结束");
                UploadComplete();

                Job = "面阵任务执行中";
                AddLog("开始执行面阵流程");
                string[] backFiles = Directory.GetFiles(test + "/back_camera");
                string[] frontFiles = Directory.GetFiles(test + "/front_camera");
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentPowerElectricity--;
                foreach (string file in backFiles)
                {
                    AddLog("上传后臂图片：" + file);
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce -= 1000;
                    string name = file.Substring(file.LastIndexOf('\\') + 1).Replace(".jpg", "");
                    UploadImage(Image.FromFile(file), name, (int)RobotName.Back);
                    ThreadSleep(1000);
                }
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentPowerElectricity--;
                foreach (string file in frontFiles)
                {
                    AddLog("上传前臂图片：" + file);
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce -= 1000;
                    string name = file.Substring(file.LastIndexOf('\\') + 1).Replace(".jpg", "");
                    UploadImage(Image.FromFile(file), name, (int)RobotName.Front);
                    ThreadSleep(1000);
                }

                UserInfo.myDeviceStat = UserEntity.key_DEVICE_INIT;
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                RunStat = false;
                AddLog("<---任务结束--->");
                Job = "上位机等待任务中";
#endif
#endif
#endif
                #endregion
                #region 相机测试
#if false
                //AddLog("初始化光源...");
                //light = new LightManager(Properties.Settings.Default.Light)
                //{
                //    LinkLight = () =>
                //    {
                //        return true;
                //    }
                //};
                for (int i = 0; i < 500; i++)
                {
                    AddLog("拍照");
                    shotFrontID = i.ToString();
                    //light.LightOn(Properties.Settings.Default.LightFrontHigh, true);
                    //AddLog("光源已打开");
                    DoCameraCmdHandle_OneShot(frontID);
                    //ThreadSleep(lightSleep);
                    //light.LightOff(true);
                    //AddLog("光源已关闭");
                    ThreadSleep(3000);
                }
#endif 
                #endregion
            });
        }

        private Form.TestForm testForm = null;

        public void TestForm()
        {
            testForm.ShowDialog();
        }

        public void MzTestForm()
        {
            new Form.TestMzPoint()
            {
                MzRunAction = (DataMzCameraLineModel model, ref int last_rgv_distance) =>
                {
                    DoRgvStopIntelligentChargeCmdHandle();
                    RunStat = true;
                    if (trainHeadLocation > 0)
                    {
                        TrainCurrentHeadDistance = trainHeadLocation;
                        MzRun(model, ref last_rgv_distance);
                    }
                }
            }.ShowDialog();
        }
        #endregion

        #region socket对象
#if controlSocket
        private class SocketBase
        {
            public string ID { get; set; }
            public string Mode { get; set; }
            public string Sn { get; set; }
        }
        private class SocketXz : SocketBase
        {
            public string CarriageLength { get; set; }
            public string ShotCount { get; set; }
        }
        private class SocketMz : SocketBase
        {
            public string GroupID { get; set; }
            public string GroupName { get; set; }
            public string RgvRunDistacnce { get; set; }
            public string Location { get; set; }
            public string Point { get; set; }
            public string CameraTypeId { get; set; }
            public string Camera3d_Id { get; set; }
            public string PartsTypeId { get; set; }
            public string RobotName { get; set; }
            public bool CanSort { get; set; }
            public string J1 { get; set; }
            public string J2 { get; set; }
            public string J3 { get; set; }
            public string J4 { get; set; }
            public string J5 { get; set; }
            public string J6 { get; set; }
        }
        private class SocketRgvInfo
        {
            public SocketRgvInfo(RgvGlobalInfo rgvGlobalInfo, DataXzCameraLineModel xzModel, DataMzCameraLineModel mzModel)
            {
                ID = Properties.Settings.Default.RobotID;
                switch (rgvGlobalInfo.RgvRunStatMonitor)
                {
                    case eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE:
                        RgvRunStatMonitor = "RUN";
                        break;
                    case eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP:
                        RgvRunStatMonitor = "STOP";
                        break;
                }
#if controlServer
                this.FrontRobotConnStat = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotConnStat;
                this.BackRobotConnStat = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotConnStat;
                switch (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor)
                {
                    case eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE:
                        this.FrontRobotRunStatMonitor = "RUN";
                        break;
                    case eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP:
                        this.FrontRobotRunStatMonitor = "STOP";
                        break;
                }
                switch (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor)
                {
                    case eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE:
                        this.BackRobotRunStatMonitor = "RUN";
                        break;
                    case eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP:
                        this.BackRobotRunStatMonitor = "STOP";
                        break;
                } 
#endif

                this.RgvCurrentCmdSetStat = rgvGlobalInfo.RgvCurrentCmdSetStat;
                this.RgvCurrentMode = rgvGlobalInfo.RgvCurrentMode;
                this.RgvCurrentParaSetStat = rgvGlobalInfo.RgvCurrentParaSetStat;
                this.RgvCurrentPowerCurrent = rgvGlobalInfo.RgvCurrentPowerCurrent;
                this.RgvCurrentPowerElectricity = rgvGlobalInfo.RgvCurrentPowerElectricity;
                this.RgvCurrentPowerStat = rgvGlobalInfo.RgvCurrentPowerStat;
                this.RgvCurrentPowerTempture = rgvGlobalInfo.RgvCurrentPowerTempture;
                this.RgvCurrentRunDistacnce = rgvGlobalInfo.RgvCurrentRunDistacnce;
                this.RgvCurrentRunSpeed = rgvGlobalInfo.RgvCurrentRunSpeed;
                this.RgvCurrentStat = rgvGlobalInfo.RgvCurrentStat;
                this.RgvIsAlarm = rgvGlobalInfo.RgvIsAlarm;
                this.RgvIsStandby = rgvGlobalInfo.RgvIsStandby;
                this.RgvTargetRunDistance = rgvGlobalInfo.RgvTargetRunDistance;
                this.RgvTargetRunSpeed = rgvGlobalInfo.RgvTargetRunSpeed;
                this.RgvTrackLength = rgvGlobalInfo.RgvTrackLength;

#if controlServer
                this.DataLine = xzModel.DataLine_Index;
                this.CarriageId = xzModel.CarriageId;
                this.CarriageType = xzModel.CarriageType;
                this.RgvCheckMinDistacnce = xzModel.RgvCheckMinDistacnce;

                this.Rgv_Distance = mzModel.Rgv_Distance;
                this.Front_Parts_Id = mzModel.Front_Parts_Id;
                this.FrontComponentType = mzModel.FrontComponentType;
                this.FrontRobot_J1 = mzModel.FrontRobot_J1;
                this.FrontRobot_J2 = mzModel.FrontRobot_J2;
                this.FrontRobot_J3 = mzModel.FrontRobot_J3;
                this.FrontRobot_J4 = mzModel.FrontRobot_J4;
                this.FrontRobot_J5 = mzModel.FrontRobot_J5;
                this.FrontRobot_J6 = mzModel.FrontRobot_J6;
                this.FrontComponentId = mzModel.FrontComponentId;
                this.Front3d_Id = mzModel.Front3d_Id;
                this.Back_Parts_Id = mzModel.Back_Parts_Id;
                this.BackComponentType = mzModel.BackComponentType;
                this.BackRobot_J1 = mzModel.BackRobot_J1;
                this.BackRobot_J2 = mzModel.BackRobot_J2;
                this.BackRobot_J3 = mzModel.BackRobot_J3;
                this.BackRobot_J4 = mzModel.BackRobot_J4;
                this.BackRobot_J5 = mzModel.BackRobot_J5;
                this.BackRobot_J6 = mzModel.BackRobot_J6;
                this.BackComponentId = mzModel.BackComponentId;
                this.Back3d_Id = mzModel.Back3d_Id; 
#endif
            }
            #region RGV
            public string ID { get; set; }
            public int RgvCurrentRunSpeed { get; set; }
            public int RgvCurrentRunDistacnce { get; set; }
            public int RgvCurrentPowerStat { get; set; }
            public int RgvCurrentPowerElectricity { get; set; }
            public int RgvCurrentPowerCurrent { get; set; }
            public int RgvCurrentPowerTempture { get; set; }
            public string RgvCurrentMode { get; set; }
            public string RgvCurrentStat { get; set; }
            public string RgvCurrentCmdSetStat { get; set; }
            public int RgvCurrentParaSetStat { get; set; }
            public int RgvIsAlarm { get; set; }
            public int RgvIsStandby { get; set; }
            public int RgvTargetRunSpeed { get; set; }
            public int RgvTargetRunDistance { get; set; }
            public int RgvTrackLength { get; set; }
            public string RgvRunStatMonitor { get; set; }
            #endregion
#if controlServer
            #region 线阵
            public int DataLine { get; set; }
            public int CarriageId { get; set; }
            public int CarriageType { get; set; }
            public int RgvCheckMinDistacnce { get; set; }
            #endregion
            #region 面阵
            public int Rgv_Distance { get; set; }
            public string Front_Parts_Id { get; set; }
            public string FrontComponentType { get; set; }
            public string FrontRobot_J1 { get; set; }
            public string FrontRobot_J2 { get; set; }
            public string FrontRobot_J3 { get; set; }
            public string FrontRobot_J4 { get; set; }
            public string FrontRobot_J5 { get; set; }
            public string FrontRobot_J6 { get; set; }
            public string FrontComponentId { get; set; }
            public int Front3d_Id { get; set; }
            public string Back_Parts_Id { get; set; }
            public string BackComponentType { get; set; }
            public string BackRobot_J1 { get; set; }
            public string BackRobot_J2 { get; set; }
            public string BackRobot_J3 { get; set; }
            public string BackRobot_J4 { get; set; }
            public string BackRobot_J5 { get; set; }
            public string BackRobot_J6 { get; set; }
            public string BackComponentId { get; set; }
            public int Back3d_Id { get; set; }
            #endregion
            #region 通用信息
            public string TrainMode { get; set; }
            public string TrainSn { get; set; }
            public int TrainCurrentHeadDistance { get; set; }
            public bool FrontRobotConnStat { get; set; }
            public string FrontRobotRunStatMonitor { get; set; }
            public bool BackRobotConnStat { get; set; }
            public string BackRobotRunStatMonitor { get; set; }
            public string Log { get; set; }
            public string Job { get; set; }
            #endregion  
#endif
        }
#endif 
        #endregion

        private class MzDataExtent
        {
            public string ID { get; set; }
            public string OnlyID { get; set; }
            public string CameraType { get; set; }
            public int ID_3D { get; set; }
            public int Distance { get; set; }
            public bool CanShot { get; set; }
            public string Location { get; set; }
            public string Point { get; set; }
            public int Axis_ID { get; set; }
            public int Axis_Distance { get; set; }
            public RobotName Robot { get; set; }
            public RobotModCtrlProtocol.RobotDataPack RobotData { get; set; }
            public MzDataExtent(DataMzCameraLineModel model, RobotName name)
            {
                Robot = name;
                Distance = model.Rgv_Distance;
                Location = model.Location;
                Point = model.Point;
                Axis_ID = (model as DataMzCameraLineModelEx).AxisID;
                Axis_Distance = (model as DataMzCameraLineModelEx).Axis_Distance;
                RobotData = new RobotModCtrlProtocol.RobotDataPack();
                switch (name)
                {
                    case RobotName.Front:
                        ID = model.Front_Parts_Id;
                        OnlyID = model.FrontComponentId;
                        CameraType = model.FrontComponentType;
                        ID_3D = model.Front3d_Id;
                        CanShot = model.FrontCamera_Enable == 0;
                        RobotData.j1 = model.FrontRobot_J1;
                        RobotData.j2 = model.FrontRobot_J2;
                        RobotData.j3 = model.FrontRobot_J3;
                        RobotData.j4 = model.FrontRobot_J4;
                        RobotData.j5 = model.FrontRobot_J5;
                        RobotData.j6 = model.FrontRobot_J6;
                        break;
                    case RobotName.Back:
                        ID = model.Back_Parts_Id;
                        OnlyID = model.BackComponentId;
                        CameraType = model.BackComponentType;
                        ID_3D = model.Back3d_Id;
                        CanShot = model.BackCamera_Enable == 0;
                        RobotData.j1 = model.BackRobot_J1;
                        RobotData.j2 = model.BackRobot_J2;
                        RobotData.j3 = model.BackRobot_J3;
                        RobotData.j4 = model.BackRobot_J4;
                        RobotData.j5 = model.BackRobot_J5;
                        RobotData.j6 = model.BackRobot_J6;
                        break;
                }
            }
        }
    }
}

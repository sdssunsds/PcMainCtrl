using Basler.Pylon;
using PcMainCtrl.Common;
using PcMainCtrl.DataAccess;
using PcMainCtrl.FtpClient;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HardWare.Stm32;
using PcMainCtrl.Model;
using PcMainCtrl.Model.PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.BaslerCamera.CameraDataHelper;

namespace PcMainCtrl.ViewModel
{
    public class XzTaskScheduleHandlePara
    {
        public bool rgvFeatureIsEnable = true;

        public bool stm32FeatureIsEnable = true;

        public bool xzCameraFeatureIsEnable = true;

        public bool ftpFeatureIsEnable = false;
    }

    public class XzTaskScheduleHandleData
    {
        //距离是否Check
        public bool rgvDistanceData_IsEnable = true;

        //高度是否Check
        public bool stm32HighData_IsEnable = false;

        //IO是否Check
        public bool stm32IOData_IsEnable = false;
    }

    public class XzTaskScheduleHandle
    {
        public int xzCameraPicDataListIndex = 0; //拼图时数据行游标计数数组

        public int rgvTaskStat = 1;       //rgv执行过程

        public int stm32TaskStat = 0;     //stm32执行过程

        public int xzCameraTaskStat = 1;   //xzCamera执行过程

        public int xzCameraPicIndex = 0;               //拍图游标计数
        public bool xzCameraPicIndexIsValid = false;   //拍图游标计数有效的(同一个地方实际会出来很多计数)

        public DataXzCameraLineModel dataline = new DataXzCameraLineModel();

        public XzTaskScheduleHandlePara handlePara = new XzTaskScheduleHandlePara();

        public XzTaskScheduleHandleData handleData = new XzTaskScheduleHandleData();
    }

    public class TaskXzCameraViewModel : NotifyBase
    {
        public const int RgvTrackLength = 250000;
        
        public int LastPicJoin = 0;

        public int CarriageId = 0;
        public bool RunStat = false;

        public int TrainCurrentHeadDistance { get; set; }

        public TaskXzCameraModel taskXzCameraModel { get; set; } = new TaskXzCameraModel();

        public XzTaskScheduleHandle taskScheduleHandleInfo { get; set; } = new XzTaskScheduleHandle();

        public List<DataXzCameraLineModel> xzCameraDataList { get; set; } = new List<DataXzCameraLineModel>();

        //Pic计数的信号量
        public Semaphore SendCmdSemaphore = null;

        //Pic计数
        public List<Int32> xzCameraPicDataList { get; set; } = new List<int>();

        //启动拼图
        public bool xzCameraPicDataJoinIsEnable = false;

        private string grabImg3;
        public string GrabImg3
        {
            get { return grabImg3; }
            set
            {
                grabImg3 = value;
                this.DoNotify();
            }
        }

        public CommandBase OneKeyStartCmd { get; set; }

        public CommandBase RgvForwardMotorCmd { get; set; }

        public CommandBase RgvBackMotorCmd { get; set; }

        public CommandBase RgvNormalStopCmd { get; set; }

        public CommandBase RgvIntelligentChargeCmd { get; set; }

        public CommandBase RgvClearAlarmCmd { get; set; }

        public TaskXzCameraViewModel()
        {
            this.OneKeyStartCmd = new CommandBase();
            this.OneKeyStartCmd.DoExecute = new Action<object>(DoOneKeyStartCmdHandle);
            this.OneKeyStartCmd.DoCanExecute = new Func<object, bool>((o) => true);

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


            //初始化Rgv
            if (taskScheduleHandleInfo.handlePara.rgvFeatureIsEnable == true)
            {
                //挂接事件
                RgvModCtrlHelper.GetInstance().RgvModInfoEvent += MyRgvModInfoEvent;

                InitRgvMod();
            }

            //初始化Stm32
            if (taskScheduleHandleInfo.handlePara.stm32FeatureIsEnable == true)
            {
                //挂接事件
                Stm32ModCtrlHelper.GetInstance().Stm32ModInfoEvent += MyStm32ModInfoEvent;

                InitStm32Mod();
            }

            //初始化相机
            if (taskScheduleHandleInfo.handlePara.xzCameraFeatureIsEnable == true)
            {
                InitCameraMod();
            }

            //查询数据库表中数据,挂在List上面
            xzCameraDataList.Clear();
            var cList = LocalDataBase.GetInstance().XzCameraDataListQurey();
            foreach (var item in cList)
            {
                xzCameraDataList.Add(item);
            }
            Trace.WriteLine("xzCameraDataList count ={0}", xzCameraDataList.Count.ToString());
            Task.Delay(3000);

            //创建命令发送信号
            SendCmdSemaphore = new Semaphore(1, 1);

            Debug.WriteLine("TaskXzCamera Is Start");
        }

        private void DoOneKeyStartCmdHandle(object obj)
        {
            Task t1, t2, t3;

            RunStat = true;

            //t1先串行:查询数据库表中数据,挂在List上面
            t1 = Task.Factory.StartNew(() =>
            {
                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //Rgv执行任务
                if (taskScheduleHandleInfo.handlePara.rgvFeatureIsEnable)
                {
                    //Rgv运动到指定位置
                    if (taskScheduleHandleInfo.rgvTaskStat == 1)
                    {
                        Trace.WriteLine(String.Format("\r\nStart Train Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                        DoRgvSetTargetDistanceCmdHandle(RgvTrackLength);
                    }
                }
            });
            Task.WaitAny(t1);

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //车头检测
            t2 = t1.ContinueWith(t =>
            {
                if (taskScheduleHandleInfo.handlePara.xzCameraFeatureIsEnable == true)
                {
                    while (true)
                    {
                        Trace.WriteLine(String.Format("\r\nCurrent Train Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                        //Trace.WriteLine(String.Format("Current Train Io1: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed1_Stat));
                        //Trace.WriteLine(String.Format("Current Train Io3: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed3_Stat));
#if false
                        if ((RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce >= 9500) &&
                            (Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.CurrentDetectHeight <= 150) &&
                            (Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.CurrentDetectHeight > 100)
                           )
#else
                        if ((RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce >= 11000) &&
                            (Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed1_Stat == 1) &&
                            (Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed3_Stat == 1))
#endif
                        {
                            Trace.WriteLine(String.Format("\r\nCheck Train Head: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                            Trace.WriteLine(String.Format("Check Train Io1: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed1_Stat));
                            Trace.WriteLine(String.Format("Check Train Io3: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed3_Stat));

                            //记录车头位置
                            TrainCurrentHeadDistance = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;

                            //相机执行任务
                            DoCameraCmdHandle_ContinuousShot(taskXzCameraModel.XzCamerainfoItem);

                            xzCameraPicDataList.Add(0);//将第1张加入数组
                            //taskScheduleHandleInfo.xzCameraPicDataListIndex++;
                            break;
                        }

                        Task.Delay(10);
                    }
                }
            });

#if true
            //线阵流程
            t3 = t2.ContinueWith(t =>
            {
                //判断拼图的条件:取决于xzCameraPicDataList
                while (true)
                {
                    if (xzCameraPicDataList.Count >= 1 && (xzCameraPicDataJoinIsEnable == true))
                    {
                        xzCameraPicDataJoinIsEnable = false;

                        DoCameraCmdHandle_Partial_Join(xzCameraPicDataList[xzCameraPicDataList.Count - 2], xzCameraPicDataList[xzCameraPicDataList.Count - 1] - 1);
                    }

                    Task.Delay(50);

                    //检测到IO2和IO4有效 &&拼接了7张
                    if ((taskScheduleHandleInfo.xzCameraPicDataListIndex == 7) &&
                            (LastPicJoin == 0) &&
                            (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce - TrainCurrentHeadDistance >= 204000) &&
                            (Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed2_Stat == 0) &&
                            (Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed4_Stat == 0))
                    {
                        LastPicJoin = 1;
                        Trace.WriteLine("Check Tail, Last Join");
                        Trace.WriteLine(String.Format("Check Train Io2: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed2_Stat));
                        Trace.WriteLine(String.Format("Check Train Io4: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed4_Stat));

                        DoCameraCmdHandle_Stop(null);

                        //尾巴的拼图:传入Pic起止的两个序号
                        DoCameraCmdHandle_Partial_Join(xzCameraPicDataList[xzCameraPicDataList.Count - 1], taskScheduleHandleInfo.xzCameraPicIndex);
                    }

                    //RGV停车
                    Task.Delay(50);
                    if(RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce- TrainCurrentHeadDistance >= 210000)
                    {
                        Trace.WriteLine("Train Stop");
                        Trace.WriteLine(String.Format("\r\n!!!Run Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                        DoRgvNormalStopCmdHandle(null);

                        break;
                    }

#if false
                    //车已经开到指定位置
                    if ((RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor == eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP) &&
                     (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce >= RgvTrackLength-10))
                    {
                        Trace.WriteLine("Train Stop");
                        Trace.WriteLine(String.Format("\r\n!!!Run Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                        DoCameraCmdHandle_Stop(null);

                        //尾巴的拼图:传入Pic起止的两个序号
                        DoCameraCmdHandle_Partial_Join(xzCameraPicDataList[xzCameraPicDataList.Count - 1], taskScheduleHandleInfo.xzCameraPicIndex);
                        break;
                    }
#endif
                    }
            });

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //判别Camera是否已经完成指定拍照动作
            if (taskScheduleHandleInfo.xzCameraTaskStat == 1)
            {
                while (true)
                {
                    if (CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor == eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP)
                    {
                        CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                        break;
                    }

                    Task.Delay(50);
                }
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //等待所有任务都完成
            Task.WaitAll(t1, t2, t3);

            LastPicJoin = 0;
            RunStat = false;
            TrainCurrentHeadDistance = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //结束任务流程时的动作,启动Ftp上传流程
            TaskRun(new Action(() =>
            {
                if (taskScheduleHandleInfo.handlePara.ftpFeatureIsEnable == true)
                {
                    DoFtpLoginCmdHandle(null);
                    DoFtpZipUploadCmdHandle(@"CH380AL-123456");
                }
            }));
#endif
                }

#region Ftp机制
        /// <summary>
        /// 列出目录
        /// </summary>
        private void ListDirectory()
        {
            bool isOk = false;

            string ipAddr = Properties.Settings.Default.FtpIP;
            string port = Properties.Settings.Default.FtpPort;
            string userName = Properties.Settings.Default.FtpUser;
            string password = Properties.Settings.Default.FtpPwd;

            if (string.IsNullOrEmpty(ipAddr) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                System.Windows.Forms.MessageBox.Show("请输入登录信息");
                return;
            }

            FtpHelper ftpHelper = FtpHelper.GetInstance(ipAddr, port, userName, password);
            string[] arrAccept = ftpHelper.ListDirectory(out isOk);//调用Ftp目录显示功能
            if (isOk)
            {
            }
            else
            {
                ftpHelper.SetPrePath();
            }
        }

        /// <summary>
        /// Ftp登录
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpLoginCmdHandle(object obj)
        {
            try
            {
                ListDirectory();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Exception caught:\n" + exception.Message);
            }
        }

        /// <summary>
        /// Ftp直接上传
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpDirectUploadCmdHandle(object obj)
        {
            String path = obj as String;

            string ipAddr = @"192.168.0.102";
            string port = @"21";
            string userName = @"robot";
            string password = @"robot";

            FtpHelper ftpHelper = FtpHelper.GetInstance(ipAddr, port, userName, password);
            if (File.Exists(path))
            {
                ftpHelper.RelatePath = string.Format("{0}/{1}", ftpHelper.RelatePath, Path.GetFileName(path));

                bool isOk = false;
                ftpHelper.UpLoad(path, out isOk);
                ftpHelper.SetPrePath();
                if (isOk)
                {
                    this.ListDirectory();
                }
                else
                {

                }
            }
        }


        /// <summary>
        /// 线阵压缩
        /// </summary>
        /// <param name="obj"></param>
        private void DoCameraCmdHandle_Compress1(object obj)
        {
            String name = obj as String;

            List<Image> img_list = new List<Image>();
            img_list.Clear();

            //读取本地的所有图片文件
            String src_dir = System.Environment.CurrentDirectory + @"\task_data\xz_join\";
            String dest_dir = System.Environment.CurrentDirectory + @"\task_data\xz_compress\" + name + ".zip";

            CameraDataHelper.CreateZipFile(src_dir, dest_dir);
        }

        /// <summary>
        /// Ftp压缩后上传
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpZipUploadCmdHandle(object obj)
        {
            //压缩打包
            //String path = obj as String;
            String path = @"CH380AL-123456";
            DoCameraCmdHandle_Compress1(path);

            //找到压缩文件
            String dest_path = System.Environment.CurrentDirectory + @"\task_data\xz_compress\" + path + ".zip";

            //上传文件
            string ipAddr = @"192.168.0.102";
            string port = @"21";
            string userName = @"robot";
            string password = @"robot";

            FtpHelper ftpHelper = FtpHelper.GetInstance(ipAddr, port, userName, password);

            if (File.Exists(dest_path))
            {
                ftpHelper.RelatePath = string.Format("{0}/{1}", ftpHelper.RelatePath, Path.GetFileName(dest_path));

                bool isOk = false;
                ftpHelper.UpLoad(dest_path, out isOk);
                ftpHelper.SetPrePath();
                if (isOk)
                {
                    this.ListDirectory();
                }
                else
                {
                }
            }
        }
#endregion

#region Stm32 IO检测
        /// <summary>
        /// Stm32继电器信息事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void MyStm32ModInfoEvent(Stm32GlobalInfo stm32info)
        {
            try
            {
                //数据协议信息
                taskXzCameraModel.CurrentDetectHeight = stm32info.CurrentDetectHeight;
                taskXzCameraModel.InfraRed1_Stat = stm32info.InfraRed1_Stat;
                taskXzCameraModel.InfraRed2_Stat = stm32info.InfraRed1_Stat;
                taskXzCameraModel.InfraRed3_Stat = stm32info.InfraRed3_Stat;
                taskXzCameraModel.InfraRed4_Stat = stm32info.InfraRed4_Stat;
            }
            catch
            {

            }
        }

        private async void InitStm32Mod()
        {
            try
            {
                //连接Stm32设备
                Stm32ModCtrlHelper.GetInstance().Stm32ModConnect();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }


            await Task.Delay(100);
        }
#endregion

#region 线阵相机

        /// <summary>
        /// 检查Stm32信息是否满足条件
        /// </summary>
        private void Stm32_CheckInfo()
        {
            //查询Stm32的数据满足数据库中的哪个参数行(min_distance,max_distance, high, io1, io2, io3, io4)
            Trace.WriteLine(taskXzCameraModel.RgvCurrentRunDistacnce.ToString(),"[check distance]={0}");
            //Trace.WriteLine(taskXzCameraModel.InfraRed1_Stat.ToString(), "[check io1]={0}");
            //Trace.WriteLine(taskXzCameraModel.InfraRed3_Stat.ToString(), "[check io3]={0}");

            //检测距离
            if (taskScheduleHandleInfo.handleData.rgvDistanceData_IsEnable == true)
            {
                if (taskScheduleHandleInfo.xzCameraPicDataListIndex < xzCameraDataList.Count && 
                    (taskXzCameraModel.RgvCurrentRunDistacnce- TrainCurrentHeadDistance) > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex].RgvCheckMinDistacnce &&
                  (taskXzCameraModel.RgvCurrentRunDistacnce - TrainCurrentHeadDistance) < xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex].RgvCheckMaxDistacnce)
                {
                    //检测高度
                    if (taskScheduleHandleInfo.handleData.stm32HighData_IsEnable == true)
                    {
                        if (taskXzCameraModel.CurrentDetectHeight > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].CurrentDetectMinHeight &&
                        taskXzCameraModel.RgvCurrentRunDistacnce > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].CurrentDetectMaxHeight)
                        {
                            //检测IO
                            if (taskScheduleHandleInfo.handleData.stm32IOData_IsEnable == true)
                            {
                                if (taskXzCameraModel.InfraRed1_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed1_Stat &&
                                taskXzCameraModel.InfraRed2_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed2_Stat &&
                                taskXzCameraModel.InfraRed3_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed3_Stat &&
                                taskXzCameraModel.InfraRed4_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed4_Stat)
                                {
                                    if (taskScheduleHandleInfo.xzCameraPicIndexIsValid == false)
                                    {
                                        //防抖
                                        taskScheduleHandleInfo.xzCameraPicIndexIsValid = true;

                                        //管理数组
                                        xzCameraPicDataList.Add(taskScheduleHandleInfo.xzCameraPicIndex);

                                        taskScheduleHandleInfo.xzCameraPicDataListIndex++;

                                        //记录当前的图片编号
                                        Trace.WriteLine("Join xzCameraPicDataListIndex={0}", taskScheduleHandleInfo.xzCameraPicDataListIndex.ToString());
                                        xzCameraPicDataJoinIsEnable = true;//启动拼图

                                    }
                                }
                            }
                            else
                            {
                                Trace.WriteLine("[join distance]={0}", taskXzCameraModel.RgvCurrentRunDistacnce.ToString());
                                Trace.WriteLine("[join high]={0}", taskXzCameraModel.CurrentDetectHeight.ToString());
                                if (taskScheduleHandleInfo.xzCameraPicIndexIsValid == false && taskScheduleHandleInfo.xzCameraPicDataListIndex < xzCameraDataList.Count)
                                {
                                    //防抖
                                    taskScheduleHandleInfo.xzCameraPicIndexIsValid = true;

                                    //管理数组
                                    xzCameraPicDataList.Add(taskScheduleHandleInfo.xzCameraPicIndex);

                                    taskScheduleHandleInfo.xzCameraPicDataListIndex++;

                                    //记录当前的图片编号
                                    Trace.WriteLine("Join xzCameraPicDataListIndex={0}", taskScheduleHandleInfo.xzCameraPicDataListIndex.ToString());
                                    xzCameraPicDataJoinIsEnable = true;//启动拼图
                                }
                            }
                        }
                    }
                    else
                    {
                        //检测IO
                        if (taskScheduleHandleInfo.handleData.stm32IOData_IsEnable == true)
                        {
                            if (taskXzCameraModel.InfraRed1_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed1_Stat &&
                            taskXzCameraModel.InfraRed2_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed2_Stat &&
                            taskXzCameraModel.InfraRed3_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed3_Stat &&
                            taskXzCameraModel.InfraRed4_Stat == xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed4_Stat)
                            {
                                if (taskScheduleHandleInfo.xzCameraPicIndexIsValid == false)
                                {
                                    //防抖
                                    taskScheduleHandleInfo.xzCameraPicIndexIsValid = true;

                                    //管理数组
                                    xzCameraPicDataList.Add(taskScheduleHandleInfo.xzCameraPicIndex);
                                    taskScheduleHandleInfo.xzCameraPicDataListIndex++;


                                    //记录当前的图片编号
                                    xzCameraPicDataJoinIsEnable = true;//启动拼图
                                }
                            }
                        }
                        else
                        {
                            Trace.WriteLine("distance ={0}", taskXzCameraModel.RgvCurrentRunDistacnce.ToString());
                            if (taskScheduleHandleInfo.xzCameraPicIndexIsValid == false && taskScheduleHandleInfo.xzCameraPicDataListIndex < xzCameraDataList.Count)
                            {
                                //防抖
                                taskScheduleHandleInfo.xzCameraPicIndexIsValid = true;

                                //管理数组
                                xzCameraPicDataList.Add(taskScheduleHandleInfo.xzCameraPicIndex);

                                //拼图时数据行游标计数数组
                                taskScheduleHandleInfo.xzCameraPicDataListIndex++;

                                //记录当前的图片编号
                                Trace.WriteLine("Join xzCameraPicDataListIndex={0}", taskScheduleHandleInfo.xzCameraPicDataListIndex.ToString());
                                xzCameraPicDataJoinIsEnable = true;//启动拼图
                            }
                        }
                    }
                }
                else
                {
                    //停止拍照
                    if (taskXzCameraModel.RgvCurrentRunDistacnce >= RgvTrackLength)
                    {
                        DoCameraCmdHandle_Stop(null);
                    }

                    taskScheduleHandleInfo.xzCameraPicIndexIsValid = false;
                }
            }
            else
            {
                //检测高度
                if (taskScheduleHandleInfo.handleData.stm32HighData_IsEnable == true)
                {
                    if (taskXzCameraModel.CurrentDetectHeight > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].CurrentDetectMinHeight &&
                    taskXzCameraModel.RgvCurrentRunDistacnce > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].CurrentDetectMaxHeight)
                    {
                        //检测IO
                        if (taskScheduleHandleInfo.handleData.stm32IOData_IsEnable == true)
                        {
                            if (taskXzCameraModel.InfraRed1_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed1_Stat &&
                            taskXzCameraModel.InfraRed2_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed2_Stat &&
                            taskXzCameraModel.InfraRed3_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed3_Stat &&
                            taskXzCameraModel.InfraRed4_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed4_Stat)
                            {
                                if (taskScheduleHandleInfo.xzCameraPicIndexIsValid == false)
                                {
                                    //防抖
                                    taskScheduleHandleInfo.xzCameraPicIndexIsValid = true;

                                    //管理数组
                                    xzCameraPicDataList.Add(taskScheduleHandleInfo.xzCameraPicIndex);
                                    taskScheduleHandleInfo.xzCameraPicDataListIndex++;


                                    //记录当前的图片编号
                                    xzCameraPicDataJoinIsEnable = true;//启动拼图
                                }
                            }
                        }
                    }
                    else
                    {
                        //检测IO
                        if (taskScheduleHandleInfo.handleData.stm32IOData_IsEnable == true)
                        {
                            if (taskXzCameraModel.InfraRed1_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed1_Stat &&
                            taskXzCameraModel.InfraRed2_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed2_Stat &&
                            taskXzCameraModel.InfraRed3_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed3_Stat &&
                            taskXzCameraModel.InfraRed4_Stat > xzCameraDataList[taskScheduleHandleInfo.xzCameraPicDataListIndex - 1].InfraRed4_Stat)
                            {
                                if (taskScheduleHandleInfo.xzCameraPicIndexIsValid == false)
                                {
                                    //防抖
                                    taskScheduleHandleInfo.xzCameraPicIndexIsValid = true;

                                    //管理数组
                                    xzCameraPicDataList.Add(taskScheduleHandleInfo.xzCameraPicIndex);
                                    taskScheduleHandleInfo.xzCameraPicDataListIndex++;

                                    //记录当前的图片编号
                                    xzCameraPicDataJoinIsEnable = true;//启动拼图
                                }
                            }
                        }
                    }
                }
                else
                {
                    taskScheduleHandleInfo.xzCameraPicIndexIsValid = false;
                }
            }

            //递增Pic计数
            taskScheduleHandleInfo.xzCameraPicIndex++;
        }

        private void Camera_CameraImageEvent(Camera camera, Bitmap bmp)
        {
            try
            {
                //获取信号量
                SendCmdSemaphore.WaitOne();

                //保存图片到本地
                String strheigth = bmp.Height.ToString();

                ICameraInfo camerainfo = camera.CameraInfo;
                if (camerainfo[CameraInfoKey.FriendlyName].Contains(@"camera_xz"))
                {
                    //启动图片传输任务
                    String strfile = System.Environment.CurrentDirectory + @"\task_data\xz_camera\" + taskScheduleHandleInfo.xzCameraPicIndex.ToString("10000") + @".jpg";
                    CameraDataHelper.SaveBitmapIntoFile(bmp, strfile, CameraDataHelper.JoinMode.Vertical);

                    //拍完一张图片
                    GrabImg3 = strfile;
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;

                    //判断当前Stm32的状态值是否满足条件,并记录张数标号进数据库,为后续拼图上传做准备
                    Stm32_CheckInfo();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }

            //释放信号量
            try
            {
                SendCmdSemaphore.Release();
            }
            catch
            {

            }
        }

        private async void InitCameraMod()
        {
            try
            {
                //搜索可用相机
                CameraCtrlHelper.GetInstance().CameraConnection();

                //管理相机设备
                foreach (var item in CameraCtrlHelper.GetInstance().myCameraList)
                {
                    //找出具体对应的相机
                    if (item[CameraInfoKey.FriendlyName].Contains(@"camera_xz"))
                    {
                        taskXzCameraModel.XzCamerainfoItem = item as ICameraInfo;
                    }
                }

                //挂接事件
                CameraCtrlHelper.GetInstance().CameraImageEvent += Camera_CameraImageEvent;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }

            await Task.Delay(1000);
        }

        private void DoCameraCmdHandle_OneShot(object obj)
        {
            ICameraInfo item = obj as ICameraInfo;

            //默认选中挂接到第1个相机
            CameraCtrlHelper.GetInstance().CameraChangeItem(item);
            CameraCtrlHelper.GetInstance().CameraOneShot();
        }

        private void DoCameraCmdHandle_ContinuousShot(object obj)
        {
            ICameraInfo item = obj as ICameraInfo;

            //默认选中挂接到第1个相机
            CameraCtrlHelper.GetInstance().CameraChangeItem(item);
            Task.Delay(500);

            Trace.WriteLine("Get Camera Device Information");
            Trace.WriteLine("=========================");
            Trace.WriteLine("Width           : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].GetValue().ToString());
            Trace.WriteLine("Height            : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].GetValue().ToString());
            Trace.WriteLine("ExposureTimeAbs : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].GetValue().ToString());
            Console.WriteLine("======================");

            //设置相机参数
            CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].SetValue(Properties.Settings.Default.CameraWidth, IntegerValueCorrection.Nearest);
            CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].SetValue(Properties.Settings.Default.CameraHeight, IntegerValueCorrection.Nearest);
            CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(Properties.Settings.Default.CameraExposureTime);
            Task.Delay(500);

            Trace.WriteLine("Get Camera Device Information");
            Trace.WriteLine("=========================");
            Trace.WriteLine("Width           : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].GetValue().ToString());
            Trace.WriteLine("Height            : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].GetValue().ToString());
            Trace.WriteLine("ExposureTimeAbs : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].GetValue().ToString());
            Console.WriteLine("======================");

            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
            CameraCtrlHelper.GetInstance().CameraContinuousShot();
        }

        void DoCameraCmdHandle_Stop(object obj)
        {
            CameraCtrlHelper.GetInstance().CameraStop();
            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
        }

        /// <summary>
        /// 线阵拼图
        /// </summary>
        /// <param name="obj"></param>
        void DoCameraCmdHandle_Join(object obj)
        {
            List<Image> img_list = new List<Image>();
            img_list.Clear();

            TaskRun(new Action(() =>
            {
                //1.首先读取本地的所有图片文件
                DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory + @"\xz_compress\");
                FileInfo[] inf = dir.GetFiles();
                foreach (FileInfo finf in inf)
                {
                    //如果扩展名为“.bmp”
                    if (finf.Extension.Equals(".jpg"))
                    {
                        //读取文件的完整目录和文件名
                        Image newImage = Image.FromFile(finf.FullName);
                        img_list.Add(newImage);
                    }
                }

                //2.然后进行拼接
                Image img = CameraDataHelper.JoinImage(img_list, JoinMode.Vertical);

                String strfile = System.Environment.CurrentDirectory + @"\xz_join\join.jpg";
                CameraDataHelper.SaveBitmapIntoFile(img as Bitmap, strfile, CameraDataHelper.JoinMode.Vertical);
            }));
        }

        /// <summary>
        /// 部分图片线阵拼图
        /// </summary>
        /// <param name="obj"></param>
        void DoCameraCmdHandle_Partial_Join(object obj1, object obj2)
        {
            Int32 pic_index1 = (Int32)obj1 + 10000;
            Int32 pic_index2 = (Int32)obj2 + 10000;

            List<Image> img_list = new List<Image>();
            img_list.Clear();

            //TaskRun(new Action(() =>
            //{

            //}));

            //1.首先读取本地的所有图片文件
            DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory + @"\task_data\xz_camera\");
            FileInfo[] inf = dir.GetFiles();
            foreach (FileInfo finf in inf)
            {
                //如果扩展名为“.bmp”
                if (finf.Extension.Equals(".jpg"))
                {
                    //表示取到文件
                    int filename_num = -1;
                    String filename = System.IO.Path.GetFileNameWithoutExtension(finf.Name);     //返回不带扩展名的文件名 
                    int.TryParse(filename, out filename_num);
                    Trace.WriteLine("filename_num:{0}", filename_num.ToString());

                    if (filename_num != -1 && (filename_num >= pic_index1 && filename_num <= pic_index2))
                    {
                        //读取文件的完整目录和文件名
                        Image newImage = Image.FromFile(finf.FullName);
                        img_list.Add(newImage);
                    }
                }
            }

            //2.然后进行拼接
            //图片规则是：380AL - 2637 - 1 - 00000001.jpg.
            //车型号 - 车编号 - 车厢号 - 机器人编号

            Image img = CameraDataHelper.JoinImage(img_list, JoinMode.Vertical);
            String strfile = System.Environment.CurrentDirectory + @"\task_data\xz_join\" + pic_index1.ToString() + @"-" + pic_index2.ToString() + @".jpg";

            //String strfile = System.Environment.CurrentDirectory + @"\task_data\xz_join\380AL-2637-" + CarriageId.ToString() + @"-00000001" + @".jpg";
            CameraDataHelper.SaveBitmapIntoFile(img as Bitmap, strfile, CameraDataHelper.JoinMode.Vertical);

            CarriageId++;
        }

        /// <summary>
        /// 线阵压缩
        /// </summary>
        /// <param name="obj"></param>
        void DoCameraCmdHandle_Compress(object obj)
        {
            List<Image> img_list = new List<Image>();
            img_list.Clear();

            TaskRun(new Action(() =>
            {
                //1.首先读取本地的所有图片文件
                DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory + @"\xz_camera\");
                FileInfo[] inf = dir.GetFiles();
                foreach (FileInfo finf in inf)
                {
                    //如果扩展名为“.jpg”
                    if (finf.Extension.Equals(".jpg"))
                    {
                        //读取文件的完整目录和文件名
                        Image newImage = Image.FromFile(finf.FullName);

                        //2.然后进行压缩
                        CameraDataHelper.SaveBitmapIntoFile((Bitmap)newImage, System.Environment.CurrentDirectory + @"\xz_compress\" + finf.Name, CameraDataHelper.JoinMode.Vertical);
                    }
                }
            }));
        }
#endregion

#region Rgv控制
        private void MyRgvModInfoEvent(RgvGlobalInfo rgvinfo)
        {
            try
            {
                //数据协议信息
                taskXzCameraModel.RgvCurrentStat = rgvinfo.RgvCurrentStat; //默认小车中间状态Msg信息

                taskXzCameraModel.RgvCurrentMode = rgvinfo.RgvCurrentMode; //默认远程模式(自动)； 本地模式(手动)

                taskXzCameraModel.RgvCurrentRunSpeed = rgvinfo.RgvCurrentRunSpeed;
                taskXzCameraModel.RgvCurrentRunDistacnce = rgvinfo.RgvCurrentRunDistacnce;
                taskXzCameraModel.RgvCurrentPowerElectricity = rgvinfo.RgvCurrentPowerElectricity;
            }
            catch
            {

            }
        }

        private async void InitRgvMod()
        {
            try
            {
                //连接Rgv设备
                RgvModCtrlHelper.GetInstance().RgvModConnect();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }

            await Task.Delay(1000);
        }
        private void DoRgvForwardMotorCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_FORWARDMOTOR);
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
            }
        }

        private void DoRgvBackMotorCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_BACKWARDMOTOR);
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
            }
        }

        /// <summary>
        /// 停止的时候除RGV，机械臂得保持安全点状态位置
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvNormalStopCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_NORMALSTOP);
            }
        }

        private void DoRgvClearAlarmCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_CLEARALARM);
            }
        }

        /// <summary>
        /// RgvCmd-设定目标运行位置
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvSetTargetDistanceCmdHandle(object obj)
        {
            int distance = (int)obj;
            if (distance > RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTrackLength || distance < 0)
            {
                System.Windows.MessageBox.Show("Rgv运行目标距离,长度设置非法:超过轨道长度");
            }
            else
            {
                if (RgvModCtrlHelper.GetInstance().IsEnable)
                {
                    //设置速度
                    int speed = 800;
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunSpeed = speed;
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETSPEED);
                    var t1 = Task.Delay(100);
                    Task.WaitAny(t1);

                    //设置距离
                    taskXzCameraModel.RgvTargetRunDistance = distance;
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunDistance = taskXzCameraModel.RgvTargetRunDistance;
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETDISATNCE);
                    var t2 = Task.Delay(100);
                    Task.WaitAny(t2);

                    //运动到指定目标位置
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_RUNAPPOINTDISTANCE);
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                    Task.Delay(100);
                }
            }
        }
#endregion
    }
}
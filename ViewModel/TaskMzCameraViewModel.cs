using Basler.Pylon;
using PcMainCtrl.Common;
using PcMainCtrl.DataAccess;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.HardWare.NetworkRelay;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HardWare.Robot;
using PcMainCtrl.HardWare.Stm32;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.Robot.RobotModCtrlProtocol;

namespace PcMainCtrl.ViewModel
{
    public class MzTaskScheduleHandlePara
    {
        public bool rgvFeatureIsEnable = true;

        public bool stm32FeatureIsEnable = true;

        public bool robotFeatureIsEnable = true;

        public bool relayFeatureIsEnable = true;

        public bool mzCameraFeatureIsEnable = true;
    }

    public class MzTaskScheduleHandle
    {
        public int mzCameraDataIndex = 0; //数据行游标计数

        public int rgvTaskStat = 0;        //rgv执行过程

        public int frontRobotTaskStat = 0; //frontRobot执行过程
        public int frontCameraTaskStat = 0;

        public int backRobotTaskStat = 0;  //backRobot执行过程
        public int backCameraTaskStat = 0;

        public DataMzCameraLineModel dataline = new DataMzCameraLineModel();

        public MzTaskScheduleHandlePara handlePara = new MzTaskScheduleHandlePara();
    }

    public class TaskMzCameraViewModel : NotifyBase
    {
        public int TrainCurrentHeadDistance { get; set; }

        public bool TaskMzCameraIsRunning { get; set; } = false;

        public TaskMzCameraModel taskMzCameraModel { get; set; } = new TaskMzCameraModel();
        public MzTaskScheduleHandle taskScheduleHandleInfo { get; set; } = new MzTaskScheduleHandle();
        public List<DataMzCameraLineModel> mzCameraDataList { get; set; } = new List<DataMzCameraLineModel>();

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

        /// <summary>
        /// Balser camera捕捉图像
        /// </summary>
        private string grabImg1;
        public string GrabImg1
        {
            get { return grabImg1; }
            set
            {
                grabImg1 = value;
                this.DoNotify();
            }
        }

        private string grabImg2;
        public string GrabImg2
        {
            get { return grabImg2; }
            set
            {
                grabImg2 = value;
                this.DoNotify();
            }
        }

        public CommandBase OneKeyStartCmd { get; set; }
        public CommandBase OneKeyStopCmd { get; set; }

        public CommandBase RgvForwardMotorCmd { get; set; }

        public CommandBase RgvBackMotorCmd { get; set; }

        public CommandBase RgvNormalStopCmd { get; set; }

        public CommandBase RgvIntelligentChargeCmd { get; set; }

        public CommandBase RgvClearAlarmCmd { get; set; }

        public TaskMzCameraViewModel()
        {
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

            FrontRobot_J1 = @"1.23";

            //查询数据库表中数据,挂在List上面
            mzCameraDataList.Clear();
            var cList = LocalDataBase.GetInstance().MzCameraDataListQurey();
            foreach (var item in cList)
            {
                mzCameraDataList.Add(item);
            }

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

            //初始化Robot
            if (taskScheduleHandleInfo.handlePara.robotFeatureIsEnable == true)
            {
                //挂接事件
                RobotModCtrlHelper.GetInstance().RobotModInfoEvent += MyRobotModInfoEvent;
                InitRobotMod();
            }

            //初始化继电器
            if (taskScheduleHandleInfo.handlePara.relayFeatureIsEnable == true)
            {
                //挂接事件
                NetworkRelayCtrlHelper.GetInstance().NetworkRelayModInfoEvent += MyNetworkRelayModInfoEvent;
                InitRelayMod();
                Task.Delay(3000);
            }

            //初始化相机
            if (taskScheduleHandleInfo.handlePara.mzCameraFeatureIsEnable == true)
            {
                InitCameraMod();
            }

            Task.Delay(3000);
        }

        private async void DoOneKeyStartCmdHandle(object obj)
        {
            //流程执行
            Task t1, t2, t3;

            //Task t11;

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //第1部分：执行车头检测
            if (taskScheduleHandleInfo.handlePara.rgvFeatureIsEnable)
            {
                //Rgv运动到指定位置
                Trace.WriteLine(String.Format("\r\nStart Train Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                DoRgvSetTargetDistanceCmdHandle(30000);

                //车头检测
                while (true)
                {
                    //Trace.WriteLine(String.Format("\r\nCurrent Train Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                    //Trace.WriteLine(String.Format("Current Train Io1: {0}\r\n", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed1_Stat));
                    //Trace.WriteLine(String.Format("Current Train Io3: {0}\r\n", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed3_Stat));

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
                        
                        //Rgv连续两次“运动到指定位置”会产生两次定位数据
                        RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_NORMALSTOP);
                        
                        while (true)
                        {
                            Trace.WriteLine(String.Format("\r\nCheck Train Stop: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                            if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                            {
                                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                                break;
                            }

                            ThreadSleep(50);
                        }


                        break;
                    }

                    ThreadSleep(10);
                }
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //第2部分：开始执行面阵流程
            TaskMzCameraIsRunning = true;
            int last_rgv_distance = 0;
            for (int i = 0; i < mzCameraDataList.Count; ++i)
            {
                //t2:找到要执行的数据行
                taskScheduleHandleInfo.mzCameraDataIndex = mzCameraDataList[i].DataLine_Index;

                taskScheduleHandleInfo.rgvTaskStat = mzCameraDataList[i].Rgv_Enable;

                taskScheduleHandleInfo.frontRobotTaskStat = mzCameraDataList[i].FrontRobot_Enable;
                taskScheduleHandleInfo.frontCameraTaskStat = mzCameraDataList[i].FrontCamera_Enable;

                taskScheduleHandleInfo.backRobotTaskStat = mzCameraDataList[i].BackRobot_Enable;
                taskScheduleHandleInfo.backCameraTaskStat = mzCameraDataList[i].BackCamera_Enable;

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //Rgv执行任务
                t1 = Task.Factory.StartNew(() =>
                {
                    if (taskScheduleHandleInfo.handlePara.rgvFeatureIsEnable)
                    {
                        //判断Rgv是否需要移动
                        if (mzCameraDataList[i].Rgv_Distance != last_rgv_distance)
                        {
                            //Rgv运动到指定位置
                            if (taskScheduleHandleInfo.rgvTaskStat == 1)
                            {
                                last_rgv_distance = mzCameraDataList[i].Rgv_Distance;
                                Trace.WriteLine(String.Format("\r\n!!!Run Distance: {0}", mzCameraDataList[i].Rgv_Distance + TrainCurrentHeadDistance));
                                DoRgvSetTargetDistanceCmdHandle(mzCameraDataList[i].Rgv_Distance + TrainCurrentHeadDistance);
                            }

                            //判别Rgv是否已经运动到指定位置
                            if (taskScheduleHandleInfo.rgvTaskStat == 1)
                            {
                                while (true)
                                {
                                    Trace.WriteLine(String.Format("\r\nRun Train Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                                    if (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor != eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE)
                                    {
                                        RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                                        break;
                                    }

                                    Task.Delay(50);
                                }
                            }
                        }
                    }
                });
                //让Rgv停稳
                await Task.Delay(100);

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //Robot执行任务
                t2 = t1.ContinueWith(t =>
                {
                    if (taskScheduleHandleInfo.handlePara.robotFeatureIsEnable)
                    {
                        //FrontRobot运动到指定位置
                        if (taskScheduleHandleInfo.frontRobotTaskStat == 1)
                        {
                            RobotDataPack robotDataPack = new RobotDataPack();

                            //得到坐标
                            robotDataPack.j1 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].FrontRobot_J1;
                            robotDataPack.j2 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].FrontRobot_J2;
                            robotDataPack.j3 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].FrontRobot_J3;
                            robotDataPack.j4 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].FrontRobot_J4;
                            robotDataPack.j5 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].FrontRobot_J5;
                            robotDataPack.j6 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].FrontRobot_J6;
                            DoRobotCmdSetDataHandle(@"1", robotDataPack);
                            Trace.WriteLine("Front Robot {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce.ToString());

                            taskMzCameraModel.FrontRobot_J1 = robotDataPack.j1;
                            taskMzCameraModel.FrontRobot_J2 = robotDataPack.j2;
                            taskMzCameraModel.FrontRobot_J3 = robotDataPack.j3;
                            taskMzCameraModel.FrontRobot_J4 = robotDataPack.j4;
                            taskMzCameraModel.FrontRobot_J5 = robotDataPack.j5;
                            taskMzCameraModel.FrontRobot_J6 = robotDataPack.j6;

                        }
                        Task.Delay(50);

                        //BackRobot运动到指定位置
                        if (taskScheduleHandleInfo.backRobotTaskStat == 1)
                        {
                            RobotDataPack robotDataPack = new RobotDataPack();

                            //得到坐标
                            robotDataPack.j1 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].BackRobot_J1;
                            robotDataPack.j2 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].BackRobot_J2;
                            robotDataPack.j3 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].BackRobot_J3;
                            robotDataPack.j4 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].BackRobot_J4;
                            robotDataPack.j5 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].BackRobot_J5;
                            robotDataPack.j6 = mzCameraDataList[taskScheduleHandleInfo.mzCameraDataIndex].BackRobot_J6;
                            DoRobotCmdSetDataHandle(@"2", robotDataPack);
                            Trace.WriteLine("Back Robot {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce.ToString());

                            taskMzCameraModel.BackRobot_J1 = robotDataPack.j1;
                            taskMzCameraModel.BackRobot_J2 = robotDataPack.j2;
                            taskMzCameraModel.BackRobot_J3 = robotDataPack.j3;
                            taskMzCameraModel.BackRobot_J4 = robotDataPack.j4;
                            taskMzCameraModel.BackRobot_J5 = robotDataPack.j5;
                            taskMzCameraModel.BackRobot_J6 = robotDataPack.j6;
                        }

                        //等待应答
                        if (taskScheduleHandleInfo.frontRobotTaskStat == 1)
                        {
                            while (true)
                            {
                                if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP)
                                {
                                    RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
                                    break;
                                }

                                Task.Delay(50);
                            }
                        }
                        if (taskScheduleHandleInfo.backRobotTaskStat == 1)
                        {
                            while (true)
                            {
                                if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP)
                                {
                                    RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
                                    break;
                                }

                                Task.Delay(50);
                            }
                        }
                    }
                });
                //让Robot停稳
                await Task.Delay(100);

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //相机执行任务
                t3 = t2.ContinueWith(t =>
                {
                    if (taskScheduleHandleInfo.handlePara.mzCameraFeatureIsEnable)
                    {
                        //FrontCamera开始拍照
                        if (taskScheduleHandleInfo.frontCameraTaskStat == 1)
                        {
                            DoCameraCmdHandle_OneShot(taskMzCameraModel.FrontCamerainfoItem);
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                        }
                        //FrontCamera
                        if (taskScheduleHandleInfo.frontCameraTaskStat == 1)
                        {
                            while (true)
                            {
                                if (CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor == eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP)
                                {
                                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                                    break;
                                }

                                Task.Delay(50);
                            }
                        }

                        //BackCamera开始拍照
                        if (taskScheduleHandleInfo.backCameraTaskStat == 1)
                        {
                            DoCameraCmdHandle_OneShot(taskMzCameraModel.BackCamerainfoItem);
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                        }
                        //BackCamera
                        if (taskScheduleHandleInfo.backCameraTaskStat == 1)
                        {
                            while (true)
                            {
                                if (CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor == eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP)
                                {
                                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
                                    break;
                                }

                                Task.Delay(50);
                            }
                        }
                    }
                });

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //等待所有任务都完成
                Task.WaitAll(t1, t2, t3);
            }

            //复位状态
            TaskMzCameraIsRunning = false;
            last_rgv_distance = 0;
            TrainCurrentHeadDistance = 0;
        }

        private void DoOneKeyStopCmdHandle(object obj)
        {
            //判断机械臂是否繁忙
            if (TaskMzCameraIsRunning)
            {
                MessageBox.Show("面阵采集流程正在工作,请稍后操作");
                return;
            }

            //机械臂归零
            if (taskScheduleHandleInfo.handlePara.robotFeatureIsEnable == true)
            {
                DoRobotCmdBackZeroHandle(@"1");
                Task.Delay(200);

                DoRobotCmdBackZeroHandle(@"2");
                Task.Delay(200);
            }

            //关闭相机
            if (taskScheduleHandleInfo.handlePara.mzCameraFeatureIsEnable == true)
            {
                DoCameraCmdHandle_Destroy(null);
            }

            //关闭Rgv
            if (taskScheduleHandleInfo.handlePara.rgvFeatureIsEnable == true)
            {
                DoRgvDisConnectCmdHandle(null);
            }
        }

        #region 面阵相机
        private void Camera_CameraImageEvent(Camera camera, Bitmap bmp)
        {
            //保存图片到本地
            String strheigth = bmp.Height.ToString();

            try
            {
                ICameraInfo camerainfo = camera.CameraInfo;
                if (camerainfo[CameraInfoKey.FriendlyName].Contains(@"camera_front"))
                {
                    //启动图片传输任务
                    String strfile = System.Environment.CurrentDirectory + @"\task_data\front_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + strheigth + @"H.jpg";
                    CameraDataHelper.SaveBitmapIntoFile(bmp, strfile, CameraDataHelper.JoinMode.Vertical);

                    GrabImg1 = strfile;
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                }
                else if (camerainfo[CameraInfoKey.FriendlyName].Contains(@"camera_back"))
                {
                    //保存图片到本地
                    String strfile = System.Environment.CurrentDirectory + @"\task_data\back_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + strheigth + @"H.jpg";
                    CameraDataHelper.SaveBitmapIntoFile(bmp, strfile, CameraDataHelper.JoinMode.Vertical);

                    GrabImg2 = strfile;
                    CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_STOP;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
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
                    if (item[CameraInfoKey.FriendlyName].Contains(@"camera_front"))
                    {
                        taskMzCameraModel.FrontCamerainfoItem = item as ICameraInfo;
                    }
                    else if (item[CameraInfoKey.FriendlyName].Contains(@"camera_back"))
                    {
                        taskMzCameraModel.BackCamerainfoItem = item as ICameraInfo;
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

        void DoCameraCmdHandle_Destroy(object obj)
        {
            CameraCtrlHelper.GetInstance().CameraDestroy();
        }

        void DoCameraCmdHandle_OneShot(object obj)
        {
            ICameraInfo item = obj as ICameraInfo;

            //默认选中挂接到第1个相机
            CameraCtrlHelper.GetInstance().CameraChangeItem(item);
            Task.Delay(200);
            Trace.WriteLine("Get Camera Device Information");
            Trace.WriteLine("=========================");
            Trace.WriteLine("Width           : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].GetValue().ToString());
            Trace.WriteLine("Height          : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].GetValue().ToString());
            Trace.WriteLine("ExposureTimeAbs : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].GetValue().ToString());
            Trace.WriteLine("======================");

            ////设置相机参数
            //CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].SetValue(4096, IntegerValueCorrection.Nearest);
            //CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].SetValue(960, IntegerValueCorrection.Nearest);
            //CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(46);
            //Task.Delay(500);

            //Trace.WriteLine("Get Camera Device Information");
            //Trace.WriteLine("=========================");
            //Trace.WriteLine("Width           : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].GetValue().ToString());
            //Trace.WriteLine("Height            : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].GetValue().ToString());
            //Trace.WriteLine("ExposureTimeAbs : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].GetValue().ToString());
            //Console.WriteLine("======================");

            CameraCtrlHelper.GetInstance().CameraOneShot();
        }

        #endregion

        #region Rgv控制
        private void MyRgvModInfoEvent(RgvGlobalInfo rgvinfo)
        {
            try
            {
                //数据协议信息
                taskMzCameraModel.RgvCurrentStat = rgvinfo.RgvCurrentStat; //默认小车中间状态Msg信息

                taskMzCameraModel.RgvCurrentMode = rgvinfo.RgvCurrentMode; //默认远程模式(自动)； 本地模式(手动)

                taskMzCameraModel.RgvCurrentRunSpeed = rgvinfo.RgvCurrentRunSpeed;
                taskMzCameraModel.RgvCurrentRunDistacnce = rgvinfo.RgvCurrentRunDistacnce;
                taskMzCameraModel.RgvCurrentPowerElectricity = rgvinfo.RgvCurrentPowerElectricity;
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
        private void DoRgvDisConnectCmdHandle(object obj)
        {
            RgvModCtrlHelper.GetInstance().RgvModDisConnect();
        }

        private void DoRgvForwardMotorCmdHandle(object obj)
        {
            //判断机械臂是否繁忙
            if (TaskMzCameraIsRunning)
            {
                MessageBox.Show("面阵采集流程正在工作,请稍后操作");
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
            //判断机械臂是否繁忙
            if (TaskMzCameraIsRunning)
            {
                MessageBox.Show("面阵采集流程正在工作,请稍后操作");
                return;
            }

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
            //判断机械臂是否繁忙
            if (TaskMzCameraIsRunning)
            {
                MessageBox.Show("面阵采集流程正在工作,请稍后操作");
                return;
            }

            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_NORMALSTOP);
            }

            //得到坐标
            RobotDataPack robotDataPack = new RobotDataPack();
            robotDataPack.j1 = @"0.00";
            robotDataPack.j2 = @"0.00";
            robotDataPack.j3 = @"0.00";
            robotDataPack.j4 = @"0.00";
            robotDataPack.j5 = @"0.00";
            robotDataPack.j6 = @"0.00";
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID, RobotData.RobotBackZeroFunCode, robotDataPack);

            //得到坐标
            robotDataPack.j1 = @"0.00";
            robotDataPack.j2 = @"0.00";
            robotDataPack.j3 = @"0.00";
            robotDataPack.j4 = @"0.00";
            robotDataPack.j5 = @"0.00";
            robotDataPack.j6 = @"0.00";
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID, RobotData.RobotBackZeroFunCode, robotDataPack);
        }

        private void DoRgvClearAlarmCmdHandle(object obj)
        {
            //判断机械臂是否繁忙
            if (TaskMzCameraIsRunning)
            {
                MessageBox.Show("面阵采集流程正在工作,请稍后操作");
                return;
            }

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
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunDistance = distance;
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
                taskMzCameraModel.CurrentDetectHeight = stm32info.CurrentDetectHeight;
                taskMzCameraModel.InfraRed1_Stat = stm32info.InfraRed1_Stat;
                taskMzCameraModel.InfraRed2_Stat = stm32info.InfraRed1_Stat;
                taskMzCameraModel.InfraRed3_Stat = stm32info.InfraRed3_Stat;
                taskMzCameraModel.InfraRed4_Stat = stm32info.InfraRed4_Stat;
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

        #region Relay继电器
        /// <summary>
        /// NetworkRelay继电器信息事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void MyNetworkRelayModInfoEvent(NetworkRelayGlobalInfo relayinfo)
        {
            try
            {
                //数据协议信息
                //taskMzCameraModel. = relayinfo.NetworkRelayRspMsg;
            }
            catch
            {

            }
        }

        private async void InitRelayMod()
        {
            //继电器操作
            DoRelayCreatServer_CmdHandle(null);
            await Task.Delay(5000);

            DoFrontRobotResetCtrl_CmdHandle(null);
            DoBackRobotResetCtrl_CmdHandle(null);
        }

        private void DoRelayCreatServer_CmdHandle(object obj)
        {
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayServerCreat();
        }

        private void DoFrontRobotResetCtrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                Task.Factory.StartNew(async () =>
                {
                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO1Ctrl_Cmd);
                    await Task.Delay(3000);

                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO2Ctrl_Cmd);
                    await Task.Delay(200);

                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO3Ctrl_Cmd);
                    await Task.Delay(200);

                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO4Ctrl_Cmd);
                });
            }
        }

        private void DoBackRobotResetCtrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                Task.Factory.StartNew(async () =>
                {
                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO5Ctrl_Cmd);
                    await Task.Delay(3000);

                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO6Ctrl_Cmd);
                    await Task.Delay(200);

                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO7Ctrl_Cmd);
                    await Task.Delay(200);

                    NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO8Ctrl_Cmd);
                });
            }
        }
        #endregion

        #region Robot机械臂
        /// <summary>
        /// Robot信息事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void MyRobotModInfoEvent(RobotGlobalInfo robotinfo, bool isFront)
        {
            try
            {
                //数据协议信息
                taskMzCameraModel.FrontRobotMsg = robotinfo.FrontRobotRspMsg;

                taskMzCameraModel.BackRobotMsg = robotinfo.BackRobotRspMsg;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }
        }

        private async void InitRobotMod()
        {
            try
            {
                //连接Robot设备
                RobotModCtrlHelper.GetInstance().RobotServerCreat();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }

            await Task.Delay(1000);
        }

        /// <summary>
        /// 控制Robot运动
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotCmdSetDataHandle(object obj, RobotDataPack robotdata)
        {
            String devid_str = obj as String;
            byte devid = 0x00;
            try
            {
                devid = Convert.ToByte(devid_str);
            }
            catch { }

            //发送事件
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(devid, RobotData.RobotSetCtrlFunCode, robotdata);

            //更新状态
            if (devid == 0x01)
            {
                RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE;
            }
            else if (devid == 0x02)
            {
                RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE;
            }
        }

        /// <summary>
        /// 控制Robot回归原点
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotCmdBackZeroHandle(object obj)
        {
            String devid_str = obj as String;
            byte devid = 0x00;
            try
            {
                devid = Convert.ToByte(devid_str);
            }
            catch { }

            RobotDataPack robotDataPack = new RobotDataPack();
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

            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(devid, RobotData.RobotBackZeroFunCode, robotDataPack);
        }
        #endregion
    }
}

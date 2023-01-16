using PcMainCtrl.Common;
using PcMainCtrl.DataAccess;
using PcMainCtrl.HardWare.NetworkRelay;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HardWare.Robot;
using PcMainCtrl.HardWare.Stm32;
using PcMainCtrl.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.Robot.RobotModCtrlProtocol;

namespace PcMainCtrl.ViewModel
{
    public class DataMzCameraViewModel : NotifyBase
    {
        //-------------------------------------------------------------------------------------------------
        /// <summary>
        /// RGV模块信息
        /// </summary>
        private String modRgvWorkMsg;
        public String ModRgvWorkMsg
        {
            get 
            { 
                return modRgvWorkMsg; 
            }
            set
            {
                modRgvWorkMsg = value;
                this.DoNotify();
            }
        }

        public const int RgvTrackLength = 18000;
        public int RgvTrackHeadLength = 0;

        /// <summary>
        /// Rgv命令字
        /// </summary>
        public CommandBase RgvModConnectCmd { get; set; }
        public CommandBase RgvModDisConnectCmd { get; set; }
        public CommandBase RgvForwardMotorCmd { get; set; }
        public CommandBase RgvBackMotorCmd { get; set; }
        public CommandBase RgvNormalStopCmd { get; set; }
        public CommandBase RgvClearAlarmCmd { get; set; }
        public CommandBase RgvCheckHeadCmd { get; set; }

        //-------------------------------------------------------------------------------------------------
        /// <summary>
        /// Relay模块信息
        /// </summary>
        private String modRelayWorkMsg;
        public String ModRelayWorkMsg
        {
            get 
            { 
                return modRelayWorkMsg; 
            }
            set
            {
                modRelayWorkMsg = value;
                this.DoNotify();
            }
        }

        public CommandBase CreatNetworkRelayServer { get; set; }
        public CommandBase CloseNetworkRelayServer { get; set; }
        public CommandBase FrontRobotResetCtrl_Cmd { get; set; }
        public CommandBase BackRobotResetCtrl_Cmd { get; set; }

        //-------------------------------------------------------------------------------------------------
        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Robot模块信息
        /// </summary>
        private String modRobotWorkMsg;
        public String ModRobotWorkMsg
        {
            get 
            { 
                return modRobotWorkMsg; 
            }
            set
            {
                modRobotWorkMsg = value;
                this.DoNotify();
            }
        }

        public CommandBase CreatRobotServer { get; set; }
        public CommandBase CloseRobotServer { get; set; }
        public CommandBase RobotCmdGetStat { get; set; }

        //-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 面阵相机数据行
        /// </summary>
        public DataMzCameraModel dataMzCameraModel { get; set; }

        public ObservableCollection<DataMzCameraLineModel> mzCameraDataList { get; set; }

        public CommandBase MzCameraDataListQuery { get; set; }
        public CommandBase MzCameraDataListSave { get; set; }
        public CommandBase MzCameraDataListSetZero { get; set; }

        //-------------------------------------------------------------------------------------------------
        public DataMzCameraViewModel()
        {
            //MzCameraData
            dataMzCameraModel = new DataMzCameraModel();
            mzCameraDataList = new ObservableCollection<DataMzCameraLineModel>();

            #region Rgv命令
            //Rgv控制
            this.RgvModConnectCmd = new CommandBase();
            this.RgvModConnectCmd.DoExecute = new Action<object>(DoRgvConnectCmdHandle);
            this.RgvModConnectCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvModDisConnectCmd = new CommandBase();
            this.RgvModDisConnectCmd.DoExecute = new Action<object>(DoRgvDisConnectCmdHandle);
            this.RgvModDisConnectCmd.DoCanExecute = new Func<object, bool>((o) => true);

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


            this.RgvCheckHeadCmd = new CommandBase();
            this.RgvCheckHeadCmd.DoExecute = new Action<object>(DoRgvCheckHeadCmdHandle);
            this.RgvCheckHeadCmd.DoCanExecute = new Func<object, bool>((o) => true);

            //挂接事件
            RgvModCtrlHelper.GetInstance().RgvModInfoEvent += MyRgvModInfoEvent;
            #endregion

            #region 网络继电器
            this.CreatNetworkRelayServer = new CommandBase();
            this.CreatNetworkRelayServer.DoExecute = new Action<object>(DoRelayCreatServer_CmdHandle);
            this.CreatNetworkRelayServer.DoCanExecute = new Func<object, bool>((o) => true);

            this.CloseNetworkRelayServer = new CommandBase();
            this.CloseNetworkRelayServer.DoExecute = new Action<object>(DoRelayCloseServer_CmddHandle);
            this.CloseNetworkRelayServer.DoCanExecute = new Func<object, bool>((o) => true);

            this.FrontRobotResetCtrl_Cmd = new CommandBase();
            this.FrontRobotResetCtrl_Cmd.DoExecute = new Action<object>(DoFrontRobotResetCtrl_CmdHandle);
            this.FrontRobotResetCtrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.BackRobotResetCtrl_Cmd = new CommandBase();
            this.BackRobotResetCtrl_Cmd.DoExecute = new Action<object>(DoBackRobotResetCtrl_CmdHandle);
            this.BackRobotResetCtrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            //模块信息
            ModRelayWorkMsg = @"...";

            //挂接事件
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayModInfoEvent += MyNetworkRelayModInfoEvent;
            #endregion

            #region 机械臂
            this.CreatRobotServer = new CommandBase();
            this.CreatRobotServer.DoExecute = new Action<object>(DoCreatRobotServerHandle);
            this.CreatRobotServer.DoCanExecute = new Func<object, bool>((o) => true);

            this.CloseRobotServer = new CommandBase();
            this.CloseRobotServer.DoExecute = new Action<object>(DoCloseRobotServerHandle);
            this.CloseRobotServer.DoCanExecute = new Func<object, bool>((o) => true);

            this.RobotCmdGetStat = new CommandBase();
            this.RobotCmdGetStat.DoExecute = new Action<object>(DoRobotCmdGetStatHandle);
            this.RobotCmdGetStat.DoCanExecute = new Func<object, bool>((o) => true);

            //模块信息
            ModRobotWorkMsg = @"...";

            dataMzCameraModel.FrontRobot_J1 = @"0.00";
            dataMzCameraModel.FrontRobot_J2 = @"0.00";
            dataMzCameraModel.FrontRobot_J3 = @"0.00";
            dataMzCameraModel.FrontRobot_J4 = @"0.00";
            dataMzCameraModel.FrontRobot_J5 = @"0.00";
            dataMzCameraModel.FrontRobot_J6 = @"0.00";
            dataMzCameraModel.FrontRobot_Enable = 0;
            dataMzCameraModel.FrontCamera_Enable = 0;
            dataMzCameraModel.FrontComponentId = @"-1";
            dataMzCameraModel.FrontComponentType = @"-1";

            dataMzCameraModel.BackRobot_J1 = @"0.00";
            dataMzCameraModel.BackRobot_J2 = @"0.00";
            dataMzCameraModel.BackRobot_J3 = @"0.00";
            dataMzCameraModel.BackRobot_J4 = @"0.00";
            dataMzCameraModel.BackRobot_J5 = @"0.00";
            dataMzCameraModel.BackRobot_J6 = @"0.00";
            dataMzCameraModel.BackRobot_Enable = 0;
            dataMzCameraModel.BackCamera_Enable = 0;
            dataMzCameraModel.BackComponentId = @"-1";
            dataMzCameraModel.BackComponentType = @"-1";
            //挂接事件
            RobotModCtrlHelper.GetInstance().RobotModInfoEvent += MyRobotModInfoEvent;
            #endregion

            #region
            //初始化Stm32
            //挂接事件
            Stm32ModCtrlHelper.GetInstance().Stm32ModInfoEvent += MyStm32ModInfoEvent;
            InitStm32Mod();
            #endregion

            #region 数据库操作
            //数据查询
            this.MzCameraDataListQuery = new CommandBase();
            this.MzCameraDataListQuery.DoExecute = new Action<object>(DoMzCameraDataListQueryHandle);
            this.MzCameraDataListQuery.DoCanExecute = new Func<object, bool>((o) => true);

            //数据保存
            this.MzCameraDataListSave = new CommandBase();
            this.MzCameraDataListSave.DoExecute = new Action<object>(DoMzCameraDataListSaveHandle);
            this.MzCameraDataListSave.DoCanExecute = new Func<object, bool>((o) => true);

            //Robot点位清0
            this.MzCameraDataListSetZero = new CommandBase();
            this.MzCameraDataListSetZero.DoExecute = new Action<object>(DoMzCameraDataListSetZeroHandle);
            this.MzCameraDataListSetZero.DoCanExecute = new Func<object, bool>((o) => true);

            #endregion
        }

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
                //taskXzCameraModel.CurrentDetectHeight = stm32info.CurrentDetectHeight;
                //taskXzCameraModel.InfraRed1_Stat = stm32info.InfraRed1_Stat;
                //taskXzCameraModel.InfraRed2_Stat = stm32info.InfraRed1_Stat;
                //taskXzCameraModel.InfraRed3_Stat = stm32info.InfraRed3_Stat;
                //taskXzCameraModel.InfraRed4_Stat = stm32info.InfraRed4_Stat;
            }
            catch
            {

            }
        }

        private void InitStm32Mod()
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

            ThreadSleep(100);
        }
        #endregion

        /// <summary>
        /// NetworkRelay继电器信息事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void MyRgvModInfoEvent(RgvGlobalInfo rgvinfo)
        {
            try
            {
                //模块信息
                ModRgvWorkMsg = rgvinfo.ModWorkMsg;

                //数据协议信息
                dataMzCameraModel.RgvCurrentRunDistacnce = rgvinfo.RgvCurrentRunDistacnce;
            }
            catch
            {

            }
        }

        #region RGV控制命令
        private void DoRgvConnectCmdHandle(object obj)
        {
            RgvModCtrlHelper.GetInstance().RgvModConnect();
        }
        private void DoRgvDisConnectCmdHandle(object obj)
        {
            RgvModCtrlHelper.GetInstance().RgvModDisConnect();
        }
        private void DoRgvForwardMotorCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_FORWARDMOTOR);
            }
        }
        private void DoRgvBackMotorCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_BACKWARDMOTOR);
            }
        }
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
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETDISATNCE);

                    //设置速度
                    int speed = 800;
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunSpeed = speed;
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETSPEED);
                    ThreadSleep(100);

                    //设置距离
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunDistance = distance;
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETDISATNCE);
                    ThreadSleep(100);

                    //运动到指定目标位置
                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_RUNAPPOINTDISTANCE);
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                }
            }
        }

        private void DoRgvCheckHeadCmdHandle(object obj)
        {

            Task t1, t2;

            //t1先串行:查询数据库表中数据,挂在List上面
            t1 = Task.Factory.StartNew(() =>
            {
                //Rgv执行任务
                if (RgvModCtrlHelper.GetInstance().IsEnable)
                {
                    //Rgv开始正向运动
                    DoRgvSetTargetDistanceCmdHandle(RgvTrackLength);
                }
            });
            Task.WaitAny(t1);

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //车头检测
            t2 = t1.ContinueWith(t =>
            {
                while (true)
                {
                    Trace.WriteLine(String.Format("\r\nCheck Train Distance: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                    Trace.WriteLine(String.Format("Check Train Io1: {0}\r\n", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed1_Stat));
                    Trace.WriteLine(String.Format("Check Train Io3: {0}\r\n", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed3_Stat));

#if false
                        //if ((RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce >= xzCameraDataList[0].RgvCheckMinDistacnce) &&
                        //    (Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.CurrentDetectHeight <= xzCameraDataList[0].CurrentDetectMaxHeight)
                        //   )
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
                        //记录当前车头定位的参数值
                        dataMzCameraModel.RgvCurrentHeadDistacnce = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce;

                        Trace.WriteLine(String.Format("\r\nCheck Train Head: {0}", RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce));
                        Trace.WriteLine(String.Format("Check Train Io1: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed1_Stat));
                        Trace.WriteLine(String.Format("Check Train Io3: {0}", Stm32ModCtrlHelper.GetInstance().myStm32GlobalInfo.InfraRed3_Stat));
                        DoRgvNormalStopCmdHandle(null);
                        break;
                    }

                    Task.Delay(10);
                }
            });
        }
        
        #endregion

        /// <summary>
        /// NetworkRelay继电器信息事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void MyNetworkRelayModInfoEvent(NetworkRelayGlobalInfo relayinfo)
        {
            try
            {
                //模块信息
                ModRelayWorkMsg = relayinfo.ModWorkMsg;

                //数据协议信息
                dataMzCameraModel.RelayRspMsg = relayinfo.NetworkRelayRspMsg;
            }
            catch
            {

            }
        }

        #region 继电器操作命令
        private void DoRelayCreatServer_CmdHandle(object obj)
        {
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayServerCreat();
        }
        private void DoRelayCloseServer_CmddHandle(object obj)
        {
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayServerClose();
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

        /// <summary>
        /// Robot信息事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void MyRobotModInfoEvent(RobotGlobalInfo robotinfo, bool isFront)
        {
            try
            {
                //模块信息
                ModRobotWorkMsg = robotinfo.ModWorkMsg;

                //数据协议信息
                dataMzCameraModel.FrontRobotMsg = robotinfo.FrontRobotRspMsg;
                dataMzCameraModel.FrontRobot_J1 = robotinfo.FrontRobotSiteData.j1;
                dataMzCameraModel.FrontRobot_J2 = robotinfo.FrontRobotSiteData.j2;
                dataMzCameraModel.FrontRobot_J3 = robotinfo.FrontRobotSiteData.j3;
                dataMzCameraModel.FrontRobot_J4 = robotinfo.FrontRobotSiteData.j4;
                dataMzCameraModel.FrontRobot_J5 = robotinfo.FrontRobotSiteData.j5;
                dataMzCameraModel.FrontRobot_J6 = robotinfo.FrontRobotSiteData.j6;

                dataMzCameraModel.BackRobotMsg = robotinfo.BackRobotRspMsg;
                dataMzCameraModel.BackRobot_J1 = robotinfo.BackRobotSiteData.j1;
                dataMzCameraModel.BackRobot_J2 = robotinfo.BackRobotSiteData.j2;
                dataMzCameraModel.BackRobot_J3 = robotinfo.BackRobotSiteData.j3;
                dataMzCameraModel.BackRobot_J4 = robotinfo.BackRobotSiteData.j4;
                dataMzCameraModel.BackRobot_J5 = robotinfo.BackRobotSiteData.j5;
                dataMzCameraModel.BackRobot_J6 = robotinfo.BackRobotSiteData.j6;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }
        }

        #region Robot操作命令
        private void DoCreatRobotServerHandle(object obj)
        {
            RobotModCtrlHelper.GetInstance().RobotServerCreat();
        }
        private void DoCloseRobotServerHandle(object obj)
        {
            RobotModCtrlHelper.GetInstance().RobotServerClose();
        }
        /// <summary>
        /// 获取Robot状态数据
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotCmdGetStatHandle(object obj)
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

            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(devid, RobotData.RobotGetDataFunCode, robotDataPack);
        }
        #endregion

        /// <summary>
        /// 数据库表数据查询
        /// </summary>
        /// <param name="obj"></param>
        private void DoMzCameraDataListQueryHandle(object obj)
        {
            mzCameraDataList.Clear();

            //查询数据库
            var cList = LocalDataBase.GetInstance().MzCameraDataListQurey();
            foreach (var item in cList)
            {
                mzCameraDataList.Add(item);
            }
        }

        /// <summary>
        /// 读取机器臂当前姿态数据保存至数据库
        /// </summary>
        /// <param name="obj"></param>
        private void DoMzCameraDataListSaveHandle(object obj)
        {
            //首先获取到当前数据
            DataMzCameraLineModelEx mzcamera_dataline = new DataMzCameraLineModelEx();

            mzcamera_dataline.TrainModel = dataMzCameraModel.TrainMode;
            mzcamera_dataline.TrainSn = dataMzCameraModel.TrainSn;

            mzcamera_dataline.Rgv_Id = 0;
            mzcamera_dataline.Rgv_Enable = 1;
            mzcamera_dataline.Rgv_Distance = dataMzCameraModel.RgvCurrentRunDistacnce - dataMzCameraModel.RgvCurrentHeadDistacnce; //相对距离

            mzcamera_dataline.FrontRobot_Id = 1;
            mzcamera_dataline.FrontRobot_Enable = dataMzCameraModel.FrontRobot_Enable;
            mzcamera_dataline.FrontRobot_J1 = dataMzCameraModel.FrontRobot_J1;
            mzcamera_dataline.FrontRobot_J2 = dataMzCameraModel.FrontRobot_J2;
            mzcamera_dataline.FrontRobot_J3 = dataMzCameraModel.FrontRobot_J3;
            mzcamera_dataline.FrontRobot_J4 = dataMzCameraModel.FrontRobot_J4;
            mzcamera_dataline.FrontRobot_J5 = dataMzCameraModel.FrontRobot_J5;
            mzcamera_dataline.FrontRobot_J6 = dataMzCameraModel.FrontRobot_J6;

            mzcamera_dataline.FrontCamera_Id = 3;
            mzcamera_dataline.FrontCamera_Enable = dataMzCameraModel.FrontRobot_Enable;
            mzcamera_dataline.FrontComponentId = dataMzCameraModel.FrontComponentId;
            mzcamera_dataline.FrontComponentType = dataMzCameraModel.FrontComponentType;

            mzcamera_dataline.BackRobot_Id = 2;
            mzcamera_dataline.BackRobot_Enable = dataMzCameraModel.BackRobot_Enable;
            mzcamera_dataline.BackRobot_J1 = dataMzCameraModel.BackRobot_J1;
            mzcamera_dataline.BackRobot_J2 = dataMzCameraModel.BackRobot_J2;
            mzcamera_dataline.BackRobot_J3 = dataMzCameraModel.BackRobot_J3;
            mzcamera_dataline.BackRobot_J4 = dataMzCameraModel.BackRobot_J4;
            mzcamera_dataline.BackRobot_J5 = dataMzCameraModel.BackRobot_J5;
            mzcamera_dataline.BackRobot_J6 = dataMzCameraModel.BackRobot_J6;

            mzcamera_dataline.BackCamera_Id = 4;
            mzcamera_dataline.BackCamera_Enable = dataMzCameraModel.BackRobot_Enable;
            mzcamera_dataline.BackComponentId = dataMzCameraModel.BackComponentId;
            mzcamera_dataline.BackComponentType = dataMzCameraModel.BackComponentType;

            //将数据存储在数据库中
            LocalDataBase.GetInstance().MzCameraDataListSave(mzcamera_dataline);
        }

        /// <summary>
        /// 机械臂点位清0
        /// </summary>
        /// <param name="obj"></param>
        private void DoMzCameraDataListSetZeroHandle(object obj)
        {
            dataMzCameraModel.FrontRobot_J1 = @"0.00";
            dataMzCameraModel.FrontRobot_J2 = @"0.00";
            dataMzCameraModel.FrontRobot_J3 = @"0.00";
            dataMzCameraModel.FrontRobot_J4 = @"0.00";
            dataMzCameraModel.FrontRobot_J5 = @"0.00";
            dataMzCameraModel.FrontRobot_J6 = @"0.00";

            dataMzCameraModel.BackRobot_J1 = @"0.00";
            dataMzCameraModel.BackRobot_J2 = @"0.00";
            dataMzCameraModel.BackRobot_J3 = @"0.00";
            dataMzCameraModel.BackRobot_J4 = @"0.00";
            dataMzCameraModel.BackRobot_J5 = @"0.00";
            dataMzCameraModel.BackRobot_J6 = @"0.00";
        }
    }
}
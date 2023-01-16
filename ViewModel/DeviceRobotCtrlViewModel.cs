using PcMainCtrl.Common;
using PcMainCtrl.DataAccess;
using PcMainCtrl.HardWare.NetworkRelay;
using PcMainCtrl.HardWare.Robot;
using PcMainCtrl.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using static PcMainCtrl.HardWare.Robot.RobotModCtrlProtocol;

namespace PcMainCtrl.ViewModel
{
    public class DeviceRobotCtrlViewModel : NotifyBase
    {
        /// <summary>
        /// 模块信息
        /// </summary>
        private String modRelayWorkMsg;
        public String ModRelayWorkMsg
        {
            get { return modRelayWorkMsg; }
            set
            {
                modRelayWorkMsg = value;
                this.DoNotify();
            }
        }
        
        public DeviceRelayCtrlModel RealyParaInfo { get; set; }

        public CommandBase CreatNetworkRelayServer { get; set; }

        public CommandBase CloseNetworkRelayServer { get; set; }

        public CommandBase FrontRobotResetCtrl_Cmd { get; set; }

        public CommandBase BackRobotResetCtrl_Cmd { get; set; }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 机械臂服务器信息
        /// </summary>
        private String modRobotWorkMsg;
        public String ModRobotWorkMsg
        {
            get { return modRobotWorkMsg; }
            set
            {
                modRobotWorkMsg = value;
                this.DoNotify();
            }
        }

        public DeviceRobotCtrlModel DeviceFrontRobotCtrlModel { get; set; }
        public DeviceRobotCtrlModel DeviceBackRobotCtrlModel { get; set; }

        public CommandBase CreatRobotServer { get; set; }

        public CommandBase CloseRobotServer { get; set; }

        public CommandBase RobotCmdSetData { get; set; }

        public CommandBase RobotCmdGetStat { get; set; }

        public CommandBase RobotCmdBackZero { get; set; }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        //数据表控制
        public ObservableCollection<DeviceRobotDataLineModel> robotDataList { get; set; }

        public CommandBase RobotDataListQuery { get; set; }
        public CommandBase RobotDataListSet { get; set; }
        public CommandBase RobotDataListSave { get; set; }
        public CommandBase RobotDataListSetZero { get; set; }

        //表格左键单击选择命令
        public CommandBase DataLineSelectOne { get; set; }

        public DeviceRobotCtrlViewModel()
        {
            //------------------------------------------------------------------------------------------------------------------
            //网络继电器
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

            //数据协议信息
            RealyParaInfo = new DeviceRelayCtrlModel();
            RealyParaInfo.RelayRspMsg = @"硬件初始化成功";

            //挂接事件
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayModInfoEvent += MyNetworkRelayModInfoEvent;

            //------------------------------------------------------------------------------------------------------------------
            //机械臂
            this.CreatRobotServer = new CommandBase();
            this.CreatRobotServer.DoExecute = new Action<object>(DoCreatRobotServerHandle);
            this.CreatRobotServer.DoCanExecute = new Func<object, bool>((o) => true);

            this.CloseRobotServer = new CommandBase();
            this.CloseRobotServer.DoExecute = new Action<object>(DoCloseRobotServerHandle);
            this.CloseRobotServer.DoCanExecute = new Func<object, bool>((o) => true);

            this.RobotCmdSetData = new CommandBase();
            this.RobotCmdSetData.DoExecute = new Action<object>(DoRobotCmdSetDataHandle);
            this.RobotCmdSetData.DoCanExecute = new Func<object, bool>((o) => true);

            this.RobotCmdGetStat = new CommandBase();
            this.RobotCmdGetStat.DoExecute = new Action<object>(DoRobotCmdGetStatHandle);
            this.RobotCmdGetStat.DoCanExecute = new Func<object, bool>((o) => true);

            this.RobotCmdBackZero = new CommandBase();
            this.RobotCmdBackZero.DoExecute = new Action<object>(DoRobotCmdBackZeroHandle);
            this.RobotCmdBackZero.DoCanExecute = new Func<object, bool>((o) => true);

            //------------------------------------------------------------------------------------------------------------------
            //数据表操作
            //点位查询
            this.RobotDataListQuery = new CommandBase();
            this.RobotDataListQuery.DoExecute = new Action<object>(DoRobotDataListQueryHandle);
            this.RobotDataListQuery.DoCanExecute = new Func<object, bool>((o) => true);

            //点位控制
            this.RobotDataListSet = new CommandBase();
            this.RobotDataListSet.DoExecute = new Action<object>(DoRobotDataListSetHandle);
            this.RobotDataListSet.DoCanExecute = new Func<object, bool>((o) => true);

            //点位保存
            this.RobotDataListSave = new CommandBase();
            this.RobotDataListSave.DoExecute = new Action<object>(DoRobotDataListSaveHandle);
            this.RobotDataListSave.DoCanExecute = new Func<object, bool>((o) => true);

            //点位插零
            this.RobotDataListSetZero = new CommandBase();
            this.RobotDataListSetZero.DoExecute = new Action<object>(DoRobotDataListSetZeroHandle);
            this.RobotDataListSetZero.DoCanExecute = new Func<object, bool>((o) => true);

            //模块信息
            ModRobotWorkMsg = @"...";

            //数据协议信息
            DeviceFrontRobotCtrlModel = new DeviceRobotCtrlModel();
            DeviceFrontRobotCtrlModel.RobotMsg = "...";
            DeviceFrontRobotCtrlModel.J1 = @"0.00";
            DeviceFrontRobotCtrlModel.J2 = @"0.00";
            DeviceFrontRobotCtrlModel.J3 = @"0.00";
            DeviceFrontRobotCtrlModel.J4 = @"0.00";
            DeviceFrontRobotCtrlModel.J5 = @"0.00";
            DeviceFrontRobotCtrlModel.J6 = @"0.00";

            DeviceBackRobotCtrlModel = new DeviceRobotCtrlModel();
            DeviceBackRobotCtrlModel.RobotMsg = "...";
            DeviceBackRobotCtrlModel.J1 = @"0.00";
            DeviceBackRobotCtrlModel.J2 = @"0.00";
            DeviceBackRobotCtrlModel.J3 = @"0.00";
            DeviceBackRobotCtrlModel.J4 = @"0.00";
            DeviceBackRobotCtrlModel.J5 = @"0.00";
            DeviceBackRobotCtrlModel.J6 = @"0.00";

            robotDataList = new ObservableCollection<DeviceRobotDataLineModel>();
            
            //表格选中某行
            this.DataLineSelectOne = new CommandBase();
            this.DataLineSelectOne.DoExecute = new Action<object>(DoDataLineSelectOneHandle);
            this.DataLineSelectOne.DoCanExecute = new Func<object, bool>((o) => true);

            //挂接事件
            RobotModCtrlHelper.GetInstance().RobotModInfoEvent += MyRobotModInfoEvent;
        }

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
                RealyParaInfo.RelayRspMsg = relayinfo.NetworkRelayRspMsg;
            }
            catch
            {

            }
        }
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
                DeviceFrontRobotCtrlModel.RobotMsg = robotinfo.FrontRobotRspMsg;
                DeviceFrontRobotCtrlModel.J1 = robotinfo.FrontRobotSiteData.j1;
                DeviceFrontRobotCtrlModel.J2 = robotinfo.FrontRobotSiteData.j2;
                DeviceFrontRobotCtrlModel.J3 = robotinfo.FrontRobotSiteData.j3;
                DeviceFrontRobotCtrlModel.J4 = robotinfo.FrontRobotSiteData.j4;
                DeviceFrontRobotCtrlModel.J5 = robotinfo.FrontRobotSiteData.j5;
                DeviceFrontRobotCtrlModel.J6 = robotinfo.FrontRobotSiteData.j6;

                DeviceBackRobotCtrlModel.RobotMsg = robotinfo.BackRobotRspMsg;
                DeviceBackRobotCtrlModel.J1 = robotinfo.BackRobotSiteData.j1;
                DeviceBackRobotCtrlModel.J2 = robotinfo.BackRobotSiteData.j2;
                DeviceBackRobotCtrlModel.J3 = robotinfo.BackRobotSiteData.j3;
                DeviceBackRobotCtrlModel.J4 = robotinfo.BackRobotSiteData.j4;
                DeviceBackRobotCtrlModel.J5 = robotinfo.BackRobotSiteData.j5;
                DeviceBackRobotCtrlModel.J6 = robotinfo.BackRobotSiteData.j6;

            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception caught:\n" + exception.Message);
            }
        }

        private void DoCreatRobotServerHandle(object obj)
        {
            RobotModCtrlHelper.GetInstance().RobotServerCreat();
        }

        private void DoCloseRobotServerHandle(object obj)
        {
            RobotModCtrlHelper.GetInstance().RobotServerClose();
        }

        /// <summary>
        /// 控制Robot运动
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotCmdSetDataHandle(object obj)
        {
            String devid_str = obj as String;
            byte devid = 0x00;
            try
            {
               devid = Convert.ToByte(devid_str);
            }
            catch { }

            RobotDataPack robotDataPack = new RobotDataPack();
            if (RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID == devid)
            {
                //得到坐标
                robotDataPack.j1 = DeviceFrontRobotCtrlModel.J1;
                robotDataPack.j2 = DeviceFrontRobotCtrlModel.J2;
                robotDataPack.j3 = DeviceFrontRobotCtrlModel.J3;
                robotDataPack.j4 = DeviceFrontRobotCtrlModel.J4;
                robotDataPack.j5 = DeviceFrontRobotCtrlModel.J5;
                robotDataPack.j6 = DeviceFrontRobotCtrlModel.J6;
            }
            else if (RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID == devid)
            {
                robotDataPack.j1 = DeviceBackRobotCtrlModel.J1;
                robotDataPack.j2 = DeviceBackRobotCtrlModel.J2;
                robotDataPack.j3 = DeviceBackRobotCtrlModel.J3;
                robotDataPack.j4 = DeviceBackRobotCtrlModel.J4;
                robotDataPack.j5 = DeviceBackRobotCtrlModel.J5;
                robotDataPack.j6 = DeviceBackRobotCtrlModel.J6;
            }
            else
            {
                return;
            }

            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(devid, RobotData.RobotSetCtrlFunCode, robotDataPack);
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

        /// <summary>
        /// 数据库表点位数据查询
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotDataListQueryHandle(object obj)
        {
            robotDataList.Clear();

            //查询数据库
            var cList = LocalDataBase.GetInstance().RobotTestDataQurey();
            foreach (var item in cList)
            {
                robotDataList.Add(item);
            }
        }

        /// <summary>
        /// 读取机器臂当前姿态数据保存至数据库
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotDataListSaveHandle(object obj)
        {
            //首先获取到当前数据
            DeviceRobotDataLineModel robot_dataline = new DeviceRobotDataLineModel();

            robot_dataline.FrontRobot_Id = @"1";
            robot_dataline.FrontRobot_Enable = @"1";
            robot_dataline.FrontRobot_J1 = DeviceFrontRobotCtrlModel.J1;
            robot_dataline.FrontRobot_J2 = DeviceFrontRobotCtrlModel.J2;
            robot_dataline.FrontRobot_J3 = DeviceFrontRobotCtrlModel.J3;
            robot_dataline.FrontRobot_J4 = DeviceFrontRobotCtrlModel.J4;
            robot_dataline.FrontRobot_J5 = DeviceFrontRobotCtrlModel.J5;
            robot_dataline.FrontRobot_J6 = DeviceFrontRobotCtrlModel.J6;

            robot_dataline.BackRobot_Id = @"2"; 
            robot_dataline.BackRobot_Enable = @"1";
            robot_dataline.BackRobot_J1 = DeviceBackRobotCtrlModel.J1;
            robot_dataline.BackRobot_J2 = DeviceBackRobotCtrlModel.J2;
            robot_dataline.BackRobot_J3 = DeviceBackRobotCtrlModel.J3;
            robot_dataline.BackRobot_J4 = DeviceBackRobotCtrlModel.J4;
            robot_dataline.BackRobot_J5 = DeviceBackRobotCtrlModel.J5;
            robot_dataline.BackRobot_J6 = DeviceBackRobotCtrlModel.J6;

            bool frontrobot_ret = robot_dataline.FrontRobot_J1.Contains(@"0.00") &&
                                    robot_dataline.FrontRobot_J2.Contains(@"0.00") &&
                                    robot_dataline.FrontRobot_J3.Contains(@"0.00") &&
                                    robot_dataline.FrontRobot_J4.Contains(@"0.00") &&
                                    robot_dataline.FrontRobot_J5.Contains(@"0.00") &&
                                    robot_dataline.FrontRobot_J6.Contains(@"0.00");

            bool backrobot_ret = robot_dataline.BackRobot_J1.Contains(@"0.00") &&
                                    robot_dataline.BackRobot_J2.Contains(@"0.00") &&
                                    robot_dataline.BackRobot_J3.Contains(@"0.00") &&
                                    robot_dataline.BackRobot_J4.Contains(@"0.00") &&
                                    robot_dataline.BackRobot_J5.Contains(@"0.00") &&
                                    robot_dataline.BackRobot_J6.Contains(@"0.00");


            if (frontrobot_ret == true && backrobot_ret == true)
            {
                robot_dataline.DataLine_Discript = @"零点";
            }
            else
            {
                robot_dataline.DataLine_Discript = @"运动点";
            }

            //将数据存储在数据库中
            LocalDataBase.GetInstance().RobotTestDataSave(robot_dataline);
        }

        /// <summary>
        /// 将零点姿态数据保存至数据库
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotDataListSetZeroHandle(object obj)
        {
            //首先获取到当前数据
            DeviceRobotDataLineModel robot_dataline = new DeviceRobotDataLineModel();

            robot_dataline.FrontRobot_Id = @"1";
            robot_dataline.FrontRobot_Enable = @"1";
            robot_dataline.FrontRobot_J1 = @"0.00";
            robot_dataline.FrontRobot_J2 = @"0.00";
            robot_dataline.FrontRobot_J3 = @"0.00";
            robot_dataline.FrontRobot_J4 = @"0.00";
            robot_dataline.FrontRobot_J5 = @"0.00";
            robot_dataline.FrontRobot_J6 = @"0.00";

            robot_dataline.BackRobot_Id = @"2";
            robot_dataline.BackRobot_Enable = @"1";
            robot_dataline.BackRobot_J1 = @"0.00";
            robot_dataline.BackRobot_J2 = @"0.00";
            robot_dataline.BackRobot_J3 = @"0.00";
            robot_dataline.BackRobot_J4 = @"0.00";
            robot_dataline.BackRobot_J5 = @"0.00";
            robot_dataline.BackRobot_J6 = @"0.00";

            robot_dataline.DataLine_Discript = @"零点";

            //将数据存储在数据库中
            LocalDataBase.GetInstance().RobotTestDataSave(robot_dataline);
        }

        /// <summary>
        /// 读取数据库表点位数据控制
        /// </summary>
        /// <param name="obj"></param>
        private void DoDataLineSelectOneHandle(object obj)
        {
            DeviceRobotDataLineModel robot_dataline = new DeviceRobotDataLineModel();

            robot_dataline = obj as DeviceRobotDataLineModel;

            DeviceFrontRobotCtrlModel.J1 = robot_dataline.FrontRobot_J1;
            DeviceFrontRobotCtrlModel.J2 = robot_dataline.FrontRobot_J2;
            DeviceFrontRobotCtrlModel.J3 = robot_dataline.FrontRobot_J3;
            DeviceFrontRobotCtrlModel.J4 = robot_dataline.FrontRobot_J4;
            DeviceFrontRobotCtrlModel.J5 = robot_dataline.FrontRobot_J5;
            DeviceFrontRobotCtrlModel.J6 = robot_dataline.FrontRobot_J6;

            DeviceBackRobotCtrlModel.J1 = robot_dataline.BackRobot_J1;
            DeviceBackRobotCtrlModel.J2 = robot_dataline.BackRobot_J2;
            DeviceBackRobotCtrlModel.J3 = robot_dataline.BackRobot_J3;
            DeviceBackRobotCtrlModel.J4 = robot_dataline.BackRobot_J4;
            DeviceBackRobotCtrlModel.J5 = robot_dataline.BackRobot_J5;
            DeviceBackRobotCtrlModel.J6 = robot_dataline.BackRobot_J6;
        }

        /// <summary>
        /// 读取数据库表点位数据双臂控制
        /// </summary>
        /// <param name="obj"></param>
        private void DoRobotDataListSetHandle(object obj)
        {

            RobotDataPack frontRobotDataPack = new RobotDataPack();
            frontRobotDataPack.j1 = DeviceFrontRobotCtrlModel.J1;
            frontRobotDataPack.j2 = DeviceFrontRobotCtrlModel.J2;
            frontRobotDataPack.j3 = DeviceFrontRobotCtrlModel.J3;
            frontRobotDataPack.j4 = DeviceFrontRobotCtrlModel.J4;
            frontRobotDataPack.j5 = DeviceFrontRobotCtrlModel.J5;
            frontRobotDataPack.j6 = DeviceFrontRobotCtrlModel.J6;

            RobotDataPack backRobotDataPack = new RobotDataPack();
            backRobotDataPack.j1 = DeviceBackRobotCtrlModel.J1;
            backRobotDataPack.j2 = DeviceBackRobotCtrlModel.J2;
            backRobotDataPack.j3 = DeviceBackRobotCtrlModel.J3;
            backRobotDataPack.j4 = DeviceBackRobotCtrlModel.J4;
            backRobotDataPack.j5 = DeviceBackRobotCtrlModel.J5;
            backRobotDataPack.j6 = DeviceBackRobotCtrlModel.J6;

            Task.Factory.StartNew(async () =>
            {
                RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID, RobotData.RobotSetCtrlFunCode, frontRobotDataPack);
                await Task.Delay(100);

                RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID, RobotData.RobotSetCtrlFunCode, backRobotDataPack);
            });
        }
    }
}

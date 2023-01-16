using PcMainCtrl.Common;
using PcMainCtrl.HardWare.NetworkRelay;
using PcMainCtrl.Model;
using System;
using System.Threading.Tasks;

namespace PcMainCtrl.ViewModel
{
    public class DeviceRelayCtrlViewModel : NotifyBase
    {
        /// <summary>
        /// 模块信息
        /// </summary>
        private String modWorkMsg;
        public String ModWorkMsg
        {
            get { return modWorkMsg; }
            set
            {
                modWorkMsg = value;
                this.DoNotify();
            }
        }

        public DeviceRelayCtrlModel RealyParaInfo { get; set; }

        public CommandBase RelayCreatServer_Cmd { get; set; }

        public CommandBase RelayCloseServer_Cmd { get; set; }

        public CommandBase RelayIO1Ctrl_Cmd { get; set; }

        public CommandBase RelayIO2Ctrl_Cmd { get; set; }

        public CommandBase RelayIO3Ctrl_Cmd { get; set; }

        public CommandBase RelayIO4Ctrl_Cmd { get; set; }

        public CommandBase FrontRobotResetCtrl_Cmd { get; set; }

        public CommandBase RelayIO5Ctrl_Cmd { get; set; }

        public CommandBase RelayIO6Ctrl_Cmd { get; set; }

        public CommandBase RelayIO7Ctrl_Cmd { get; set; }

        public CommandBase RelayIO8Ctrl_Cmd { get; set; }

        public CommandBase BackRobotResetCtrl_Cmd { get; set; }

        public DeviceRelayCtrlViewModel()
        {
            this.RelayCreatServer_Cmd = new CommandBase();
            this.RelayCreatServer_Cmd.DoExecute = new Action<object>(DoRelayCreatServer_CmdHandle);
            this.RelayCreatServer_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayCloseServer_Cmd = new CommandBase();
            this.RelayCloseServer_Cmd.DoExecute = new Action<object>(DoRelayCloseServer_CmddHandle);
            this.RelayCloseServer_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO1Ctrl_Cmd = new CommandBase();
            this.RelayIO1Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO1Ctrl_CmdHandle);
            this.RelayIO1Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO2Ctrl_Cmd = new CommandBase();
            this.RelayIO2Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO2Ctrl_CmdHandle);
            this.RelayIO2Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO3Ctrl_Cmd = new CommandBase();
            this.RelayIO3Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO3Ctrl_CmdHandle);
            this.RelayIO3Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO4Ctrl_Cmd = new CommandBase();
            this.RelayIO4Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO4Ctrl_CmdHandle);
            this.RelayIO4Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.FrontRobotResetCtrl_Cmd = new CommandBase();
            this.FrontRobotResetCtrl_Cmd.DoExecute = new Action<object>(DoFrontRobotResetCtrl_CmdHandle);
            this.FrontRobotResetCtrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO5Ctrl_Cmd = new CommandBase();
            this.RelayIO5Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO5Ctrl_CmdHandle);
            this.RelayIO5Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO6Ctrl_Cmd = new CommandBase();
            this.RelayIO6Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO6Ctrl_CmdHandle);
            this.RelayIO6Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO7Ctrl_Cmd = new CommandBase();
            this.RelayIO7Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO7Ctrl_CmdHandle);
            this.RelayIO7Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RelayIO8Ctrl_Cmd = new CommandBase();
            this.RelayIO8Ctrl_Cmd.DoExecute = new Action<object>(DoRelayIO8Ctrl_CmdHandle);
            this.RelayIO8Ctrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.BackRobotResetCtrl_Cmd = new CommandBase();
            this.BackRobotResetCtrl_Cmd.DoExecute = new Action<object>(DoBackRobotResetCtrl_CmdHandle);
            this.BackRobotResetCtrl_Cmd.DoCanExecute = new Func<object, bool>((o) => true);

            //模块信息
            ModWorkMsg = @"...";

            //数据协议信息
            RealyParaInfo = new DeviceRelayCtrlModel();
            RealyParaInfo.RelayRspMsg = @"硬件初始化成功";

            //挂接事件
            NetworkRelayCtrlHelper.GetInstance().NetworkRelayModInfoEvent += MyNetworkRelayModInfoEvent;
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
                ModWorkMsg = relayinfo.ModWorkMsg;

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

        private void DoRelayIO1Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO1Ctrl_Cmd);
            }
        }
        private void DoRelayIO2Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO2Ctrl_Cmd);
            }
        }
        private void DoRelayIO3Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO3Ctrl_Cmd);
            }
        }
        private void DoRelayIO4Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO4Ctrl_Cmd);
            }
        }
        private void DoRelayIO5Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO5Ctrl_Cmd);
            }
        }
        private void DoRelayIO6Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO6Ctrl_Cmd);
            }
        }
        private void DoRelayIO7Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO7Ctrl_Cmd);
            }
        }
        private void DoRelayIO8Ctrl_CmdHandle(object obj)
        {
            if (NetworkRelayCtrlHelper.GetInstance().IsEnable)
            {
                NetworkRelayCtrlHelper.GetInstance().DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayIO8Ctrl_Cmd);
            }
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
    }
}

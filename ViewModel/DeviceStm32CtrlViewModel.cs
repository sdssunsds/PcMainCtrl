using PcMainCtrl.Common;
using PcMainCtrl.HardWare.Stm32;
using PcMainCtrl.Model;
using System;
using static PcMainCtrl.HardWare.Stm32.Stm32ModCtrlProtocol;

namespace PcMainCtrl.ViewModel
{
    class DeviceStm32CtrlViewModel : NotifyBase
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

        public DeviceStm32CtrlModel Stm32CtrlModelInfo { get; set; }

        public CommandBase ConnectStm32ModServerCmd { get; set; }
        public CommandBase DisConnectStm32ModServerCmd { get; set; }
        public CommandBase SetCmdForRobotStartWorkVoiceCmd { get; set; }
        public CommandBase SetCmdForRobotStopWorkVoiceCmd { get; set; }
        public CommandBase GetTrainSiteCmd { get; set; }

        public DeviceStm32CtrlViewModel()
        {
            this.ConnectStm32ModServerCmd = new CommandBase();
            this.ConnectStm32ModServerCmd.DoExecute = new Action<object>(DoConnectStm32ModServerCmdHandle);
            this.ConnectStm32ModServerCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.DisConnectStm32ModServerCmd = new CommandBase();
            this.DisConnectStm32ModServerCmd.DoExecute = new Action<object>(DoDisConnectStm32ModServerCmdHandle);
            this.DisConnectStm32ModServerCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.SetCmdForRobotStartWorkVoiceCmd = new CommandBase();
            this.SetCmdForRobotStartWorkVoiceCmd.DoExecute = new Action<object>(DoSetCmdForRobotStartWorkVoiceCmdHandle);
            this.SetCmdForRobotStartWorkVoiceCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.SetCmdForRobotStopWorkVoiceCmd = new CommandBase();
            this.SetCmdForRobotStopWorkVoiceCmd.DoExecute = new Action<object>(DoSetCmdForRobotStopWorkVoiceCmdHandle);
            this.SetCmdForRobotStopWorkVoiceCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.GetTrainSiteCmd = new CommandBase();
            this.GetTrainSiteCmd.DoExecute = new Action<object>(DoGetTrainSiteCmdHandle);
            this.GetTrainSiteCmd.DoCanExecute = new Func<object, bool>((o) => true);

            //模块信息
            ModWorkMsg = @"...";

            //数据协议信息
            Stm32CtrlModelInfo = new DeviceStm32CtrlModel();
            Stm32CtrlModelInfo.VoiceMsg = @"...";
            Stm32CtrlModelInfo.Light1Percentage = 50;
            Stm32CtrlModelInfo.Light2Percentage = 50;

            Stm32CtrlModelInfo.CurrentDetectSite = @"库端";
            Stm32CtrlModelInfo.CurrentDetectHeight = -1;

            Stm32CtrlModelInfo.InfraRed1_Stat = -1;
            Stm32CtrlModelInfo.InfraRed2_Stat = -1;
            Stm32CtrlModelInfo.InfraRed3_Stat = -1;
            Stm32CtrlModelInfo.InfraRed4_Stat = -1;

            //挂接事件
            Stm32ModCtrlHelper.GetInstance().Stm32ModInfoEvent += MyStm32ModInfoEvent;
        }

        /// <summary>
        /// Stm32继电器信息事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void MyStm32ModInfoEvent(Stm32GlobalInfo stm32info)
        {
            try
            {
                //模块信息
                ModWorkMsg = stm32info.ModWorkMsg;

                //数据协议信息
                Stm32CtrlModelInfo.CurrentDetectHeight = stm32info.CurrentDetectHeight;
                Stm32CtrlModelInfo.InfraRed1_Stat = stm32info.InfraRed1_Stat;
                Stm32CtrlModelInfo.InfraRed2_Stat = stm32info.InfraRed2_Stat;
                Stm32CtrlModelInfo.InfraRed3_Stat = stm32info.InfraRed3_Stat;
                Stm32CtrlModelInfo.InfraRed4_Stat = stm32info.InfraRed4_Stat;
            }
            catch
            {

            }
        }

        /// <summary>
        /// 连接STM32控制板
        /// </summary>
        /// <param name="obj"></param>
        private void DoConnectStm32ModServerCmdHandle(object obj)
        {
            bool ret = Stm32ModCtrlHelper.GetInstance().Stm32ModConnect();
        }

        /// <summary>
        /// 断开STM32控制板 
        /// </summary>
        /// <param name="obj"></param>
        private void DoDisConnectStm32ModServerCmdHandle(object obj)
        {
            bool ret = Stm32ModCtrlHelper.GetInstance().Stm32ModDisConnect();
        }

        /// <summary>
        /// 命令：开始播报"开始作业语音"
        /// </summary>
        /// <param name="obj"></param>
        private void DoSetCmdForRobotStartWorkVoiceCmdHandle(object obj)
        {
            if (Stm32ModCtrlHelper.GetInstance().IsEnable==true)
            {
                //固定数据长度：64字节
                byte[] cmdbuf = new byte[Stm32Data.PcNetworkMsgBufsize];
                Array.Clear(cmdbuf, 0, cmdbuf.Length);
                byte cmdlen = 0;

                cmdbuf[0] = Stm32Data.LOCAL_STM32_DEVICE_ID;
                cmdlen++;

                Stm32ModCtrlHelper.GetInstance().DoStm32OpCmdHandle(Stm32Data.Stm32SetRobotStartVoice_FuncCode, cmdbuf, cmdlen);
            }
        }

        /// <summary>
        /// 命令：开始播报"停止作业语音"
        /// </summary>
        /// <param name="obj"></param>
        private void DoSetCmdForRobotStopWorkVoiceCmdHandle(object obj)
        {
            if (Stm32ModCtrlHelper.GetInstance().IsEnable == true)
            {
                //固定数据长度：64字节
                byte[] cmdbuf = new byte[Stm32Data.PcNetworkMsgBufsize];
                Array.Clear(cmdbuf, 0, cmdbuf.Length);
                byte cmdlen = 0;

                cmdbuf[0] = Stm32Data.LOCAL_STM32_DEVICE_ID;
                cmdlen++;

                Stm32ModCtrlHelper.GetInstance().DoStm32OpCmdHandle(Stm32Data.Stm32SetRobotStopVoice_FuncCode, cmdbuf, cmdlen);
            }
        }

        /// <summary>
        /// 命令：开始检测车头位置/密接车钩位置
        /// </summary>
        /// <param name="obj"></param>
        private void DoGetTrainSiteCmdHandle(object obj)
        {
            if (Stm32ModCtrlHelper.GetInstance().IsEnable == true)
            {
                //固定数据长度：64字节
                byte[] cmdbuf = new byte[Stm32Data.PcNetworkMsgBufsize];
                Array.Clear(cmdbuf, 0, cmdbuf.Length);
                byte cmdlen = 0;

                cmdbuf[0] = Stm32Data.LOCAL_STM32_DEVICE_ID;
                cmdlen++;

                Stm32ModCtrlHelper.GetInstance().DoStm32OpCmdHandle(Stm32Data.Stm32GetTrainSite_FuncCode, cmdbuf, cmdlen);
            }
        }
    }
}

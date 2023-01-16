using PcMainCtrl.Common;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.Model;
using System;

namespace PcMainCtrl.ViewModel
{
    class DeviceRgvCtrlViewModel : NotifyBase
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

        public DeviceRgvCtrlModel RgvParaInfo { get; set; }

        /// <summary>
        /// Rgv命令字
        /// </summary>
        public CommandBase RgvModConnectCmd { get; set; }
        public CommandBase RgvModDisConnectCmd { get; set; }

        public CommandBase RgvForwardMotorCmd{ get; set; }
        public CommandBase RgvBackMotorCmd { get; set; }
        public CommandBase RgvNormalStopCmd { get; set; }
        public CommandBase RgvStartIntelligentChargeCmd { get; set; }
        public CommandBase RgvStopIntelligentChargeCmd { get; set; }
        public CommandBase RgvClearAlarmCmd { get; set; }
        public CommandBase RgvRunAppointDistanceCmd { get; set; }

        /// <summary>
        /// 设定目标距离
        /// </summary>
        public CommandBase RgvSetTargetDistanceCmd { get; set; }

        /// <summary>
        /// 设定目标速度
        /// </summary>
        public CommandBase RgvSetTargetSpeedCmd { get; set; }

        /// <summary>
        /// 设定轨道长度
        /// </summary>
        public CommandBase RgvSetTrackLengthCmd { get; set; }

        public DeviceRgvCtrlViewModel()
        {
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

            this.RgvStartIntelligentChargeCmd = new CommandBase();
            this.RgvStartIntelligentChargeCmd.DoExecute = new Action<object>(DoRgvStartIntelligentChargeCmdHandle);
            this.RgvStartIntelligentChargeCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvStopIntelligentChargeCmd = new CommandBase();
            this.RgvStopIntelligentChargeCmd.DoExecute = new Action<object>(DoRgvStopIntelligentChargeCmdHandle);
            this.RgvStopIntelligentChargeCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvClearAlarmCmd = new CommandBase();
            this.RgvClearAlarmCmd.DoExecute = new Action<object>(DoRgvClearAlarmCmdHandle);
            this.RgvClearAlarmCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvRunAppointDistanceCmd = new CommandBase();
            this.RgvRunAppointDistanceCmd.DoExecute = new Action<object>(DoRgvRunAppointDistanceCmdHandle);
            this.RgvRunAppointDistanceCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvSetTargetDistanceCmd = new CommandBase();
            this.RgvSetTargetDistanceCmd.DoExecute = new Action<object>(DoRgvSetTargetDistanceCmdHandle);
            this.RgvSetTargetDistanceCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvSetTargetSpeedCmd = new CommandBase();
            this.RgvSetTargetSpeedCmd.DoExecute = new Action<object>(DoRgvSetTargetSpeedCmdHandle);
            this.RgvSetTargetSpeedCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.RgvSetTrackLengthCmd = new CommandBase();
            this.RgvSetTrackLengthCmd.DoExecute = new Action<object>(DoRgvSetTrackLengthCmdHandle);
            this.RgvSetTrackLengthCmd.DoCanExecute = new Func<object, bool>((o) => true);

            //模块信息
            ModWorkMsg = @"...";

            //数据协议信息
            RgvParaInfo = new DeviceRgvCtrlModel();
            RgvParaInfo.RgvCurrentMode = @"自动模式"; //默认远程模式(自动)； 本地模式(手动)
            RgvParaInfo.RgvCurrentStat = @"硬件初始化成功"; ; //默认小车状态

            RgvParaInfo.RgvCurrentCmdSetStat = @"N/A";
            RgvParaInfo.RgvCurrentParaSetStat = -1;

            RgvParaInfo.RgvCurrentRunSpeed = 500;
            RgvParaInfo.RgvCurrentRunDistacnce = 10000;
            RgvParaInfo.RgvTargetRunSpeed = 500;
            RgvParaInfo.RgvTargetRunDistance = 10000;
            RgvParaInfo.RgvTrackLength = 1000000;

            RgvParaInfo.RgvCurrentPowerStat = -1;
            RgvParaInfo.RgvCurrentPowerElectricity = -1;
            RgvParaInfo.RgvCurrentPowerCurrent = -1;
            RgvParaInfo.RgvCurrentPowerTempture = -1;

            RgvParaInfo.RgvIsAlarm = 0;
            RgvParaInfo.RgvIsStandby = 1;

            //挂接事件
            RgvModCtrlHelper.GetInstance().RgvModInfoEvent += MyRgvModInfoEvent;
        }

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
                ModWorkMsg = rgvinfo.ModWorkMsg;

                //数据协议信息
                RgvParaInfo.RgvCurrentStat = rgvinfo.RgvCurrentStat; //默认小车中间状态Msg信息

                RgvParaInfo.RgvCurrentMode = rgvinfo.RgvCurrentMode; //默认远程模式(自动)； 本地模式(手动)
                RgvParaInfo.RgvIsAlarm = rgvinfo.RgvIsAlarm; //Rgv小车异常状态：0 - 设备正常,1 - 设备异常
                RgvParaInfo.RgvIsStandby = rgvinfo.RgvIsStandby;//Rgv小车运行状态：0-运动状态,1-待机停止

                RgvParaInfo.RgvCurrentCmdSetStat = rgvinfo.RgvCurrentCmdSetStat;
                RgvParaInfo.RgvCurrentParaSetStat = rgvinfo.RgvCurrentParaSetStat;

                RgvParaInfo.RgvCurrentRunSpeed = rgvinfo.RgvCurrentRunSpeed;
                RgvParaInfo.RgvCurrentRunDistacnce = rgvinfo.RgvCurrentRunDistacnce;

                //反过来修改
                //rgvinfo.RgvTargetRunSpeed = RgvParaInfo.RgvTargetRunSpeed;
                //rgvinfo.RgvTargetRunDistance = RgvParaInfo.RgvTargetRunDistance;
                //rgvinfo.RgvTrackLength = RgvParaInfo.RgvTrackLength;

                //RgvParaInfo.RgvTargetRunSpeed = rgvinfo.RgvTargetRunSpeed;
                //RgvParaInfo.RgvTargetRunDistance = rgvinfo.RgvTargetRunDistance;
                //RgvParaInfo.RgvTrackLength = rgvinfo.RgvTrackLength;

                RgvParaInfo.RgvCurrentPowerStat = rgvinfo.RgvCurrentPowerStat;
                RgvParaInfo.RgvCurrentPowerElectricity = rgvinfo.RgvCurrentPowerElectricity;
                RgvParaInfo.RgvCurrentPowerCurrent = rgvinfo.RgvCurrentPowerCurrent;
                RgvParaInfo.RgvCurrentPowerTempture = rgvinfo.RgvCurrentPowerTempture;
            }
            catch
            {

            }
        }

        private void DoRgvConnectCmdHandle(object obj)
        {
            RgvModCtrlHelper.GetInstance().RgvModConnect();
        }

        private void DoRgvDisConnectCmdHandle(object obj)
        {
            RgvModCtrlHelper.GetInstance().RgvModDisConnect();
        }

        /// <summary>
        /// 正向运动
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvForwardMotorCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_FORWARDMOTOR);
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
            }
        }

        /// <summary>
        /// 反向运动
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvBackMotorCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_BACKWARDMOTOR);
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
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

        private void DoRgvStartIntelligentChargeCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_STARTPOWERCHARGE);
            }
        }

        private void DoRgvStopIntelligentChargeCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_STOPPOWERCHARGE);
            }
        }

        /// <summary>
        /// RgvCmd-运动到指定位置
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvRunAppointDistanceCmdHandle(object obj)
        {
            if (RgvModCtrlHelper.GetInstance().IsEnable)
            {
                RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_RUNAPPOINTDISTANCE);
                RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
            }
        }

        /// <summary>
        /// RgvCmd-设定目标运行位置
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvSetTargetDistanceCmdHandle(object obj)
        {
            //String distance_str = obj as String;
            int distance = 0x00;
            try
            {
                distance = (Int32)obj;
            }
            catch { }

            if (distance > RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTrackLength || distance < 0)
            {
                System.Windows.MessageBox.Show("Rgv运行目标距离,长度设置非法:超过轨道长度");
            }
            else
            {
                if (RgvModCtrlHelper.GetInstance().IsEnable)
                {
                    RgvParaInfo.RgvTargetRunDistance = distance;
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunDistance = RgvParaInfo.RgvTargetRunDistance;

                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETDISATNCE);
                }
            }
        }

        /// <summary>
        /// RgvCmd-设定目标运行速度
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvSetTargetSpeedCmdHandle(object obj)
        {
            int speed = 0x00;
            try
            {
                speed = (Int32) obj;
            }
            catch { }

            if (speed > 2000 || speed < 0)
            {
                System.Windows.MessageBox.Show("Rgv运行速度设置非法: >2000 或 <0");
            }
            else
            {
                if (RgvModCtrlHelper.GetInstance().IsEnable)
                {
                    RgvParaInfo.RgvTargetRunSpeed = speed;
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTargetRunSpeed = RgvParaInfo.RgvTargetRunSpeed;

                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETSPEED);
                }
            }
        }

        /// <summary>
        /// RgvCmd-设定轨道长度
        /// </summary>
        /// <param name="obj"></param>
        private void DoRgvSetTrackLengthCmdHandle(object obj)
        {
            int length = 0x00;
            try
            {
                length = (Int32) obj;
            }
            catch { }

            if (length > 1000000 || length < 0)
            {
                System.Windows.MessageBox.Show("Rgv运行速度设置非法: >1000m 或 <0");
            }
            else
            {
                if (RgvModCtrlHelper.GetInstance().IsEnable)
                {
                    RgvParaInfo.RgvTrackLength = length;
                    RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvTrackLength = RgvParaInfo.RgvTrackLength;

                    RgvModCtrlHelper.GetInstance().DoRgvOpCmdHandle(eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTRACKLENGTH);
                }
            }
        }
    }
}

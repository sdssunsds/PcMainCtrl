using PcMainCtrl.Common;
using PcMainCtrl.DataAccess;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HardWare.Stm32;
using PcMainCtrl.Model;
using System;
using System.Collections.ObjectModel;
using static PcMainCtrl.HardWare.Stm32.Stm32ModCtrlProtocol;

namespace PcMainCtrl.ViewModel
{
    public class DataXzCameraViewModel : NotifyBase
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

        /// <summary>
        /// Rgv命令字
        /// </summary>
        public CommandBase RgvModConnectCmd { get; set; }
        public CommandBase RgvModDisConnectCmd { get; set; }
        public CommandBase RgvForwardMotorCmd { get; set; }
        public CommandBase RgvBackMotorCmd { get; set; }
        public CommandBase RgvNormalStopCmd { get; set; }
        public CommandBase RgvClearAlarmCmd { get; set; }

        //-------------------------------------------------------------------------------------------------
        /// <summary>
        /// Stm32模块信息
        /// </summary>
        private String modStm32WorkMsg;
        public String ModStm32WorkMsg
        {
            get { return modStm32WorkMsg; }
            set
            {
                modStm32WorkMsg = value;
                this.DoNotify();
            }
        }
        public CommandBase ConnectStm32ModServerCmd { get; set; }
        public CommandBase DisConnectStm32ModServerCmd { get; set; }
        public CommandBase GetTrainSiteCmd { get; set; }
        //-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 面阵相机数据行
        /// </summary>
        public DataXzCameraModel dataXzCameraModel { get; set; }

        public ObservableCollection<DataXzCameraLineModel> xzCameraDataList { get; set; }

        public CommandBase XzCameraDataListQuery { get; set; }
        public CommandBase XzCameraDataListSave { get; set; }

        public DataXzCameraViewModel()
        {
            //XzCameraData
            dataXzCameraModel = new DataXzCameraModel();
            xzCameraDataList = new ObservableCollection<DataXzCameraLineModel>();

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

            //挂接事件
            RgvModCtrlHelper.GetInstance().RgvModInfoEvent += MyRgvModInfoEvent;
            #endregion

            #region Stm32命令
            this.ConnectStm32ModServerCmd = new CommandBase();
            this.ConnectStm32ModServerCmd.DoExecute = new Action<object>(DoConnectStm32ModServerCmdHandle);
            this.ConnectStm32ModServerCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.DisConnectStm32ModServerCmd = new CommandBase();
            this.DisConnectStm32ModServerCmd.DoExecute = new Action<object>(DoDisConnectStm32ModServerCmdHandle);
            this.DisConnectStm32ModServerCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.GetTrainSiteCmd = new CommandBase();
            this.GetTrainSiteCmd.DoExecute = new Action<object>(DoGetTrainSiteCmdHandle);
            this.GetTrainSiteCmd.DoCanExecute = new Func<object, bool>((o) => true);

            //模块信息
            ModStm32WorkMsg = @"...";

            //数据协议信息
            dataXzCameraModel.CurrentDetectHeight = -1;

            dataXzCameraModel.InfraRed1_Stat = -1;
            dataXzCameraModel.InfraRed2_Stat = -1;
            dataXzCameraModel.InfraRed3_Stat = -1;
            dataXzCameraModel.InfraRed4_Stat = -1;

            //挂接事件
            Stm32ModCtrlHelper.GetInstance().Stm32ModInfoEvent += MyStm32ModInfoEvent;
            #endregion

            #region 数据库操作
            //数据查询
            this.XzCameraDataListQuery = new CommandBase();
            this.XzCameraDataListQuery.DoExecute = new Action<object>(DoXzCameraDataListQueryHandle);
            this.XzCameraDataListQuery.DoCanExecute = new Func<object, bool>((o) => true);

            //数据保存
            this.XzCameraDataListSave = new CommandBase();
            this.XzCameraDataListSave.DoExecute = new Action<object>(DoXzCameraDataListSaveHandle);
            this.XzCameraDataListSave.DoCanExecute = new Func<object, bool>((o) => true);
            #endregion
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
                ModRgvWorkMsg = rgvinfo.ModWorkMsg;

                //数据协议信息
                dataXzCameraModel.RgvCurrentRunDistacnce = rgvinfo.RgvCurrentRunDistacnce;
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
        #endregion

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
                ModStm32WorkMsg = stm32info.ModWorkMsg;

                //数据协议信息
                dataXzCameraModel.CurrentDetectHeight = stm32info.CurrentDetectHeight;
                dataXzCameraModel.InfraRed1_Stat = stm32info.InfraRed1_Stat;
                dataXzCameraModel.InfraRed2_Stat = stm32info.InfraRed2_Stat;
                dataXzCameraModel.InfraRed3_Stat = stm32info.InfraRed3_Stat;
                dataXzCameraModel.InfraRed4_Stat = stm32info.InfraRed4_Stat;
            }
            catch
            {

            }
        }

        #region Stm32模块命令
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
        #endregion

        /// <summary>
        /// 数据库表数据查询
        /// </summary>
        /// <param name="obj"></param>
        private void DoXzCameraDataListQueryHandle(object obj)
        {
            xzCameraDataList.Clear();

            //查询数据库
            var cList = LocalDataBase.GetInstance().XzCameraDataListQurey();
            foreach (var item in cList)
            {
                xzCameraDataList.Add(item);
            }
        }

        /// <summary>
        /// 读取机器臂当前姿态数据保存至数据库
        /// </summary>
        /// <param name="obj"></param>
        private void DoXzCameraDataListSaveHandle(object obj)
        {
            //首先获取到当前数据
            DataXzCameraLineModel xzcamera_dataline = new DataXzCameraLineModel();

            xzcamera_dataline.TrainModel = dataXzCameraModel.TrainMode;
            xzcamera_dataline.TrainSn = dataXzCameraModel.TrainSn;

            xzcamera_dataline.CarriageId = dataXzCameraModel.CarriageId;
            xzcamera_dataline.CarriageType = dataXzCameraModel.CarriageType;

            xzcamera_dataline.Rgv_Id = 0;
            xzcamera_dataline.Rgv_Enable = 1;
            xzcamera_dataline.RgvCheckMinDistacnce = dataXzCameraModel.RgvCurrentRunDistacnce - 500;
            xzcamera_dataline.RgvCheckMaxDistacnce = dataXzCameraModel.RgvCurrentRunDistacnce + 500;

            xzcamera_dataline.CurrentDetectMinHeight = dataXzCameraModel.CurrentDetectHeight - 50;
            xzcamera_dataline.CurrentDetectMaxHeight = dataXzCameraModel.CurrentDetectHeight + 50;

            xzcamera_dataline.InfraRed1_Stat = dataXzCameraModel.InfraRed1_Stat;
            xzcamera_dataline.InfraRed2_Stat = dataXzCameraModel.InfraRed2_Stat;
            xzcamera_dataline.InfraRed3_Stat = dataXzCameraModel.InfraRed3_Stat;
            xzcamera_dataline.InfraRed4_Stat = dataXzCameraModel.InfraRed4_Stat;

            //将数据存储在数据库中
            LocalDataBase.GetInstance().XzCameraDataListSave(xzcamera_dataline);
        }
    }
}

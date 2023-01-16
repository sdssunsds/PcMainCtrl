using PcMainCtrl.Common;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.HardWare.NetworkRelay;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HardWare.Stm32;
using System;
using System.Reflection;
using System.Windows;
using static PcMainCtrl.Common.GlobalValues;

namespace PcMainCtrl.ViewModel
{
    class DeviceManagerViewModel : NotifyBase
    {
        private int logCount = 0;
        private int logMaxLine = 1000;
        private string log;
        private object logLock = new object();
        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 定义一个窗体框架属性
        /// </summary>
        private FrameworkElement _deviceMangerMainContent;
        public FrameworkElement DeviceManagerContent
        {
            get { return _deviceMangerMainContent; }
            set 
            { 
                _deviceMangerMainContent = value; 
                this.DoNotify(); 
            }
        }

        /// <summary>
        ///上一个页面 
        /// </summary>
        public String LastMainContentName { get; set; }

        /// <summary>
        /// 用于切换页面的命令
        /// </summary>
        public CommandBase DevManagerChangedCommand { get; set; }

        public DeviceManagerViewModel()
        {
            AddLogEvent += new AddLogs((string log, int type) =>
            {
                if (type == 0)
                {
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
            });

            this.DevManagerChangedCommand = new CommandBase();
            this.DevManagerChangedCommand.DoExecute = new Action<object>(DoDevManagerChanged);
            this.DevManagerChangedCommand.DoCanExecute = new Func<object, bool>((o) => true);

            DoDevManagerChanged("DeviceStm32CtrlView");
        }

        private void DoDevManagerChanged(object obj)
        {
            //判别当前页面的模块是否已使能,如果使能提示先正常关闭
            bool ret = false;
            if (LastMainContentName != null)
            {
                //得到当前页面的View
                if (LastMainContentName.Contains("DeviceStm32CtrlView") == true && Stm32ModCtrlHelper.GetInstance().IsEnable == true)
                {
                    ret = true;
                }
                else if (LastMainContentName.Contains("DeviceRelayCtrlView") == true && NetworkRelayCtrlHelper.GetInstance().IsEnable == true)
                {
                    ret = true;
                }
                else if (LastMainContentName.Contains("DeviceRgvCtrlView") == true && RgvModCtrlHelper.GetInstance().IsEnable == true)
                {
                    ret = true;
                }
                else if (LastMainContentName.Contains("DeviceRobotCtrlView") == true ) 
                {
                    if (Stm32ModCtrlHelper.GetInstance().IsEnable == true)
                    {
                        ret = true;
                    }
                }
                else if (LastMainContentName.Contains("DeviceCameraCtrlView") == true && CameraCtrlHelper.GetInstance().IsEnable == true)
                {
                    ret = true;
                }
            }

            if (ret == true)
            {
                MessageBox.Show("!!!当前硬件模块已打开,请先关闭...\n");
            }
            else
            {
                Type type = Type.GetType("PcMainCtrl.View." + obj.ToString());
                ConstructorInfo cti = type.GetConstructor(System.Type.EmptyTypes);
                this.DeviceManagerContent = (FrameworkElement)cti.Invoke(null);

                //记录上一个页面
                LastMainContentName = obj.ToString();
            }
        }
    }
}

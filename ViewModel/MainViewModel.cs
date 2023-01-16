using PcMainCtrl.Common;
using PcMainCtrl.DataAccess.DataEntity;
using PcMainCtrl.HardWare;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PcMainCtrl.ViewModel
{
    public class MainViewModel : NotifyBase
    {
        /// <summary>
        /// 定义一个窗体框架属性
        /// </summary>
        private FrameworkElement _mainContent;
        public FrameworkElement MainContent
        {
            get { return _mainContent; }
            set { _mainContent = value; this.DoNotify(); }
        }

        public String LastMainContentName{get; set;}

        /// <summary>
        /// 用于切换页面的命令
        /// </summary>
        public CommandBase NavChangedCommand { get; set; }

        public MainViewModel()
        {
            this.NavChangedCommand = new CommandBase();
            this.NavChangedCommand.DoExecute = new Action<object>(DoNavChanged);
            this.NavChangedCommand.DoCanExecute = new Func<object, bool>((o) => true);

            DoNavChanged("HomePageView");
        }

        private void DoNavChanged(object obj)
        {
            //处于繁忙状态，不能切换页面
            if (GlobalValues.UserInfo.myDeviceStat == UserEntity.key_DEVICE_BUSY)
            {
                MessageBox.Show("!!!机器人设备正在作业过程中,请勿操作...\n");
            }
            //处于空闲状态，允许切换页面,关闭所有的生产流程
            else
            {
                //HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_BASLERCAMERA);
                HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_ROBOT);
                HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_STM32);
                HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_ROBOT);
                HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_NETWORKRELAY);

                //切换页面
                Type type = Type.GetType("PcMainCtrl.View." + obj.ToString());
                ConstructorInfo cti = type.GetConstructor(System.Type.EmptyTypes);
                this.MainContent = (FrameworkElement)cti.Invoke(null);

                //记录上一个页面
                LastMainContentName = obj.ToString();
            }
        }
    }
}

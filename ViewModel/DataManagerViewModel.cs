using PcMainCtrl.Common;
using System;
using System.Reflection;
using System.Windows;

namespace PcMainCtrl.ViewModel
{
    public class DataManagerViewModel : NotifyBase
    {
        /// <summary>
        /// 定义一个窗体框架属性
        /// </summary>
        private FrameworkElement _dataMangerMainContent;
        public FrameworkElement DeviceManagerContent
        {
            get { return _dataMangerMainContent; }
            set
            {
                _dataMangerMainContent = value;
                this.DoNotify();
            }
        }


        /// <summary>
        /// 用于切换页面的命令
        /// </summary>
        public CommandBase DataManagerChangedCommand { get; set; }

        public DataManagerViewModel()
        {
            this.DataManagerChangedCommand = new CommandBase();
            this.DataManagerChangedCommand.DoExecute = new Action<object>(DoDataManagerChanged);
            this.DataManagerChangedCommand.DoCanExecute = new Func<object, bool>((o) => true);

            DoDataManagerChanged("DataXzCameraView");
        }

        private void DoDataManagerChanged(object obj)
        {
            Type type = Type.GetType("PcMainCtrl.View." + obj.ToString());
            ConstructorInfo cti = type.GetConstructor(System.Type.EmptyTypes);
            this.DeviceManagerContent = (FrameworkElement)cti.Invoke(null);
        }
    }
}

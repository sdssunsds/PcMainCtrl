using PcMainCtrl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PcMainCtrl.ViewModel
{
    public class TaskManagerViewModel : NotifyBase
    {
        /// <summary>
        /// 定义一个窗体框架属性
        /// </summary>
        private FrameworkElement _taskMangerMainContent;
        public FrameworkElement DeviceManagerContent
        {
            get { return _taskMangerMainContent; }
            set
            {
                _taskMangerMainContent = value;
                this.DoNotify();
            }
        }

        /// <summary>
        /// 用于切换页面的命令
        /// </summary>
        public CommandBase TaskManagerChangedCommand { get; set; }

        public TaskManagerViewModel()
        {
            this.TaskManagerChangedCommand = new CommandBase();
            this.TaskManagerChangedCommand.DoExecute = new Action<object>(DoTaskManagerChanged);
            this.TaskManagerChangedCommand.DoCanExecute = new Func<object, bool>((o) => true);

            DoTaskManagerChanged("TaskMzCameraView");
        }

        private void DoTaskManagerChanged(object obj)
        {
            Type type = Type.GetType("PcMainCtrl.View." + obj.ToString());
            ConstructorInfo cti = type.GetConstructor(System.Type.EmptyTypes);
            this.DeviceManagerContent = (FrameworkElement)cti.Invoke(null);
        }
    }
}

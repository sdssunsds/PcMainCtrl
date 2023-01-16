using PcMainCtrl.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PcMainCtrl.View
{
    /// <summary>
    /// TaskXzCameraView.xaml 的交互逻辑
    /// </summary>
    public partial class TaskMzCameraView : UserControl
    {
        public TaskMzCameraView()
        {
            InitializeComponent();

            TaskMzCameraViewModel model = new TaskMzCameraViewModel();
            this.DataContext = model;
        }
    }
}

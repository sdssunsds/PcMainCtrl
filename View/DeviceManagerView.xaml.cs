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
    /// DeviceManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceManagerView : UserControl
    {
        public DeviceManagerView()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.PrimaryScreenHeight;

            DeviceManagerViewModel model = new DeviceManagerViewModel();
            this.DataContext = model;
        }
    }
}

using PcMainCtrl.ViewModel;
using System.Windows;

namespace PcMainCtrl.View
{
    /// <summary>
    /// LogView.xaml 的交互逻辑
    /// </summary>
    public partial class LogView : Window
    {
        public LogView()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.PrimaryScreenHeight;

            LogViewModel model = new LogViewModel();
            this.DataContext = model;
        }
    }
}

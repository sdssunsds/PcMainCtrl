using PcMainCtrl.HardWare;
using PcMainCtrl.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace PcMainCtrl.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.PrimaryScreenHeight;

            MainViewModel model = new MainViewModel();
            this.DataContext = model;
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            HomePageView.model.TestForm();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            HomePageView.model.MzTestForm();
        }

        private void BtnSet_Click(object sender, RoutedEventArgs e)
        {
            new LogView().Show();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized ?
                WindowState.Normal : WindowState.Maximized);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //释放设备的资源
            HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_BASLERCAMERA);
            HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_ROBOT);
            HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_STM32);
            HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_ROBOT);
            HardwareHelper.HardwareDeInit(eHAL_TYPE_DEF.HAL_TYPE_NETWORKRELAY);

            System.Environment.Exit(0);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}

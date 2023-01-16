using PcMainCtrl.Common;
using PcMainCtrl.View;
using System.Windows;

namespace PcMainCtrl
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainView w = new MainView();
            w.ShowDialog();
            ThreadManager.CloseAllThread();
            Application.Current.Shutdown();
        }
    }
}

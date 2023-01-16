using Basler.Pylon;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.Model;
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
using static System.Windows.Forms.ListView;

namespace PcMainCtrl.View
{
    /// <summary>
    /// DeviceCameraCtrlView.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceCameraCtrlView : UserControl
    {
        public DeviceCameraCtrlView()
        {
            InitializeComponent();

            DeviceCameraCtrlViewModel model = new DeviceCameraCtrlViewModel();
            this.DataContext = model;

            //lv_camera.ItemsSource = model.CameraItemList;
        }

        private void lv_camera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(lv_camera.SelectedItem.GetType().ToString());
            String item = (lv_camera.SelectedItem as DeviceCameraCtrlModel).CameraFriendlyName;

            foreach (ICameraInfo camerainfo in CameraCtrlHelper.GetInstance().myCameraList)
            {
                if (item == camerainfo[CameraInfoKey.FriendlyName])
                {
                    CameraCtrlHelper.GetInstance().CameraChangeItem(camerainfo);
                    break;
                }
            }
        }
    }
}

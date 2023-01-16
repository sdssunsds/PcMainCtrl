using Basler.Pylon;
using PcMainCtrl.Common;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.BaslerCamera.CameraDataHelper;

namespace PcMainCtrl.ViewModel
{
    public class DeviceCameraCtrlViewModel : NotifyBase
    {
        /// <summary>
        /// 获取相机设备列表
        /// </summary>
        public ObservableCollection<DeviceCameraCtrlModel> CameraItemList { get; set; } = new ObservableCollection<DeviceCameraCtrlModel>();

        /// <summary>
        /// Balser camera捕捉图像
        /// </summary>
        private string grabImg1;
        public string GrabImg1
        {
            get { return grabImg1; }
            set
            {
                grabImg1 = value;
                this.DoNotify();
            }
        }

        private string grabImg2;
        public string GrabImg2
        {
            get { return grabImg2; }
            set
            {
                grabImg2 = value;
                this.DoNotify();
            }
        }

        private string grabImg3;
        public string GrabImg3
        {
            get { return grabImg3; }
            set
            {
                grabImg3 = value;
                this.DoNotify();
            }
        }

        #region  Camera控制命令
        public CommandBase CameraCmd_Scan { get; set; }
        public CommandBase CameraCmd_Destroy { get; set; }
 
        /// <summary>
        /// Camera单拍命令
        /// </summary>
        public CommandBase CameraCmd_OneShot { get; set; }

        /// <summary>
        /// Camera连续抓拍命令
        /// </summary>
        public CommandBase CameraCmd_ContinuousShot { get; set; }

        /// <summary>
        /// Camera停止命令
        /// </summary>
        public CommandBase CameraCmd_Stop { get; set; }

        /// <summary>
        /// Camera拼图
        /// </summary>
        public CommandBase CameraCmd_Join { get; set; }

        /// <summary>
        /// Camera压缩
        /// </summary>
        public CommandBase CameraCmd_Compress { get; set; }
        #endregion

        public DeviceCameraCtrlViewModel()
        {
            //绑定命令
            this.CameraCmd_Scan = new CommandBase();
            this.CameraCmd_Scan.DoExecute = new Action<object>(DoCameraCmdHandle_Scan);
            this.CameraCmd_Scan.DoCanExecute = new Func<object, bool>((o) => true);

            this.CameraCmd_Destroy = new CommandBase();
            this.CameraCmd_Destroy.DoExecute = new Action<object>(DoCameraCmdHandle_Destroy);
            this.CameraCmd_Destroy.DoCanExecute = new Func<object, bool>((o) => true);

            this.CameraCmd_OneShot = new CommandBase();
            this.CameraCmd_OneShot.DoExecute = new Action<object>(DoCameraCmdHandle_OneShot);
            this.CameraCmd_OneShot.DoCanExecute = new Func<object, bool>((o) => true);

            this.CameraCmd_ContinuousShot = new CommandBase();
            this.CameraCmd_ContinuousShot.DoExecute = new Action<object>(DoCameraCmdHandle_ContinuousShot);
            this.CameraCmd_ContinuousShot.DoCanExecute = new Func<object, bool>((o) => true);

            this.CameraCmd_Stop = new CommandBase();
            this.CameraCmd_Stop.DoExecute = new Action<object>(DoCameraCmdHandle_Stop);
            this.CameraCmd_Stop.DoCanExecute = new Func<object, bool>((o) => true);

            this.CameraCmd_Join = new CommandBase();
            this.CameraCmd_Join.DoExecute = new Action<object>(DoCameraCmdHandle_Join);
            this.CameraCmd_Join.DoCanExecute = new Func<object, bool>((o) => true);

            this.CameraCmd_Compress = new CommandBase();
            this.CameraCmd_Compress.DoExecute = new Action<object>(DoCameraCmdHandle_Compress);
            this.CameraCmd_Compress.DoCanExecute = new Func<object, bool>((o) => true);

            //挂接事件
            CameraCtrlHelper.GetInstance().CameraImageEvent += Camera_CameraImageEvent;
        }

        /// <summary>
        /// 相机捕捉图片事件
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        private void Camera_CameraImageEvent(Camera camera, Bitmap bmp)
        {
            //保存图片到本地
            String strheigth = bmp.Height.ToString();

            try
            {
                ICameraInfo camerainfo = camera.CameraInfo;
                if (camerainfo[CameraInfoKey.FriendlyName].Contains(@"camera_front"))
                {
                    //保存图片到本地
                    String strfile = System.Environment.CurrentDirectory + @"\test_data\front_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + strheigth + @"H.jpg";
                    CameraDataHelper.SaveBitmapIntoFile(bmp, strfile, CameraDataHelper.JoinMode.Vertical);

                    GrabImg1 = strfile;
                }
                else if (camerainfo[CameraInfoKey.FriendlyName].Contains(@"camera_back"))
                {
                    //保存图片到本地
                    String strfile = System.Environment.CurrentDirectory + @"\test_data\back_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + strheigth + @"H.jpg";
                    CameraDataHelper.SaveBitmapIntoFile(bmp, strfile, CameraDataHelper.JoinMode.Vertical);

                    GrabImg2 = strfile;
                }
                else if (camerainfo[CameraInfoKey.FriendlyName].Contains(@"xz"))
                {
                    String strfile = System.Environment.CurrentDirectory + @"\test_data\xz_camera\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + strheigth + @"H.jpg";
                    CameraDataHelper.SaveBitmapIntoFile(bmp, strfile, CameraDataHelper.JoinMode.Vertical);

                    GrabImg3 = strfile;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("DeviceCameraCtrlViewModel Exception caught:\n" + exception.Message);
            }
        }

        /// <summary>
        /// 搜索可用相机
        /// </summary>
        void InitCameraList()
        {
            try
            {
                //搜索可用相机
                CameraCtrlHelper.GetInstance().CameraConnection();

                //添加到相机列表视图
                CameraItemList.Clear();
                foreach (var item in CameraCtrlHelper.GetInstance().myCameraList)
                {
                    DeviceCameraCtrlModel CameraItem = new DeviceCameraCtrlModel();
                    CameraItem.CameraFriendlyName = item[CameraInfoKey.FriendlyName];

                    CameraItemList.Add(CameraItem);
                }
            }
            catch
            {

            }
        }
        void DoCameraCmdHandle_Scan(object obj)
        {
            //关闭已有相机
            if (CameraCtrlHelper.GetInstance().IsEnable == false)
            {
                CameraCtrlHelper.GetInstance().CameraDestroy();
            }

            //搜索相机列表
            InitCameraList();

            if (CameraCtrlHelper.GetInstance().IsEnable)
            {
                //默认选中挂接到第1个相机
                CameraCtrlHelper.GetInstance().CameraChangeItem(CameraCtrlHelper.GetInstance().myCameraList[0]);
            }
        }

        void DoCameraCmdHandle_Destroy(object obj)
        {
            CameraCtrlHelper.GetInstance().CameraDestroy();
        }

        #region 命令处理数据流
        void DoCameraCmdHandle_OneShot(object obj)
        {
            //默认选中挂接到第1个相机
            ThreadSleep(200);
            Trace.WriteLine("Get Camera Device Information");
            Trace.WriteLine("=========================");
            Trace.WriteLine("Width           : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].GetValue().ToString());
            Trace.WriteLine("Height          : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].GetValue().ToString());
            Trace.WriteLine("ExposureTimeAbs : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].GetValue().ToString());
            Trace.WriteLine("======================");

            Trace.WriteLine("Get Camera Device Information");
            Trace.WriteLine("=========================");
            Trace.WriteLine("Width           : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Width].GetValue().ToString());
            Trace.WriteLine("Height            : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.Height].GetValue().ToString());
            Trace.WriteLine("ExposureTimeAbs : {0}", CameraCtrlHelper.GetInstance().camera.Parameters[PLCamera.ExposureTimeAbs].GetValue().ToString());
            Console.WriteLine("======================");

            CameraCtrlHelper.GetInstance().CameraOneShot();
        }

        void DoCameraCmdHandle_ContinuousShot(object obj)
        {
            CameraCtrlHelper.GetInstance().CameraContinuousShot();
        }

        void DoCameraCmdHandle_Stop(object obj)
        {
            CameraCtrlHelper.GetInstance().CameraStop();
        }

        void DoCameraCmdHandle_Join1(object obj)
        {
            List<Image> img_list= new List<Image>();
            img_list.Clear();

            TaskRun(new Action(() =>
            {
                //1.首先读取本地的所有图片文件
                DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory + @"\test_data\xz_camera\");
                FileInfo[] inf = dir.GetFiles();
                foreach (FileInfo finf in inf)
                {
                    //如果扩展名为“.bmp”
                    if (finf.Extension.Equals(".jpg"))
                    {
                        //读取文件的完整目录和文件名
                        Image newImage = Image.FromFile(finf.FullName);
                        img_list.Add(newImage);
                    }
                }

                //2.然后进行拼接
                Image img = CameraDataHelper.JoinImage(img_list, JoinMode.Vertical);
                img.Save(System.Environment.CurrentDirectory  + @"\test_data\xz_join\join.jpg");
            }));
        }

        /// <summary>
        /// 线阵拼图
        /// </summary>
        /// <param name="obj"></param>
        void DoCameraCmdHandle_Join(object obj)
        {
            List<Image> img_list = new List<Image>();
            img_list.Clear();

            TaskRun(new Action(() =>
            {
                //1.首先读取本地的所有图片文件
                DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory + @"\test_data\xz_compress\");
                FileInfo[] inf = dir.GetFiles();
                foreach (FileInfo finf in inf)
                {
                    //如果扩展名为“.bmp”
                    if (finf.Extension.Equals(".jpg"))
                    {
                        //读取文件的完整目录和文件名
                        Image newImage = Image.FromFile(finf.FullName);
                        img_list.Add(newImage);
                    }
                }

                //2.然后进行拼接
                Image img = CameraDataHelper.JoinImage(img_list, JoinMode.Vertical);
                String strfile = System.Environment.CurrentDirectory + @"\test_data\xz_join\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff_") + @"join.jpg";
                CameraDataHelper.SaveBitmapIntoFile(img, strfile, CameraDataHelper.JoinMode.Vertical);
            }));
        }

        /// <summary>
        /// 线阵压缩
        /// </summary>
        /// <param name="obj"></param>
        void DoCameraCmdHandle_Compress(object obj)
        {
            String name = obj as String;

            List<Image> img_list = new List<Image>();
            img_list.Clear();

            TaskRun(new Action(() =>
            {
                //1.首先读取本地的所有图片文件
                String src_dir = System.Environment.CurrentDirectory + @"\test_data\xz_camera\";
                String dest_dir = System.Environment.CurrentDirectory + @"\test_data\xz_compress\" + name + ".zip";

                CameraDataHelper.CreateZipFile(src_dir, dest_dir);
            }));
        }

        void DoCameraCmdHandle_UpdateDeviceList(object obj)
        {
            CameraCtrlHelper.GetInstance().CameraScanner();
        }
        #endregion
    }
}

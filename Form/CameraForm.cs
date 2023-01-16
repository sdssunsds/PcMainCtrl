using Basler;
using PcMainCtrl.HardWare.BaslerCamera;
using System;
using System.Drawing;
using System.Windows.Forms;
using static PcMainCtrl.Common.GlobalValues;

namespace PcMainCtrl.Form
{
    public partial class CameraForm : System.Windows.Forms.Form
    {
        private static int frontID, backID;
        public static CameraManager cameraManager = null;
        public bool IsFrontCamaer { get; set; }

        public CameraForm()
        {
            InitializeComponent();
        }

        private void CameraForm_Load(object sender, EventArgs e)
        {
            if (IsFrontCamaer)
            {
                this.Text = "前机械臂相机";
            }
            else
            {
                this.Text = "后机械臂相机";
            }
            InitCameraMod();
            DoCameraCmdHandle_ContinuousShot();
        }

        private void CameraForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoCameraCmdHandle_Stop();
        }

        #region 相机
        private void Item_ImageReady(object sender, CameraEventArgs e)
        {
            try
            {
                BaslerCamera camera = sender as BaslerCamera;
                if (e.Image.Bitmap == null)
                {
                    return;
                }
                bool isMap = IsFrontCamaer ? camera.Name.ToLower().Contains("camera_front") : camera.Name.ToLower().Contains("camera_back");
                Bitmap bitmap = e.Image.Bitmap;
                if (!this.Disposing && isMap)
                {
                    BeginInvoke(new Action(() =>
                    {
                        pictureBox1.Image = bitmap;
                    }));
                }
            }
            catch (Exception) { }
        }

        private void InitCameraMod()
        {
            try
            {
                if (cameraManager != null)
                {
                    int i = 0;
                    foreach (ICamera item in cameraManager.Cameras)
                    {
                        item.ImageReady += Item_ImageReady;
                        if (item.Name.ToLower().Contains("camera_front"))
                        {
                            frontID = i;
                            item.Format = "";
                        }
                        else if (item.Name.ToLower().Contains("camera_back"))
                        {
                            backID = i;
                            item.Format = "";
                        }
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                AddLog("CameraForm.class InitCameraMod: " + e.Message, -1);
            }
        }

        private void DoCameraCmdHandle_ContinuousShot()
        {
            try
            {
                int id = IsFrontCamaer ? frontID : backID;
                if (cameraManager != null)
                {
                    if (!cameraManager.Cameras[id].Opened)
                    {
                        cameraManager.Cameras[id].Open();
                    }
                    if (cameraManager.Cameras[id].Opened)
                    {
                        if (id == frontID)
                        {
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                        }
                        else if (id == backID)
                        {
                            CameraCtrlHelper.GetInstance().myCameraGlobalInfo.BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_SNAP;
                        }
                        cameraManager.Cameras[id].ContinuousShot();
                    } 
                }
            }
            catch (Exception e)
            {
                AddLog("CameraForm.class DoCameraCmdHandle_ContinuousShot: " + e.Message, -1);
            }
        }

        private void DoCameraCmdHandle_Stop()
        {
            try
            {
                if (cameraManager != null)
                {
                    int id = IsFrontCamaer ? frontID : backID;
                    if (cameraManager.Cameras[id].Opened)
                    {
                        cameraManager.Cameras[id].Stop(); 
                    }
                }
            }
            catch (Exception e)
            {
                AddLog("CameraForm.class DoCameraCmdHandle_Stop: " + e.Message, -1);
            }
        }
        #endregion
    }
}

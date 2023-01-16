using Basler.Pylon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.HardWare.BaslerCamera
{
    public enum eCAMERAMODRUNSTAT
    {
        /// <summary>
        /// 准备
        /// </summary>
        CAMERAMODRUNSTAT_READY = 0,
        /// <summary>
        /// 启动
        /// </summary>
        CAMERAMODRUNSTAT_SNAP = 1,
        /// <summary>
        /// 停止
        /// </summary>
        CAMERAMODRUNSTAT_STOP = 2,
    }

    public class CameraGlobalInfo
    {
        public eCAMERAMODRUNSTAT FrontCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
        public eCAMERAMODRUNSTAT BackCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
        public eCAMERAMODRUNSTAT XzCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
        public eCAMERAMODRUNSTAT XzLeftCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
        public eCAMERAMODRUNSTAT XzRightCameraRunStatMonitor = eCAMERAMODRUNSTAT.CAMERAMODRUNSTAT_READY;
    }

    /// <summary>
    /// 定义一个camera实例 && 相机抓拍事件回调
    /// </summary>
    public class CameraCtrlHelper
    {
        private bool canProcess = false;
        public bool IsEnable = false;

        #region Camera模块单例
        private static CameraCtrlHelper instance;
        private CameraCtrlHelper() { }
        public static CameraCtrlHelper GetInstance()
        {
            try
            {
                return instance ?? (instance = new CameraCtrlHelper());
            }
            catch (Exception)
            {
                return instance = null;
            }
        }
        #endregion

        /// <summary>
        /// 委托+事件 = 回调函数，用于传递相机抓取的图像
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="bmp"></param>
        public delegate void CameraImage(Camera camera, Bitmap bmp);
        public event CameraImage CameraImageEvent;
        public event Action<Exception> CameraErrorEvent;

        public CameraGlobalInfo myCameraGlobalInfo = new CameraGlobalInfo();

        /// <summary>
        /// 条目选中的camera
        /// </summary>
        public Camera camera = null;

        public bool IsCanContinuousShot { get; set; }

        public bool IsCanOneShot { get; set; }

        public bool IsCanStop { get; set; }

        /// <summary>
        /// basler里用于将相机采集的图像转换成位图
        /// </summary>
        private PixelDataConverter converter = new PixelDataConverter();

        /// <summary>
        /// 抓图处理完的结果
        /// </summary>
        public PictureBox pic = new PictureBox();

        /// <summary>
        /// 查询到的所有相机设备信息
        /// </summary>
        public List<ICameraInfo> myCameraList = new List<ICameraInfo>();

        #region Camera事件处理
        // Occurs when a device with an opened connection is removed.
        private void OnConnectionLost(Object sender, EventArgs e)
        {
            TaskRun(new Action(()=>
            {
                // Close the camera object.
                CameraDestroy();

                // Because one device is gone, the list needs to be updated.
                CameraScanner();
            }));
        }

        // Occurs when the connection to a camera device is opened.
        private void OnCameraOpened(Object sender, EventArgs e)
        {
            // The image provider is ready to grab. Enable the grab buttons.
            CameraEnableOpRefresh(true, false);
        }

        // Occurs when the connection to a camera device is closed.
        private void OnCameraClosed(Object sender, EventArgs e)
        {
            // The camera connection is closed. Disable all buttons.
            CameraEnableOpRefresh(false, false);
        }

        // Occurs when a camera starts grabbing.
        private void OnGrabStarted(Object sender, EventArgs e)
        {
            // The camera is grabbing. Disable the grab buttons. Enable the stop button.
            CameraEnableOpRefresh(false, true);
        }

        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                // Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.

                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;

                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);

                    //转换grabResult为BitmapData
                    // Lock the bits of the bitmap.
                    BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                    // Place the pointer to the buffer of the bitmap.
                    converter.OutputPixelFormat = PixelType.BGRA8packed;
                    IntPtr ptrBmp = bmpData.Scan0; //在位图中第一个像素数据的地址
                    converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult); //Exception handling TODO
                    bitmap.UnlockBits(bmpData);

                    // Assign a temporary variable to dispose the bitmap after assigning the new bitmap to the display control.
                    Bitmap bitmapOld = pic.Image as Bitmap;

                    // Provide the display control with the new bitmap. This action automatically updates the display.
                    pic.Image = bitmap;
                    if (bitmapOld != null)
                    {
                        // Dispose the bitmap.
                        bitmapOld.Dispose();
                    }

                    //发送事件
                    TaskRun(() =>
                    {
                        CameraImageEvent(camera, pic.Image as Bitmap);
                    });
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }

        // Occurs when a camera has stopped grabbing.
        private void OnGrabStopped(Object sender, GrabStopEventArgs e)
        {
            // The camera stopped grabbing. Enable the grab buttons. Disable the stop button.
            CameraEnableOpRefresh(true, false);

            // If the grabbed stop due to an error, display the error message.
            if (e.Reason != GrabStopReason.UserRequest)
            {
                MessageBox.Show("A grab error occured:\n" + e.ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region  Camera操作方法
        // Shows exceptions in a message box.
        private void ShowException(Exception e)
        {
            AddLog("相机 Exception: " + e.Message, -1);
            CameraErrorEvent?.Invoke(e);
        }

        public List<ICameraInfo> CameraScanner()
        {
            myCameraList.Clear();

            try
            {
                myCameraList = CameraFinder.Enumerate();
                List<ICameraInfo> list = new List<ICameraInfo>();
                foreach (ICameraInfo item in myCameraList)
                {
                    if (list.Find(l=>l[CameraInfoKey.DeviceIpAddress] == item[CameraInfoKey.DeviceIpAddress]) == null)
                    {
                        list.Add(item);
                    }
                }
                myCameraList.Clear();
                myCameraList.AddRange(list);

                if (myCameraList.Count == 0)
                {
                    canProcess = true;
                    myCameraList.Add(new CameraModel());
                    myCameraList.Add(new CameraModel());
                    myCameraList.Add(new CameraModel());
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }

            return myCameraList;
        }

        // Helps to set the states of all buttons.
        private void CameraEnableOpRefresh(bool canGrab, bool canStop)
        {
            IsCanContinuousShot = canGrab;
            IsCanOneShot = canGrab;
            IsCanStop = canStop;
        }

        public void CameraConnection()
        {
            CameraScanner();

            if (myCameraList != null)
            {
                //更新状态
                IsEnable = true;
            }
        }
        private Dictionary<ICameraInfo, Camera> dict = new Dictionary<ICameraInfo, Camera>();
        public void CameraChangeItem(ICameraInfo selectedCamera)
        {
            if (!canProcess)
            {
                //CameraDestroy();
                if (dict.ContainsKey(selectedCamera))
                {
                    camera = dict[selectedCamera];
                }

                else
                {
                    try
                    {
                        camera = new Camera(selectedCamera);
                        camera.CameraOpened += Configuration.AcquireContinuous;
                        camera.ConnectionLost += OnConnectionLost;
                        camera.CameraOpened += OnCameraOpened;
                        camera.CameraClosed += OnCameraClosed;
                        camera.StreamGrabber.GrabStarted += OnGrabStarted;
                        camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                        camera.StreamGrabber.GrabStopped += OnGrabStopped;
                        camera.Open();
                        dict.Add(selectedCamera, camera); ;
                    }
                    catch (Exception exception)
                    {
                        ShowException(exception);
                    } 
                }
            }
        }

        // Starts the grabbing of a single image and handles exceptions.
        public void CameraOneShot(string name = "", string path = "")
        {
            if (canProcess)
            {
                Process.Start("CameraForm.exe", "1 " + name + " " + path);
                WaitImage(name, path);
            }
            else
            {
                try
                {
                    camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                }
                try
                {
                    camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                }
            }
        }

        // Starts the continuous grabbing of images and handles exceptions.
        public void CameraContinuousShot(string path = "")
        {
            if (canProcess)
            {
                Process.Start("CameraForm.exe", "2 xz " + path);
                WaitImage("xz", path);
            }
            else
            {
                CameraStop();
                try
                {
                    camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                }
                try
                {
                    camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                }
            }
        }

        // Stops the grabbing of images and handles exceptions.
        public void CameraStop()
        {
            if (canProcess)
            {
                if (!File.Exists(Application.StartupPath + @"\stop.txt"))
                {
                    File.Create(Application.StartupPath + @"\stop.txt"); 
                }
            }
            else
            {
                try
                {
                    camera?.StreamGrabber.Stop();
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                } 
            }
        }

        // Closes the camera object and handles exceptions.
        public void CameraDestroy()
        {
            try
            {
                if (camera != null)
                {
                    camera.Close();
                    camera.Dispose();
                    camera = null;
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }

            //更新状态
            IsEnable = false;
        }

        public void CameraAllClose()
        {
            foreach (KeyValuePair<ICameraInfo, Camera> item in dict)
            {
                try
                {
                    if (item.Value != null)
                    {
                        item.Value.Close();
                        item.Value.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                }

            }
            dict.Clear();
        }

        private void WaitImage(string name, string path)
        {
            TaskRun(() =>
            {
                do
                {
                    ThreadSleep(50);
                } while (!File.Exists(path));
                CameraImageEvent?.Invoke(new CameraData() { Name = name, Path = path }, null);
            });
        }
        #endregion 
    }
}

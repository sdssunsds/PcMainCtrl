using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace PcMainCtrl.HardWare
{
    public class DkamHelper
    {
        #region 相机参数
        private const int exposure_red = 30000;  // 红外曝光
        private const int exposure_rgb = 30000;  // 彩图曝光
        #endregion

        private object reLinkLock = new object();
        private bool isInit = false;
        private int width, height;
        private int pointsize = 0;
        private int graysize = 0;
        private int RGBsize = 0;
        private byte[] point_pixel = null;
        private byte[] gray_pixel = null;
        private byte[] RGB_pixel = null;
        private PhotoInfoCSharp PointCloud_data = null;
        private PhotoInfoCSharp gray_data = null;
        private PhotoInfoCSharp RGB_data = null;
        private SWIGTYPE_p_CAMERA_OBJECT camera_obj = null;
        private float[] kc = new float[5];
        private float[] kk = new float[9];
        private List<string> cameraIPs = null;
        private List<string> linkIPs = null;

        private static DkamHelper instance = null;
        public static DkamHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DkamHelper();
                }
                return instance;
            }
        }
        public bool IsInit
        {
            get { return isInit; }
        }
        public Action<string, int> Addlog { private get; set; }
        public Func<bool> ReLinkCameraFaid { private get; set; }
        private DkamHelper()
        {
            cameraIPs = new List<string>();
            linkIPs = new List<string>();
            DkamSDK_CSharp.SetLogLevel(1, 0, 0, 0);
        }
        /// <summary>
        /// 关闭相机
        /// </summary>
        public void Close()
        {
            int streamoff = 0, streamoffpoint = 0, streamoffRGB = 0, dis = 0;
            try
            {
                DkamSDK_CSharp.AcquisitionStop(camera_obj);
                streamoff = DkamSDK_CSharp.StreamOff(camera_obj, 0);
                Addlog("StreamOff Gray = " + streamoff, 5);
                streamoffpoint = DkamSDK_CSharp.StreamOff(camera_obj, 1);
                Addlog("StreamOff Point = " + streamoffpoint, 5);
                streamoffRGB = DkamSDK_CSharp.StreamOff(camera_obj, 2);
                Addlog("StreamOff RGB = " + streamoffRGB, 5);
                dis = DkamSDK_CSharp.CameraDisconnect(camera_obj);
                Addlog("CameraDisconnect = " + dis, 5);
                DkamSDK_CSharp.DestroyCamera(camera_obj);
            }
            catch (Exception e)
            {
                Addlog("关闭点云相机时异常：" + e.Message, -1);
            }
        }
        /// <summary>
        /// 驱动初始化
        /// </summary>
        public bool Init()
        {
            try
            {
                int[] data = new int[1];
                DkamSDK_CSharp.GetCameraCCPStatus(camera_obj, data);
                Addlog("DkamStatus: " + data[0], 5);
                DkamSDK_CSharp.SaveXmlToLocal(camera_obj, Application.StartupPath + "\\");
                #region 设置曝光模式
                int SetAutoExposureRGB = DkamSDK_CSharp.SetAutoExposure(camera_obj, 1, 1);
                Addlog("SetAutoExposureRGB = " + SetAutoExposureRGB, 5);
                int SetAutoExposure = DkamSDK_CSharp.SetAutoExposure(camera_obj, 1, 0);
                Addlog("SetAutoExposure = " + SetAutoExposure, 5);
                #endregion
                #region 设置曝光等级(当前只对RGB有效)
                SetAutoExposureRGB = DkamSDK_CSharp.SetAutoExposure(camera_obj, 1, 1);
                int setExposureGainLevel = DkamSDK_CSharp.SetCamExposureGainLevel(camera_obj, 1, 3);
                Addlog("Set CamExposureGainLevel = " + setExposureGainLevel, 5);
                #endregion
                #region 设置多重曝光
                int setMutipleEx = DkamSDK_CSharp.SetMutipleExposure(camera_obj, 1);
                Addlog("Set MutipleExposure = " + setMutipleEx, 5);
                #endregion
                #region 设置增益
                int setGain = DkamSDK_CSharp.SetGain(camera_obj, 1, 3, 0);
                Addlog("SetGain = " + setGain, 5);
                #endregion
                DkamSDK_CSharp.GetCamInternelParameter(camera_obj, 0, kc, kk);

                SWIGTYPE_p_int width_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraWidth(camera_obj, width_gray, 0);
                width = DkamSDK_CSharp.intArray_getitem(width_gray, 0);

                SWIGTYPE_p_int height_gray = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraHeight(camera_obj, height_gray, 0);
                height = DkamSDK_CSharp.intArray_getitem(height_gray, 0);

                #region 设置心跳
                int setHeart = DkamSDK_CSharp.SetHeartBeatTimeout(camera_obj, 15000);
                Addlog("Set Heart = " + setHeart, 5); 
                #endregion
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 初始化全流程
        /// </summary>
        /// <param name="ip">相机地址</param>
        /// <param name="initInfraRed">初始化红外</param>
        /// <param name="initPointCloud">初始化点云和深度图</param>
        /// <param name="initRGB">初始化彩色相机</param>
        public bool Init(string ip, bool initInfraRed = false, bool initPointCloud = false, bool initRGB = false)
        {
            if (LinkCamera(ip))
            {
                if (Init())
                {
                    if (initInfraRed)
                    {
                        if (!InitInfraRed())
                        {
                            return false;
                        }
                    }

                    if (initPointCloud)
                    {
                        if (!InitPointCloud())
                        {
                            return false;
                        }
                    }

                    if (initRGB)
                    {
                        if (!InitRGB())
                        {
                            return false;
                        }
                    }

                    return isInit = Inited();
                } 
            }
            return false;
        }
        /// <summary>
        /// 完成初始化
        /// </summary>
        public bool Inited()
        {
            try
            {
                //接受数据
                int start = DkamSDK_CSharp.AcquisitionStart(camera_obj);
                Addlog("AcquisitionStart = " + start, 5);
                //设置重发包请求
                DkamSDK_CSharp.SetResendRequest(camera_obj, 0, 1);
                Addlog("GetResendRequest = " + DkamSDK_CSharp.GetResendRequest(camera_obj, 0), 5);
                //设置红外触发模式
                int TirggMode = DkamSDK_CSharp.SetTriggerMode(camera_obj, 1);
                Addlog("Tirgger Mode = " + TirggMode, 5);
                //设置RGB触发模式
                int TirggModeRGB = DkamSDK_CSharp.SetRGBTriggerMode(camera_obj, 1);
                Addlog("Tirgger Mode RGB = " + TirggModeRGB, 5);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 红外初始化
        /// </summary>
        public bool InitInfraRed()
        {
            try
            {
                gray_data = new PhotoInfoCSharp();
                graysize = width * height;
                gray_pixel = new byte[graysize];

                int setexposureTime = DkamSDK_CSharp.SetExposureTime(camera_obj, exposure_red, 0);
                Addlog("Set ExposureTime = " + setexposureTime, 5);
                if (setexposureTime < 0)
                {
                    Addlog("点云相机设置红外曝光时出现错误码：" + setexposureTime, -1);
                }

                int streamgray = DkamSDK_CSharp.StreamOn(camera_obj, 0);
                return streamgray == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 点云与深度相机初始化
        /// </summary>
        public bool InitPointCloud()
        {
            try
            {
                PointCloud_data = new PhotoInfoCSharp();
                pointsize = width * height * 6;
                point_pixel = new byte[pointsize];
                int i = DkamSDK_CSharp.StreamOn(camera_obj, 1);
                if (i < 0)
                {
                    Addlog("点云相机StreamOn时出现错误码：" + i, -1);
                }
                return i == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 彩色相机初始化
        /// </summary>
        public bool InitRGB()
        {
            try
            {
                SWIGTYPE_p_int width_rgb = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraWidth(camera_obj, width_rgb, 1);
                int widthRGB = DkamSDK_CSharp.intArray_getitem(width_rgb, 0);
                SWIGTYPE_p_int height_rgb = DkamSDK_CSharp.new_intArray(0);
                DkamSDK_CSharp.GetCameraHeight(camera_obj, height_rgb, 1);
                int heightRGB = DkamSDK_CSharp.intArray_getitem(height_rgb, 0);

                RGB_data = new PhotoInfoCSharp();
                RGBsize = widthRGB * heightRGB * 3;
                RGB_pixel = new byte[RGBsize];

                int setexposureTimeRGB = DkamSDK_CSharp.SetExposureTime(camera_obj, exposure_rgb, 1);
                Addlog("Set ExposureTime RGB = " + setexposureTimeRGB, 5);

                return DkamSDK_CSharp.StreamOn(camera_obj, 2) == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 寻找相机
        /// </summary>
        public List<string> FindCamera()
        {
            cameraIPs.Clear();
            int camera_num = DkamSDK_CSharp.DiscoverCamera();
            Addlog("找到相机数量：" + camera_num, 0);
            DkamSDK_CSharp.CameraSort(0);
            for (int i = 0; i < camera_num; i++)
            {
                cameraIPs.Add(DkamSDK_CSharp.CameraIP(i));
            }
            return cameraIPs;
        }
        /// <summary>
        /// 获取相机内参
        /// </summary>
        public float[] Getkc()
        {
            return kc;
        }
        /// <summary>
        /// 获取相机内参
        /// </summary>
        public float[] Getkk()
        {
            return kk;
        }
        /// <summary>
        /// 获取SDK版本号
        /// </summary>
        public string GetVersion()
        {
            return DkamSDK_CSharp.SDKVersion(camera_obj);
        }
        /// <summary>
        /// 连接相机
        /// </summary>
        /// <param name="ip">相机地址</param>
        public bool LinkCamera(string ip)
        {
            string ips = "";
            foreach (string item in cameraIPs)
            {
                ips += item + "|";
            }
            Addlog("Find Camera: " + ips, 5);
            int i = cameraIPs.FindIndex(c => c == ip);
            if (i < 0)
            {
                Addlog("未找到点云相机", 0);
                return false;
            }
            if (linkIPs.IndexOf(ip) < 0)
            {
                linkIPs.Add(ip); 
            }
            camera_obj = DkamSDK_CSharp.CreateCamera(i);
            i = DkamSDK_CSharp.CameraConnect(camera_obj);
            if (i < 0)
            {
                Addlog("点云相机创建时出现错误码；" + i, -1); 
            }
            return i == 0;
        }
        /// <summary>
        /// 红外拍照
        /// </summary>
        /// <param name="savePath">红外存储路径</param>
        public bool InfraRedShot(string savePath)
        {
            if (!CheckShot(() =>
            {
                int trigger_count = DkamSDK_CSharp.SetTriggerCount(camera_obj);
                Addlog("set gray trigger count = " + trigger_count, 5);
                return trigger_count;
            }, 10))
            {
                return false;
            }

            int capturegray = DkamSDK_CSharp.CaptureCSharp(camera_obj, 0, gray_data, gray_pixel, graysize);
            Addlog("Capture Gray = " + capturegray, 5);

            int savegray = DkamSDK_CSharp.SaveToBMPCSharp(camera_obj, gray_data, gray_pixel, graysize, savePath);
            return savegray == 0;
        }
        /// <summary>
        /// 点云与深度拍照
        /// </summary>
        /// <param name="savePath_Point">点云存储路径</param>
        /// <param name="savePth_Depth">深度图存储路径</param>
        public bool PointCloudShot(string savePath_Point = "", string savePth_Depth = "")
        {
            bool savePoint = true, saveDepth = true;

            if (!CheckShot(() =>
            {
                int capturepoint = DkamSDK_CSharp.TimeoutCaptureCSharp(camera_obj, 1, PointCloud_data, point_pixel, pointsize, 3000000);
                Addlog("Capture PointCloud = " + capturepoint, 5);
                return capturepoint;
            }, 10))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(savePath_Point))
            {
                int savepoint = DkamSDK_CSharp.SavePointCloudToPcdCSharp(camera_obj, PointCloud_data, point_pixel, pointsize, savePath_Point);
                Addlog("Image Savepoint = " + savepoint, 5);
                savePoint = savepoint == 0;
            }

            if (!string.IsNullOrEmpty(savePth_Depth))
            {
                int savedepth = DkamSDK_CSharp.SaveDepthToPngCSharp(camera_obj, PointCloud_data, point_pixel, pointsize, savePth_Depth);
                Addlog("Image Savedepth = " + savedepth, 5);
                saveDepth = savedepth == 0;
            }

            return savePoint && saveDepth;
        }
        /// <summary>
        /// 彩色相机拍照
        /// </summary>
        /// <param name="savePath">彩色相机存储路径</param>
        public bool RGBShot(string savePath)
        {
            int rgb_trigger_count = DkamSDK_CSharp.SetRGBTriggerCount(camera_obj);
            Addlog("set RGB trigger count = " + rgb_trigger_count, 5);

            int capturergb = DkamSDK_CSharp.CaptureCSharp(camera_obj, 2, RGB_data, RGB_pixel, RGBsize);
            Addlog("Capture RGB = " + capturergb, 5);
            
            int saveRGB = DkamSDK_CSharp.SaveToBMPCSharp(camera_obj, RGB_data, RGB_pixel, RGBsize, savePath);
            return saveRGB == 0;
        }
        /// <summary>
        /// 刷新红外缓存
        /// </summary>
        public void RefreshInfraRed()
        {
            DkamSDK_CSharp.FlushBuffer(camera_obj, 0);
        }
        /// <summary>
        /// 刷新点云缓存
        /// </summary>
        public void RefreshPointCloud()
        {
            DkamSDK_CSharp.FlushBuffer(camera_obj, 1);
        }
        /// <summary>
        /// 刷新彩色相机缓存
        /// </summary>
        public void RefreshRGB()
        {
            DkamSDK_CSharp.FlushBuffer(camera_obj, 2);
        }

        private bool CheckShot(Func<int> func, int maxNum)
        {
            bool relinked = false;
            Thread thread = new Thread(new ThreadStart(() =>
            {
                lock (reLinkLock)
                {
                    for (int i = 1; i <= maxNum; i++)
                    {
                        int code = func();
                        switch (code)
                        {
                            case -11:
                                Addlog("尝试第" + i + "次重新连接点云相机...", 0);
                                Close();
                                foreach (string ip in linkIPs)
                                {
                                    Addlog("初始化点云相机：" + ip, 5);
                                    Init(ip, true, true);
                                }
                                break;
                            default:
                                i = maxNum + 1;
                                break;
                        }
                    }
                    relinked = true; 
                }
            }));
            thread.Start();
            for (int i = 0; i < 30; i++)
            {
                if (relinked)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            if (!relinked)
            {
                thread.Abort();
                Addlog("点云相机重连超时", 0);
                if (ReLinkCameraFaid())
                {
                    return CheckShot(func, maxNum);
                }
            }
            return true;
        }
    }
}

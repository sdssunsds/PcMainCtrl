using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.HardWare.NetworkRelay;
using PcMainCtrl.HardWare.Rgv;

namespace PcMainCtrl.HardWare
{
    /// <summary>
    /// 硬件类型
    /// </summary>
    public enum eHAL_TYPE_DEF
    {
        HAL_TYPE_INVALID = 0xFF, //无效设备

        HAL_TYPE_BASLERCAMERA = 0x01, //巴斯勒相机
        HAL_TYPE_NETWORKRELAY = 0x02, //网络继电器
        HAL_TYPE_RGV = 0x03,          //RGV设备
        HAL_TYPE_ROBOT = 0x04,        //机械臂
        HAL_TYPE_STM32 = 0x05,        //Stm32开发板
    }

    public class HardwareHelper
    {
        public static void HardwareInit(eHAL_TYPE_DEF type)
        {
            switch (type)
            {
                //打开BaslerCamera
                case eHAL_TYPE_DEF.HAL_TYPE_BASLERCAMERA:
                    {
                        if (CameraCtrlHelper.GetInstance().IsEnable == false)
                        {
                            CameraCtrlHelper.GetInstance().CameraScanner();
                        }
                    }
                    break;

                //打开NetworkRelay
                case eHAL_TYPE_DEF.HAL_TYPE_NETWORKRELAY:
                    {
                        if (NetworkRelayCtrlHelper.GetInstance().IsEnable == false)
                        {
                            NetworkRelayCtrlHelper.GetInstance().NetworkRelayServerCreat();
                        }
                    }
                    break;

                //打开Rgv
                case eHAL_TYPE_DEF.HAL_TYPE_RGV:
                    {
                        if (RgvModCtrlHelper.GetInstance().IsEnable == false)
                        {
                            RgvModCtrlHelper.GetInstance().RgvModConnect();
                        }
                    }
                    break;

                //打开Robot
                case eHAL_TYPE_DEF.HAL_TYPE_ROBOT:
                    {
                        if (Robot.RobotModCtrlHelper.GetInstance().IsEnable == false)
                        {
                            Robot.RobotModCtrlHelper.GetInstance().RobotServerCreat();
                        }
                    }
                    break;

                //打开Stm32
                case eHAL_TYPE_DEF.HAL_TYPE_STM32:
                    {
                        if (Stm32.Stm32ModCtrlHelper.GetInstance().IsEnable == false)
                        {
                            Stm32.Stm32ModCtrlHelper.GetInstance().Stm32ModConnect();
                        }
                    }
                    break;
            }
        }

        public static void HardwareDeInit(eHAL_TYPE_DEF type)
        {
            switch (type)
            {
                //关闭BaslerCamera
                case eHAL_TYPE_DEF.HAL_TYPE_BASLERCAMERA:
                    {
                        if (CameraCtrlHelper.GetInstance() != null && CameraCtrlHelper.GetInstance().IsEnable)
                        {
                            CameraCtrlHelper.GetInstance().CameraAllClose();
                        }
                    }
                    break;

                //关闭NetworkRelay
                case eHAL_TYPE_DEF.HAL_TYPE_NETWORKRELAY:
                    {
                        if (NetworkRelayCtrlHelper.GetInstance().IsEnable == true)
                        {
                            NetworkRelayCtrlHelper.GetInstance().NetworkRelayServerClose();
                        }
                    }
                    break;

                //关闭Rgv
                case eHAL_TYPE_DEF.HAL_TYPE_RGV:
                    {
                        if (RgvModCtrlHelper.GetInstance().IsEnable == true)
                        {
                            RgvModCtrlHelper.GetInstance().RgvModDisConnect();
                        }
                    }
                    break;
                
                //关闭Robot
                case eHAL_TYPE_DEF.HAL_TYPE_ROBOT:
                    {
                        if (Robot.RobotModCtrlHelper.GetInstance().IsEnable == true)
                        {
                            Robot.RobotModCtrlHelper.GetInstance().RobotServerClose();
                        }
                    }
                    break;

                //关闭Stm32
                case eHAL_TYPE_DEF.HAL_TYPE_STM32:
                    {
                        if (Stm32.Stm32ModCtrlHelper.GetInstance().IsEnable == true)
                        {
                            Stm32.Stm32ModCtrlHelper.GetInstance().Stm32ModDisConnect();
                        }
                    }
                    break;
            }
        }
    }
}

using System.Collections.Generic;

namespace PcMainCtrl.DataAccess.DataEntity
{
    /// <summary>
    /// 管理不同车型序号的车底定位边界信息，用于判断当前运动到哪个车底位置
    /// 用于管理线阵相机的参数
    /// </summary>
    public class TrainSiteDetectInfo
    {
        public int DefRead_CarDistance { get; set; }
        public int DefRead_CarHeight { get; set; }
        public bool DefRead_InfraRed1_Stat { get; set; }
        public bool DefRead_InfraRed2_Stat { get; set; }
        public bool DefRead_InfraRed3_Stat { get; set; }
        public bool DefRead_InfraRed4_Stat { get; set; }
    }

    public class UserEntity
    {
        #region 机器人设备作业状态管理
        /// <summary>
        /// 机器人设备作业状态管理-code
        /// </summary>
        public const string key_DEVICE_INVALID = @"DEVICE_INVALID";
        public const string key_DEVICE_INIT = @"DEVICE_INIT";
        public const string key_DEVICE_IDLE = @"DEVICE_IDLE";
        public const string key_DEVICE_BUSY = @"DEVICE_BUSY";

        public const string key_DEVICE_ERR_RGV_ERR = @"DEVICE_ERR_RGV_ERR";
        public const string key_DEVICE_ERR_ROBOT_ERR = @"DEVICE_ERR_ROBOT_ERR";
        public const string key_DEVICE_ERR_CAMERA_ERR = @"DEVICE_ERR_CAMERA_ERR";
        public const string key_DEVICE_ERR_STM32BOARD_ERR = @"DEVICE_ERR_STM32BOARD_ERR";
        #endregion

        public Dictionary<string, string> globalDeviceStat = new Dictionary<string, string>()
        {
            { key_DEVICE_INVALID, "机器人设备未初始化"},
            { key_DEVICE_INIT, "机器人设备处于初始化状态"},
            { key_DEVICE_IDLE, "机器人设备处于空闲状态"},
            { key_DEVICE_BUSY, "机器人设备处于繁忙状态"},
            { key_DEVICE_ERR_RGV_ERR, "机器人设备处于ERR状态—RGV状态异常"},
            { key_DEVICE_ERR_ROBOT_ERR, "机器人设备处于ERR状态—机械臂状态异常"},
            { key_DEVICE_ERR_CAMERA_ERR, "机器人设备处于ERR状态—相机状态异常"},
            { key_DEVICE_ERR_STM32BOARD_ERR, "机器人设备处于ERR状态—信号板状态异常"}
        };

        /// <summary>
        /// 当前车体状态(Key值)
        /// </summary>
        public string myDeviceStat { get; set; }

        /// <summary>
        /// 当前车体状态码
        /// </summary>
        public string myDeviceValue { get; set; }

        public string GetState(string code)
        {
            if (!string.IsNullOrEmpty(code) && globalDeviceStat.ContainsKey(code))
            {
                if (code == "机器人设备处于ERR状态—RGV状态异常")
                {
                    return myDeviceValue;
                }
                else
                {
                    return globalDeviceStat[code]; 
                }
            }
            return "";
        }
    }
}

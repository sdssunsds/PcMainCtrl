using PLC;
using System;

namespace PcMainCtrl.HardWare
{
    /// <summary>
    /// PLC通信
    /// 仅打开面阵相机、光源和机械臂：地址12，4363；地址13：12288
    /// </summary>
    public class ModbusTCP
    {
        private static PLCManager.PLC_Parent mb = null;
        private static PLCManager.PLC_Parent address12 = null;
        private static PLCManager.PLC_Parent address13 = null;
        private static int[] address12Values = new int[16];
        private static int[] address13Values = new int[16];

        public static bool IsLink
        {
            get
            {
                try
                {
                    short? s = mb?.ShortValue;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        static ModbusTCP()
        {
            if (mb == null)
            {
                mb = PLCManager.GetModbusTcp(Properties.Settings.Default.ModbusTcp, 502, 1);
                mb.Address = "2";
            }

            if (address12 == null)
            {
                address12 = PLCManager.GetModbusTcp();
                address12.Address = "12";
            }

            if (address13 == null)
            {
                address13 = PLCManager.GetModbusTcp();
                address13.Address = "13";
            }
        }

        public static PLCManager.PLC_Parent GetMB()
        {
            return PLCManager.GetModbusTcp();
        }

        /// <summary>
        /// 获取传感器状态
        /// </summary>
        /// <param name="i">索引，从0开始</param>
        public static bool GetIO(int i)
        {
            short val = 1;
            string bytestring = Convert.ToString(val, 2);
            for (int j = 0; j < 16; j++)
            {
                if (bytestring.Length < 16)
                {
                    bytestring = "0" + bytestring;
                }
            }
            return bytestring[i] == '1';
        }

        public static void SetAddress12(Address12Type type, int val)
        {
            try
            {
                switch (type)
                {
                    case Address12Type.RobotMzLedPower:
                        address12Values[15] = val; // 15是协议第一位
                        break;
                    case Address12Type.RobotFrontMzPower:
                        address12Values[14] = val;
                        break;
                    case Address12Type.RobotXzLedPower:
                        address12Values[13] = val; // 13是协议第三位
                        break;
                    case Address12Type.RobotBackMzPower:
                        address12Values[12] = val;
                        break;
                    case Address12Type.FrontRobotStepMotorPower:
                        address12Values[11] = val;
                        break;
                    case Address12Type.RobotXZPower:
                        address12Values[10] = val;
                        break;
                    case Address12Type.BackRobotStepMotorPower:
                        address12Values[9] = val;
                        break;
                    case Address12Type.FrontRobotStart:
                        address12Values[7] = val;
                        break;
                    case Address12Type.FrontRobotStop:
                        address12Values[6] = val;
                        break;
                    case Address12Type.FrontRobotEMGRst:
                        address12Values[5] = val;
                        break;
                    case Address12Type.FrontRobotAlmClear:
                        address12Values[4] = val;
                        break;
                    case Address12Type.BackRobotStart:
                        address12Values[3] = val;
                        break;
                    case Address12Type.BackRobotStop:
                        address12Values[2] = val;
                        break;
                    case Address12Type.BackRobotEMGRst:
                        address12Values[1] = val;
                        break;
                    case Address12Type.BackRobotAlmClear:
                        address12Values[0] = val;
                        break;
                }
                string s = "";
                for (int i = 0; i < address12Values.Length; i++)
                {
                    s += address12Values[i];
                }
                address12.ShortValue = Convert.ToInt16(s, 2);
            }
            catch (Exception) { }
        }

        public static void SetAddress13(Address13Type type, int val)
        {
            try
            {
                switch (type)
                {
                    case Address13Type._3D:
                        address13Values[0] = val;
                        break;
                    case Address13Type.Laser:
                        address13Values[1] = val;
                        break;
                    case Address13Type.Custom:
                        address13Values[2] = val;
                        break;
                    case Address13Type.Red:
                        address13Values[8] = val;
                        break;
                    case Address13Type.Green:
                        address13Values[9] = val;
                        break;
                    case Address13Type.Yellow:
                        address13Values[10] = val;
                        break;
                    case Address13Type.Buzzer:
                        address13Values[11] = val;
                        break;
                    case Address13Type.Front_Robot_Mz_LED:
                        address13Values[12] = val;
                        break;
                    case Address13Type.Back_Robot_Mz_LED:
                        address13Values[13] = val;
                        break;
                }
                string s = "";
                for (int i = address13Values.Length - 1; i >= 0; i--)
                {
                    s += address13Values[i];
                }
                address13.ShortValue = Convert.ToInt16(s, 2);
            }
            catch (Exception) { }
        }
    }

    public enum Address12Type
    {
        /// <summary>
        /// 面阵光源电源
        /// </summary>
        RobotMzLedPower,
        /// <summary>
        /// 前相机电源控制
        /// </summary>
        RobotFrontMzPower,
        /// <summary>
        /// 线阵光源电源
        /// </summary>
        RobotXzLedPower,
        /// <summary>
        /// 后相机电源控制
        /// </summary>
        RobotBackMzPower,
        /// <summary>
        /// 前滑台电源控制
        /// </summary>
        FrontRobotStepMotorPower,
        /// <summary>
        /// 线阵相机电源控制
        /// </summary>
        RobotXZPower,
        /// <summary>
        /// 后滑台电源控制
        /// </summary>
        BackRobotStepMotorPower,
        /// <summary>
        /// 前机械臂启动（通知继电器）
        /// </summary>
        FrontRobotStart,
        /// <summary>
        /// 前机械臂终止
        /// </summary>
        FrontRobotStop,
        /// <summary>
        /// 前机械臂急停复位
        /// </summary>
        FrontRobotEMGRst,
        /// <summary>
        /// 前机械臂清除报警
        /// </summary>
        FrontRobotAlmClear,
        /// <summary>
        /// 后机械臂启动（通知继电器）
        /// </summary>
        BackRobotStart,
        /// <summary>
        /// 后机械臂终止
        /// </summary>
        BackRobotStop,
        /// <summary>
        /// 后机械臂急停复位
        /// </summary>
        BackRobotEMGRst,
        /// <summary>
        /// 后机械臂清除报警
        /// </summary>
        BackRobotAlmClear
    }

    public enum Address13Type
    {
        /// <summary>
        /// 点云开关
        /// </summary>
        _3D,
        /// <summary>
        /// 激光
        /// </summary>
        Laser,
        Custom,
        /// <summary>
        /// 红灯（报警时）
        /// </summary>
        Red,
        /// <summary>
        /// 绿灯（初始化完成后）
        /// </summary>
        Green,
        /// <summary>
        /// 黄灯（作业中）
        /// </summary>
        Yellow,
        /// <summary>
        /// 蜂鸣（RGV运动时）
        /// </summary>
        Buzzer,
        /// <summary>
        /// 前面阵光源
        /// </summary>
        Front_Robot_Mz_LED,
        /// <summary>
        /// 后面阵光源
        /// </summary>
        Back_Robot_Mz_LED
    }
}

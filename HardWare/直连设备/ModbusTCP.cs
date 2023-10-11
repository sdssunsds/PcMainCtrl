#define newCode

using PLC;
using System;

namespace PcMainCtrl.HardWare
{
    /// <summary>
    /// PLC通信
    /// 用于打开面阵相机、滑台、雷达、光源和机械臂：地址12：4363；地址13：12288
    /// </summary>
    public class ModbusTCP
    {
        private static PLCManager.PLC_Parent mb = null;
        private static PLCManager.PLC_Parent address12 = null;
        private static PLCManager.PLC_Parent address13 = null;
#if newCode
        private static ushort address12Value = 0;
        private static ushort address13Value = 4;
#else
        private static int[] address12Values = new int[16];
        private static int[] address13Values = new int[16]; 
#endif

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
#if !newCode
            address13Values[2] = 1; 
#endif
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

        public static int GetAddress2Value(Address2Type type)
        {
            ushort? s = mb?.UShortValue;
            if (s != null)
            {
#if newCode
                address12Value = s.Value;
                string bytestring = Convert.ToString(address12Value, 2).PadLeft(16, '0');
#else
                string bytestring = Convert.ToString(s.Value, 2);
                bytestring = (int.Parse(bytestring)).ToString("0000000000000000");
#endif
                switch (type)
                {
                    case Address2Type.Sensor_IO_1:
                        return int.Parse(bytestring[bytestring.Length - 1].ToString());
                    case Address2Type.Sensor_IO_2:
                        return int.Parse(bytestring[bytestring.Length - 2].ToString());
                    default:
                        return -1;
                }
            }
            return -1;
        }

        public static void SetAddress12(Address12Type type, int val)
        {
            try
            {
                int ix = 0;
                switch (type)
                {
                    case Address12Type.RobotMzLedPower:
                        ix = 15; // 15是协议第一位
                        break;
                    case Address12Type.RobotFrontMzPower:
                        ix = 14;
                        break;
                    case Address12Type.RobotXzLedPower:
                        ix = 13; // 13是协议第三位
                        break;
                    case Address12Type.RobotBackMzPower:
                        ix = 12;
                        break;
                    case Address12Type.FrontRobotStepMotorPower:
                        ix = 11;
                        break;
                    case Address12Type.RobotXZPower:
                        ix = 10;
                        break;
                    case Address12Type.BackRobotStepMotorPower:
                        ix = 9;
                        break;
                    case Address12Type.FrontRobotStart:
                        ix = 7;
                        break;
                    case Address12Type.FrontRobotStop:
                        ix = 6;
                        break;
                    case Address12Type.FrontRobotEMGRst:
                        ix = 5;
                        break;
                    case Address12Type.FrontRobotAlmClear:
                        ix = 4;
                        break;
                    case Address12Type.BackRobotStart:
                        ix = 3;
                        break;
                    case Address12Type.BackRobotStop:
                        ix = 2;
                        break;
                    case Address12Type.BackRobotEMGRst:
                        ix = 1;
                        break;
                    case Address12Type.BackRobotAlmClear:
                        ix = 0;
                        break;
                }
#if newCode
                address12.UShortValue = address12Value = (ushort)(address12Value | (val << ix));
#else
                address12Values[ix] = val;
                string s = "";
                for (int i = 0; i < address12Values.Length; i++)
                {
                    s += address12Values[i];
                }
                address12.ShortValue = Convert.ToInt16(s, 2);
#endif
            }
            catch (Exception) { }
        }

        public static void SetAddress13(Address13Type type, int val)
        {
            try
            {
                int ix = 0;
                switch (type)
                {
                    case Address13Type._3D:
                        ix = 0;
                        break;
                    case Address13Type.Laser:
                        ix = 1;
                        break;
                    case Address13Type.Custom:
                        ix = 2;
                        break;
                    case Address13Type.Red:
                        ix = 8;
                        break;
                    case Address13Type.Green:
                        ix = 9;
                        break;
                    case Address13Type.Yellow:
                        ix = 10;
                        break;
                    case Address13Type.Buzzer:
                        ix = 11;
                        break;
                    case Address13Type.Front_Robot_Mz_LED:
                        ix = 12;
                        break;
                    case Address13Type.Back_Robot_Mz_LED:
                        ix = 13;
                        break;
                }
#if newCode
                address13.UShortValue = address13Value = (ushort)(address13Value | (val << ix));
#else
                address13Values[ix] = val;
                string s = "";
                for (int i = address13Values.Length - 1; i >= 0; i--)
                {
                    s += address13Values[i];
                }
                address13.ShortValue = Convert.ToInt16(s, 2);
#endif
            }
            catch (Exception) { }
        }
    }

    public enum Address2Type
    {
        Sensor_IO_1,
        Sensor_IO_2
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
        /// 点云开关（初始化及定位异常时开启供电，默认值 0）
        /// </summary>
        _3D,
        /// <summary>
        /// 激光（二次定位时开启供电，默认值 0）
        /// </summary>
        Laser,
        /// <summary>
        /// 雷达或光敏电源控制（设备上电即供电，默认值 1）
        /// </summary>
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

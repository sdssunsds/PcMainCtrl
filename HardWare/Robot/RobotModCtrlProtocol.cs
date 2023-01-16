using System;

namespace PcMainCtrl.HardWare.Robot
{
    public class RobotModCtrlProtocol
    {
        public class RobotDataPack
        {
            public string j1 { get; set; } = "0.00";

            public string j2 { get; set; } = "0.00";

            public string j3 { get; set; } = "0.00";

            public string j4 { get; set; } = "0.00";

            public string j5 { get; set; } = "0.00";

            public string j6 { get; set; } = "0.00";
        }

        public class RobotData
        {
            #region
            /// <summary>
            /// Robot功能字:"1"-回到安全点
            /// </summary>
            public const String RobotBackZeroFunCode = @"1.00";

            /// <summary>
            /// Robot功能字:"2"-控制运动
            /// </summary>
            public const String RobotSetCtrlFunCode = @"2.00";

            /// <summary>
            /// Robot功能字:"3"-获取坐标状态
            /// </summary>
            public const String RobotGetDataFunCode = @"3.00";

            /// <summary>
            /// Robot功能字:"4"-获取机器臂工作状态(预留)
            /// </summary>
            public const String RobotGetStatFunCode = @"4.00";

            /// <summary>
            /// Robot功能字:"5"-关闭机械臂socket
            /// </summary>
            public const String RobotCloseSocketFunCode = @"5.00";

            /// <summary>
            /// 
            /// </summary>
            public const String RAKAE_ROBOT_DATA_SPLIT = @",";

            /// <summary>
            /// Roake Robot的协议末尾需要携带\r
            /// </summary>
            public const byte RAKAE_ROBOT_DATA_TAIL = 0x0d;
            #endregion

            /// <summary>
            /// 机械臂Id
            /// </summary>
            public byte mRobotDeviceId;

            /// <summary>
            /// 机械臂功能字
            /// </summary>
            public String mRobotFunc;

            /// <summary>
            /// 坐标数据
            /// </summary>
            public String mCoordinateVal;

            /// <summary>
            /// 发送命令:命令字,数据内容,\r
            /// 例子："2.00,1.00，2.00，3.00,4.00,5.00,6.00"'\r'
            /// </summary>
            /// <param name="databuf"></param>
            /// <param name="dataLen"></param>
            /// <returns></returns>
            public void RobotSendPck(String databuf, String cmd , out String cmdbuf)
            {
                //报文数据缓冲区
                String SendDataBuf = @"";

                //a.功能字
                SendDataBuf  += cmd;
                SendDataBuf += RAKAE_ROBOT_DATA_SPLIT;

                //b.拷贝Robot的数据内容(坐标)
                SendDataBuf += databuf;

                //c.在尾巴上加上1个字节‘\r’

                cmdbuf = SendDataBuf;
            }
        }

        public class RokaeRobotProtocolFunc
        {
            /// <summary>
            /// 控制机械臂运动
            /// </summary>
            public static void RobotSetCtrlCmd(string databuf, out string cmdbuf)
            {
                RobotData pack = new RobotData();
                pack.RobotSendPck(databuf, RobotData.RobotSetCtrlFunCode, out cmdbuf);
            }

            /// <summary>
            /// 获取机械臂运动数据
            /// </summary>
            public static void RobotGetStatCmd(string databuf, out string cmdbuf)
            {
                RobotData pack = new RobotData();
                pack.RobotSendPck(databuf, RobotData.RobotGetDataFunCode, out cmdbuf);
            }

            /// <summary>
            /// 获取机械臂运动数据
            /// </summary>
            public static void RobotBackZeroCmd(string databuf, out string cmdbuf)
            {
                RobotData pack = new RobotData();
                pack.RobotSendPck(databuf, RobotData.RobotBackZeroFunCode, out cmdbuf);
            }

            /// <summary>
            /// 获取机械臂运动数据
            /// </summary>
            public static void RobotCloseCmd(string databuf, out string cmdbuf)
            {
                RobotData pack = new RobotData();
                pack.RobotSendPck(databuf, RobotData.RobotCloseSocketFunCode, out cmdbuf);
            }
        }
    }
}

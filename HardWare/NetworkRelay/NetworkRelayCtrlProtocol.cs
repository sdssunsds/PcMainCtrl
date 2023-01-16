using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcMainCtrl.HardWare.NetworkRelay
{
    public static class NetworkRelayCtrlProtocol
    {
        /// <summary>
        /// 功能字:控制继电器IO1
        /// </summary>
        public const String RelayIO1Ctrl_Cmd = @"AT+STACH1=1,3";
        public const String RelayIO1Stop_Cmd = @"AT+STACH1=1,0";

        /// <summary>
        /// 功能字:控制继电器IO2
        /// </summary>
        public const String RelayIO2Ctrl_Cmd = @"AT+STACH2=1,1";
        public const String RelayIO2Stop_Cmd = @"AT+STACH2=1,0";

        /// <summary>
        /// 功能字:控制继电器IO3
        /// </summary>
        public const String RelayIO3Ctrl_Cmd = @"AT+STACH3=1,1";
        public const String RelayIO3Stop_Cmd = @"AT+STACH3=1,0";

        /// <summary>
        /// 功能字:控制继电器IO4
        /// </summary>
        public const String RelayIO4Ctrl_Cmd = @"AT+STACH4=1,1";
        public const String RelayIO4Stop_Cmd = @"AT+STACH4=1,0";

        /// <summary>
        /// 功能字:控制继电器IO5
        /// </summary>
        public const String RelayIO5Ctrl_Cmd = @"AT+STACH5=1,3";
        public const String RelayIO5Stop_Cmd = @"AT+STACH5=1,0";

        /// <summary>
        /// 功能字:控制继电器IO6
        /// </summary>
        public const String RelayIO6Ctrl_Cmd = @"AT+STACH6=1,1";
        public const String RelayIO6Stop_Cmd = @"AT+STACH6=1,0";

        /// <summary>
        /// 功能字:控制继电器IO7
        /// </summary>
        public const String RelayIO7Ctrl_Cmd = @"AT+STACH7=1,1";
        public const String RelayIO7Stop_Cmd = @"AT+STACH7=1,0";

        /// <summary>
        /// 功能字:控制继电器IO8
        /// </summary>
        public const String RelayIO8Ctrl_Cmd = @"AT+STACH8=1,1";
        public const String RelayIO8Stop_Cmd = @"AT+STACH8=1,0";

        /// <summary>
        /// 功能字:获取网络状态
        /// </summary>
        public const String RelayReqHeartBeatCmd = @"AT";

        /// <summary>
        /// 功能字:获取网络状态
        /// </summary>
        public const String RelayRspHeartBeatCmd = @"AT+ACK\r\n";

        /// <summary>
        /// 功能字:获取网络状态
        /// </summary>
        public const String RelayRspActionCmd = @"OK";
    }
}

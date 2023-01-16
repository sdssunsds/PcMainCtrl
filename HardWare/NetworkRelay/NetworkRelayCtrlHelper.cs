using System;
using System.Net.Sockets;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.Network.Network;

namespace PcMainCtrl.HardWare.NetworkRelay
{
    public class NetworkRelayGlobalInfo
    {
        public int ModWorkCode{ get; set; }
        public string ModWorkMsg { get; set; }

        /// <summary>
        /// NetworkRelay应答数据
        /// </summary>
        public String NetworkRelayRspMsg { get; set; }

        public bool NetworkRelayConnStat { get; set; } = false;
    }

    /// <summary>
    /// 网络继电器(用于机器臂异常自动状态修复,通过控制相应的机器臂控制IO时序实现)
    /// </summary>
    public class NetworkRelayCtrlHelper
    {
        public bool IsEnable = false;

        /// <summary>
        /// 网络继电器模块
        /// </summary>
        private static NetworkRelayCtrlHelper instance;
        private NetworkRelayCtrlHelper() { }
        public static NetworkRelayCtrlHelper GetInstance()
        {
            return instance ?? (instance = new NetworkRelayCtrlHelper());
        }

        /// <summary>
        /// 当前的监听socket对象
        /// </summary>
        private TcpServiceSocket ServerSocket = null;
        private object sendLock = new object();

        /// <summary>
        /// Relay本地服务器连接信息：192.168.0.191:6000
        /// </summary>
        private readonly string ip = Properties.Settings.Default.RelayIP;
        private readonly int port = Properties.Settings.Default.RelayPort;

        /// <summary>
        /// 网络继电器设备信息
        /// </summary>
        public NetworkRelayGlobalInfo myNetworkRelayGlobalInfo = new NetworkRelayGlobalInfo();

        /// <summary>
        /// 委托+事件 = 回调函数，用于传递网络继电器的消息
        /// </summary>
        /// <param name="relayinfo"></param>
        public delegate void NetworkRelayModInfo(NetworkRelayGlobalInfo relayinfo);
        public event NetworkRelayModInfo NetworkRelayModInfoEvent;

        /// <summary>
        /// 创建Tcp服务器
        /// </summary>
        public void NetworkRelayServerCreat()
        {
            try
            {
                //创建服务器
                if (IsEnable == true && ServerSocket != null)
                {
                    return;
                }

                ServerSocket = new TcpServiceSocket(ip, port, 1);
                ServerSocket.accpetInfoEvent += new Action<Socket>(NetworkRelayServerAccpetData);
                ServerSocket.recvMessageEvent += new Action<Socket, string>(NetworkRelayServerRecvData);
                ServerSocket.Start();

                //更新状态
                IsEnable = true;
                myNetworkRelayGlobalInfo.ModWorkMsg = @"服务器创建成功，开始监听客户端";
                myNetworkRelayGlobalInfo.ModWorkCode = 0;

                //发送事件
                NetworkRelayModInfoEvent(myNetworkRelayGlobalInfo);
            }
            catch (Exception e)
            {
                AddLog("继电器 Exception (NetworkRelayServerCreat): " + e.Message, -1);
                NetworkRelayServerClose();
            }
        }

        /// <summary>
        /// 关闭监听套接字和所有session会话
        /// </summary>
        public bool NetworkRelayServerClose()
        {
            if ((ServerSocket != null) || (IsEnable == true))
            {
                try
                {
                    //关闭socket
                    ServerSocket.CloseAllClientSocket();
                }
                catch (Exception e)
                {
                    AddLog("继电器 Exception (NetworkRelayServerClose): " + e.Message, -1);
                }
            }

            //更新状态
            IsEnable = false;
            myNetworkRelayGlobalInfo.ModWorkMsg = @"服务器已断开连接";
            myNetworkRelayGlobalInfo.ModWorkCode = 0;

            //发送事件
            NetworkRelayModInfoEvent(myNetworkRelayGlobalInfo);
            return true;
        }

        /// <summary>
        /// 客户端信息数据
        /// </summary>
        private void NetworkRelayServerAccpetData(Socket socket)
        {
            TaskRun(new Action(() =>
            {
                try
                {
                    //获取客户端ip信息
                    string ip_endinfo = socket.RemoteEndPoint.ToString();
                    if (ip_endinfo != null)
                    {
                        myNetworkRelayGlobalInfo.NetworkRelayRspMsg = ip_endinfo;
                        myNetworkRelayGlobalInfo.NetworkRelayConnStat = true;
                    }

                    //发送事件
                    NetworkRelayModInfoEvent(myNetworkRelayGlobalInfo);
                }
                catch (Exception e)
                {
                    AddLog("继电器 Exception (NetworkRelayServerAccpetData): " + e.Message, -1);
                }
            }));
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void NetworkRelayServerRecvData(Socket socket, string message)
        {
            TaskRun(new Action(() =>
            {
                byte[] buffer = new byte[128];
                try
                {
                    //解析报文,进行了转换
                    if (message.Contains(NetworkRelayCtrlProtocol.RelayRspActionCmd) == true)
                    {
                        myNetworkRelayGlobalInfo.NetworkRelayRspMsg = @"设置成功";
                    }
                    else if (message.Contains(NetworkRelayCtrlProtocol.RelayReqHeartBeatCmd))
                    {
                        DoNetworkRelayOpCmdHandle(NetworkRelayCtrlProtocol.RelayRspHeartBeatCmd);
                        myNetworkRelayGlobalInfo.NetworkRelayRspMsg = @"心跳在线";
                    }
                    else
                    {
                        myNetworkRelayGlobalInfo.NetworkRelayRspMsg = @"设置失败,请重新设置";
                    }

                    //发送事件
                    NetworkRelayModInfoEvent(myNetworkRelayGlobalInfo);
                }
                catch (Exception e)
                {
                    AddLog("继电器 Exception (NetworkRelayServerRecvData): " + e.Message, -1);
                }
            }));
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void DoNetworkRelayOpCmdHandle(string cmd)
        {
            if (!IsEnable)
            {
                return;
            }
            try
            {
                lock (sendLock)
                {
                    //发送数据
                    int SendDataBufLen = System.Text.Encoding.Default.GetBytes(cmd).Length;
                    byte[] SendDataBuf = new byte[SendDataBufLen + 2];

                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(cmd);
                    Array.Copy(byteArray, 0, SendDataBuf, 0, SendDataBufLen);
                    SendDataBuf[SendDataBufLen] = 0x0d;
                    SendDataBuf[SendDataBufLen + 1] = 0x0a;

                    ServerSocket.SendMessageToAllClientsAsync(System.Text.Encoding.UTF8.GetString(SendDataBuf)); 
                }
            }
            catch (Exception e)
            {
                AddLog("继电器 Exception (DoNetworkRelayScheduleHandle): " + e.Message, -1);
            }
        }
    }
}

#define robotIsServer  // 机械臂做服务端

using System;
using System.Collections.Generic;
#if !robotIsServer
using System.Net.Sockets; 
#endif
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.Robot.RobotModCtrlProtocol;
using static PcMainCtrl.Network.Network;

namespace PcMainCtrl.HardWare.Robot
{
    //Rgv运动状态：启动、停止
    public enum eROBOTMODRUNSTAT
    {
        /// <summary>
        /// 准备
        /// </summary>
        ROBOTMODRUNSTAT_READY = 0,
        /// <summary>
        /// 启动
        /// </summary>
        ROBOTMODRUNSTAT_MOVE = 1,
        /// <summary>
        /// 停止
        /// </summary>
        ROBOTMODRUNSTAT_STOP = 2,
    }

    public class RobotGlobalInfo
    {
        public int ModWorkCode { get; set; }
        public String ModWorkMsg { get; set; }

        /// <summary>
        /// Robot应答数据
        /// </summary>
        public String FrontRobotRspMsg { get; set; }
        public bool FrontRobotConnStat { get; set; } = false;

        /// <summary>
        /// Robot应答数据
        /// </summary>
        public String BackRobotRspMsg { get; set; }
        public bool BackRobotConnStat { get; set; } = false;

        /// <summary>
        /// FrontRobot设备坐标数据
        /// </summary>
        public RobotDataPack FrontRobotSiteData { get; set; } = new RobotDataPack();

        /// <summary>
        /// BackRobot设备坐标数据
        /// </summary>
        public RobotDataPack BackRobotSiteData { get; set; } = new RobotDataPack();

        /// <summary>
        /// Robot运动指令执行状态监控
        /// </summary>
        public eROBOTMODRUNSTAT FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
        public eROBOTMODRUNSTAT BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
    }

    /// <summary>
    /// 定义一个robot实例instance && 消息队列RobotModCtrlQueueHelper && 消息队列处理任务RobotHandleTask
    /// </summary>
    public class RobotModCtrlHelper
    {
        public bool IsEnable = false;
        public const bool isServer =
#if robotIsServer
            true;
#else
            false;
#endif

        private static RobotModCtrlHelper instance;
        private RobotModCtrlHelper() { }
        public static RobotModCtrlHelper GetInstance()
        {
            return instance ?? (instance = new RobotModCtrlHelper());
        }

        /// <summary>
        /// 机械臂设备子地址
        /// </summary>
        public const byte FRONT_ROBOT_DEVICE_ID = 0x01;
        public const byte BACK_ROBOT_DEVICE_ID = 0x02;

        private object sendLock = new object();
        /// <summary>
        /// 机械臂连接对象
        /// </summary>
        private Dictionary<byte, TcpClientSocket> RobotSocket = new Dictionary<byte, TcpClientSocket>();

        public bool this[byte id] { get { return RobotSocket.ContainsKey(id) && RobotSocket[id].connectSocket != null ? RobotSocket[id].connectSocket.Connected : false; } }

#if !robotIsServer
        /// <summary>
        /// Robot本地服务器连接信息：192.168.0.191:7000
        /// </summary>
        private string ip = Properties.Settings.Default.RobotIP;
        private int port = Properties.Settings.Default.RobotPort;

        /// <summary>
        /// 当前的监听socket对象
        /// </summary>
        private TcpServiceSocket ServerSocket = null;

        /// <summary>
        /// 机械臂本地信息
        /// </summary>
        public Dictionary<byte, String> dicRobotDevices = new Dictionary<byte, string>{
            {FRONT_ROBOT_DEVICE_ID, Properties.Settings.Default.RobotFrontIP },
            {BACK_ROBOT_DEVICE_ID, Properties.Settings.Default.RobotBackIP }
        }; 
#endif

        /// <summary>
        /// 网络继电器设备信息
        /// </summary>
        public RobotGlobalInfo myRobotGlobalInfo = new RobotGlobalInfo();

        /// <summary>
        /// 委托+事件 = 回调函数，用于传递Robot的消息
        /// </summary>
        public delegate void RobotModInfo(RobotGlobalInfo robotinfo, bool isFront = true);
        public event RobotModInfo RobotModInfoEvent;

        /// <summary>
        /// 创建Tcp服务器
        /// </summary>
        public void RobotServerCreat(Func<int, bool> pass = null)
        {
            try
            {
                //创建服务器
                if (IsEnable
#if !robotIsServer
                    && ServerSocket != null 
#endif
                    )
                {
                    return;
                }

#if robotIsServer
                RobotSocket.Add(FRONT_ROBOT_DEVICE_ID, new TcpClientSocket(Properties.Settings.Default.RobotFrontIP, Properties.Settings.Default.RobotFrontPort));
                RobotSocket.Add(BACK_ROBOT_DEVICE_ID, new TcpClientSocket(Properties.Settings.Default.RobotBackIP, Properties.Settings.Default.RobotBackPort));

                RobotSocket[FRONT_ROBOT_DEVICE_ID].recvMessageEvent = new Action<string>(RobotFrontRecvData);
                RobotSocket[FRONT_ROBOT_DEVICE_ID].sendResultEvent = new Action<int>(RobotSendFront);
                RobotSocket[BACK_ROBOT_DEVICE_ID].recvMessageEvent = new Action<string>(RobotBackRecvData);
                RobotSocket[BACK_ROBOT_DEVICE_ID].sendResultEvent = new Action<int>(RobotSendBack);

                do
                {
                    if (pass != null && pass(1))
                    {
                        break;
                    }
                    myRobotGlobalInfo.FrontRobotConnStat = RobotSocket[FRONT_ROBOT_DEVICE_ID].Start(0);
                    if (myRobotGlobalInfo.FrontRobotConnStat)
                    {
                        break;
                    }
                    AddLog("前机械臂尝试3秒后重连...");
                    ThreadSleep(3000);
                } while (true);

                do
                {
                    if (pass != null && pass(2))
                    {
                        break;
                    }
                    myRobotGlobalInfo.BackRobotConnStat = RobotSocket[BACK_ROBOT_DEVICE_ID].Start(0);
                    if (myRobotGlobalInfo.BackRobotConnStat)
                    {
                        break;
                    }
                    AddLog("后机械臂尝试3秒后重连...");
                    ThreadSleep(3000);
                } while (true);
#else
                ServerSocket = new TcpServiceSocket(ip, port, 2);
                ServerSocket.accpetInfoEvent += new Action<Socket>(RobotServerAccpetData);
                ServerSocket.recvMessageEvent += new Action<Socket, string>(RobotServerRecvData);
                ServerSocket.Start(); 
#endif

                //更新状态
                IsEnable = true;
                myRobotGlobalInfo.ModWorkMsg =
#if robotIsServer
                    "客户端创建成功";
#else
                    "服务器创建成功";
#endif
                myRobotGlobalInfo.ModWorkCode = 0;

                myRobotGlobalInfo.FrontRobotRspMsg = @"...";
                myRobotGlobalInfo.FrontRobotSiteData.j1 = "0.00";
                myRobotGlobalInfo.FrontRobotSiteData.j2 = "0.00";
                myRobotGlobalInfo.FrontRobotSiteData.j3 = "0.00";
                myRobotGlobalInfo.FrontRobotSiteData.j4 = "0.00";
                myRobotGlobalInfo.FrontRobotSiteData.j5 = "0.00";
                myRobotGlobalInfo.FrontRobotSiteData.j6 = "0.00";

                myRobotGlobalInfo.BackRobotRspMsg = @"...";
                myRobotGlobalInfo.BackRobotSiteData.j1 = "0.00";
                myRobotGlobalInfo.BackRobotSiteData.j2 = "0.00";
                myRobotGlobalInfo.BackRobotSiteData.j3 = "0.00";
                myRobotGlobalInfo.BackRobotSiteData.j4 = "0.00";
                myRobotGlobalInfo.BackRobotSiteData.j5 = "0.00";
                myRobotGlobalInfo.BackRobotSiteData.j6 = "0.00";

                //发送事件
                RobotModInfoEvent?.Invoke(myRobotGlobalInfo);
            }
            catch (Exception e)
            {
                AddLog("Robot Exception (RobotServerCreat): " + e.Message, -1);
                RobotServerClose();
            }
        }

        /// <summary>
        /// 控制Robot回归原点
        /// </summary>
        private void DoRobotCmdBackZeroHandle(object obj)
        {
            String devid_str = obj as String;
            byte devid = 0x00;
            try
            {
                devid = Convert.ToByte(devid_str);
            }
            catch (Exception e)
            {
                AddLog("Robot Exception (DoRobotCmdBackZeroHandle): " + e.Message, -1);
            }

            RobotDataPack robotDataPack = new RobotDataPack();
            if (RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID == devid || RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID == devid)
            {
                //得到坐标
                robotDataPack.j1 = @"0.00";
                robotDataPack.j2 = @"0.00";
                robotDataPack.j3 = @"0.00";
                robotDataPack.j4 = @"0.00";
                robotDataPack.j5 = @"0.00";
                robotDataPack.j6 = @"0.00";
            }
            else
            {
                return;
            }

            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(devid, RobotData.RobotBackZeroFunCode, robotDataPack);
        }

        /// <summary>
        /// 控制Robot断开连接
        /// </summary>
        private void DoRobotCmdCloseSocketHandle(object obj)
        {
            string devid_str = obj as String;
            byte devid = 0x00;
            try
            {
                devid = Convert.ToByte(devid_str);
            }
            catch (Exception e)
            {
                AddLog("Robot Exception (DoRobotCmdCloseSocketHandle): " + e.Message, -1);
            }

            RobotDataPack robotDataPack = new RobotDataPack();
            if (FRONT_ROBOT_DEVICE_ID == devid || BACK_ROBOT_DEVICE_ID == devid)
            {
                //得到坐标
                robotDataPack.j1 = @"0.00";
                robotDataPack.j2 = @"0.00";
                robotDataPack.j3 = @"0.00";
                robotDataPack.j4 = @"0.00";
                robotDataPack.j5 = @"0.00";
                robotDataPack.j6 = @"0.00";
            }
            else
            {
                return;
            }

            DoRobotOpCmdHandle(devid, RobotData.RobotCloseSocketFunCode, robotDataPack);
        }

        /// <summary>
        /// 关闭Tcp服务器
        /// </summary>
        public bool RobotServerClose()
        {
            if (IsEnable
#if !robotIsServer
        || ServerSocket != null 
#endif
                )
            {
                try
                {
                    //发送归零指令
                    DoRobotCmdBackZeroHandle(@"1");
                    ThreadSleep(200);

                    DoRobotCmdBackZeroHandle(@"2");
                    ThreadSleep(200);

                    //发送关闭socket指令
                    DoRobotCmdCloseSocketHandle(@"1");
                    ThreadSleep(200);

                    DoRobotCmdCloseSocketHandle(@"2");
                    ThreadSleep(1000);

#if robotIsServer
                    foreach (KeyValuePair<byte, TcpClientSocket> item in RobotSocket)
                    {
                        item.Value.CloseClientSocket();
                    }
                    RobotSocket.Clear();
#else
                    ServerSocket.CloseAllClientSocket();
#endif
                }
                catch (Exception e)
                {
                    AddLog("Robot Exception (RobotServerClose): " + e.Message, -1);
                }
            }

            //更新状态
            IsEnable = false;
            myRobotGlobalInfo.ModWorkMsg = @"服务器已断开连接";
            myRobotGlobalInfo.ModWorkCode = 0;

            myRobotGlobalInfo.FrontRobotRspMsg = @"...";
            myRobotGlobalInfo.FrontRobotSiteData.j1 = "0.00";
            myRobotGlobalInfo.FrontRobotSiteData.j2 = "0.00";
            myRobotGlobalInfo.FrontRobotSiteData.j3 = "0.00";
            myRobotGlobalInfo.FrontRobotSiteData.j4 = "0.00";
            myRobotGlobalInfo.FrontRobotSiteData.j5 = "0.00";
            myRobotGlobalInfo.FrontRobotSiteData.j6 = "0.00";

            myRobotGlobalInfo.BackRobotRspMsg = @"...";
            myRobotGlobalInfo.BackRobotSiteData.j1 = "0.00";
            myRobotGlobalInfo.BackRobotSiteData.j2 = "0.00";
            myRobotGlobalInfo.BackRobotSiteData.j3 = "0.00";
            myRobotGlobalInfo.BackRobotSiteData.j4 = "0.00";
            myRobotGlobalInfo.BackRobotSiteData.j5 = "0.00";
            myRobotGlobalInfo.BackRobotSiteData.j6 = "0.00";

            //发送事件
            RobotModInfoEvent(myRobotGlobalInfo);
            return true;
        }

#if robotIsServer
        /// <summary>
        /// 接收数据
        /// </summary>
        private void RobotFrontRecvData(string message)
        {
            RobotServerRecvData(message, FRONT_ROBOT_DEVICE_ID);
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void RobotBackRecvData(string message)
        {
            RobotServerRecvData(message, BACK_ROBOT_DEVICE_ID);
        }

        private void RobotServerRecvData(string message, byte id)
        {
            TaskRun(new Action(() =>
            {
                byte cmd = 0;
                try
                {
                    //解析报文
                    if (message.Contains("START,") && (message.Contains(",END")))
                    {
                        //按照','字符进行分割
                        string[] subArray = message.Split(',');
                        if (subArray[1].Contains("OK"))
                        {
                            cmd = 1;//命令控制报文
                        }
                        else
                        {
                            if (subArray.Length == 8)
                            {
                                cmd = 2;//获取数据报文 
                            }
                        }

                        //处理数据
                        switch (cmd)
                        {
                            case 1:
                                //获取客户端ip信息
                                switch (id)
                                {
                                    case FRONT_ROBOT_DEVICE_ID:
                                        myRobotGlobalInfo.FrontRobotRspMsg = @"设置成功";

                                        //表明机械臂执行完毕
                                        if (myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                                        {
                                            myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP;
                                        }
                                        break;
                                    case BACK_ROBOT_DEVICE_ID:
                                        myRobotGlobalInfo.BackRobotRspMsg = @"设置成功";

                                        //表明机械臂执行完毕
                                        if (myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                                        {
                                            myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP;
                                        }
                                        break;
                                }
                                break;

                            case 2:
                                switch (id)
                                {
                                    case FRONT_ROBOT_DEVICE_ID:
                                        myRobotGlobalInfo.FrontRobotRspMsg = @"获取数据成功";
                                        myRobotGlobalInfo.FrontRobotSiteData.j1 = subArray[1];
                                        myRobotGlobalInfo.FrontRobotSiteData.j2 = subArray[2];
                                        myRobotGlobalInfo.FrontRobotSiteData.j3 = subArray[3];
                                        myRobotGlobalInfo.FrontRobotSiteData.j4 = subArray[4];
                                        myRobotGlobalInfo.FrontRobotSiteData.j5 = subArray[5];
                                        myRobotGlobalInfo.FrontRobotSiteData.j6 = subArray[6];
                                        break;
                                    case BACK_ROBOT_DEVICE_ID:
                                        myRobotGlobalInfo.BackRobotRspMsg = @"获取数据成功";
                                        myRobotGlobalInfo.BackRobotSiteData.j1 = subArray[1];
                                        myRobotGlobalInfo.BackRobotSiteData.j2 = subArray[2];
                                        myRobotGlobalInfo.BackRobotSiteData.j3 = subArray[3];
                                        myRobotGlobalInfo.BackRobotSiteData.j4 = subArray[4];
                                        myRobotGlobalInfo.BackRobotSiteData.j5 = subArray[5];
                                        myRobotGlobalInfo.BackRobotSiteData.j6 = subArray[6];
                                        break;
                                }
                                break;

                            default:
                                switch (id)
                                {
                                    case FRONT_ROBOT_DEVICE_ID:
                                        myRobotGlobalInfo.FrontRobotRspMsg = @"数据协议异常";
                                        break;
                                    case BACK_ROBOT_DEVICE_ID:
                                        myRobotGlobalInfo.BackRobotRspMsg = @"数据协议异常";
                                        break;
                                }
                                break;
                        }

                        myRobotGlobalInfo.ModWorkCode = 0;
                    }
                    else
                    {
                        myRobotGlobalInfo.ModWorkMsg = @"设置失败,请重新设置";
                        myRobotGlobalInfo.ModWorkCode = -1;
                    }

                    //发送事件
                    switch (id)
                    {
                        case FRONT_ROBOT_DEVICE_ID:
                            RobotModInfoEvent(myRobotGlobalInfo, true);
                            break;
                        case BACK_ROBOT_DEVICE_ID:
                            RobotModInfoEvent(myRobotGlobalInfo, false);
                            break;
                    }
                }
                catch (Exception e)
                {
                    AddLog("Robot Exception (RobotServerRecvData): " + e.Message, -1);
                }
            }));
        }

        /// <summary>
        /// 发送数据后
        /// </summary>
        private void RobotSendFront(int len)
        {
            RobotSendBack(len, FRONT_ROBOT_DEVICE_ID);
        }

        /// <summary>
        /// 发送数据后
        /// </summary>
        private void RobotSendBack(int len)
        {
            RobotSendBack(len, BACK_ROBOT_DEVICE_ID);
        }

        private void RobotSendBack(int len, byte id)
        {
            Console.WriteLine(id.ToString() + "[Send]: " + len);
        }

        public void DoRobotOpCmdHandle(byte devid, string cmd, RobotDataPack data)
        {
            try
            {
                lock (sendLock)
                {
                    string sendpack = "";
                    string val = data.j1 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j2 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j3 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j4 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j5 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j6;
                    //构造数据包
                    if (cmd == RobotData.RobotSetCtrlFunCode)
                    {
                        RokaeRobotProtocolFunc.RobotSetCtrlCmd(val, out sendpack);
                    }
                    else if (cmd == RobotData.RobotGetDataFunCode)
                    {
                        RokaeRobotProtocolFunc.RobotGetStatCmd(val, out sendpack);
                    }
                    else if (cmd == RobotData.RobotBackZeroFunCode)
                    {
                        RokaeRobotProtocolFunc.RobotBackZeroCmd(val, out sendpack);
                    }
                    else if (cmd == RobotData.RobotCloseSocketFunCode)
                    {
                        RokaeRobotProtocolFunc.RobotCloseCmd(val, out sendpack);
                    }
                    else
                    {
                        return;
                    }
                    AddLog("机械臂指令：" + sendpack);

                    byte[] senddataby = System.Text.Encoding.Default.GetBytes(sendpack);
                    byte[] sendpackby = new byte[senddataby.Length + 1];
                    Array.Copy(senddataby, 0, sendpackby, 0, senddataby.Length);

                    sendpackby[senddataby.Length] = RobotData.RAKAE_ROBOT_DATA_TAIL;

                    //发送数据
                    RobotSocket[devid].SendAsync(System.Text.Encoding.UTF8.GetString(sendpackby)); 
                }
            }
            catch (Exception e)
            {
                AddLog("Robot Exception (DoRobotScheduleHandle): " + e.Message, -1);
            }
        }
#else
        /// <summary>
        /// 客户端信息数据
        /// </summary>
        private void RobotServerAccpetData(Socket socket)
        {
            TaskRun(new Action(() =>
            {
                try
                {
                    //获取客户端ip信息
                    string ip_endinfo = socket.RemoteEndPoint.ToString();
                    if (ip_endinfo != null)
                    {
                        if (ip_endinfo.Contains(dicRobotDevices[FRONT_ROBOT_DEVICE_ID]))
                        {
                            myRobotGlobalInfo.FrontRobotRspMsg = ip_endinfo;
                            myRobotGlobalInfo.FrontRobotConnStat = true;
                        }

                        if (ip_endinfo.Contains(dicRobotDevices[BACK_ROBOT_DEVICE_ID]))
                        {
                            myRobotGlobalInfo.BackRobotRspMsg = ip_endinfo;
                            myRobotGlobalInfo.BackRobotConnStat = true;
                        }
                    }

                    //发送事件
                    RobotModInfoEvent(myRobotGlobalInfo);
                }
                catch (Exception e)
                {
                    AddLog("Robot Exception (RobotServerAccpetData): " + e.StackTrace, -1);
                }
            }));
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void RobotServerRecvData(Socket socket, string message)
        {
            TaskRun(new Action(() =>
            {
                bool isFront = true;
                byte cmd = 0;
                try
                {
                    //解析报文
                    if (message.Contains("START,") && (message.Contains(",END")))
                    {
                        //按照','字符进行分割
                        string[] subArray = message.Split(',');
                        if (subArray[1].Contains("OK"))
                        {
                            cmd = 1;//命令控制报文
                        }
                        else
                        {
                            if (subArray.Length == 8)
                            {
                                cmd = 2;//获取数据报文 
                            }
                        }

                        //处理数据
                        switch (cmd)
                        {
                            case 1:
                                {
                                    //获取客户端ip信息
                                    string ip_endinfo = socket.RemoteEndPoint.ToString();
                                    if (ip_endinfo != null)
                                    {
                                        if (ip_endinfo.Contains(dicRobotDevices[FRONT_ROBOT_DEVICE_ID]) == true)
                                        {
                                            myRobotGlobalInfo.FrontRobotRspMsg = @"设置成功";

                                            //表明机械臂执行完毕
                                            if (myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                                            {
                                                myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP;
                                            }
                                        }
                                        else if (ip_endinfo.Contains(dicRobotDevices[BACK_ROBOT_DEVICE_ID]) == true)
                                        {
                                            myRobotGlobalInfo.BackRobotRspMsg = @"设置成功";

                                            //表明机械臂执行完毕
                                            if (myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                                            {
                                                myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP;
                                            }
                                        }
                                    }
                                }
                                break;

                            case 2:
                                {
                                    //获取客户端ip信息
                                    string ip_endinfo = socket.RemoteEndPoint.ToString();
                                    if (ip_endinfo != null)
                                    {
                                        if (ip_endinfo.Contains(dicRobotDevices[FRONT_ROBOT_DEVICE_ID]) == true)
                                        {
                                            myRobotGlobalInfo.FrontRobotRspMsg = @"获取数据成功";

                                            myRobotGlobalInfo.FrontRobotSiteData.j1 = subArray[1];
                                            myRobotGlobalInfo.FrontRobotSiteData.j2 = subArray[2];
                                            myRobotGlobalInfo.FrontRobotSiteData.j3 = subArray[3];
                                            myRobotGlobalInfo.FrontRobotSiteData.j4 = subArray[4];
                                            myRobotGlobalInfo.FrontRobotSiteData.j5 = subArray[5];
                                            myRobotGlobalInfo.FrontRobotSiteData.j6 = subArray[6];
                                        }
                                        else if (ip_endinfo.Contains(dicRobotDevices[BACK_ROBOT_DEVICE_ID]) == true)
                                        {
                                            myRobotGlobalInfo.BackRobotRspMsg = @"获取数据成功";

                                            myRobotGlobalInfo.BackRobotSiteData.j1 = subArray[1];
                                            myRobotGlobalInfo.BackRobotSiteData.j2 = subArray[2];
                                            myRobotGlobalInfo.BackRobotSiteData.j3 = subArray[3];
                                            myRobotGlobalInfo.BackRobotSiteData.j4 = subArray[4];
                                            myRobotGlobalInfo.BackRobotSiteData.j5 = subArray[5];
                                            myRobotGlobalInfo.BackRobotSiteData.j6 = subArray[6];

                                            isFront = false;
                                        }
                                    }
                                }
                                break;

                            default:
                                {
                                    //获取客户端ip信息
                                    string ip_endinfo = socket.RemoteEndPoint.ToString();
                                    if (ip_endinfo != null)
                                    {
                                        if (ip_endinfo.Contains(dicRobotDevices[FRONT_ROBOT_DEVICE_ID]) == true)
                                        {
                                            myRobotGlobalInfo.FrontRobotRspMsg = @"数据协议异常";
                                        }
                                        else if (ip_endinfo.Contains(dicRobotDevices[BACK_ROBOT_DEVICE_ID]) == true)
                                        {
                                            myRobotGlobalInfo.BackRobotRspMsg = @"数据协议异常";
                                        }
                                    }
                                }
                                break;
                        }

                        myRobotGlobalInfo.ModWorkCode = 0;
                    }
                    else
                    {
                        myRobotGlobalInfo.ModWorkMsg = @"设置失败,请重新设置";
                        myRobotGlobalInfo.ModWorkCode = -1;
                    }

                    //发送事件
                    RobotModInfoEvent(myRobotGlobalInfo, isFront);
                }
                catch (Exception e)
                {
                    AddLog("Robot Exception (RobotServerRecvData): " + e.StackTrace, -1);
                }
            }));
        }

        public void DoRobotOpCmdHandle(byte devid, string cmd, RobotDataPack data)
        {
            try
            {
                lock (sendLock)
                {
                    string ipstr = "";
                    string sendpack = "";
                    string val = data.j1 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j2 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j3 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j4 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j5 + RobotData.RAKAE_ROBOT_DATA_SPLIT +
                                 data.j6;
                    Socket client_sock = null;
                    bool ret = false;

                    //判别需要发往哪个Robot的dev_id
                    if (dicRobotDevices.TryGetValue(devid, out ipstr))
                    {
                        //查找session的socket,查看往哪个机器人发送数据
                        try
                        {
                            foreach (var socket in ServerSocket?.clientSockets)
                            {
                                client_sock = socket as Socket;
                                if ((client_sock.RemoteEndPoint).ToString().Contains(ipstr))
                                {
                                    ret = true;
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            AddLog("Robot Exception (DoRobotScheduleHandle1): " + e.StackTrace, -1);
                        }

                        try
                        {
                            if (ret == true && client_sock != null)
                            {
                                //构造数据包
                                if (cmd == RobotData.RobotSetCtrlFunCode)
                                {
                                    RokaeRobotProtocolFunc.RobotSetCtrlCmd(val, out sendpack);
                                }
                                else if (cmd == RobotData.RobotGetDataFunCode)
                                {
                                    RokaeRobotProtocolFunc.RobotGetStatCmd(val, out sendpack);
                                }
                                else if (cmd == RobotData.RobotBackZeroFunCode)
                                {
                                    RokaeRobotProtocolFunc.RobotBackZeroCmd(val, out sendpack);
                                }
                                else if (cmd == RobotData.RobotCloseSocketFunCode)
                                {
                                    RokaeRobotProtocolFunc.RobotCloseCmd(val, out sendpack);
                                }
                                else
                                {
                                    return;
                                }
                                AddLog("机械臂指令：" + sendpack);

                                //!!!这里的珞石机械臂协议比较特殊,需要发送字符串协议,并且在尾巴需要单独加字符'\r';
                                //原则上不推荐在应用层去做协议包的处理
                                byte[] senddataby = System.Text.Encoding.Default.GetBytes(sendpack);
                                byte[] sendpackby = new byte[senddataby.Length + 1];
                                Array.Copy(senddataby, 0, sendpackby, 0, senddataby.Length);

                                sendpackby[senddataby.Length] = RobotData.RAKAE_ROBOT_DATA_TAIL;

                                //发送数据
                                ServerSocket.SendAsync(client_sock, System.Text.Encoding.UTF8.GetString(sendpackby));
                            }
                        }
                        catch (Exception e)
                        {
                            AddLog("Robot Exception (DoRobotScheduleHandle1): " + e.StackTrace, -1);
                        }
                    } 
                }
            }
            catch (Exception e)
            {
                AddLog("Robot Exception (DoRobotScheduleHandle2): " + e.StackTrace, -1);
            }
        }
#endif
    }
}

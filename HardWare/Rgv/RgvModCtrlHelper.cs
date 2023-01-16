using System;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.Network.Network;

namespace PcMainCtrl.HardWare.Rgv
{
    public enum eRGVMODCTRLCMD
    {
        /// <summary>
        /// 无效命令
        /// </summary>
        RGVMODCTRLCMD_INVALID = 0xFFFF,
        /// <summary>
        /// 连接设备
        /// </summary>
        RGVMODCTRLCMD_CONNECT = 0xF000,
        /// <summary>
        /// 断开设备
        /// </summary>
        RGVMODCTRLCMD_DISCONNECT = 0xF001,
        /// <summary>
        /// 正常停止
        /// </summary>
        RGVMODCTRLCMD_NORMALSTOP = 0x0000,
        /// <summary>
        /// 正向运动
        /// </summary>
        RGVMODCTRLCMD_FORWARDMOTOR = 0x0001,
        /// <summary>
        /// 反向运动
        /// </summary>
        RGVMODCTRLCMD_BACKWARDMOTOR = 0x0002,
        /// <summary>
        /// 停止智能充电
        /// </summary>
        RGVMODCTRLCMD_STOPPOWERCHARGE = 0x000C,
        /// <summary>
        /// 清除报警
        /// </summary>
        RGVMODCTRLCMD_CLEARALARM = 0x0007,
        /// <summary>
        /// 开始智能充电
        /// </summary>
        RGVMODCTRLCMD_STARTPOWERCHARGE = 0x000B,
        /// <summary>
        /// 运动到指定距离
        /// </summary>
        RGVMODCTRLCMD_RUNAPPOINTDISTANCE = 0x0005,
        /// <summary>
        /// 设置目标运行速度
        /// </summary>
        RGVMODCTRLCMD_SETTARGETSPEED = 0x000D,
        /// <summary>
        /// 设置目标运行距离
        /// </summary>
        RGVMODCTRLCMD_SETTARGETDISATNCE = 0x000E,
        /// <summary>
        /// 设置轨道长度
        /// </summary>
        RGVMODCTRLCMD_SETTRACKLENGTH = 0x000F,
        /// <summary>
        /// 查询状态
        /// </summary>
        Qurey = 0x1000
    }

    //Rgv运动状态：启动、停止
    public enum eRGVMODRUNSTAT 
    {
        /// <summary>
        /// 启动
        /// </summary>
        RGVMODRUNSTAT_MOVE = 1,
        /// <summary>
        /// 停止
        /// </summary>
        RGVMODRUNSTAT_STOP = 2
    }

    public class RgvGlobalInfo
    {
        public int ModWorkCode { get; set; }
        public string ModWorkMsg { get; set; }

        private int speed = 0;
        /// <summary>
        /// Rgv当前运行速度
        /// </summary>
        public int RgvCurrentRunSpeed
        {
            get { return speed; }
            set
            {
                speed = value;
                if (speed > 0)
                {
                    RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE;
                }
                else if (speed == 0)
                {
                    RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                }
            }
        }

        /// <summary>
        /// Rgv当前运行距离
        /// </summary>
        public int RgvCurrentRunDistacnce { get; set; }

        /// <summary>
        /// Rgv电池状态
        /// </summary>
        public int RgvCurrentPowerStat { get; set; }

        /// <summary>
        /// Rgv当前电池电量
        /// </summary>
        public int RgvCurrentPowerElectricity { get; set; }

        /// <summary>
        /// Rgv当前电池电流
        /// </summary>
        public int RgvCurrentPowerCurrent { get; set; }

        /// <summary>
        /// Rgv当前电池温度
        /// </summary>
        public int RgvCurrentPowerTempture { get; set; }

        /// <summary>
        /// Rgv当前模式：本地模式,远程模式
        /// </summary>
        public string RgvCurrentMode { get; set; }

        /// <summary>
        /// Rgv当前状态信息
        /// </summary>
        public string RgvCurrentStat { get; set; }

        /// <summary>
        /// Rgv命令设置状态
        /// </summary>
        public string RgvCurrentCmdSetStat { get; set; }

        /// <summary>
        /// Rgv参数设置状态
        /// </summary>
        public int RgvCurrentParaSetStat { get; set; }

        /// <summary>
        /// Rgv小车异常状态：0-设备正常,1-设备异常
        /// </summary>
        public int RgvIsAlarm { get; set; } = 0; //默认设备正常

        /// <summary>
        /// Rgv小车运行状态：0-运动状态,1-待机停止
        /// </summary>
        public int RgvIsStandby { get; set; } = 1; //默认设备待机

        /// <summary>
        /// Rgv目标运行速度
        /// </summary>
        public int RgvTargetRunSpeed { get; set; }

        /// <summary>
        /// Rgv目标运行距离
        /// </summary>
        public int RgvTargetRunDistance { get; set; }

        /// <summary>
        /// Rgv轨道运行长度
        /// </summary>
        public int RgvTrackLength { get; set; }

        /// <summary>
        /// Rgv运动指令执行状态监控
        /// </summary>
        public eRGVMODRUNSTAT RgvRunStatMonitor { get; set; } = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
    }

    /// <summary>
    /// 定义一个rgv实例instance && 定时器RgvCollectInfoTimer事件回调 && 消息队列RgvModCtrlQueueHelper && 消息队列处理任务RgvHandleTask
    /// </summary>
    public class RgvModCtrlHelper
    {
        public bool IsEnable = false;
        private eRGVMODCTRLCMD upCmd;

        /// <summary>
        /// Rgv模块
        /// </summary>
        private static RgvModCtrlHelper instance;
        private RgvModCtrlHelper() { }
        public static RgvModCtrlHelper GetInstance()
        {
            return instance ?? (instance = new RgvModCtrlHelper());
        }

        /// <summary>
        /// Rgv客户端连接信息：192.168.0.254:8899
        /// </summary>
        private string ip = Properties.Settings.Default.RgvIP;
        private int port = Properties.Settings.Default.RgvPort;

        private object sendLock = new object();

        /// <summary>
        /// Rgv创建网络客户端的sock连接
        /// </summary>
        private TcpClientSocket myRgvClientSock = null;
        /// <summary>
        /// 车辆设备信息
        /// </summary>
        public RgvGlobalInfo myRgvGlobalInfo = new RgvGlobalInfo();
        public bool IsLink
        {
            get { return myRgvClientSock?.IsLink ?? false; }
        }

        /// <summary>
        /// 委托+事件 = 回调函数，用于传递Rgv的消息
        /// </summary>
        /// <param name="relayinfo"></param>
        public delegate void RgvModInfo(RgvGlobalInfo rgvinfo);
        public event RgvModInfo RgvModInfoEvent;

        /// <summary>
        /// 连接Rgv设备
        /// </summary>
        public bool RgvModConnect(Func<bool> pass = null)
        {
            try
            {
                //创建Socket连接
                if (myRgvClientSock != null && IsEnable == true)
                {
                    return true;
                }

                myRgvClientSock = new TcpClientSocket(ip, port);
                myRgvClientSock.recvByteEvent += new Action<byte[], int>(RgvModRecvByteData);
                myRgvClientSock.sendResultEvent += new Action<int>(RgvModSendData);
            }
            catch (Exception e)
            {
                AddLog(e.Message);
                return false;
            }

        reset:
            if (pass != null && !pass())
            {
                return true;
            }
            try
            {
                bool ret = myRgvClientSock.Start(1);
                if (ret == true)
                {
                    //创建定时器:50ms间隔的定时器
                    InitTimer();

                    //更新状态
                    IsEnable = true;
                    myRgvGlobalInfo.ModWorkMsg = "Rgv小车已连接";
                    myRgvGlobalInfo.ModWorkCode = 0;

                    myRgvGlobalInfo.RgvCurrentRunDistacnce = 0;
                    myRgvGlobalInfo.RgvCurrentRunSpeed = 0;
                    myRgvGlobalInfo.RgvCurrentPowerElectricity = 100;

                    myRgvGlobalInfo.RgvTargetRunSpeed = 500;
                    myRgvGlobalInfo.RgvTrackLength = 1000000;

                    //发送事件
                    RgvModInfoEvent(myRgvGlobalInfo);
                    AddLog(myRgvGlobalInfo.ModWorkMsg);
                }
                else
                {
                    //更新状态
                    myRgvGlobalInfo.ModWorkMsg = "Rgv已断开或网络异常";
                    myRgvGlobalInfo.ModWorkCode = 0;

                    myRgvGlobalInfo.RgvCurrentRunDistacnce = 0;
                    myRgvGlobalInfo.RgvCurrentRunSpeed = 0;
                    myRgvGlobalInfo.RgvCurrentPowerElectricity = 100;

                    //发送事件
                    RgvModInfoEvent(myRgvGlobalInfo);
                    AddLog(myRgvGlobalInfo.ModWorkMsg);

                    Schedule.ScheduleManager.RgvComplete = false;
                    AddLog("RGV尝试重连...");
                    ThreadSleep(3000);
                    goto reset;
                }
            }
            catch (Exception e)
            {
                AddLog("Rgv Exception (RgvModConnect): " + e.Message, -1);
                Schedule.ScheduleManager.RgvComplete = false;
                AddLog("RGV尝试重连...");
                ThreadSleep(3000);
                goto reset;
            }
            return Schedule.ScheduleManager.RgvComplete = true;
        }

        /// <summary>
        /// 定时查询车的数据
        /// </summary>
        private void InitTimer()
        {
            TaskRun(() =>
            {
                while (IsEnable)
                {
                    DoRgvOpCmdHandle(eRGVMODCTRLCMD.Qurey);
                    if (!IsLink)
                    {
                        AddLog("RGV连接中断", -1);
                        RgvModDisConnect();
                        AddLog("断开RGV", -1);
                        break;
                    }
                    ThreadSleep(50);
                }
                RgvModConnect();
                AddLog("重新连接RGV");
            });
        }

        /// <summary>
        /// 断开Rgv设备连接
        /// </summary>
        public bool RgvModDisConnect()
        {
            if (myRgvClientSock != null || (IsEnable == true))
            {
                try
                {
                    //关闭socket
                    myRgvClientSock.CloseClientSocket();
                }
                catch (Exception e)
                {
                    AddLog("Rgv Exception (RgvModDisConnect): " + e.Message, -1);
                }
            }

            //更新状态
            IsEnable = false;
            myRgvGlobalInfo.ModWorkMsg = "Rgv已断开或网络异常";
            myRgvGlobalInfo.ModWorkCode = 0;

            myRgvGlobalInfo.RgvCurrentRunDistacnce = 0;
            myRgvGlobalInfo.RgvCurrentRunSpeed = 0;
            myRgvGlobalInfo.RgvCurrentPowerElectricity = 100;

            //发送事件
            RgvModInfoEvent(myRgvGlobalInfo);
            return true;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void RgvModRecvByteData(byte[] ReceiveBuf, int len)
        {
            TaskRun(new Action(() =>
            {
                try
                {
                    int r = len;

                    int backdata = 0;
                    int shedingshuju = 0;

                    if (r > 31 && r % 31 == 0)
                    {
                        r = 31;
                    }

                    //解析报文
                    if (r == 12)
                    {
                        if (ReceiveBuf[2] == 0x0 && ReceiveBuf[3] == 0x00 && ReceiveBuf[4] == 0x00 && ReceiveBuf[5] == 0x06 && ReceiveBuf[6] == 0x01 && ReceiveBuf[7] == 0x06 && ReceiveBuf[8] == 0x00)//写单个寄存器
                    {
                            int x = ReceiveBuf[9];
                            switch (x)
                            {
                                case 0x01:
                                    {
                                        if (ReceiveBuf[11] == 0x0)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"停车成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x01)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"控制小车正向运动成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x02)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"控制小车反向运动成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x03)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"返回零点成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x05)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"定位到指定位置成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x06)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"紧急停止成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x07)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"清除报警成功";
                                            myRgvGlobalInfo.RgvIsAlarm = 0; //管理Rgv的异常状态
                                    }
                                        else if (ReceiveBuf[11] == 0x08)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"智能返回成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x09)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"刹车盘开启成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x0a)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"刹车盘关闭成功";
                                        }
                                        else if (ReceiveBuf[11] == 0x0b)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"开始充电";
                                        }
                                        else if (ReceiveBuf[11] == 0x0c)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"停止充电";
                                        }
                                    }
                                    break;
                                case 04: //设定运动目标位置
                                {
                                        backdata = ReceiveBuf[10];
                                        backdata = (backdata << 8) + ReceiveBuf[11];

                                        shedingshuju = myRgvGlobalInfo.RgvTargetRunDistance;
                                        if (backdata == shedingshuju)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"设定目标位置成功";
                                        }
                                    }
                                    break;

                                case 02: //设定运动轨道长度
                                {
                                        backdata = ReceiveBuf[10];
                                        backdata = (backdata << 8) + ReceiveBuf[11];

                                        shedingshuju = myRgvGlobalInfo.RgvTrackLength;
                                        if (backdata == shedingshuju)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"设定轨道长度成功";
                                        }
                                    }
                                    break;

                                case 10: //设定目标速度
                                {
                                        backdata = ReceiveBuf[10];
                                        backdata = (backdata << 8) + ReceiveBuf[11];

                                        shedingshuju = myRgvGlobalInfo.RgvTargetRunSpeed;
                                        if (backdata == shedingshuju)
                                        {
                                            myRgvGlobalInfo.RgvCurrentCmdSetStat = @"设定目标速度成功";
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    else if (r == 31)
                    {
                        int temp1 = 0;
                        int temp2 = 0;
                        int temp3 = 0;
                        int temp4 = 0;

                        int temp = 0;
                        String str = @"";

                    //00 1e 00 00 00 19 01 03 16 00 00 00 00 00 00 13 89 00 64 02 bc 00 1a 00 01 00 00 00 00 00 00 
                    if (ReceiveBuf[2] == 0x00 && ReceiveBuf[3] == 0x00
                            && ReceiveBuf[4] == 0x00 && ReceiveBuf[5] == 0x19
                            && ReceiveBuf[6] == 0x01 && ReceiveBuf[7] == 0x03
                            && ReceiveBuf[8] == 0x16)
                        {
                        //得到速度
                        temp1 = ReceiveBuf[9];
                            temp2 = ReceiveBuf[10];
                            temp3 = ReceiveBuf[11];
                            temp4 = ReceiveBuf[12];
                            temp = (temp1 << 24) + (temp2 << 16) + (temp3 << 8) + temp4;
                            myRgvGlobalInfo.RgvCurrentRunSpeed = temp;

                        //得到当前位置(mm)
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp1 = ReceiveBuf[13];
                            temp2 = ReceiveBuf[14];
                            temp3 = ReceiveBuf[15];
                            temp4 = ReceiveBuf[16];
                            temp = (temp1 << 24) + (temp2 << 16) + (temp3 << 8) + temp4;
                            myRgvGlobalInfo.RgvCurrentRunDistacnce = temp;

                        //得到当前电量
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp3 = ReceiveBuf[17];
                            temp4 = ReceiveBuf[18];
                            temp = (temp3 << 8) + temp4;
                            myRgvGlobalInfo.RgvCurrentPowerElectricity = temp;

                        //得到当前电流(mA)
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp3 = ReceiveBuf[19];
                            temp4 = ReceiveBuf[20];
                            temp = (temp3 << 8) + temp4;
                            myRgvGlobalInfo.RgvCurrentPowerCurrent = temp;

                        //得到当前电池温度
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp3 = ReceiveBuf[21];
                            temp4 = ReceiveBuf[22];
                            temp = (temp3 << 8) + temp4;
                            myRgvGlobalInfo.RgvCurrentPowerTempture = temp;

                        //得到当前模式
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp3 = ReceiveBuf[23];
                            temp4 = ReceiveBuf[24];
                            temp = (temp3 << 8) + temp4;
                            if (temp == 1)
                            {
                                myRgvGlobalInfo.RgvCurrentMode = @"自动模式";
                            }
                            else
                            {
                                myRgvGlobalInfo.RgvCurrentMode = @"手动模式";
                            }

                        //得到当前状态,待机状态位标识：0表示待机，1表示正在运行
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp3 = ReceiveBuf[25];
                            temp4 = ReceiveBuf[26];
                            temp = (temp3 << 8) + temp4;

                            if (temp == 0)
                            {
                                str += "待机状态";
                                myRgvGlobalInfo.RgvIsStandby = 1;
                                myRgvGlobalInfo.RgvIsAlarm = 0;
                                ChangeState();
                            }
                            else if ((temp & 0x0001) == 1)
                            {
                                str += "急停状态";
                                myRgvGlobalInfo.RgvIsAlarm = 1;
                                ChangeState();
                            }
                            else if ((temp & 0x2) == 2)
                            {
                                str += "位置数据异常";
                                myRgvGlobalInfo.RgvIsAlarm = 2;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x0004) == 4)
                            {
                                str += "前限位异常";
                                myRgvGlobalInfo.RgvIsAlarm = 4;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x0008) == 8)
                            {
                                str += "后限位异常";
                                myRgvGlobalInfo.RgvIsAlarm = 8;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x10) == 0x10)
                            {
                                str += "驱动电机异常";
                                myRgvGlobalInfo.RgvIsAlarm = 10;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x20) == 0x20)
                            {
                                str += "前减速光电异常";
                                myRgvGlobalInfo.RgvIsAlarm = 20;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x40) == 0x40)
                            {
                                str += "后减速光电异常";
                                myRgvGlobalInfo.RgvIsAlarm = 40;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x80) == 0x80)
                            {
                                str += "前障碍物报警";
                                myRgvGlobalInfo.RgvIsAlarm = 80;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x100) == 0x100)
                            {
                                str += "前障碍物停车";
                                myRgvGlobalInfo.RgvIsAlarm = 100;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x200) == 0x200)
                            {
                                str += "后障碍物报警";
                                myRgvGlobalInfo.RgvIsAlarm = 1;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x0400) == 0x400)
                            {
                                str += "后障碍物停车";
                                myRgvGlobalInfo.RgvIsAlarm = 1;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x0800) == 0x800)
                            {
                                str += "上位机紧急停车";
                                myRgvGlobalInfo.RgvIsAlarm = 1;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x1000) == 0x1000)
                            {
                                str += "运行参数设置异常";
                                myRgvGlobalInfo.RgvIsAlarm = 1;
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
                            }
                            else if ((temp & 0x2000) == 0x2000)
                            {
                                str += "正向运动过程中";
                                myRgvGlobalInfo.RgvIsStandby = 0;
                                myRgvGlobalInfo.RgvIsAlarm = 0;
                                //处于运动状态
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE;
                            }
                            else if ((temp & 0x4000) == 0x4000)
                            {
                                str += "反向运动过程中";
                                myRgvGlobalInfo.RgvIsStandby = 0;
                                myRgvGlobalInfo.RgvIsAlarm = 0;
                                //处于运动状态
                                myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_MOVE;
                            }
                            else
                            {
                                myRgvGlobalInfo.RgvIsAlarm = 0;
                            }
                            myRgvGlobalInfo.RgvCurrentStat = str;

                        //得到设定参数状态
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp3 = ReceiveBuf[27];
                            temp4 = ReceiveBuf[28];
                            temp = (temp3 << 8) + temp4;
                            myRgvGlobalInfo.RgvCurrentParaSetStat = temp;

                        //得到当前电池状态
                        temp1 = 0; temp2 = 0; temp3 = 0; temp4 = 0; temp = 0;
                            temp3 = ReceiveBuf[29];
                            temp4 = ReceiveBuf[30];
                            temp = (temp3 << 8) + temp4;
                            myRgvGlobalInfo.RgvCurrentPowerStat = temp;
                        }
                    }

                    //发送事件
                    RgvModInfoEvent(myRgvGlobalInfo);
                }
                catch (Exception e)
                {
                    AddLog("Rgv Exception (RgvModRecvByteData): " + e.Message, -1);
                }
            })); 
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private void RgvModSendData(int len)
        {
            if (len > 12)
            {
                AddLog("RGV发送数据长度：" + len, 4); 
            }
        }

        /// <summary>
        /// Rgv执行任务
        /// </summary>
        public void DoRgvOpCmdHandle(eRGVMODCTRLCMD rgvcmd)
        {
            try
            {
                byte[] sendby;
                lock (sendLock)
                {
                    if (upCmd != rgvcmd || rgvcmd != eRGVMODCTRLCMD.Qurey)
                    {
                        upCmd = rgvcmd;
                        AddLog("RGV指令：" + rgvcmd.ToString(), 4); 
                    }
                    switch (rgvcmd)
                    {
                        //连接RGV
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_CONNECT:
                            RgvModConnect();
                            break;
                        //断开RGV
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_DISCONNECT:
                            RgvModDisConnect();
                            break;
                        //正常停止
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_NORMALSTOP:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_NormalStop();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //正向运动
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_FORWARDMOTOR:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_ForwardMotor();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //反向运动
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_BACKWARDMOTOR:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_BackwardMotor();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //清除报警
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_CLEARALARM:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_ClearAlarm();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //开始充电
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_STARTPOWERCHARGE:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_StartPowerCharge();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //停止充电
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_STOPPOWERCHARGE:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_StopPowerCharge();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //设置目标速度
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETSPEED:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_SetTargetSpeed(myRgvGlobalInfo.RgvTargetRunSpeed);
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //设置目标距离
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTARGETDISATNCE:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_SetTargetDistance(myRgvGlobalInfo.RgvTargetRunDistance);
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //设置轨道长度
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_SETTRACKLENGTH:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_SetTrackLength(myRgvGlobalInfo.RgvTrackLength);
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        //运动到指定位置
                        case eRGVMODCTRLCMD.RGVMODCTRLCMD_RUNAPPOINTDISTANCE:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_RunAppointDistance();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                        case eRGVMODCTRLCMD.Qurey:
                            sendby = RgvModCtrlProtocol.RgvModCtrlFun.RgvModCtrlCmd_QureyStat();
                            myRgvClientSock?.SendByteAsync(sendby, sendby.Length);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                AddLog("Rgv Exception (RgvModCtrlSendCmd1): " + e.Message, -1);
            } 
        }

        private void ChangeState()
        {
            myRgvGlobalInfo.RgvRunStatMonitor = eRGVMODRUNSTAT.RGVMODRUNSTAT_STOP;
        }
    }
}

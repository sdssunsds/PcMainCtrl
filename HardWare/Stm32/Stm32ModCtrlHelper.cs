using System;
using System.IO.Ports;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.Stm32.Stm32ModCtrlProtocol;

namespace PcMainCtrl.HardWare.Stm32
{
    /// <summary>
    /// 当前Stm32检测的车底信息
    /// </summary>
    public class Stm32GlobalInfo
    {
        public int ModWorkCode { get; set; }
        public string ModWorkMsg { get; set; }

        /// <summary>
        /// 当前检测车底高度
        /// </summary>
        public int CurrentDetectHeight { get; set; }

        /// <summary>
        /// 红外开关IO检测状态
        /// </summary>
        public int InfraRed1_Stat { get; set; }

        /// <summary>
        /// 红外开关IO检测状态
        /// </summary>
        public int InfraRed2_Stat { get; set; }

        /// <summary>
        /// 红外开关IO检测状态
        /// </summary>
        public int InfraRed3_Stat { get; set; }

        /// <summary>
        /// 红外开关IO检测状态
        /// </summary>
        public int InfraRed4_Stat { get; set; }
    }

    /// <summary>
    /// Stm32开发板用于控制语音播报、面阵相机光源调光
    /// </summary>
    public class Stm32ModCtrlHelper
    {
        public bool IsEnable = false;
        public const bool Stm32NetworkFlag = false;

        public bool IsLink
        {
            get { return serialPort != null && serialPort.IsOpen; }
        }

        /// <summary>
        /// Stm32控制板模块
        /// </summary>
        private static Stm32ModCtrlHelper instance;
        private Stm32ModCtrlHelper() { }
        public static Stm32ModCtrlHelper GetInstance()
        {
            return instance ?? (instance = new Stm32ModCtrlHelper());
        }

        private object sendLock = new object();
        private SerialPort serialPort;

        /// <summary>
        /// 车辆设备信息
        /// </summary>
        public Stm32GlobalInfo myStm32GlobalInfo = new Stm32GlobalInfo();

        /// <summary>
        /// 委托+事件 = 回调函数，用于传递Stm32的消息
        /// </summary>
        /// <param name="stm32info"></param>
        public delegate void Stm32ModInfo(Stm32GlobalInfo stm32info);
        public event Stm32ModInfo Stm32ModInfoEvent;

        /// <summary>
        /// Stm32采集数据定时器
        /// </summary>
        public System.Timers.Timer Stm32CollectInfoTimer;

        /// <summary>
        /// 打开串口的方法
        /// </summary>
        public bool OpenPort()
        {
            try//这里写成异常处理的形式以免串口打不开程序崩溃
            {
                serialPort.Open();
            }
            catch (Exception e)
            {
                AddLog("Stm32 Exception (OpenPort): " + e.Message, -1);
            }

            if (serialPort.IsOpen)
            {
                return true;
            }
            else
            {
                AddLog("串口打开失败!");
                return false;
            }
        }

        /// 串口接收通信配置方法
        /// 端口名称
        public bool InitCOM(string PortName)
        {
            serialPort = new SerialPort(PortName, Properties.Settings.Default.Stm32BaudRate, (Parity)Properties.Settings.Default.Stm32Parity, Properties.Settings.Default.Stm32DataBits, (StopBits)Properties.Settings.Default.Stm32StopBits);

            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceivedBytes);//DataReceived事件委托
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.RtsEnable = true;

            return OpenPort();//串口打开
        }

        /// <summary>
        /// 向串口发送数据(字节流)
        /// </summary>
        public void SendCommandBytes(byte[] buffer, int len)
        {
            serialPort.Write(buffer, 0, len);
        }

        /// <summary>
        /// 数据接收事件(字节流)
        /// </summary>
        private void serialPort_DataReceivedBytes(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

                byte[] readBuffer = new byte[32];
                int len = serialPort.Read(readBuffer, 0, readBuffer.Length);

                //解析数据
                if (len > 0)
                {
                    Stm32ModRecvByteData(readBuffer, len);
                }
            }
            catch (Exception ex)
            {
                AddLog("Stm32 Exception (serialPort_DataReceivedBytes): " + ex.Message, -1);
            }
        }

        /// <summary>
        /// 连接Rgv设备
        /// </summary>
        public bool Stm32ModConnect()
        {
            try
            {
                //创建Socket连接
                if (serialPort != null && IsEnable == true)
                {
                    return true;
                }

                bool ret = InitCOM(Properties.Settings.Default.Stm32Com);
                if (ret == true)
                {
                    //创建定时器:50ms间隔的定时器
                    InitTimer();
                    Stm32CollectInfoTimer.Start();

                    //更新状态
                    IsEnable = true;

                    myStm32GlobalInfo.ModWorkMsg = @"Stm32已连接";
                    myStm32GlobalInfo.ModWorkCode = 0;

                    myStm32GlobalInfo.CurrentDetectHeight = 0;
                    myStm32GlobalInfo.InfraRed1_Stat = 0;
                    myStm32GlobalInfo.InfraRed2_Stat = 0;
                    myStm32GlobalInfo.InfraRed3_Stat = 0;
                    myStm32GlobalInfo.InfraRed4_Stat = 0;

                    //发送事件
                    Stm32ModInfoEvent(myStm32GlobalInfo);
                    AddLog(myStm32GlobalInfo.ModWorkMsg);
                }
                else
                {
                    myStm32GlobalInfo.ModWorkMsg = @"Stm32已断开或串口异常";
                    myStm32GlobalInfo.ModWorkCode = 0;

                    myStm32GlobalInfo.CurrentDetectHeight = 0;
                    myStm32GlobalInfo.InfraRed1_Stat = 0;
                    myStm32GlobalInfo.InfraRed2_Stat = 0;
                    myStm32GlobalInfo.InfraRed3_Stat = 0;
                    myStm32GlobalInfo.InfraRed4_Stat = 0;

                    //发送事件
                    Stm32ModInfoEvent(myStm32GlobalInfo);
                    AddLog(myStm32GlobalInfo.ModWorkMsg);
                    Stm32ModDisConnect();
                    return Schedule.ScheduleManager.StmComplete = false;
                }
            }
            catch (Exception e)
            {
                AddLog("Stm32 Exception (Stm32ModConnect): " + e.Message, -1);
                Stm32ModDisConnect();
                return Schedule.ScheduleManager.StmComplete = false;
            }

            return true;
        }

        public bool Stm32ModDisConnect()
        {
            if (serialPort != null && IsEnable == true)
            {
                try
                {
                    //关闭定时器
                    Stm32CollectInfoTimer.Stop();

                    //关闭socket
                    serialPort.Close();
                }
                catch (Exception e)
                {
                    AddLog("Stm32 Exception (Stm32ModDisConnect): " + e.Message, -1);
                }
            }

            //更新状态
            IsEnable = false;
            myStm32GlobalInfo.ModWorkMsg = @"Stm32已断开或网络异常";
            myStm32GlobalInfo.ModWorkCode = 0;

            myStm32GlobalInfo.CurrentDetectHeight = 0;
            myStm32GlobalInfo.InfraRed1_Stat = 0;
            myStm32GlobalInfo.InfraRed2_Stat = 0;
            myStm32GlobalInfo.InfraRed2_Stat = 0;
            myStm32GlobalInfo.InfraRed2_Stat = 0;

            //发送事件
            Stm32ModInfoEvent(myStm32GlobalInfo);
            return true;
        }

        /// <summary>
        /// 定时查询车底的数据
        /// </summary>
        private void InitTimer()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 100;
            Stm32CollectInfoTimer = new System.Timers.Timer(interval);

            //设置执行一次（false）还是一直执行(true)
            Stm32CollectInfoTimer.AutoReset = true;

            //绑定Elapsed事件
            Stm32CollectInfoTimer.Elapsed += new System.Timers.ElapsedEventHandler(getRgvInfoTimer_Tick);
        }

        public void getRgvInfoTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsEnable == true)
            {
                //发送数据
                //固定数据长度：64字节
                byte[] cmdbuf = new byte[Stm32Data.PcNetworkMsgBufsize];
                Array.Clear(cmdbuf, 0, cmdbuf.Length);
                byte cmdlen = 0;

                cmdbuf[0] = Stm32Data.LOCAL_STM32_DEVICE_ID;
                cmdlen++;

                DoStm32OpCmdHandle(Stm32Data.Stm32GetTrainSite_FuncCode, cmdbuf, cmdlen);
            }
        }

        private void Stm32ModRecvByteData(byte[] buffer, int len)
        {
            TaskRun(new Action(() =>
            {
                try
                {
                    int r = len;

                    //解析报文
                    if (r == Stm32Data.Stm32LocalFunPackLen)
                    {
                        //判断包头包尾
                        if (buffer[0] == Stm32Data.PC_NETWORK_MSG_HEAD && buffer[Stm32Data.Stm32LocalFunPackLen - 1] == Stm32Data.PC_NETWORK_MSG_TAIL)
                        {
                            //判别设备地址
                            if (buffer[3] == Stm32Data.LOCAL_STM32_DEVICE_ID)
                            {
                                //判别功能字
                                if (buffer[1] == Stm32Data.Stm32GetTrainSite_FuncCode)
                                {
                                    if (buffer[2] == 9)
                                    {
                                        //获取数据
                                        byte[] carhigh_by = new byte[4];
                                        Array.Clear(carhigh_by, 0, 4);
                                        carhigh_by[0] = buffer[7];
                                        carhigh_by[1] = buffer[6];
                                        carhigh_by[2] = buffer[5];
                                        carhigh_by[3] = buffer[4];
                                        int carhigh_val = BitConverter.ToInt32(carhigh_by, 0);

                                        byte cario1_val = buffer[8];
                                        byte cario2_val = buffer[9];
                                        byte cario3_val = buffer[10];
                                        byte cario4_val = buffer[11];

                                        //数据传递
                                        myStm32GlobalInfo.ModWorkMsg = @"数据获取成功";
                                        myStm32GlobalInfo.ModWorkCode = 0;

                                        myStm32GlobalInfo.CurrentDetectHeight = carhigh_val;
                                        myStm32GlobalInfo.InfraRed1_Stat = cario1_val;
                                        myStm32GlobalInfo.InfraRed2_Stat = cario2_val;
                                        myStm32GlobalInfo.InfraRed3_Stat = cario3_val;
                                        myStm32GlobalInfo.InfraRed4_Stat = cario4_val;
                                    }
                                    else if (buffer[2] == 2)
                                    {
                                        if (buffer[4] == 0x00)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"命令设置成功";
                                            myStm32GlobalInfo.ModWorkCode = 0;
                                        }
                                        else if (buffer[4] == 0x82)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据获取Bad";
                                            myStm32GlobalInfo.ModWorkCode = -1;

                                        }
                                        else if (buffer[5] == 0x81)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据获取Busy";
                                            myStm32GlobalInfo.ModWorkCode = -2;
                                        }
                                        else
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据协议异常";
                                            myStm32GlobalInfo.ModWorkCode = -3;
                                        }
                                    }
                                    else
                                    {
                                        myStm32GlobalInfo.ModWorkMsg = @"数据长度异常";
                                        myStm32GlobalInfo.ModWorkCode = -4;
                                    }
                                }
                                else if (buffer[1] == Stm32Data.Stm32SetRobotStartVoice_FuncCode)
                                {
                                    if (buffer[2] == 2)
                                    {
                                        if (buffer[4] == 0x00)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"命令设置成功";
                                            myStm32GlobalInfo.ModWorkCode = 0;
                                        }
                                        else if (buffer[4] == 0x82)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据获取Bad";
                                            myStm32GlobalInfo.ModWorkCode = -1;
                                        }
                                        else if (buffer[5] == 0x81)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据获取Busy";
                                            myStm32GlobalInfo.ModWorkCode = -2;
                                        }
                                        else
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据协议异常";
                                            myStm32GlobalInfo.ModWorkCode = -3;
                                        }
                                    }
                                    else
                                    {
                                        myStm32GlobalInfo.ModWorkMsg = @"数据长度异常";
                                        myStm32GlobalInfo.ModWorkCode = -4;
                                    }
                                }
                                else if (buffer[1] == Stm32Data.Stm32SetRobotStopVoice_FuncCode)
                                {
                                    if (buffer[2] == 2)
                                    {
                                        if (buffer[4] == 0x00)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"命令设置成功";
                                            myStm32GlobalInfo.ModWorkCode = 0;
                                        }
                                        else if (buffer[4] == 0x82)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据获取Bad";
                                            myStm32GlobalInfo.ModWorkCode = -1;
                                        }
                                        else if (buffer[5] == 0x81)
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据获取Busy";
                                            myStm32GlobalInfo.ModWorkCode = -2;
                                        }
                                        else
                                        {
                                            myStm32GlobalInfo.ModWorkMsg = @"数据协议异常";
                                            myStm32GlobalInfo.ModWorkCode = -3;
                                        }
                                    }
                                    else
                                    {
                                        myStm32GlobalInfo.ModWorkMsg = @"数据长度异常";
                                        myStm32GlobalInfo.ModWorkCode = -4;
                                    }
                                }
                            }
                        }
                        else
                        {
                            myStm32GlobalInfo.ModWorkMsg = @"设备地址异常";
                            myStm32GlobalInfo.ModWorkCode = -5;
                        }
                    }

                    //发送事件
                    Stm32ModInfoEvent(myStm32GlobalInfo);
                }
                catch (Exception e)
                {
                    AddLog("Stm32 Exception (Stm32ModRecvByteData): " + e.Message, -1);
                }
            }));
        }

        /// <summary>
        /// Stm32任务队列入队
        /// </summary>
        public void DoStm32OpCmdHandle(byte cmd, byte[] cmddata, byte cmdlen)
        {
            try
            {
                byte[] sendby = Stm32ModCtrlFun.Stm32ModCtrlCmdForLocal(
                        cmd, cmddata, cmdlen);

                Stm32ModCtrlSendCmd(sendby, sendby.Length);
            }
            catch (Exception e)
            {
                AddLog("Stm32 Exception (DoStm32ScheduleHandle): " + e.Message, -1);
            }
        }

        /// <summary>
        /// Stm32任务队列出队
        /// </summary>
        public void Stm32ModCtrlSendCmd(byte[] sendby, int cmdlen)
        {
            ThreadStart(() =>
            {
                lock (sendLock)
                {
                    try
                    {
                        Stm32CollectInfoTimer.Stop();
                        SendCommandBytes(sendby, cmdlen);
                    }
                    catch (Exception e)
                    {
                        AddLog("Stm32 Exception (Stm32ModCtrlSendCmd1): " + e.Message, -1);
                    }

                    try
                    {
                        Stm32CollectInfoTimer.Start();
                    }
                    catch (Exception e)
                    {
                        AddLog("Stm32 Exception (Stm32ModCtrlSendCmd2): " + e.Message, -1);
                    } 
                }
            });
        }
    }
}
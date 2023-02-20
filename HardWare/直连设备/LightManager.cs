using System;
using System.IO.Ports;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.Common.GlobalValues;
using System.Text;

namespace PcMainCtrl.HardWare
{
    /// <summary>
    /// 光源控制
    /// 开前灯：5A 53 64 b7 5B；开后灯：5A 63 64 c7 5B （亮度100）
    /// 关前灯：5A 53 00 53 5B；关后灯：5A 63 00 63 5B
    /// </summary>
    public class LightManager : IPower
    {
        private bool isFront = false;
        private int outTime = 2;  // 超时时间，单位秒
        private string frontID = "53";
        private string backID = "63";
        private string writeCMD = "";
        private StringBuilder frontCmdStr = new StringBuilder();
        private StringBuilder backCmdStr = new StringBuilder();
        private SerialPort serialPort;
        private object lockObj = new object();

        public bool IsLink { get { return serialPort.IsOpen; } }

        public bool IsReceived { get; private set; }

        public Func<bool> LinkLight { private get; set; }

        public LightManager() { }

        public LightManager(string com, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            try
            {
                serialPort = new SerialPort(com, baudRate, parity, dataBits, stopBits);
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.ErrorReceived += SerialPort_ErrorReceived;
                serialPort.PinChanged += SerialPort_PinChanged;
                serialPort.Open();
            }
            catch (Exception e)
            {
                Common.GlobalValues.AddLog(e.Message);
            }
        }

        public void LightOn(int lightValue, bool front)
        {
            if (LinkLight())
            {
                int id = Convert.ToInt32(front ? frontID : backID, 16);
                string v = Convert.ToString(lightValue, 16);
                if (v.Length < 2)
                {
                    v = "0" + v;
                }
                lock (lockObj)
                {
                    isFront = front;
                    Write(string.Format("5A {0} {1} {2} 5B", front ? frontID : backID, v, Convert.ToString(id + lightValue, 16))); 
                }
            }
        }

        public void LightOff(bool front)
        {
            if (LinkLight())
            {
                lock (lockObj)
                {
                    isFront = front;
                    if (front)
                    {
                        Write(string.Format("5A {0} 00 {0} 5B", frontID));
                    }
                    else
                    {
                        Write(string.Format("5A {0} 00 {0} 5B", backID));
                    }  
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[serialPort.BytesToRead];
            int length = serialPort.Read(buffer, 0, buffer.Length);
            if (length > 0)
            {
                string cmd = "";
                try
                {
                    foreach (byte item in buffer)
                    {
                        cmd += item.ToString("X2") + " ";
                    }
                    if (isFront)
                    {
                        frontCmdStr.Append(cmd);
                        cmd = frontCmdStr.ToString().Trim();
                    }
                    else
                    {
                        backCmdStr.Append(cmd);
                        cmd = backCmdStr.ToString().Trim();
                    }
                    int start = cmd.IndexOf("5A");
                    int end = cmd.IndexOf("5B");
                    if (start >= 0 && end > start && end - start == 12)
                    {
                        cmd = cmd.Substring(start, 14);
                    }
                    IsReceived = cmd == writeCMD.ToUpper();
                    if (IsReceived)
                    {
                        if (isFront)
                        {
                            frontCmdStr.Clear();
                        }
                        else
                        {
                            backCmdStr.Clear();
                        }
                    }
                    AddLog("收到" + (isFront ? "前" : "后") + "光源回复：" + cmd, isFront ? 1 : 2);
                }
                catch (Exception ex)
                {
                    AddLog((isFront ? "前" : "后") + "光源接收数据时异常：" + ex.Message + "[" + cmd + "]", -1);
                }
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {

        }

        private void Write(string cmd)
        {
            IsReceived = false;
            if (serialPort.IsOpen)
            {
            WriteData:
                writeCMD = cmd;
                byte[] bytes = HexStringToByteArray(cmd);
                serialPort.Write(bytes, 0, bytes.Length);
                int length = outTime * 10;
                for (int i = 0; i < length; i++)
                {
                    if (IsReceived)
                    {
                        ThreadSleep(100);
                        return;
                    }
                    ThreadSleep(100);
                }
                AddLog("光源指令超时，重新发送指令", isFront ? 1 : 2);
                goto WriteData;
            }
        }

        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        public void PowerOn()
        {
            ModbusTCP.SetAddress12(Address12Type.RobotMzLedPower, 1);
        }

        public void PowerOff()
        {
            ModbusTCP.SetAddress12(Address12Type.RobotMzLedPower, 2);
        }
    }
}

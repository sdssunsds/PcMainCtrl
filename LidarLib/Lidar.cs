using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LidarLib
{
    public class Lidar
    {
        private const int radius = 4000;  // 图像半径，最大值10000，数值越大，资源占用越大
        private readonly float num = 16000f / radius;
        private static Lidar lidar = null;
        private static SerialPort serialPort = null;
        private bool isData = false;
        private bool canRece = false;
        private object lockDict = new object();
        private LidarType lidarType = LidarType.A2;
        private StringBuilder StringBuilder = null;
        private Dictionary<float, int> datas = new Dictionary<float, int>();

        public bool IsLink { get; private set; }

        public event Action<float, int> AccpData;
        public event Action<float, int> HeadsData;
        public event Action<float, int> HeadData;
        public event Action<string, int> AddLog;

        private Lidar(LidarType lidarType)
        {
            StringBuilder = new StringBuilder();
            if (serialPort == null)
            {
                this.lidarType = lidarType;
                int baudRate = 115200;
                switch (lidarType)
                {
                    case LidarType.A2:
                        baudRate = 115200;
                        break;
                    case LidarType.A3:
                        baudRate = 256000;
                        break;
                    case LidarType.S1:
                        baudRate = 256000;
                        break;
                    case LidarType.S2:
                        baudRate = 1000000;
                        break;
                }
                serialPort = new SerialPort(Properties.Settings.Default.LidarCom,
                    baudRate, Parity.None, 8, StopBits.One);
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            Open();
        }

        public static Lidar Instance(LidarType lidarType, Action<float, int> accpInit = null, Action<float, int> headsInit = null, Action<float, int> headInit = null, Action<string, int> logInit = null)
        {
            if (lidar == null)
            {
                lidar = new Lidar(lidarType);
                if (accpInit != null)
                {
                    lidar.AccpData += accpInit;
                }
                if (headsInit != null)
                {
                    lidar.HeadsData += accpInit;
                }
                if (headInit != null)
                {
                    lidar.HeadData += accpInit;
                }
                if (logInit != null)
                {
                    lidar.AddLog += logInit;
                }
            }
            return lidar;
        }

        public double GetTrainHead(Dictionary<float, int> datas = null)
        {
            if (datas == null)
            {
                datas = this.datas;
            }
            float min_dis = 10000;
            int l = 0;
            float r = 0;
            PointF point = new PointF(0, 0);
            lock (lockDict)
            {
                try
                {
                    foreach (KeyValuePair<float, int> item in datas)
                    {
                        if (item.Key > 0 && item.Key < 90)
                        {
                            float x = (float)(Math.Sin(item.Key * Math.PI / 180) * (item.Value));
                            float y = (float)(Math.Cos(item.Key * Math.PI / 180) * (item.Value));
                            //通过高度和距离来过滤掉无效的和干扰的点
                            if (y >= 500 && y < 3000 && x >= 1000)
                            {
                                if (min_dis > x)
                                {
                                    AddLog?.Invoke($"雷达点位—角度：{item.Key} 距离：{item.Value}", 3);
                                    AddLog?.Invoke($"转换后的坐标—X：{x} Y：{y}", 3);
                                    AddLog?.Invoke($"当前记录的最小值：{min_dis} 当前点位值：{x}", 3);
                                    min_dis = x;
                                    point.X = x;
                                    point.Y = y;
                                    l = item.Value;
                                    r = item.Key;
                                    HeadsData?.Invoke(r, l);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AddLog(e.Message, -1);
                    return 0;
                }
            }
            AddLog?.Invoke($"通过计算得到的雷达点位—角度：{r} 距离：{l}", 3);
            HeadData?.Invoke(r, l);
            AddLog?.Invoke($"通过计算得到的X轴的值：{point.X}", 3);
            return point.X;
        }

        public Bitmap DrawImage(float r, int l, Color color, Bitmap bitmap)
        {
            Graphics g = null;
            if (bitmap == null)
            {
                bitmap = new Bitmap(radius * 2, radius * 2);
                g = Graphics.FromImage(bitmap);
                for (int i = 0; i < 24; i++)
                {
                    g.DrawLine(new Pen(Color.Black), new Point(radius, radius), GetPointF(15 * i, 200000));
                }
            }
            else
            {
                g = Graphics.FromImage(bitmap); 
            }
            PointF endPoint = GetPointF(r, l);
            Rectangle rectangle = new Rectangle((int)endPoint.X - 5, (int)endPoint.Y - 5, 10, 10);
            g.FillRectangle(new SolidBrush(color), rectangle);
            g.Dispose();
            return bitmap;
        }

        public Bitmap DrawLine(float r, int l, Color color, Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }
            Graphics g = Graphics.FromImage(bitmap);
            PointF endPoint = GetPointF(r, l);
            g.DrawLine(new Pen(color), new PointF(radius, radius), endPoint);
            g.Dispose();
            return bitmap;
        }

        public void Open()
        {
            try
            {
                if (serialPort != null && !serialPort.IsOpen)
                {
                    serialPort.Open();
                    IsLink = true;
                }
            }
            catch (Exception e)
            {
                AddLog?.Invoke(e.Message, -1);
            }
        }

        public void Close()
        {
            serialPort.Close();
            IsLink = false;
        }

        public void Start()
        {
            lock (lockDict)
            {
                datas.Clear(); 
            }
            canRece = true;
            if (lidarType == LidarType.A2)
            {
                Send("A5 50");
                Send("A5 F0 02 94 02 C1"); 
            }
            Send("A5 20");
        }

        public void End()
        {
            canRece = false;
            if (lidarType == LidarType.A2)
            {
                Send("A5 F0 02 00 00 57");
            }
            else
            {
                Send("A5 25");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!canRece)
            {
                return;
            }
            byte[] buffer = new byte[serialPort.BytesToRead];
            int length = serialPort.Read(buffer, 0, buffer.Length);
            if (length > 0)
            {
                string value = "";
                for (int i = 0; i < buffer.Length; i++)
                {
                    value += Convert.ToString(buffer[i], 16) + " ";
                }

                StringBuilder.Append(value);

                value = StringBuilder.ToString();
                if (value.Contains("a5 5a 5 0 0 40 81 "))
                {
                    isData = true;
                    StringBuilder.Clear();
                    StringBuilder.Append(value.Substring(value.IndexOf("a5 5a 5 0 0 40 81 ") + 18));
                }

                if (isData)
                {
                    string[] vs = StringBuilder.ToString().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (vs.Length >= 5)
                    {
                        int len = vs.Length % 5;
                        StringBuilder.Clear();
                        for (int i = vs.Length - len; i < vs.Length; i++)
                        {
                            StringBuilder.Append(vs[i] + " ");
                        }

                        Task.Run(() =>
                        {
                            int index = -1;
                            for (int i = 0; i < 4; i++)
                            {
                                if (Convert.ToInt32(vs[i], 16) % 2 == 0 &&
                                    Convert.ToInt32(vs[i + 1], 16) % 2 == 1)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            if (index < 0)
                            {
                                return;
                            }

                            for (int i = index; i < vs.Length - 5; i += 5)
                            {
                                if (vs[i + 3] != "0" && vs[i + 4] != "0")
                                {
                                    #region 角度
                                    int _i = Convert.ToInt32(vs[i + 1], 16);
                                    string _s = Convert.ToString(_i, 2);
                                    _s = new string('0', 8 - _s.Length) + _s;
                                    _s = _s.Substring(0, 7);
                                    string tmp = Convert.ToString(Convert.ToInt32(vs[i + 2], 16), 2);
                                    tmp = new string('0', 8 - tmp.Length) + tmp;
                                    _s = tmp + _s;
                                    int i1 = Convert.ToInt32(_s.Substring(0, 9), 2), i2 = Convert.ToInt32(_s.Substring(9), 2);
                                    float r = i1 + (float)i2 / 60;
                                    #endregion
                                    #region 距离
                                    _i = Convert.ToInt32(vs[i + 4] + vs[i + 3], 16);
                                    _s = Convert.ToString(_i, 2);
                                    _s = _s.Substring(0, _s.Length - 2);
                                    int l = Convert.ToInt32(_s, 2);
                                    #endregion
                                    lock (lockDict)
                                    {
                                        try
                                        {
                                            if (datas.ContainsKey(r))
                                            {
                                                datas[r] = l;
                                            }
                                            else
                                            {
                                                datas.Add(r, l);
                                            }
                                        }
                                        catch (Exception) { } 
                                    }
                                    AccpData?.Invoke(r, l);
                                }
                            }
                        });
                    }
                }
            }
        }

        private void Send(string s)
        {
            byte[] bytes = HexStringToByteArray(s);
        reWrite:
            if (serialPort.IsOpen)
            {
                serialPort.Write(bytes, 0, bytes.Length); 
            }
            else
            {
                AddLog?.Invoke("串口异常关闭，重新打开发送", 0);
                Open();
                Thread.Sleep(50);
                goto reWrite;
            }
            Thread.Sleep(50);
        }

        private PointF GetPointF(float r, int l)
        {
            PointF pointF = new PointF();
            pointF.X = (float)(l / num * Math.Cos((r + 270) * Math.PI / 180) + radius);
            pointF.Y = (float)(l / num * Math.Sin((r + 270) * Math.PI / 180) + radius);
            return pointF;
        }

        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
    }

    public enum LidarType
    {
        A2, A3, S1, S2
    }
}

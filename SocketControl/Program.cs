using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static SocketControl.Network;

namespace SocketControl
{
    class Program
    {
        static string IP = Properties.Settings.Default.ServerIP;
        static int Port = Properties.Settings.Default.ServerPort;
        static string ID = Properties.Settings.Default.RobotID;
        static void Main(string[] args)
        {
            TcpClientSocket client = new TcpClientSocket(IP, Port);
            client.recvMessageEvent = (string msg) =>
            {
                Addlog("收到：" + msg);
                string[] vs = msg.Split('/');
                if (vs != null && vs.Length > 2 && vs[0] == "START")
                {
                    Task.Run(() =>
                    {
                        if (vs[1] != "robot_online" && vs[1] != "robot_state")
                        {
                            WriteFile(vs[1], vs[2]);
                            Thread.Sleep(500);
                            msg = "START/" + vs[1] + "/" + ReadFile(vs[1]) + "/END";
                            Addlog("发送：" + msg);
                            client.SendAsync(msg); 
                        }
                    });
                }
            };
            Task.Run(() =>
            {
                try
                {
                    WriteFile("link", false.ToString());
                    do
                    {
                        if (client.connectSocket != null)
                        {
                            WriteFile("link", client.connectSocket.Connected.ToString()); 
                        }
                        if (client.connectSocket == null || !client.connectSocket.Connected)
                        {
                            while (!client.Start(0))
                            {
                                WriteFile("link", false.ToString());
                                Thread.Sleep(3000);
                            }
                            Addlog("发送：START/robot_online/" + ID + "/END");
                            client.SendAsync("START/robot_online/" + ID + "/END");
                        }
                        Thread.Sleep(500);
                        if (client.connectSocket != null && client.connectSocket.Connected)
                        {
                            string json = GetSocketRgvInfo();
                            if (!string.IsNullOrEmpty(json))
                            {
                                //Addlog("发送：START/robot_state/" + json + "/END");
                                client.SendAsync("START/robot_state/" + json + "/END");
                            }
                            Thread.Sleep(500);
                            if (File.Exists("socket\\robot_outtime_alarm.r"))
                            {
                                string msg = "START/robot_outtime_alarm/" + ReadFile("robot_outtime_alarm") + "/END";
                                Addlog("发送：" + msg);
                                client.SendAsync(msg); 
                            }
                        }
                    } while (true);
                }
                catch (Exception ex)
                {
                    Addlog(ex.Message);
                }
            });
            Console.ReadKey();
        }
        static void Addlog(string s)
        {
            Console.WriteLine(s);
        }
        static string GetSocketRgvInfo()
        {
        Reader:
            try
            {
                if (File.Exists("socket\\SocketRgvInfo.json"))
                {
                    string json = "";
                    using (StreamReader sr = new StreamReader("socket\\SocketRgvInfo.json"))
                    {
                        json = sr.ReadToEnd();
                    }
                    SocketRgvInfo rgvInfo = JsonConvert.DeserializeObject<SocketRgvInfo>(json);
                    rgvInfo.Log = "";
                    try
                    {
                        using (StreamWriter sw = new StreamWriter("socket\\SocketRgvInfo.json"))
                        {
                            sw.Write(JsonConvert.SerializeObject(rgvInfo));
                        }
                    }
                    catch (Exception) { }
                    return json;
                }
            }
            catch (Exception)
            {
                Thread.Sleep(10);
                goto Reader;
            }
            return "";
        }
        static string ReadFile(string cmd)
        {
            while (!File.Exists("socket\\" + cmd + ".r"))
            {
                Thread.Sleep(100);
            }
            string value = "";
        read:
            try
            {
                using (StreamReader sr = new StreamReader("socket\\" + cmd + ".r"))
                {
                    value = sr.ReadToEnd();
                }
            }
            catch (Exception)
            {
                Thread.Sleep(10);
                goto read;
            }
        delete:
            try
            {
                File.Delete("socket\\" + cmd + ".r");
            }
            catch (Exception)
            {
                Thread.Sleep(10);
                goto delete;
            }
            return value;
        }
        static void WriteFile(string cmd, string s)
        {
            if (!File.Exists("socket\\" + cmd + ".c"))
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter("socket\\" + cmd + ".c"))
                    {
                        sw.Write(s);
                    }
                }
                catch (Exception) { }
            }
        }
    }

    public class SocketRgvInfo
    {
        #region RGV
        public string ID { get; set; }
        public int RgvCurrentRunSpeed { get; set; }
        public int RgvCurrentRunDistacnce { get; set; }
        public int RgvCurrentPowerStat { get; set; }
        public int RgvCurrentPowerElectricity { get; set; }
        public int RgvCurrentPowerCurrent { get; set; }
        public int RgvCurrentPowerTempture { get; set; }
        public string RgvCurrentMode { get; set; }
        public string RgvCurrentStat { get; set; }
        public string RgvCurrentCmdSetStat { get; set; }
        public int RgvCurrentParaSetStat { get; set; }
        public int RgvIsAlarm { get; set; }
        public int RgvIsStandby { get; set; }
        public int RgvTargetRunSpeed { get; set; }
        public int RgvTargetRunDistance { get; set; }
        public int RgvTrackLength { get; set; }
        public string RgvRunStatMonitor { get; set; }
        #endregion
        #region 线阵
        public int DataLine { get; set; }
        public int CarriageId { get; set; }
        public int CarriageType { get; set; }
        public int RgvCheckMinDistacnce { get; set; }
        #endregion
        #region 面阵
        public int Rgv_Distance { get; set; }
        public string Front_Parts_Id { get; set; }
        public string FrontComponentType { get; set; }
        public string FrontRobot_J1 { get; set; }
        public string FrontRobot_J2 { get; set; }
        public string FrontRobot_J3 { get; set; }
        public string FrontRobot_J4 { get; set; }
        public string FrontRobot_J5 { get; set; }
        public string FrontRobot_J6 { get; set; }
        public string FrontComponentId { get; set; }
        public int Front3d_Id { get; set; }
        public string Back_Parts_Id { get; set; }
        public string BackComponentType { get; set; }
        public string BackRobot_J1 { get; set; }
        public string BackRobot_J2 { get; set; }
        public string BackRobot_J3 { get; set; }
        public string BackRobot_J4 { get; set; }
        public string BackRobot_J5 { get; set; }
        public string BackRobot_J6 { get; set; }
        public string BackComponentId { get; set; }
        public int Back3d_Id { get; set; }
        #endregion
        #region 通用信息
        public string TrainMode { get; set; }
        public string TrainSn { get; set; }
        public int TrainCurrentHeadDistance { get; set; }
        public bool FrontRobotConnStat { get; set; }
        public string FrontRobotRunStatMonitor { get; set; }
        public bool BackRobotConnStat { get; set; }
        public string BackRobotRunStatMonitor { get; set; }
        public string Log { get; set; }
        public string Job { get; set; }
        #endregion
    }
}

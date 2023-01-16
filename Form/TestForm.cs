using GW.Function.FileFunction;
using PcMainCtrl.Common;
using PcMainCtrl.HttpServer;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PcMainCtrl.Form
{
    public partial class TestForm : System.Windows.Forms.Form
    {
        #region
        private bool asynchronous = true;
        private bool checkHead = true;
        private bool clear3D = true;
        private bool clearLocalImage = true;
        private bool clearLog = false;
        private bool clearOrc = false;
        private bool clearTask = true;
        private bool clearTest = false;
        private bool clearWork = false;
        private bool controlEnable = true;
        private bool controlEnableTmp = true;
        private bool http = true;
        private bool io = false;
        private bool isEnable = false;
        private bool join = false;
        private bool laser = false;
        private bool laserNormal = true;
        private bool laserSlow = false;
        private bool light = true;
        private bool location = true;
        private bool mzAll = true;
        private bool mzPart = false;
        private bool normal = true;
        private bool outTime = true;
        private bool plc = true;
        private bool radar = true;
        private bool rgv = true;
        private bool runBack = true;
        private bool runBackMz = true;
        private bool runBackXz = false;
        private bool runXz = true;
        private bool savePointPic = false;
        private bool simulation = false;
        private bool socket = true;
        private bool speedMode = false;
        private bool stm32 = true;
        private bool wcf = true;
        private bool xzAll = true;
        private bool xzDouble = false;
        private bool xzPart = false;
        private bool xzSingle = true;
        private bool zip = false;
        private bool zoom = false;
        private bool[] camera = { true, true };
        private bool[] mzLoc = { true, true, true, true, true, true, true, true };
        private bool[] mzType = { true, true, true };
        private bool[] xzLoc = { false, false, false, false, false, false, false, false };
        private bool[] robot = { true, true };
        private bool[] sliding = { true, true };
        private string iniPath = Application.StartupPath + "\\operation.ini";
        private Dictionary<string, List<string>> modeSn = new Dictionary<string, List<string>>(); 
        #endregion

        public Action InitAct { private get; set; }
        public Action SpeedCorrect { private get; set; }
        public Action<object, Action> Run { private get; set; }
        public Action<object> Stop { private get; set; }
        public Func<int> GetSpeed { private get; set; }
        public Func<int, bool> GetState { private get; set; }
        public Func<bool> Inspect { private get; set; }

        public TestForm()
        {
            InitializeComponent();
        }

        public bool GetEnable(EnableEnum enable)
        {
            switch (enable)
            {
                case EnableEnum.清理日志:
                    return clearLocalImage && clearLog;
                case EnableEnum.清理Task:
                    return clearLocalImage && clearTask;
                case EnableEnum.清理Test:
                    return clearLocalImage && clearTest;
                case EnableEnum.清理Work:
                    return clearLocalImage && clearWork;
                case EnableEnum.清理Orc:
                    return clearLocalImage && clearOrc;
                case EnableEnum.清理3D:
                    return clearLocalImage && clear3D;
                case EnableEnum.车头检测:
                    return checkHead;
                case EnableEnum.激光测车头:
                    return checkHead && laser && laserNormal;
                case EnableEnum.激光慢速测车头:
                    return checkHead && laser && laserSlow;
                case EnableEnum.传感器测车头:
                    return checkHead && io;
                case EnableEnum.雷达测车头:
                    return checkHead && radar;
                case EnableEnum.执行线阵:
                    return runXz;
                case EnableEnum.完整线阵:
                    return runXz && xzAll;
                case EnableEnum.部分线阵:
                    return runXz && xzPart;
                case EnableEnum.返回线阵:
                    return runBack && runBackXz;
                case EnableEnum.完整面阵:
                    return runBack && runBackMz && mzAll;
                case EnableEnum.部分面阵:
                    return runBack && runBackMz && mzPart;
                case EnableEnum.面阵相机:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && mzType[0];
                case EnableEnum.油位窗:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && mzType[1];
                case EnableEnum.三维相机:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && mzType[2];
                case EnableEnum.前机械臂:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && robot[0];
                case EnableEnum.后机械臂:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && robot[1];
                case EnableEnum.前滑台:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && sliding[0];
                case EnableEnum.后滑台:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && sliding[1];
                case EnableEnum.前相机:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && camera[0];
                case EnableEnum.后相机:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && camera[1];
                case EnableEnum.同步:
                    return runBack && runBackMz && !asynchronous;
                case EnableEnum.异步:
                    return runBack && runBackMz && asynchronous;
                case EnableEnum.轴定位:
                    if (mzAll)
                    {
                        return true;
                    }
                    return runBack && runBackMz && mzPart && location;
                case EnableEnum.正常流程:
                    return normal;
                case EnableEnum.模拟流程:
                    return simulation;
                case EnableEnum.记录速度:
                    return speedMode;
                case EnableEnum.拉伸速度图片:
                    return speedMode && zoom;
                case EnableEnum.拼图:
                    return join;
                case EnableEnum.RGV:
                    return rgv;
                case EnableEnum.Stm32:
                    return stm32;
                case EnableEnum.PLC:
                    return plc;
                case EnableEnum.光源:
                    return light;
                case EnableEnum.Socket:
                    return socket;
                case EnableEnum.传图服务:
                    return wcf;
                case EnableEnum.业务接口:
                    return http;
                case EnableEnum.压缩:
                    return zip;
                case EnableEnum.超时:
                    return outTime;
                case EnableEnum.单线阵:
                    return xzSingle;
                case EnableEnum.双线阵:
                    return xzDouble;
                case EnableEnum.点云图:
                    return savePointPic;
                default:
                    return false;
            }
        }

        public List<DataMzCameraLineModelEx> GetMzData(List<DataMzCameraLineModelEx> list)
        {
            List<DataMzCameraLineModelEx> tmp = new List<DataMzCameraLineModelEx>();
            foreach (DataMzCameraLineModelEx item in list)
            {
                try
                {
                    string s = item.Location;
                    int i = int.Parse(s.Split('_')[0]);
                    if (mzLoc[i])
                    {
                        tmp.Add(item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return tmp;
        }

        public int GetXzLocationStart(int start = -100000)
        {
            if (start == -100000)
            {
                for (int i = 0; i < xzLoc.Length; i++)
                {
                    if (xzLoc[i])
                    {
                        start = (i - 1) * 25000 + (i > 0 ? 25500 : 0);
                    }
                }
            }
            int loc = -1;
            if (start <= 0)
            {
                return loc + 1;
            }
            else
            {
                start = start - 25000;
                return loc + start / 25000 + 1;
            }
        }

        public List<TrainLocation> GetXzLocation()
        {
            List<TrainLocation> list = new List<TrainLocation>();
            for (int i = 0; i < xzLoc.Length; i++)
            {
                if (i == 0 && xzLoc[i])
                {
                    TrainLocation location = new TrainLocation();
                    location.Start = -4000;
                    location.End = 28500;  // 车头26.5米，延长2米结束拍照
                    list.Add(location);
                }
                else if (i > 0 && i < 7 && xzLoc[i - 1] && xzLoc[i])
                {
                    list[list.Count - 1].End = 28500 + i * 25000;
                }
                else if (i == 7 && xzLoc[i])
                {
                    if (xzLoc[6])
                    {
                        list[list.Count - 1].End = 207000;
                    }
                    else
                    {
                        TrainLocation location = new TrainLocation();
                        location.Start = 176500;
                        location.End = 207000;
                        list.Add(location); 
                    }
                }
                else if (xzLoc[i])
                {
                    TrainLocation location = new TrainLocation();
                    location.Start = 25500 + (i - 1) * 25000;  // 车头26.5米，提前一米开始拍照
                    location.End = 28500 + i * 25000;
                    list.Add(location);
                }
            }
            return list;
        }

        public void SetConfig()
        {
            string path = Application.StartupPath + "\\json\\";
            string[] dirs = Directory.GetDirectories(path);
            if (dirs != null && dirs.Length > 0)
            {
                int index = 0, i = 0;
                string mode = FileSystem.ReadIniFile("车型车号", "车型", "0", iniPath);
                foreach (string dir in dirs)
                {
                    if (dir.Length > 3 && dir.Contains("_"))
                    {
                        string[] ms = dir.Replace(path, "").Split('_');
                        if (!modeSn.ContainsKey(ms[0]))
                        {
                            modeSn.Add(ms[0], new List<string>());
                            comboBox1.Items.Add(ms[0]);
                            if (ms[0] == mode)
                            {
                                index = i;
                            }
                        }
                        modeSn[ms[0]].Add(ms[1]);
                        i++;
                    }
                }
                comboBox1.SelectedIndex = index;
            }

            checkBox1.Checked = bool.Parse(FileSystem.ReadIniFile("流程开关", checkBox1.Text, "True", iniPath));
            checkBox2.Checked = bool.Parse(FileSystem.ReadIniFile("流程开关", checkBox2.Text, "True", iniPath));
            checkBox3.Checked = bool.Parse(FileSystem.ReadIniFile("流程开关", checkBox3.Text, "True", iniPath));
            checkBox4.Checked = bool.Parse(FileSystem.ReadIniFile("流程开关", checkBox4.Text, "True", iniPath));
            radioButton1.Checked = bool.Parse(FileSystem.ReadIniFile("流程开关", radioButton1.Text, "False", iniPath));
            radioButton2.Checked = bool.Parse(FileSystem.ReadIniFile("流程开关", radioButton2.Text, "True", iniPath));
            checkBox5.Checked = bool.Parse(FileSystem.ReadIniFile("本地缓存", checkBox5.Text, "False", iniPath));
            checkBox6.Checked = bool.Parse(FileSystem.ReadIniFile("本地缓存", checkBox6.Text, "True", iniPath));
            checkBox7.Checked = bool.Parse(FileSystem.ReadIniFile("本地缓存", checkBox7.Text, "False", iniPath));
            checkBox8.Checked = bool.Parse(FileSystem.ReadIniFile("本地缓存", checkBox8.Text, "False", iniPath));
            checkBox9.Checked = bool.Parse(FileSystem.ReadIniFile("本地缓存", checkBox9.Text, "False", iniPath));
            checkBox51.Checked = bool.Parse(FileSystem.ReadIniFile("本地缓存", checkBox51.Text, "True", iniPath));
            radioButton3.Checked = bool.Parse(FileSystem.ReadIniFile("车头检测", radioButton3.Text, "False", iniPath));
            radioButton4.Checked = bool.Parse(FileSystem.ReadIniFile("车头检测", radioButton4.Text, "True", iniPath));
            radioButton5.Checked = bool.Parse(FileSystem.ReadIniFile("车头检测", radioButton5.Text, "False", iniPath));
            radioButton6.Checked = bool.Parse(FileSystem.ReadIniFile("车头检测", radioButton6.Text, "False", iniPath));
            radioButton7.Checked = bool.Parse(FileSystem.ReadIniFile("车头检测", radioButton7.Text, "True", iniPath));
            radioButton8.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", radioButton8.Text, "True", iniPath));
            radioButton9.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", radioButton9.Text, "False", iniPath));
            checkBox10.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox10.Text, "False", iniPath));
            checkBox11.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox11.Text, "False", iniPath));
            checkBox12.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox12.Text, "False", iniPath));
            checkBox13.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox13.Text, "False", iniPath));
            checkBox14.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox14.Text, "False", iniPath));
            checkBox15.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox15.Text, "False", iniPath));
            checkBox16.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox16.Text, "False", iniPath));
            checkBox17.Checked = bool.Parse(FileSystem.ReadIniFile("线阵", checkBox17.Text, "False", iniPath));
            radioButton10.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", radioButton10.Text, "False", iniPath));
            radioButton11.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", radioButton11.Text, "True", iniPath));
            checkBox18.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox18.Text, "True", iniPath));
            checkBox19.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox19.Text, "True", iniPath));
            checkBox20.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox20.Text, "True", iniPath));
            checkBox21.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox21.Text, "True", iniPath));
            checkBox22.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox22.Text, "True", iniPath));
            checkBox23.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox23.Text, "True", iniPath));
            checkBox24.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox24.Text, "True", iniPath));
            checkBox25.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox25.Text, "True", iniPath));
            checkBox26.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox26.Text, "True", iniPath));
            checkBox27.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox27.Text, "True", iniPath));
            checkBox28.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox28.Text, "True", iniPath));
            checkBox29.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox29.Text, "True", iniPath));
            checkBox30.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox30.Text, "True", iniPath));
            checkBox31.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox31.Text, "True", iniPath));
            checkBox32.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox32.Text, "True", iniPath));
            checkBox33.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox33.Text, "True", iniPath));
            checkBox34.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox34.Text, "True", iniPath));
            checkBox35.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox35.Text, "True", iniPath));
            checkBox36.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox36.Text, "True", iniPath));
            checkBox52.Checked = bool.Parse(FileSystem.ReadIniFile("面阵", checkBox52.Text, "True", iniPath));
            radioButton12.Checked = bool.Parse(FileSystem.ReadIniFile("模式开关", radioButton12.Text, "True", iniPath));
            radioButton13.Checked = bool.Parse(FileSystem.ReadIniFile("模式开关", radioButton13.Text, "False", iniPath));
            checkBox37.Checked = bool.Parse(FileSystem.ReadIniFile("模式开关", checkBox37.Text, "False", iniPath));
            checkBox38.Checked = bool.Parse(FileSystem.ReadIniFile("模式开关", checkBox38.Text, "False", iniPath));
            checkBox39.Checked = bool.Parse(FileSystem.ReadIniFile("模式开关", checkBox39.Text, "False", iniPath));
            checkBox40.Checked = bool.Parse(FileSystem.ReadIniFile("设备开关", checkBox40.Text, "True", iniPath));
            checkBox41.Checked = bool.Parse(FileSystem.ReadIniFile("设备开关", checkBox41.Text, "True", iniPath));
            checkBox42.Checked = bool.Parse(FileSystem.ReadIniFile("设备开关", checkBox42.Text, "True", iniPath));
            checkBox43.Checked = bool.Parse(FileSystem.ReadIniFile("设备开关", checkBox43.Text, "True", iniPath));
            checkBox44.Checked = bool.Parse(FileSystem.ReadIniFile("通信开关", checkBox44.Text, "True", iniPath));
            checkBox45.Checked = bool.Parse(FileSystem.ReadIniFile("通信开关", checkBox45.Text, "True", iniPath));
            checkBox46.Checked = bool.Parse(FileSystem.ReadIniFile("通信开关", checkBox46.Text, "True", iniPath));
            checkBox47.Checked = bool.Parse(FileSystem.ReadIniFile("功能开关", checkBox47.Text, "False", iniPath));
            checkBox48.Checked = bool.Parse(FileSystem.ReadIniFile("功能开关", checkBox48.Text, "True", iniPath));
            checkBox49.Checked = bool.Parse(FileSystem.ReadIniFile("功能开关", checkBox49.Text, "True", iniPath));
            checkBox50.Checked = bool.Parse(FileSystem.ReadIniFile("功能开关", checkBox50.Text, "False", iniPath));
            checkBox53.Checked = bool.Parse(FileSystem.ReadIniFile("功能开关", checkBox53.Text, "False", iniPath));
            if (string.IsNullOrEmpty(tb_head.Text))
            {
                tb_head.Text = FileSystem.ReadIniFile("数据", "车头设置", "0", iniPath);
                ViewModel.HomePageViewModel.trainHeadLocation = int.Parse(tb_head.Text); 
            }
        }

        public void SetEnable(bool isEnable)
        {
            this.isEnable = isEnable;
        }

        public void SetHead(int head)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() => { tb_head.Text = head.ToString(); }));
                }
            }
            catch (Exception) { }
        }

        public void SetOnOff(bool on)
        {
            controlEnableTmp = on;
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            SetConfig();
        }

        private void TestForm_Shown(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveConfig();
        }

        private void tb_head_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            InputForm input = new InputForm() { HeadLocation = int.Parse(tb_head.Text) };
            input.ShowDialog(this);
            tb_head.Text = input.HeadLocation.ToString();
            ViewModel.HomePageViewModel.trainHeadLocation = input.HeadLocation;
            SaveConfig();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            int index = 0, i = 0;
            string sn = FileSystem.ReadIniFile("车型车号", "车号", "0", iniPath);
            if (modeSn.ContainsKey(comboBox1.Text))
            {
                foreach (string item in modeSn[comboBox1.Text])
                {
                    comboBox2.Items.Add(item);
                    if (item == sn)
                    {
                        index = i;
                    }
                    i++;
                }
                comboBox2.SelectedIndex = index;
            }
        }

        private void control_Enter(object sender, EventArgs e)
        {
            if (sender == checkBox6)
            {
                toolTip1.Show("清理每次流程存储下来的照片", checkBox6);
            }
            else if (sender == checkBox7)
            {
                toolTip1.Show("清理本地测试用的照片", checkBox7);
            }
            else if (sender == checkBox8)
            {
                toolTip1.Show("清理部分功能备份用的照片", checkBox8);
            }
            else if (sender == checkBox9)
            {
                toolTip1.Show("清理在记录速度模式下的照片", checkBox9);
            }
            else if (sender == checkBox26)
            {
                toolTip1.Show("完整2D相机拍照流程（包含：机械臂，光源和拍照）", checkBox26);
            }
            else if (sender == checkBox27)
            {
                toolTip1.Show("完整油位窗拍照流程（包含：机械臂，滑台，光源和拍照）", checkBox27);
            }
            else if (sender == checkBox28)
            {
                toolTip1.Show("完整2D相机拍照流程（包含：机械臂，滑台和拍照）", checkBox28);
            }
            else if (sender == checkBox29)
            {
                toolTip1.Show("仅控制前机械臂运动", checkBox29);
            }
            else if (sender == checkBox30)
            {
                toolTip1.Show("仅控制后机械臂运动", checkBox30);
            }
            else if (sender == checkBox31)
            {
                toolTip1.Show("仅控制前滑台运动", checkBox31);
            }
            else if (sender == checkBox32)
            {
                toolTip1.Show("仅控制后滑台运动", checkBox32);
            }
            else if (sender == checkBox33)
            {
                toolTip1.Show("仅控制前2D和3D相机拍照", checkBox33);
            }
            else if (sender == checkBox34)
            {
                toolTip1.Show("仅控制后2D和3D相机拍照", checkBox34);
            }
            else if (sender == checkBox45)
            {
                toolTip1.Show("上传图片（线阵、面阵）", checkBox45);
            }
            else if (sender == checkBox46)
            {
                toolTip1.Show("上传至业务端（线阵、面阵、3D数据）", checkBox46);
            }
            else if (sender == radioButton13)
            {
                toolTip1.Show("使用本地（test_data）图片上传（假拍照）", radioButton13);
            }
        }

        private void control_Leave(object sender, EventArgs e)
        {
            toolTip1.Hide(this);
        }

        private void opend_CheckedChanged(object sender, EventArgs e)
        {
            #region 流程开关
            if (sender == checkBox1)
            {
                clearLocalImage = groupBox3.Enabled = checkBox1.Checked;
            }
            else if (sender == checkBox2)
            {
                checkHead = groupBox4.Enabled = checkBox2.Checked;
            }
            else if (sender == checkBox3)
            {
                runXz = groupBox6.Enabled = checkBox3.Checked;
            }
            else if (sender == checkBox4)
            {
                runBack = groupBox2.Enabled = groupBox8.Enabled = checkBox4.Checked;
            }
            else if (sender == radioButton1)
            {
                runBackXz = radioButton1.Checked;
            }
            else if (sender == radioButton2)
            {
                runBackMz = groupBox8.Enabled = radioButton2.Checked;
            }
            #endregion
            #region 本地缓存
            else if (sender == checkBox5)
            {
                clearLog = checkBox5.Checked;
            }
            else if (sender == checkBox6)
            {
                clearTask = checkBox6.Checked;
            }
            else if (sender == checkBox7)
            {
                clearTest = checkBox7.Checked;
            }
            else if (sender == checkBox8)
            {
                clearWork = checkBox8.Checked;
            }
            else if (sender == checkBox9)
            {
                clearOrc = checkBox9.Checked;
            }
            else if (sender == checkBox51)
            {
                clear3D = checkBox51.Checked;
            }
            #endregion
            #region 车头检测
            else if (sender == radioButton3)
            {
                laser = groupBox5.Enabled = radioButton3.Checked;
            }
            else if (sender == radioButton4)
            {
                laserNormal = radioButton4.Checked;
            }
            else if (sender == radioButton5)
            {
                laserSlow = radioButton5.Checked;
            }
            else if (sender == radioButton6)
            {
                io = radioButton6.Checked;
            }
            else if (sender == radioButton7)
            {
                radar = radioButton7.Checked;
            }
            #endregion
            #region 线阵
            else if (sender == checkBox10)
            {
                xzLoc[0] = checkBox10.Checked;
            }
            else if (sender == checkBox11)
            {
                xzLoc[1] = checkBox11.Checked;
            }
            else if (sender == checkBox12)
            {
                xzLoc[2] = checkBox12.Checked;
            }
            else if (sender == checkBox13)
            {
                xzLoc[3] = checkBox13.Checked;
            }
            else if (sender == checkBox14)
            {
                xzLoc[4] = checkBox14.Checked;
            }
            else if (sender == checkBox15)
            {
                xzLoc[5] = checkBox15.Checked;
            }
            else if (sender == checkBox16)
            {
                xzLoc[6] = checkBox16.Checked;
            }
            else if (sender == checkBox17)
            {
                xzLoc[7] = checkBox17.Checked;
            }
            else if (sender == radioButton8)
            {
                xzAll = radioButton8.Checked;
            }
            else if (sender == radioButton9)
            {
                xzPart = groupBox7.Enabled = radioButton9.Checked;
            }
            #endregion
            #region 面阵
            else if (sender == checkBox18)
            {
                mzLoc[7] = checkBox18.Checked;
            }
            else if (sender == checkBox19)
            {
                mzLoc[6] = checkBox19.Checked;
            }
            else if (sender == checkBox20)
            {
                mzLoc[5] = checkBox20.Checked;
            }
            else if (sender == checkBox21)
            {
                mzLoc[4] = checkBox21.Checked;
            }
            else if (sender == checkBox22)
            {
                mzLoc[3] = checkBox22.Checked;
            }
            else if (sender == checkBox23)
            {
                mzLoc[2] = checkBox23.Checked;
            }
            else if (sender == checkBox24)
            {
                mzLoc[1] = checkBox24.Checked;
            }
            else if (sender == checkBox25)
            {
                mzLoc[0] = checkBox25.Checked;
            }
            else if (sender == checkBox26)
            {
                mzType[0] = checkBox26.Checked;
            }
            else if (sender == checkBox27)
            {
                mzType[1] = checkBox27.Checked;
            }
            else if (sender == checkBox28)
            {
                mzType[2] = checkBox28.Checked;
            }
            else if (sender == checkBox29)
            {
                robot[0] = checkBox29.Checked;
            }
            else if (sender == checkBox30)
            {
                robot[1] = checkBox30.Checked;
            }
            else if (sender == checkBox31)
            {
                sliding[0] = checkBox31.Checked;
            }
            else if (sender == checkBox32)
            {
                sliding[1] = checkBox32.Checked;
            }
            else if (sender == checkBox33)
            {
                camera[0] = checkBox33.Checked;
            }
            else if (sender == checkBox34)
            {
                camera[1] = checkBox34.Checked;
            }
            else if (sender == checkBox35)
            {
                checkBox36.Checked = !checkBox35.Checked;
            }
            else if (sender == checkBox36)
            {
                checkBox35.CheckedChanged -= opend_CheckedChanged;
                checkBox35.Checked = !checkBox36.Checked;
                checkBox35.CheckedChanged += opend_CheckedChanged;
                asynchronous = checkBox36.Checked;
            }
            else if (sender == radioButton10)
            {
                mzPart = groupBox9.Enabled = radioButton10.Checked;
            }
            else if (sender == radioButton11)
            {
                mzAll = radioButton11.Checked;
            }
            else if (sender == checkBox52)
            {
                location = checkBox52.Checked;
            }
            #endregion
            #region 模式开关
            else if (sender == checkBox37)
            {
                speedMode = checkBox38.Enabled = checkBox37.Checked;
            }
            else if (sender == checkBox38)
            {
                zoom = checkBox38.Checked;
            }
            else if (sender == checkBox39)
            {
                join = checkBox47.Enabled = checkBox39.Checked;
            }
            else if (sender == radioButton12)
            {
                normal = radioButton12.Checked;
            }
            else if (sender == radioButton13)
            {
                simulation = radioButton13.Checked;
            }
            #endregion
            #region 设备开关
            else if (sender == checkBox40)
            {
                rgv = checkBox40.Checked;
            }
            else if (sender == checkBox41)
            {
                stm32 = checkBox41.Checked;
            }
            else if (sender == checkBox42)
            {
                plc = checkBox42.Checked;
            }
            else if (sender == checkBox43)
            {
                light = checkBox43.Checked;
            }
            #endregion
            #region 通信开关
            else if (sender == checkBox44)
            {
                socket = checkBox44.Checked;
            }
            else if (sender == checkBox45)
            {
                wcf = checkBox45.Checked;
            }
            else if (sender == checkBox46)
            {
                http = checkBox46.Checked;
            }
            #endregion
            #region 功能开关
            else if (sender == checkBox47)
            {
                zip = checkBox47.Checked;
            }
            else if (sender == checkBox48)
            {
                outTime = checkBox48.Checked;
            }
            else if (sender == checkBox49)
            {
                xzSingle = checkBox49.Checked;
            }
            else if (sender == checkBox50)
            {
                xzDouble = checkBox50.Checked;
            }
            else if (sender == checkBox53)
            {
                savePointPic = checkBox53.Checked;
            }
            #endregion
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Inspect != null && !Inspect())
            {
                if (MessageBox.Show("是否重新初始化硬件设备？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    InitAct?.Invoke();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    this.Invoke(new Action(() =>
                    {
                        AppRemoteCtrl_TrainPara obj = new AppRemoteCtrl_TrainPara()
                        {
                            robot_id = Properties.Settings.Default.RobotID,
                            train_mode = comboBox1.Text,
                            train_sn = comboBox2.Text
                        };
                        if (button2.Text == "终止运行")
                        {
                            Stop?.Invoke(obj);
                            ThreadManager.ThreadSleep(1000);
                            Stop?.Invoke(obj);
                            SetOnOff(true);
                        }
                        else
                        {
                            SetOnOff(false);
                            ViewModel.HomePageViewModel.socketRunStart = false;
                            Run?.Invoke(obj, () =>
                            {
                                try
                                {
                                    SetOnOff(true);
                                }
                                catch (Exception) { }
                            });
                        }
                    }));
                }
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SpeedCorrect?.Invoke();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            button2.Enabled = isEnable;
            ThreadManager.ThreadStart(() =>
            {
                int[] imgIndexs = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
                if (GetState != null)
                {
                    for (int i = 0; i < imgIndexs.Length; i++)
                    {
                        imgIndexs[i] = GetState(i) ? 0 : 1;
                    }
                }
                try
                {
                    if (!this.IsDisposed)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            if (controlEnable != controlEnableTmp)
                            {
                                controlEnable = controlEnableTmp;
                                ChangeEnable();
                            }
                            if (GetSpeed != null)
                            {
                                tb_speed.Text = GetSpeed().ToString();
                            }
                            label3.ImageIndex = imgIndexs[0];
                            label4.ImageIndex = imgIndexs[1];
                            label5.ImageIndex = imgIndexs[2];
                            label6.ImageIndex = imgIndexs[3];
                            label7.ImageIndex = imgIndexs[4];
                            label8.ImageIndex = imgIndexs[5];
                            label9.ImageIndex = imgIndexs[6];
                            label10.ImageIndex = imgIndexs[7];
                            label11.ImageIndex = imgIndexs[8];
                            label12.ImageIndex = imgIndexs[9];
                            label13.ImageIndex = imgIndexs[10];
                            label14.ImageIndex = imgIndexs[11];
                            label15.ImageIndex = imgIndexs[12];
                            label17.ImageIndex = imgIndexs[13];
                            label18.ImageIndex = imgIndexs[14];
                            timer1.Enabled = true;
                        }));
                    }
                }
                catch (Exception) { }
            });
        }

        private void ChangeEnable(Control control = null)
        {
            if (control == null)
            {
                control = flowLayoutPanel1;
            }
            foreach (Control item in control.Controls)
            {
                if (item is CheckBox || item is RadioButton)
                {
                    item.Enabled = controlEnable;
                }
                else if (item == button2)
                {
                    if (controlEnable)
                    {
                        item.Text = "根据设置\r\n\r\n运行流程";
                    }
                    else
                    {
                        item.Text = "终止运行";
                    }
                }
                else
                {
                    ChangeEnable(item);
                }
            }
        }

        private void SaveConfig()
        {
            FileSystem.WriteIniFile("流程开关", checkBox1.Text, checkBox1.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("流程开关", checkBox2.Text, checkBox2.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("流程开关", checkBox3.Text, checkBox3.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("流程开关", checkBox4.Text, checkBox4.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("流程开关", radioButton1.Text, radioButton1.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("流程开关", radioButton2.Text, radioButton2.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("本地缓存", checkBox5.Text, checkBox5.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("本地缓存", checkBox6.Text, checkBox6.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("本地缓存", checkBox7.Text, checkBox7.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("本地缓存", checkBox8.Text, checkBox8.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("本地缓存", checkBox9.Text, checkBox9.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("本地缓存", checkBox51.Text, checkBox51.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("车头检测", radioButton3.Text, radioButton3.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("车头检测", radioButton4.Text, radioButton4.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("车头检测", radioButton5.Text, radioButton5.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("车头检测", radioButton6.Text, radioButton6.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("车头检测", radioButton7.Text, radioButton7.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", radioButton8.Text, radioButton8.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", radioButton9.Text, radioButton9.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox10.Text, checkBox10.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox11.Text, checkBox11.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox12.Text, checkBox12.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox13.Text, checkBox13.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox14.Text, checkBox14.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox15.Text, checkBox15.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox16.Text, checkBox16.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("线阵", checkBox17.Text, checkBox17.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", radioButton10.Text, radioButton10.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", radioButton11.Text, radioButton11.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox18.Text, checkBox18.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox19.Text, checkBox19.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox20.Text, checkBox20.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox21.Text, checkBox21.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox22.Text, checkBox22.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox23.Text, checkBox23.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox24.Text, checkBox24.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox25.Text, checkBox25.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox26.Text, checkBox26.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox27.Text, checkBox27.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox28.Text, checkBox28.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox29.Text, checkBox29.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox30.Text, checkBox30.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox31.Text, checkBox31.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox32.Text, checkBox32.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox33.Text, checkBox33.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox34.Text, checkBox34.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox35.Text, checkBox35.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox36.Text, checkBox36.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("面阵", checkBox52.Text, checkBox52.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("模式开关", radioButton12.Text, radioButton12.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("模式开关", radioButton13.Text, radioButton13.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("模式开关", checkBox37.Text, checkBox37.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("模式开关", checkBox38.Text, checkBox38.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("模式开关", checkBox39.Text, checkBox39.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("设备开关", checkBox40.Text, checkBox40.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("设备开关", checkBox41.Text, checkBox41.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("设备开关", checkBox42.Text, checkBox42.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("设备开关", checkBox43.Text, checkBox43.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("通信开关", checkBox44.Text, checkBox44.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("通信开关", checkBox45.Text, checkBox45.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("通信开关", checkBox46.Text, checkBox46.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("功能开关", checkBox47.Text, checkBox47.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("功能开关", checkBox48.Text, checkBox48.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("功能开关", checkBox49.Text, checkBox49.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("功能开关", checkBox50.Text, checkBox50.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("功能开关", checkBox53.Text, checkBox53.Checked.ToString(), iniPath);
            FileSystem.WriteIniFile("车型车号", "车型", comboBox1.Text, iniPath);
            FileSystem.WriteIniFile("车型车号", "车号", comboBox2.Text, iniPath);
            FileSystem.WriteIniFile("数据", "车头设置", ViewModel.HomePageViewModel.trainHeadLocation.ToString(), iniPath);
        }
    }

    public class TrainLocation
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    public enum EnableEnum
    {
        清理日志, 清理Task, 清理Test, 清理Work, 清理Orc, 清理3D,
        车头检测, 激光测车头, 激光慢速测车头, 传感器测车头, 雷达测车头,
        执行线阵, 完整线阵, 部分线阵, 返回线阵,
        完整面阵, 部分面阵, 面阵相机, 油位窗, 三维相机, 前机械臂, 后机械臂, 前滑台, 后滑台, 前相机, 后相机, 同步, 异步, 轴定位,
        正常流程, 模拟流程, 记录速度, 拉伸速度图片, 拼图,
        RGV, Stm32, PLC, 光源,
        Socket, 传图服务, 业务接口,
        压缩, 超时, 单线阵, 双线阵, 点云图
    }
}

using CognexLib;
using PcMainCtrl.DataAccess;
using PcMainCtrl.HardWare;
using PcMainCtrl.HardWare.Rgv;
using PcMainCtrl.HardWare.Robot;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.Robot.RobotModCtrlProtocol;

namespace PcMainCtrl.Form
{
    public partial class SaveRobotPoint : UserControl
    {
        private bool frontLight = false, backLight = false;
        private int selectedIndex = -1;
        private DataMzCameraLineModel selected = null;
        private List<DataMzCameraLineModelEx> models = null;
        public int HeadLocation { private get; set; } = 0;
        public PLC3DCamera PLC3DCamera { private get; set; }
        public LightManager Light { private get; set; }

        public SaveRobotPoint()
        {
            InitializeComponent();
        }

        private void SaveRobotPoint_Load(object sender, EventArgs e)
        {
            TaskRun(() =>
            {
                while (true)
                {
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            lb_runLocation.Text = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce.ToString();
                            lb_speed.Text = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunSpeed.ToString();
                            lb_power.Text = RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentPowerElectricity.ToString();
                            lb_headLength.Text = (RgvModCtrlHelper.GetInstance().myRgvGlobalInfo.RgvCurrentRunDistacnce - HeadLocation).ToString();
                        }));
        }
                    catch (Exception)
                    {
                        break;
                    }
                    ThreadSleep(1000);
                }
            });

            lb_headLocation.Text = HeadLocation.ToString();
            Dictionary<int, string> xieyiDict = new Manager().xieyiDict;
            foreach (KeyValuePair<int, string> item in xieyiDict)
            {
                string s = item.Key + ":" + item.Value;
                cb_back.Items.Add(s);
                cb_front.Items.Add(s);
            }
            dataGridView1.DataSource = models = LocalDataBase.GetInstance().MzCameraDataListQurey();
        }

        public void RobotModInfoEvent(RobotGlobalInfo robotinfo, bool isFront)
        {
            BeginInvoke(new Action(() =>
            {
                if (isFront)
                {
                    lb_front_j1.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotSiteData.j1;
                    lb_front_j2.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotSiteData.j2;
                    lb_front_j3.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotSiteData.j3;
                    lb_front_j4.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotSiteData.j4;
                    lb_front_j5.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotSiteData.j5;
                    lb_front_j6.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotSiteData.j6;
                }
                else
                {
                    lb_back_j1.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotSiteData.j1;
                    lb_back_j2.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotSiteData.j2;
                    lb_back_j3.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotSiteData.j3;
                    lb_back_j4.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotSiteData.j4;
                    lb_back_j5.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotSiteData.j5;
                    lb_back_j6.Text = RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotSiteData.j6;
                }
            }));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tb_front_j1.Text = lb_front_j1.Text;
            tb_front_j2.Text = lb_front_j2.Text;
            tb_front_j3.Text = lb_front_j3.Text;
            tb_front_j4.Text = lb_front_j4.Text;
            tb_front_j5.Text = lb_front_j5.Text;
            tb_front_j6.Text = lb_front_j6.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tb_back_j1.Text = lb_back_j1.Text;
            tb_back_j2.Text = lb_back_j2.Text;
            tb_back_j3.Text = lb_back_j3.Text;
            tb_back_j4.Text = lb_back_j4.Text;
            tb_back_j5.Text = lb_back_j5.Text;
            tb_back_j6.Text = lb_back_j6.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < models.Count; i++)
                {
                    models[i].DataLine_Index = i;
                    models[i].Rgv_Enable = 1;
                    models[i].FrontCamera_Id = 3;
                    models[i].FrontCamera_Enable = 1;
                    models[i].FrontComponentId = "1";
                    models[i].BackCamera_Id = 4;
                    models[i].BackCamera_Enable = 1;
                    models[i].BackComponentId = "2";
                }
                LocalDataBase.GetInstance().MzCameraDataListSave(models);
                MessageBox.Show("保存完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            models.Add(new DataMzCameraLineModelEx());
            dataGridView1.DataSource = models;
            dataGridView1.ClearSelection();
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
            selectedIndex = dataGridView1.Rows.Count - 1;
            selected = models[models.Count - 1];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selected != null && selectedIndex > -1)
            {
                dataGridView1.DataSource = null;
                models.Insert(selectedIndex, new DataMzCameraLineModelEx()
                {
                    BackCamera_Enable = selected.BackCamera_Enable,
                    BackCamera_Id = selected.BackCamera_Id,
                    BackComponentId = selected.BackComponentId,
                    BackComponentType = selected.BackComponentType,
                    BackRobot_Enable = selected.BackRobot_Enable,
                    BackRobot_Id = selected.BackRobot_Id,
                    BackRobot_J1 = selected.BackRobot_J1,
                    BackRobot_J2 = selected.BackRobot_J2,
                    BackRobot_J3 = selected.BackRobot_J3,
                    BackRobot_J4 = selected.BackRobot_J4,
                    BackRobot_J5 = selected.BackRobot_J5,
                    BackRobot_J6 = selected.BackRobot_J6,
                    Back_Parts_Id = selected.Back_Parts_Id,
                    DataLine_Index = selected.DataLine_Index,
                    FrontCamera_Enable = selected.FrontCamera_Enable,
                    FrontCamera_Id = selected.FrontCamera_Id,
                    FrontComponentId = selected.FrontComponentId,
                    FrontComponentType = selected.FrontComponentType,
                    FrontRobot_Enable = selected.FrontRobot_Enable,
                    FrontRobot_Id = selected.FrontRobot_Id,
                    FrontRobot_J1 = selected.FrontRobot_J1,
                    FrontRobot_J2 = selected.FrontRobot_J2,
                    FrontRobot_J3 = selected.FrontRobot_J3,
                    FrontRobot_J4 = selected.FrontRobot_J4,
                    FrontRobot_J5 = selected.FrontRobot_J5,
                    FrontRobot_J6 = selected.FrontRobot_J6,
                    Front_Parts_Id = selected.Front_Parts_Id,
                    Rgv_Distance = selected.Rgv_Distance,
                    Rgv_Enable = selected.Rgv_Enable,
                    Rgv_Id = selected.Rgv_Id,
                    TrainModel = selected.TrainModel,
                    TrainSn = selected.TrainSn
                });
                dataGridView1.DataSource = models;
                dataGridView1.ClearSelection();
                dataGridView1.Rows[selectedIndex].Selected = true;
                selected = models[selectedIndex];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedIndex > -1)
            {
                models.RemoveAt(selectedIndex);
                selected = null;
                selectedIndex = -1;

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = models;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.FRONT_ROBOT_DEVICE_ID, RobotData.RobotGetDataFunCode, new RobotDataPack());
            RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(RobotModCtrlHelper.BACK_ROBOT_DEVICE_ID, RobotData.RobotGetDataFunCode, new RobotDataPack());
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (frontLight)
            {
                Light.LightOff(true);
                button8.Text = "前光源ON";
            }
            else
            {
                Light.LightOn(Properties.Settings.Default.LightFrontHigh, true);
                button8.Text = "前光源OFF";
            }
            frontLight = !frontLight;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (backLight)
            {
                Light.LightOff(false);
                button9.Text = "后光源ON";
            }
            else
            {
                Light.LightOn(Properties.Settings.Default.LightBackHigh, false);
                button9.Text = "后光源OFF";
            }
            backLight = !backLight;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            PLC3DCamera.SetForward(RobotName.Front);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            PLC3DCamera.SetForward(RobotName.Back);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            PLC3DCamera.SetBackoff(RobotName.Front);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            PLC3DCamera.SetBackoff(RobotName.Back);
        }

        private void tb_TextChanged(object sender, EventArgs e)
        {
            SetModel();
        }

        private void cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetModel();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedIndex = e.RowIndex;
            if (e.RowIndex > -1)
            {
                DataMzCameraLineModel model = selected = models[e.RowIndex];
                tb_back_j1.Text = model.BackRobot_J1;
                tb_back_j2.Text = model.BackRobot_J2;
                tb_back_j3.Text = model.BackRobot_J3;
                tb_back_j4.Text = model.BackRobot_J4;
                tb_back_j5.Text = model.BackRobot_J5;
                tb_back_j6.Text = model.BackRobot_J6;
                tb_front_j1.Text = model.FrontRobot_J1;
                tb_front_j2.Text = model.FrontRobot_J2;
                tb_front_j3.Text = model.FrontRobot_J3;
                tb_front_j4.Text = model.FrontRobot_J4;
                tb_front_j5.Text = model.FrontRobot_J5;
                tb_front_j6.Text = model.FrontRobot_J6;
                tb_id.Text = model.Rgv_Id.ToString();
                tb_location.Text = model.Rgv_Distance.ToString();
                tb_model.Text = model.TrainModel;
                tb_sn.Text = model.TrainSn;
                cb_back.SelectedIndex = int.Parse(model.Back_Parts_Id);
                cb_back_camera.SelectedIndex = model.BackComponentType == "Mz" ? 0 : 1;
                cb_front.SelectedIndex = int.Parse(model.Front_Parts_Id);
                cb_front_camera.SelectedIndex = model.FrontComponentType == "Mz" ? 0 : 1;
            }
        }

        private void SetModel()
        {
            if (selected != null)
            {
                selected.BackComponentType = cb_back_camera.SelectedIndex == 0 ? "Mz" : "3d";
                selected.BackRobot_J1 = tb_back_j1.Text;
                selected.BackRobot_J2 = tb_back_j2.Text;
                selected.BackRobot_J3 = tb_back_j3.Text;
                selected.BackRobot_J4 = tb_back_j4.Text;
                selected.BackRobot_J5 = tb_back_j5.Text;
                selected.BackRobot_J6 = tb_back_j6.Text;
                if (!string.IsNullOrEmpty(cb_back.Text))
                {
                    selected.Back_Parts_Id = cb_back.Text.Split(':')[0]; 
                }
                selected.FrontComponentType = cb_front_camera.SelectedIndex == 0 ? "Mz" : "3d";
                selected.FrontRobot_J1 = tb_front_j1.Text;
                selected.FrontRobot_J2 = tb_front_j2.Text;
                selected.FrontRobot_J3 = tb_front_j3.Text;
                selected.FrontRobot_J4 = tb_front_j4.Text;
                selected.FrontRobot_J5 = tb_front_j5.Text;
                selected.FrontRobot_J6 = tb_front_j6.Text;
                if (!string.IsNullOrEmpty(cb_front.Text))
                {
                    selected.Front_Parts_Id = cb_front.Text.Split(':')[0]; 
                }
                try
                {
                    selected.Rgv_Distance = int.Parse(tb_location.Text);
                }
                catch (Exception) { }
                try
                {
                    selected.Rgv_Id = int.Parse(tb_id.Text);
                }
                catch (Exception) { }
                selected.TrainModel = tb_model.Text;
                selected.TrainSn = tb_sn.Text;

                dataGridView1.Refresh();
            }
        }
    }
}

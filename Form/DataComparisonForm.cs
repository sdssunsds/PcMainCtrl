using PcMainCtrl.DataAccess;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PcMainCtrl.Form
{
    public partial class DataComparisonForm : System.Windows.Forms.Form
    {
        private string path = "";
        private LocalDataBase dataBase = new LocalDataBase();
        private List<DataMzCameraLineModelEx> mzList = null;
        private List<DataMzCameraLineModelEx> updList = new List<DataMzCameraLineModelEx>();
        private Dictionary<int, int> axDict = new Dictionary<int, int>();

        public DataComparisonForm()
        {
            InitializeComponent();
        }

        private void DataComparisonForm_Load(object sender, EventArgs e)
        {
            dgv.AutoGenerateColumns = false;
            path = Application.StartupPath + "\\json\\";
            string[] dirs = Directory.GetDirectories(path);
            foreach (string item in dirs)
            {
                cb_mode_sn.Items.Add(item.Substring(item.LastIndexOf("\\") + 1));
            }
        }

        private void btn_distance_Click(object sender, EventArgs e)
        {
            foreach (DataMzCameraLineModelEx item in updList)
            {
                int axDistance = !axDict.ContainsKey(item.AxisID) ? 0 : axDict[item.AxisID];
                item.Rgv_Distance = axDistance + item.Axis_Distance;
            }
            dataBase.MzCameraDataListSave(mzList);
        }

        private void cb_mode_sn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_mode_sn.SelectedIndex > -1 && IsComparison())
            {
                btn_distance.Enabled = Comparison();
            }
        }

        private bool Comparison()
        {
            axDict.Clear();
            dgv.Rows.Clear();
            mzList?.Clear();
            updList.Clear();

            dataBase.jsonPath = path + cb_mode_sn.SelectedItem.ToString() + "\\";
            List<Axis> axes = dataBase.AxesDataListQurey();
            mzList = dataBase.MzCameraDataListQurey();
            foreach (Axis item in axes)
            {
                axDict.Add(item.ID, item.Distance);
            }

            foreach (DataMzCameraLineModelEx item in mzList)
            {
                int axDistance = !axDict.ContainsKey(item.AxisID) ? 0 : axDict[item.AxisID];
                string s = !axDict.ContainsKey(item.AxisID) ? "缺失" : axDict[item.AxisID].ToString();
                if (!axDict.ContainsKey(item.AxisID) || item.Rgv_Distance != axDict[item.AxisID] + item.Axis_Distance)
                {
                    dgv.Rows.Add(item.FrontComponentId, "前臂", item.Rgv_Distance, s, item.Axis_Distance, Math.Abs(item.Rgv_Distance - item.Axis_Distance - axDistance));
                    dgv.Rows.Add(item.BackComponentId, "后臂", item.Rgv_Distance, s, item.Axis_Distance, Math.Abs(item.Rgv_Distance - item.Axis_Distance - axDistance));
                    updList.Add(item);
                }
            }
            return dgv.Rows.Count > 0;
        }

        private bool IsComparison()
        {
            return cb_distance.Checked;
        }
    }
}

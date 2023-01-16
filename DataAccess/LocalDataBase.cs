#define saveJson

using Newtonsoft.Json;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace PcMainCtrl.DataAccess
{
    public class LocalDataBase
    {
#if saveJson
        public string jsonPath = System.Windows.Forms.Application.StartupPath + "/json/" + Properties.Settings.Default.TrainMode + "_" + Properties.Settings.Default.TrainSn + "/";
        private object jsonLock = new object();
        private List<DataMzCameraLineModelEx> dataMzCameraLineModelList = null;
        private List<DataXzCameraLineModel> dataXzCameraLineModelList = null;
#endif

        private static LocalDataBase instance;
        private LocalDataBase() { }

        public static LocalDataBase GetInstance()
        {
            return instance ?? (instance = new LocalDataBase());
        }

        SqlConnection conn;
        SqlCommand comm;
        SqlDataAdapter adapter;

        private void Dispose()
        {
            if (adapter != null)
            {
                adapter.Dispose();
                adapter = null;
            }
            if (comm != null)
            {
                comm.Dispose();
                comm = null;
            }
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
        }

        private bool DBConnection()
        {
            string connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
            if (conn == null)
                conn = new SqlConnection(connStr);
            try
            {
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------
        //RobotTest
        #region
        /// <summary>
        /// 数据库添加点位数据行
        /// </summary>
        /// <param name="robot_dataline"></param>
        public void RobotTestDataSave(DeviceRobotDataLineModel robot_dataline)
        {
            try
            {
                if (DBConnection())
                {
                    string userSql = @"insert into RobotTestDataTable
                                    (discrption ,
                                    front_robot_devid, front_robot_enable,
                                    front_robot_j1, front_robot_j2, front_robot_j3, front_robot_j4, front_robot_j5, front_robot_j6,
                                    back_robot_devid, back_robot_enable,
                                    back_robot_j1, back_robot_j2, back_robot_j3, back_robot_j4, back_robot_j5, back_robot_j6) 
                                    VALUES 
                                    (@discrption_info ,
                                    @robot1_devid, @robot1_enable,
                                    @robot1_j1,@robot1_j2,@robot1_j3,@robot1_j4,@robot1_j5,@robot1_j6,
                                    @robot2_devid, @robot2_enable,
                                    @robot2_j1,@robot2_j2,@robot2_j3,@robot2_j4,@robot2_j5,@robot2_j6)";

                    adapter = new SqlDataAdapter();
                    comm = new SqlCommand(userSql, conn);

                    //插入数据
                    comm.Parameters.Add(new SqlParameter("@discrption_info", SqlDbType.VarChar) { Value = robot_dataline.DataLine_Discript });

                    comm.Parameters.Add(new SqlParameter("@robot1_devid", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_Id });
                    comm.Parameters.Add(new SqlParameter("@robot1_enable", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_Enable });
                    comm.Parameters.Add(new SqlParameter("@robot1_j1", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_J1 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j2", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_J2 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j3", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_J3 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j4", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_J4 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j5", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_J5 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j6", SqlDbType.VarChar) { Value = robot_dataline.FrontRobot_J6 });

                    comm.Parameters.Add(new SqlParameter("@robot2_devid", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_Id });
                    comm.Parameters.Add(new SqlParameter("@robot2_enable", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_Enable });
                    comm.Parameters.Add(new SqlParameter("@robot2_j1", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_J1 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j2", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_J2 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j3", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_J3 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j4", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_J4 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j5", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_J5 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j6", SqlDbType.VarChar) { Value = robot_dataline.BackRobot_J6 });

                    adapter.InsertCommand = comm;
                    adapter.InsertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// 数据库查询点位数据行
        /// </summary>
        /// <param name="front_robot"></param>
        /// <param name="back_robot"></param>
        public List<DeviceRobotDataLineModel> RobotTestDataQurey()
        {
            try
            {
                List<DeviceRobotDataLineModel> dataline_list = new List<DeviceRobotDataLineModel>();

                if (DBConnection())
                {
                    //查询数据
                    string userSql = @"select * from RobotTestDataTable";
                    adapter = new SqlDataAdapter(userSql, conn);

                    DataTable table = new DataTable();
                    int count = adapter.Fill(table);

                    //处理数据
                    DeviceRobotDataLineModel line = null;
                    foreach (DataRow dr in table.AsEnumerable())
                    {
                        int temp_id = dr.Field<int>("id");
                        string temp_discrption = dr.Field<string>("discrption");

                        string temp_front_robot_devid = dr.Field<string>("front_robot_devid");
                        string temp_front_robot_enable = dr.Field<string>("front_robot_enable");
                        string temp_front_robot_j1 = dr.Field<string>("front_robot_j1");
                        string temp_front_robot_j2 = dr.Field<string>("front_robot_j2");
                        string temp_front_robot_j3 = dr.Field<string>("front_robot_j3");
                        string temp_front_robot_j4 = dr.Field<string>("front_robot_j4");
                        string temp_front_robot_j5 = dr.Field<string>("front_robot_j5");
                        string temp_front_robot_j6 = dr.Field<string>("front_robot_j6");

                        string temp_back_robot_devid = dr.Field<string>("back_robot_devid");
                        string temp_back_robot_enable = dr.Field<string>("back_robot_enable");
                        string temp_back_robot_j1 = dr.Field<string>("back_robot_j1");
                        string temp_back_robot_j2 = dr.Field<string>("back_robot_j2");
                        string temp_back_robot_j3 = dr.Field<string>("back_robot_j3");
                        string temp_back_robot_j4 = dr.Field<string>("back_robot_j4");
                        string temp_back_robot_j5 = dr.Field<string>("back_robot_j5");
                        string temp_back_robot_j6 = dr.Field<string>("back_robot_j6");

                        line = new DeviceRobotDataLineModel();
                        line.DataLine_Index = temp_id;
                        line.DataLine_Discript = temp_discrption;

                        line.FrontRobot_Id = temp_front_robot_devid;
                        line.FrontRobot_Enable = temp_front_robot_enable;

                        line.FrontRobot_J1 = temp_front_robot_j1;
                        line.FrontRobot_J2 = temp_front_robot_j2;
                        line.FrontRobot_J3 = temp_front_robot_j3;
                        line.FrontRobot_J4 = temp_front_robot_j4;
                        line.FrontRobot_J5 = temp_front_robot_j5;
                        line.FrontRobot_J6 = temp_front_robot_j6;

                        line.BackRobot_Id = temp_back_robot_devid;
                        line.BackRobot_Enable = temp_back_robot_enable;

                        line.BackRobot_J1 = temp_back_robot_j1;
                        line.BackRobot_J2 = temp_back_robot_j2;
                        line.BackRobot_J3 = temp_back_robot_j3;
                        line.BackRobot_J4 = temp_back_robot_j4;
                        line.BackRobot_J5 = temp_back_robot_j5;
                        line.BackRobot_J6 = temp_back_robot_j6;

                        dataline_list.Add(line);
                    }
                }

                return dataline_list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------------------
        //MzCameraData
        #region
        /// <summary>
        /// 数据库添加数据行
        /// </summary>
        /// <param name="robot_dataline"></param>
        public void MzCameraDataListSave(DataMzCameraLineModelEx mz_camera_dataline)
        {
#if saveJson
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            if (dataMzCameraLineModelList == null)
            {
                MzCameraDataListQurey();
            }

            if (dataMzCameraLineModelList.Find(d => d.BackRobot_J1 == mz_camera_dataline.BackRobot_J1 &&
                d.BackRobot_J2 == mz_camera_dataline.BackRobot_J2 && d.BackRobot_J3 == mz_camera_dataline.BackRobot_J3 &&
                d.BackRobot_J4 == mz_camera_dataline.BackRobot_J4 && d.BackRobot_J5 == mz_camera_dataline.BackRobot_J5 &&
                d.BackRobot_J6 == mz_camera_dataline.BackRobot_J6 && d.FrontRobot_J1 == mz_camera_dataline.FrontRobot_J1 &&
                d.FrontRobot_J2 == mz_camera_dataline.FrontRobot_J2 && d.FrontRobot_J3 == mz_camera_dataline.FrontRobot_J3 &&
                d.FrontRobot_J4 == mz_camera_dataline.FrontRobot_J4 && d.FrontRobot_J5 == mz_camera_dataline.FrontRobot_J5 &&
                d.FrontRobot_J6 == mz_camera_dataline.FrontRobot_J6 && d.Rgv_Distance == mz_camera_dataline.Rgv_Distance) != null)
            {
                return;
            }

            // 没有前机械臂数据时
            if (string.IsNullOrEmpty(mz_camera_dataline.FrontRobot_J1) &&
                string.IsNullOrEmpty(mz_camera_dataline.FrontRobot_J2) &&
                string.IsNullOrEmpty(mz_camera_dataline.FrontRobot_J3) &&
                string.IsNullOrEmpty(mz_camera_dataline.FrontRobot_J4) &&
                string.IsNullOrEmpty(mz_camera_dataline.FrontRobot_J5) &&
                string.IsNullOrEmpty(mz_camera_dataline.FrontRobot_J6))
            {
                // 仅查找没有后机械臂数据的索引
                int index = dataMzCameraLineModelList.FindIndex(d =>
                    string.IsNullOrEmpty(d.BackRobot_J1) &&
                    string.IsNullOrEmpty(d.BackRobot_J2) &&
                    string.IsNullOrEmpty(d.BackRobot_J3) &&
                    string.IsNullOrEmpty(d.BackRobot_J4) &&
                    string.IsNullOrEmpty(d.BackRobot_J5) &&
                    string.IsNullOrEmpty(d.BackRobot_J6));
                if (index < 0)
                {
                    dataMzCameraLineModelList.Add(mz_camera_dataline);
                }
                else
                {
                    dataMzCameraLineModelList[index] = mz_camera_dataline;
                }
            }
            // 没有后机械臂数据时
            else if (string.IsNullOrEmpty(mz_camera_dataline.BackRobot_J1) &&
                string.IsNullOrEmpty(mz_camera_dataline.BackRobot_J2) &&
                string.IsNullOrEmpty(mz_camera_dataline.BackRobot_J3) &&
                string.IsNullOrEmpty(mz_camera_dataline.BackRobot_J4) &&
                string.IsNullOrEmpty(mz_camera_dataline.BackRobot_J5) &&
                string.IsNullOrEmpty(mz_camera_dataline.BackRobot_J6))
            {
                // 仅查找没有前机械臂数据的索引
                int index = dataMzCameraLineModelList.FindIndex(d =>
                    string.IsNullOrEmpty(d.FrontRobot_J1) &&
                    string.IsNullOrEmpty(d.FrontRobot_J2) &&
                    string.IsNullOrEmpty(d.FrontRobot_J3) &&
                    string.IsNullOrEmpty(d.FrontRobot_J4) &&
                    string.IsNullOrEmpty(d.FrontRobot_J5) &&
                    string.IsNullOrEmpty(d.FrontRobot_J6));
                if (index < 0)
                {
                    dataMzCameraLineModelList.Add(mz_camera_dataline);
                }
                else
                {
                    dataMzCameraLineModelList[index].FrontRobot_Id = mz_camera_dataline.FrontRobot_Id;
                    dataMzCameraLineModelList[index].FrontRobot_Enable = mz_camera_dataline.FrontRobot_Enable;
                    dataMzCameraLineModelList[index].FrontRobot_J1 = mz_camera_dataline.FrontRobot_J1;
                    dataMzCameraLineModelList[index].FrontRobot_J1 = mz_camera_dataline.FrontRobot_J1;
                    dataMzCameraLineModelList[index].FrontRobot_J1 = mz_camera_dataline.FrontRobot_J1;
                    dataMzCameraLineModelList[index].FrontRobot_J1 = mz_camera_dataline.FrontRobot_J1;
                    dataMzCameraLineModelList[index].FrontRobot_J1 = mz_camera_dataline.FrontRobot_J1;
                    dataMzCameraLineModelList[index].FrontRobot_J1 = mz_camera_dataline.FrontRobot_J1;
                }
            }
            else
            {
                dataMzCameraLineModelList.Add(mz_camera_dataline);
            }

            string json = JsonConvert.SerializeObject(dataMzCameraLineModelList);
            lock (jsonLock)
            {
                StreamWriter sw = new StreamWriter(jsonPath + "DataMzCameraLineModel.json", false);
                sw.Write(json);
                sw.Close(); 
            }
#else
            try
            {
                if (DBConnection())
                {
                    string userSql = @"insert into MzCameraDataTable
(train_mode , train_sn,
rgv_devid, rgv_enable, rgv_distance,
front_robot_devid, front_robot_enable,front_robot_j1, front_robot_j2, front_robot_j3, front_robot_j4, front_robot_j5, front_robot_j6,
front_camera_devid, front_camera_enable,front_component_id, front_component_type,
back_robot_devid, back_robot_enable,back_robot_j1, back_robot_j2, back_robot_j3, back_robot_j4, back_robot_j5, back_robot_j6,
back_camera_devid, back_camera_enable,back_component_id, back_component_type) 
VALUES 
(@train_mode , @train_sn,
@rgv_devid, @rgv_enable, @rgv_distance,
@robot1_devid, @robot1_enable, @robot1_j1, @robot1_j2, @robot1_j3, @robot1_j4, @robot1_j5, @robot1_j6,
@camera1_devid, @camera1_enable, @front_component_id, @front_component_type,
@robot2_devid, @robot2_enable, @robot2_j1, @robot2_j2, @robot2_j3, @robot2_j4, @robot2_j5, @robot2_j6,
@camera2_devid, @camera2_enable, @back_component_id, @back_component_type)";

                    adapter = new SqlDataAdapter();
                    comm = new SqlCommand(userSql, conn);

                    //插入数据
                    comm.Parameters.Add(new SqlParameter("@train_mode", SqlDbType.VarChar) { Value = mz_camera_dataline.TrainModel });
                    comm.Parameters.Add(new SqlParameter("@train_sn", SqlDbType.VarChar) { Value = mz_camera_dataline.TrainSn });

                    comm.Parameters.Add(new SqlParameter("@rgv_devid", SqlDbType.Int) { Value = mz_camera_dataline.Rgv_Id });
                    comm.Parameters.Add(new SqlParameter("@rgv_enable", SqlDbType.Int) { Value = mz_camera_dataline.Rgv_Enable });
                    comm.Parameters.Add(new SqlParameter("@rgv_distance", SqlDbType.Int) { Value = mz_camera_dataline.Rgv_Distance });

                    comm.Parameters.Add(new SqlParameter("@robot1_devid", SqlDbType.Int) { Value = mz_camera_dataline.FrontRobot_Id });
                    comm.Parameters.Add(new SqlParameter("@robot1_enable", SqlDbType.Int) { Value = mz_camera_dataline.FrontRobot_Enable });
                    comm.Parameters.Add(new SqlParameter("@robot1_j1", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontRobot_J1 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j2", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontRobot_J2 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j3", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontRobot_J3 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j4", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontRobot_J4 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j5", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontRobot_J5 });
                    comm.Parameters.Add(new SqlParameter("@robot1_j6", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontRobot_J6 });

                    comm.Parameters.Add(new SqlParameter("@camera1_devid", SqlDbType.Int) { Value = mz_camera_dataline.FrontCamera_Id });
                    comm.Parameters.Add(new SqlParameter("@camera1_enable", SqlDbType.Int) { Value = mz_camera_dataline.FrontCamera_Enable });
                    comm.Parameters.Add(new SqlParameter("@front_component_id", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontComponentId });
                    comm.Parameters.Add(new SqlParameter("@front_component_type", SqlDbType.VarChar) { Value = mz_camera_dataline.FrontComponentType });

                    comm.Parameters.Add(new SqlParameter("@robot2_devid", SqlDbType.Int) { Value = mz_camera_dataline.BackRobot_Id });
                    comm.Parameters.Add(new SqlParameter("@robot2_enable", SqlDbType.Int) { Value = mz_camera_dataline.BackRobot_Enable });
                    comm.Parameters.Add(new SqlParameter("@robot2_j1", SqlDbType.VarChar) { Value = mz_camera_dataline.BackRobot_J1 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j2", SqlDbType.VarChar) { Value = mz_camera_dataline.BackRobot_J2 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j3", SqlDbType.VarChar) { Value = mz_camera_dataline.BackRobot_J3 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j4", SqlDbType.VarChar) { Value = mz_camera_dataline.BackRobot_J4 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j5", SqlDbType.VarChar) { Value = mz_camera_dataline.BackRobot_J5 });
                    comm.Parameters.Add(new SqlParameter("@robot2_j6", SqlDbType.VarChar) { Value = mz_camera_dataline.BackRobot_J6 });

                    comm.Parameters.Add(new SqlParameter("@camera2_devid", SqlDbType.Int) { Value = mz_camera_dataline.BackCamera_Id });
                    comm.Parameters.Add(new SqlParameter("@camera2_enable", SqlDbType.Int) { Value = mz_camera_dataline.BackCamera_Enable });
                    comm.Parameters.Add(new SqlParameter("@back_component_id", SqlDbType.VarChar) { Value = mz_camera_dataline.BackComponentId });
                    comm.Parameters.Add(new SqlParameter("@back_component_type", SqlDbType.VarChar) { Value = mz_camera_dataline.BackComponentType });

                    adapter.InsertCommand = comm;
                    adapter.InsertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
#endif
        }

        public void MzCameraDataListSave(List<DataMzCameraLineModelEx> list)
        {
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            dataMzCameraLineModelList = list;
            string json = JsonConvert.SerializeObject(dataMzCameraLineModelList);
            lock (jsonLock)
            {
                StreamWriter sw = new StreamWriter(jsonPath + "DataMzCameraLineModel.json", false);
                sw.Write(json);
                sw.Close();
            }
        }

        /// <summary>
        /// 数据库查询数据行
        /// </summary>
        /// <param name="front_robot"></param>
        /// <param name="back_robot"></param>
        public List<DataMzCameraLineModelEx> MzCameraDataListQurey()
        {
#if saveJson
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            dataMzCameraLineModelList = new List<DataMzCameraLineModelEx>();
            if (File.Exists(jsonPath + "DataMzCameraLineModel.json"))
            {
                string json = "";
                lock (jsonLock)
                {
                    StreamReader sr = new StreamReader(jsonPath + "DataMzCameraLineModel.json");
                    json = sr.ReadToEnd();
                    sr.Close();
                }
                if (!string.IsNullOrEmpty(json))
                {
                    dataMzCameraLineModelList = JsonConvert.DeserializeObject<List<DataMzCameraLineModelEx>>(json);
                }
            }
            return dataMzCameraLineModelList;
#else
            try
            {
                List<DataMzCameraLineModel> dataline_list = new List<DataMzCameraLineModel>();

                if (DBConnection())
                {
                    //查询数据
                    string userSql = @"select * from MzCameraDataTable";
                    adapter = new SqlDataAdapter(userSql, conn);

                    DataTable table = new DataTable();
                    int count = adapter.Fill(table);

                    //处理数据
                    DataMzCameraLineModel line = null;
                    foreach (DataRow dr in table.AsEnumerable())
                    {
                        int temp_id = dr.Field<int>("id");

                        string temp_train_mode = dr.Field<string>("train_mode");
                        string temp_train_sn = dr.Field<string>("train_sn");

                        int temp_rgv_devid = dr.Field<int>("rgv_devid");
                        int temp_rgv_enable = dr.Field<int>("rgv_enable");
                        int temp_rgv_distance = dr.Field<int>("rgv_distance");

                        int temp_front_robot_devid = dr.Field<int>("front_robot_devid");
                        int temp_front_robot_enable = dr.Field<int>("front_robot_enable");
                        string temp_front_robot_j1 = dr.Field<string>("front_robot_j1");
                        string temp_front_robot_j2 = dr.Field<string>("front_robot_j2");
                        string temp_front_robot_j3 = dr.Field<string>("front_robot_j3");
                        string temp_front_robot_j4 = dr.Field<string>("front_robot_j4");
                        string temp_front_robot_j5 = dr.Field<string>("front_robot_j5");
                        string temp_front_robot_j6 = dr.Field<string>("front_robot_j6");

                        int temp_front_camera_devid = dr.Field<int>("front_camera_devid");
                        int temp_front_camera_enable = dr.Field<int>("front_camera_enable");
                        string temp_front_component_id = dr.Field<string>("front_component_id");
                        string temp_front_component_type = dr.Field<string>("front_component_type");

                        int temp_back_robot_devid = dr.Field<int>("back_robot_devid");
                        int temp_back_robot_enable = dr.Field<int>("back_robot_enable");
                        string temp_back_robot_j1 = dr.Field<string>("back_robot_j1");
                        string temp_back_robot_j2 = dr.Field<string>("back_robot_j2");
                        string temp_back_robot_j3 = dr.Field<string>("back_robot_j3");
                        string temp_back_robot_j4 = dr.Field<string>("back_robot_j4");
                        string temp_back_robot_j5 = dr.Field<string>("back_robot_j5");
                        string temp_back_robot_j6 = dr.Field<string>("back_robot_j6");

                        int temp_back_camera_devid = dr.Field<int>("back_camera_devid");
                        int temp_back_camera_enable = dr.Field<int>("back_camera_enable");
                        string temp_back_component_id = dr.Field<string>("back_component_id");
                        string temp_back_component_type = dr.Field<string>("back_component_type");

                        line = new DataMzCameraLineModel();

                        line.DataLine_Index = temp_id;

                        line.TrainModel = temp_train_mode;
                        line.TrainSn = temp_train_sn;

                        line.Rgv_Id = temp_rgv_devid;
                        line.Rgv_Enable = temp_rgv_enable;
                        line.Rgv_Distance = temp_rgv_distance;

                        line.FrontRobot_Id = temp_front_robot_devid;
                        line.FrontRobot_Enable = temp_front_robot_enable;
                        line.FrontRobot_J1 = temp_front_robot_j1;
                        line.FrontRobot_J2 = temp_front_robot_j2;
                        line.FrontRobot_J3 = temp_front_robot_j3;
                        line.FrontRobot_J4 = temp_front_robot_j4;
                        line.FrontRobot_J5 = temp_front_robot_j5;
                        line.FrontRobot_J6 = temp_front_robot_j6;

                        line.FrontCamera_Id = temp_front_camera_devid;
                        line.FrontCamera_Enable = temp_front_camera_enable;
                        line.FrontComponentId = temp_front_component_id;
                        line.FrontComponentType = temp_front_component_type;

                        line.BackRobot_Id = temp_back_robot_devid;
                        line.BackRobot_Enable = temp_back_robot_enable;
                        line.BackRobot_J1 = temp_back_robot_j1;
                        line.BackRobot_J2 = temp_back_robot_j2;
                        line.BackRobot_J3 = temp_back_robot_j3;
                        line.BackRobot_J4 = temp_back_robot_j4;
                        line.BackRobot_J5 = temp_back_robot_j5;
                        line.BackRobot_J6 = temp_back_robot_j6;

                        line.BackCamera_Id = temp_back_camera_devid;
                        line.BackCamera_Enable = temp_back_camera_enable;
                        line.BackComponentId = temp_back_component_id;
                        line.BackComponentType = temp_back_component_type;

                        dataline_list.Add(line);
                    }
                }

                return dataline_list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
#endif
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------------------
        //XzCameraData
        #region
        /// <summary>
        /// 数据库添加数据行
        /// </summary>
        /// <param name="robot_dataline"></param>
        public void XzCameraDataListSave(DataXzCameraLineModel xz_camera_dataline)
        {
#if saveJson
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            if (dataXzCameraLineModelList == null)
            {
                XzCameraDataListQurey();
            }

            dataXzCameraLineModelList.Add(xz_camera_dataline);

            string json = JsonConvert.SerializeObject(dataXzCameraLineModelList);
            lock (jsonLock)
            {
                StreamWriter sw = new StreamWriter(jsonPath + "DataXzCameraLineModel.json", false);
                sw.Write(json);
                sw.Close();
            }
#else
            try
            {
                if (DBConnection())
                {
                    string userSql = @"insert into New_XzCameraDataTable
(train_mode , train_sn,
carriage_id, carriage_type,
rgv_devid, rgv_enable, rgv_min_distance,rgv_max_distance,
min_high, max_high,
ir1, ir2, ir3, ir4) 
VALUES 
(@train_mode , @train_sn,
@carriage_id , @carriage_type,
@rgv_devid, @rgv_enable, @rgv_min_distance, @rgv_max_distance,
@min_high, @max_high,
@ir1, @ir2, @ir3, @ir4)";

                    adapter = new SqlDataAdapter();
                    comm = new SqlCommand(userSql, conn);

                    //插入数据
                    comm.Parameters.Add(new SqlParameter("@train_mode", SqlDbType.VarChar) { Value = xz_camera_dataline.TrainModel });
                    comm.Parameters.Add(new SqlParameter("@train_sn", SqlDbType.VarChar) { Value = xz_camera_dataline.TrainSn });

                    comm.Parameters.Add(new SqlParameter("@carriage_id", SqlDbType.Int) { Value = xz_camera_dataline.CarriageId });
                    comm.Parameters.Add(new SqlParameter("@carriage_type", SqlDbType.Int) { Value = xz_camera_dataline.CarriageType });

                    comm.Parameters.Add(new SqlParameter("@rgv_devid", SqlDbType.Int) { Value = xz_camera_dataline.Rgv_Id });
                    comm.Parameters.Add(new SqlParameter("@rgv_enable", SqlDbType.Int) { Value = xz_camera_dataline.Rgv_Enable });
                    comm.Parameters.Add(new SqlParameter("@rgv_min_distance", SqlDbType.Int) { Value = xz_camera_dataline.RgvCheckMinDistacnce });
                    comm.Parameters.Add(new SqlParameter("@rgv_max_distance", SqlDbType.Int) { Value = xz_camera_dataline.RgvCheckMaxDistacnce });

                    comm.Parameters.Add(new SqlParameter("@min_high", SqlDbType.Int) { Value = xz_camera_dataline.CurrentDetectMinHeight });
                    comm.Parameters.Add(new SqlParameter("@max_high", SqlDbType.Int) { Value = xz_camera_dataline.CurrentDetectMaxHeight });

                    comm.Parameters.Add(new SqlParameter("@ir1", SqlDbType.Int) { Value = xz_camera_dataline.InfraRed1_Stat });
                    comm.Parameters.Add(new SqlParameter("@ir2", SqlDbType.Int) { Value = xz_camera_dataline.InfraRed2_Stat });
                    comm.Parameters.Add(new SqlParameter("@ir3", SqlDbType.Int) { Value = xz_camera_dataline.InfraRed3_Stat });
                    comm.Parameters.Add(new SqlParameter("@ir4", SqlDbType.Int) { Value = xz_camera_dataline.InfraRed4_Stat });

                    adapter.InsertCommand = comm;
                    adapter.InsertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
#endif
        }

        public void XzCameraDataListSave(List<DataXzCameraLineModel> list)
        {
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            dataXzCameraLineModelList = list;
            string json = JsonConvert.SerializeObject(dataXzCameraLineModelList);
            lock (jsonLock)
            {
                StreamWriter sw = new StreamWriter(jsonPath + "DataXzCameraLineModel.json", false);
                sw.Write(json);
                sw.Close();
            }
        }

        /// <summary>
        /// 数据库查询数据行
        /// </summary>
        /// <param name="front_robot"></param>
        /// <param name="back_robot"></param>
        public List<DataXzCameraLineModel> XzCameraDataListQurey()
        {
#if saveJson
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            dataXzCameraLineModelList = Schedule.ScheduleManager.CreateXzData();
            if (File.Exists(jsonPath + "DataXzCameraLineModel.json"))
            {
                string json = "";
                lock (jsonLock)
                {
                    StreamReader sr = new StreamReader(jsonPath + "DataXzCameraLineModel.json");
                    json = sr.ReadToEnd();
                    sr.Close();
                }
                if (!string.IsNullOrEmpty(json))
                {
                    dataXzCameraLineModelList = JsonConvert.DeserializeObject<List<DataXzCameraLineModel>>(json);
                }
            }
            return dataXzCameraLineModelList;
#else
            try
            {
                List<DataXzCameraLineModel> dataline_list = new List<DataXzCameraLineModel>();

                if (DBConnection())
                {
                    //查询数据
                    string userSql = @"select * from New_XzCameraDataTable";
                    adapter = new SqlDataAdapter(userSql, conn);

                    DataTable table = new DataTable();
                    int count = adapter.Fill(table);

                    //处理数据
                    DataXzCameraLineModel line = null;
                    foreach (DataRow dr in table.AsEnumerable())
                    {
                        int temp_id = dr.Field<int>("id");

                        string temp_train_mode = dr.Field<string>("train_mode");
                        string temp_train_sn = dr.Field<string>("train_sn");

                        int temp_carriage_id = dr.Field<int>("carriage_id");
                        int temp_carriage_type = dr.Field<int>("carriage_type");

                        int temp_rgv_devid = dr.Field<int>("rgv_devid");
                        int temp_rgv_enable = dr.Field<int>("rgv_enable");
                        int temp_rgv_min_distance = dr.Field<int>("rgv_min_distance");
                        int temp_rgv_max_distance = dr.Field<int>("rgv_max_distance");

                        int temp_min_high = dr.Field<int>("min_high");
                        int temp_max_high = dr.Field<int>("max_high");

                        int temp_ir1 = dr.Field<int>("ir1");
                        int temp_ir2 = dr.Field<int>("ir2");
                        int temp_ir3 = dr.Field<int>("ir3");
                        int temp_ir4 = dr.Field<int>("ir4");

                        line = new DataXzCameraLineModel();

                        line.DataLine_Index = temp_id;

                        line.TrainModel = temp_train_mode;
                        line.TrainSn = temp_train_sn;

                        line.CarriageId = temp_carriage_id;
                        line.CarriageType = temp_carriage_type;

                        line.Rgv_Id = temp_rgv_devid;
                        line.Rgv_Enable = temp_rgv_enable;
                        line.RgvCheckMinDistacnce = temp_rgv_min_distance;
                        line.RgvCheckMaxDistacnce = temp_rgv_max_distance;

                        line.CurrentDetectMinHeight = temp_min_high;
                        line.CurrentDetectMaxHeight = temp_max_high;

                        line.InfraRed1_Stat = temp_ir1;
                        line.InfraRed2_Stat = temp_ir2;
                        line.InfraRed3_Stat = temp_ir3;
                        line.InfraRed4_Stat = temp_ir4;

                        dataline_list.Add(line);
                    }
                }

                return dataline_list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
#endif
        }
        #endregion
        #region
        public void AxesDataListSave(List<Axis> list)
        {
#if saveJson
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            string json = JsonConvert.SerializeObject(list);
            lock (jsonLock)
            {
                StreamWriter sw = new StreamWriter(jsonPath + "Axis.json", false);
                sw.Write(json);
                sw.Close();
            } 
#endif
        }

        public List<Axis> AxesDataListQurey()
        {
#if saveJson
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            List<Axis> list = new List<Axis>();
            if (File.Exists(jsonPath + "Axis.json"))
            {
                string json = "";
                lock (jsonLock)
                {
                    StreamReader sr = new StreamReader(jsonPath + "Axis.json");
                    json = sr.ReadToEnd();
                    sr.Close();
                }
                if (!string.IsNullOrEmpty(json))
                {
                    list = JsonConvert.DeserializeObject<List<Axis>>(json);
                }
            }
            return list;
#endif
        }
        #endregion
    }
}
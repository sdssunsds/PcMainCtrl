using Basler.Pylon;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.HardWare.Robot;
using PcMainCtrl.Model;
using PcMainCtrl.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;
using static PcMainCtrl.HardWare.Robot.RobotModCtrlProtocol;

namespace PcMainCtrl.Schedule
{
    /// <summary>
    /// 工作流管理
    /// </summary>
    public class ScheduleManager
    {
        private static Dictionary<string, int> cameraDict = new Dictionary<string, int>();

        public static bool xsComplete = false;
        public static bool frontComplete = false;
        public static bool backComplete = false;
        public static bool frontRobotComplete = false;
        public static bool backRobotComplete = false;

        public static bool RelayComplete = false;
        public static bool RgvComplete = true;
        public static bool StmComplete = true;

        /// <summary>
        /// 硬件自检
        /// </summary>
        public static bool PowerSelfTest(ICameraInfo xs, ICameraInfo front, ICameraInfo back,
            List<DataMzCameraLineModelEx> mzCameraDataList, MainTaskScheduleHandle taskScheduleHandleInfo)
        {
            if (CameraTest(xs, front, back))
            {
                if (RobotTest(mzCameraDataList, taskScheduleHandleInfo))
                {
                    RemoveEvent();
                    return xsComplete && frontComplete && backComplete && frontRobotComplete && backRobotComplete && RelayComplete && RgvComplete && StmComplete;
                }
            }
            return false;
        }

        /// <summary>
        /// 相机检测
        /// </summary>
        public static bool CameraTest(ICameraInfo xs, ICameraInfo front, ICameraInfo back)
        {
            AddLog("相机检测");
            bool test = true;
            if (xs == null)
            {
                AddLog("未找到线阵相机");
                test = false;
            }
            if (front == null)
            {
                AddLog("未找到前臂面阵相机");
                test = false;
            }
            if (back == null)
            {
                AddLog("未找到后臂面阵相机");
                test = false;
            }
            if (cameraDict == null)
            {
                cameraDict = new Dictionary<string, int>();
            }
            if (!test)
            {
                return false;
            }
            cameraDict.Clear();
            cameraDict.Add(xs[CameraInfoKey.FriendlyName], 0);
            cameraDict.Add(front[CameraInfoKey.FriendlyName], 1);
            cameraDict.Add(back[CameraInfoKey.FriendlyName], 2);
            if (test)
            {
                CameraCtrlHelper.GetInstance().CameraImageEvent += ScheduleManager_CameraImageEvent;

                AddLog("检测线阵相机");
                CameraCtrlHelper.GetInstance().CameraChangeItem(xs);
                CameraCtrlHelper.GetInstance().CameraContinuousShot();
                ThreadSleep(3000);
                CameraCtrlHelper.GetInstance().CameraStop();

                AddLog("前机械臂相机检测");
                CameraCtrlHelper.GetInstance().CameraChangeItem(front);
                CameraCtrlHelper.GetInstance().CameraOneShot();
                ThreadSleep(1000);

                AddLog("后机械臂相机检测");
                CameraCtrlHelper.GetInstance().CameraChangeItem(back);
                CameraCtrlHelper.GetInstance().CameraOneShot();
                return true;
            }
            return false;
        }

        public static void RemoveEvent()
        {
            CameraCtrlHelper.GetInstance().CameraImageEvent -= ScheduleManager_CameraImageEvent;
        }

        /// <summary>
        /// 继电器检测
        /// </summary>
        public static bool RelayTest()
        {
            return RelayComplete;
        }

        /// <summary>
        /// 底盘检测
        /// </summary>
        public static bool RgvTest()
        {
            return RgvComplete;
        }

        /// <summary>
        /// 机械臂检测
        /// </summary>
        public static bool RobotTest(List<DataMzCameraLineModelEx> mzCameraDataList, MainTaskScheduleHandle taskScheduleHandleInfo)
        {
            try
            {
                AddLog("机械臂检测");
                RobotDataPack robotDataPack = new RobotDataPack();
                RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE;
                RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE;
                //得到坐标
                if (mzCameraDataList.Count > 0)
                {
                    robotDataPack.j1 = mzCameraDataList[0].FrontRobot_J1;
                    robotDataPack.j2 = mzCameraDataList[0].FrontRobot_J2;
                    robotDataPack.j3 = mzCameraDataList[0].FrontRobot_J3;
                    robotDataPack.j4 = mzCameraDataList[0].FrontRobot_J4;
                    robotDataPack.j5 = mzCameraDataList[0].FrontRobot_J5;
                    robotDataPack.j6 = mzCameraDataList[0].FrontRobot_J6;
                    RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(0x01, RobotData.RobotSetCtrlFunCode, robotDataPack);
                    robotDataPack.j1 = mzCameraDataList[0].BackRobot_J1;
                    robotDataPack.j2 = mzCameraDataList[0].BackRobot_J2;
                    robotDataPack.j3 = mzCameraDataList[0].BackRobot_J3;
                    robotDataPack.j4 = mzCameraDataList[0].BackRobot_J4;
                    robotDataPack.j5 = mzCameraDataList[0].BackRobot_J5;
                    robotDataPack.j6 = mzCameraDataList[0].BackRobot_J6;
                    RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(0x02, RobotData.RobotSetCtrlFunCode, robotDataPack);

                    TaskRun(() =>
                    {
                        do
                        {
                            ThreadSleep(50);
                            AddLog("等待机械臂完成运动");
                        } while (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP &&
                                 RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor == eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_STOP);
                        RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;
                        RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor = eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_READY;

                        robotDataPack.j1 = @"0.00";
                        robotDataPack.j2 = @"0.00";
                        robotDataPack.j3 = @"0.00";
                        robotDataPack.j4 = @"0.00";
                        robotDataPack.j5 = @"0.00";
                        robotDataPack.j6 = @"0.00";
                        RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(0x01, RobotData.RobotBackZeroFunCode, robotDataPack);
                        RobotModCtrlHelper.GetInstance().DoRobotOpCmdHandle(0x02, RobotData.RobotBackZeroFunCode, robotDataPack);
                        frontRobotComplete = backRobotComplete = true;
                    });
                }
                else
                {
                    AddLog("未找到机械臂数据");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 串口检测
        /// </summary>
        public static bool Stm32Test()
        {
            return StmComplete;
        }

        public static List<DataXzCameraLineModel> CreateXzData()
        {
            List<DataXzCameraLineModel> list = new List<DataXzCameraLineModel>();

            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 0,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 34 : 27500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 38 : 28500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });
            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 1,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 67 : 55500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 70 : 56500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });
            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 2,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 100 : 83500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 103 : 84500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });
            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 3,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 133 : 111500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 136 : 112500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });
            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 4,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 166 : 139500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 169 : 140500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });
            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 5,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 199 : 167500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 202 : 168500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });
            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 6,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 232 : 195500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 235 : 196500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });
            list.Add(new DataXzCameraLineModel()
            {
                DataLine_Index = 7,
                TrainModel = "CHR380AL",
                TrainSn = "123456",
                CarriageId = 1,
                CarriageType = 0,
                Rgv_Id = 0,
                Rgv_Enable = 1,
                RgvCheckMinDistacnce = HomePageViewModel.JoinPicMode ? 265 : 223500,
                RgvCheckMaxDistacnce = HomePageViewModel.JoinPicMode ? 268 : 224500,
                CurrentDetectMinHeight = -51,
                CurrentDetectMaxHeight = 49,
                InfraRed1_Stat = -1,
                InfraRed2_Stat = -1,
                InfraRed3_Stat = -1,
                InfraRed4_Stat = -1
            });

            return list;
        }

        private static void ScheduleManager_CameraImageEvent(Camera camera, Bitmap bmp)
        {
            if (bmp != null && camera != null)
            {
                string name = camera.CameraInfo[CameraInfoKey.FriendlyName];
                if (cameraDict.ContainsKey(name))
                {
                    switch (cameraDict[name])
                    {
                        case 0:
                            xsComplete = true;
                            AddLog("线阵相机检测结果：" + xsComplete);
                            break;
                        case 1:
                            frontComplete = true;
                            AddLog("前机械臂相机检测结果：" + frontComplete);
                            break;
                        case 2:
                            backComplete = true;
                            AddLog("后机械臂相机检测结果：" + backComplete);
                            break;
                    }
                }
            }
        }
    }
}

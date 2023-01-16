using PcMainCtrl.Common;
using PLC;
using System;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.HardWare
{
    public enum RobotName
    {
        Front = 0, Back = 1
    }
    public class PLC3DCamera
    {
        private const short zero = 1;
        private const short forward = 2;
        private const short backoff = 4;
        private const short unenable = 8;
        private const short halt = 16;
        private const short rst = 32;
        private PLCManager.PLC_Parent frontWrite, frontRead, backWrite, backRead;

        public ModbusState FrontModbus { get; private set; }
        public ModbusState BackModbus { get; private set; }

        public event Action FrontAlarmEvent;
        public event Action BackAlarmEvent;

        public PLC3DCamera()
        {
            frontWrite = ModbusTCP.GetMB();
            frontRead = ModbusTCP.GetMB();
            backWrite = ModbusTCP.GetMB();
            backRead = ModbusTCP.GetMB();

            frontWrite.Address = "10";
            frontRead.Address = "0";
            backWrite.Address = "11";
            backRead.Address = "1";

            FrontModbus = new ModbusState();
            BackModbus = new ModbusState();

            FrontModbus.AlarmEvent = FrontAlarmEvent;
            FrontModbus.GetState = GetFrontState;
            BackModbus.AlarmEvent = BackAlarmEvent;
            BackModbus.GetState = GetBackState;

            ModbusTCP.SetAddress12(Address12Type.FrontRobotStepMotorPower, 1);
            ModbusTCP.SetAddress12(Address12Type.BackRobotStepMotorPower, 1);
            GlobalValues.AddLog("滑台上电等待5秒");
            ThreadSleep(5000);
            SetZero(RobotName.Front);
            SetZero(RobotName.Back);
        }

        /// <summary>
        /// 获取滑台报警
        /// </summary>
        public bool GetDriverAlarm(RobotName robot)
        {
            return robot == RobotName.Front ? FrontModbus.Driver_Alarm : BackModbus.Driver_Alarm;
        }

        /// <summary>
        /// 获取PLC报警
        /// </summary>
        public bool GetPLCAlarm(RobotName robot)
        {
            return robot == RobotName.Front ? FrontModbus.PLC_Alarm : BackModbus.PLC_Alarm;
        }

        /// <summary>
        /// 获取滑台状态
        /// </summary>
        public void GetState(RobotName robot)
        {
            switch (robot)
            {
                case RobotName.Front:
                    FrontModbus.ReadState();
                    break;
                case RobotName.Back:
                    BackModbus.ReadState();
                    break;
            }
        }

        /// <summary>
        /// 滑台回原点
        /// </summary>
        public void SetZero(RobotName robot)
        {
            try
            {
                WaitAlarm(robot);
                switch (robot)
                {
                    case RobotName.Front:
                        frontWrite.ShortValue = 0;
                        ThreadSleep(50);
                        frontWrite.ShortValue = zero;
                        FrontModbus.Home_Complete = false;
                        break;
                    case RobotName.Back:
                        backWrite.ShortValue = 0;
                        ThreadSleep(50);
                        backWrite.ShortValue = zero;
                        BackModbus.Home_Complete = false;
                        break;
                }
                WaitAlarm(robot, GetDriverAlarm(robot) || GetPLCAlarm(robot));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 滑台前进
        /// </summary>
        public void SetForward(RobotName robot)
        {
            WaitAlarm(robot);
            switch (robot)
            {
                case RobotName.Front:
                    frontWrite.ShortValue = 0;
                    ThreadSleep(50);
                    frontWrite.ShortValue = forward;
                    FrontModbus.MoveAbsolute_Complete = false;
                    break;
                case RobotName.Back:
                    backWrite.ShortValue = 0;
                    ThreadSleep(50);
                    backWrite.ShortValue = forward;
                    BackModbus.MoveAbsolute_Complete = false;
                    break;
            }
            WaitAlarm(robot, GetDriverAlarm(robot) || GetPLCAlarm(robot));
        }

        /// <summary>
        /// 滑台后退
        /// </summary>
        public void SetBackoff(RobotName robot)
        {
            WaitAlarm(robot);
            switch (robot)
            {
                case RobotName.Front:
                    frontWrite.ShortValue = 0;
                    ThreadSleep(50);
                    frontWrite.ShortValue = backoff;
                    FrontModbus.MoveAbsolute_Complete = false;
                    break;
                case RobotName.Back:
                    backWrite.ShortValue = 0;
                    ThreadSleep(50);
                    backWrite.ShortValue = backoff;
                    BackModbus.MoveAbsolute_Complete = false;
                    break;
            }
            WaitAlarm(robot, GetDriverAlarm(robot) || GetPLCAlarm(robot));
        }

        /// <summary>
        /// 滑台暂停
        /// </summary>
        public void SetHalt(RobotName robot)
        {
            WaitAlarm(robot);
            switch (robot)
            {
                case RobotName.Front:
                    frontWrite.ShortValue = 0;
                    ThreadSleep(50);
                    frontWrite.ShortValue = halt;
                    break;
                case RobotName.Back:
                    backWrite.ShortValue = 0;
                    ThreadSleep(50);
                    backWrite.ShortValue = halt;
                    break;
            }
            WaitAlarm(robot, GetDriverAlarm(robot) || GetPLCAlarm(robot));
        }

        /// <summary>
        /// PLC复位（可清除PLC报警）
        /// </summary>
        public void SetRst(RobotName robot)
        {
            switch (robot)
            {
                case RobotName.Front:
                    frontWrite.ShortValue = 0;
                    ThreadSleep(50);
                    frontWrite.ShortValue = rst;
                    break;
                case RobotName.Back:
                    backWrite.ShortValue = 0;
                    ThreadSleep(50);
                    backWrite.ShortValue = rst;
                    break;
            }
        }

        public void SetAlarmEvent(Action FrontAlarmEvent, Action BackAlarmEvent)
        {
            FrontModbus.AlarmEvent = FrontAlarmEvent;
            BackModbus.AlarmEvent = BackAlarmEvent;
        }

        /// <summary>
        /// 清除滑台报警
        /// </summary>
        public void ClearAlarm(RobotName robot)
        {
            switch (robot)
            {
                case RobotName.Front:
                    ModbusTCP.SetAddress12(Address12Type.FrontRobotStepMotorPower, 0);
                    ThreadSleep(1000);
                    ModbusTCP.SetAddress12(Address12Type.FrontRobotStepMotorPower, 1);
                    break;
                case RobotName.Back:
                    ModbusTCP.SetAddress12(Address12Type.BackRobotStepMotorPower, 0);
                    ThreadSleep(1000);
                    ModbusTCP.SetAddress12(Address12Type.BackRobotStepMotorPower, 1);
                    break;
            }
        }

        private short GetFrontState()
        {
            return frontRead.ShortValue;
        }

        private short GetBackState()
        {
            return backRead.ShortValue;
        }

        private void WaitAlarm(RobotName robot)
        {
            GetState(robot);
            ThreadSleep(50);
            while (GetDriverAlarm(robot))
            {
                ThreadSleep(50);
            }
            while (GetPLCAlarm(robot))
            {
                ThreadSleep(50);
            }
        }

        private void WaitAlarm(RobotName robot, bool isAlarm)
        {
            if (isAlarm)
            {
                SetZero(robot);
            }
        }

        public class ModbusState
        {
            /// <summary>
            /// 原点传感器
            /// </summary>
            public bool Home { get; private set; }
            /// <summary>
            /// 左传感器
            /// </summary>
            public bool Left_Limit { get; private set; }
            /// <summary>
            /// 右传感器
            /// </summary>
            public bool Right_Limit { get; private set; }
            /// <summary>
            /// 归原点中
            /// </summary>
            public bool Home_Moving { get; private set; }
            /// <summary>
            /// 已完成归原点
            /// </summary>
            public bool Home_Complete { get; set; }
            /// <summary>
            /// 移动中
            /// </summary>
            public bool MoveAbsolute_Moving { get; private set; }
            /// <summary>
            /// 已完成移动
            /// </summary>
            public bool MoveAbsolute_Complete { get; set; }
            /// <summary>
            /// 换台暂停中
            /// </summary>
            public bool Halt { get; private set; }
            /// <summary>
            /// PLC复位完成
            /// </summary>
            public bool PLC_RST_Complete { get; private set; }
            /// <summary>
            /// 滑台报警
            /// </summary>
            public bool Driver_Alarm { get; private set; }
            /// <summary>
            /// PLC报警
            /// </summary>
            public bool PLC_Alarm { get; private set; }

            public Action AlarmEvent;
            public Func<short> GetState;

            public void ReadState()
            {
                short val = GetState();
                string bytestring = Convert.ToString(val, 2);
                int index = 0;
                for (int i = bytestring.Length - 1; i >= 0; i--)
                {
                    bool exp = bytestring[i] == '1';
                    switch (index)
                    {
                        case 0:
                            Home = exp;
                            break;
                        case 1:
                            Left_Limit = exp;
                            break;
                        case 2:
                            Right_Limit = exp;
                            break;
                        case 3:
                            Home_Moving = exp;
                            break;
                        case 4:
                            Home_Complete = exp;
                            break;
                        case 5:
                            MoveAbsolute_Moving = exp;
                            break;
                        case 6:
                            MoveAbsolute_Complete = exp;
                            break;
                        case 8:
                            Halt = exp;
                            break;
                        case 9:
                            PLC_RST_Complete = exp;
                            break;
                        case 14:
                            Driver_Alarm = exp;
                            AlarmEvent?.Invoke();
                            break;
                        case 15:
                            PLC_Alarm = exp;
                            AlarmEvent?.Invoke();
                            break;
                    }
                    index++;
                }
            }
        }
    }
}

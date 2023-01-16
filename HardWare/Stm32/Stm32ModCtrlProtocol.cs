using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PcMainCtrl.HardWare.Stm32
{
    public class Stm32ModCtrlProtocol
    {
        public class Stm32Data
        {
            #region
            /// <summary>
            /// PC报文的head和tail
            /// </summary>
            public const byte PC_NETWORK_MSG_HEAD = 0xFE;
            public const byte PC_NETWORK_MSG_TAIL = 0xFF;

            /// <summary>
            /// Stm32子设备的地址(本地)
            /// </summary>
            public const byte LOCAL_STM32_DEVICE_ID = 0x00;

            /// <summary>
            /// Stm32激光和语音控制命令字
            /// </summary>
            public const byte Stm32GetTrainSite_FuncCode = 0x01;
            public const byte Stm32SetRobotStartVoice_FuncCode = 0x05;
            public const byte Stm32SetRobotStopVoice_FuncCode = 0x06;

            /// <summary>
            /// 协议包长度信息管理
            /// </summary>
            public const int PcNetworkMsgBufsize = unchecked((short)16);
            public const byte Stm32LocalFunPackLen = PcNetworkMsgBufsize + 4; //68字节

            /// <summary>
            /// Stm32功能字
            /// </summary>
            public byte  mStm32Func;
            public byte[] mStm32CmdBuf = new byte[PcNetworkMsgBufsize];
            public byte mStm32CmdLen;
            #endregion

            /// <summary> 
            /// 将一个object对象序列化，返回一个byte[]         
            /// </summary> 
            /// <param name="obj">能序列化的对象</param>         
            /// <returns></returns> 
            private static byte[] ObjectToBytes(object obj)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter(); 
                    formatter.Serialize(ms, obj); 
                    return ms.GetBuffer();
                }
            }

            /// <summary> 
            /// 将一个序列化后的byte[]数组还原         
            /// </summary>
            /// <param name="Bytes"></param>         
            /// <returns></returns> 
            private static object BytesToObject(byte[] Bytes)
            {
                using (MemoryStream ms = new MemoryStream(Bytes))
                {
                    IFormatter formatter = new BinaryFormatter(); 
                    return formatter.Deserialize(ms);
                }
            }

            /// <summary>
            /// Stm32本地命令控制字
            /// e1:控制车底信息采集
            /// e2:控制语音播放
            /// e3:控制光源调光
            /// e4:控制3D扫描仪
            /// </summary>
            /// <param name="cmd"></param>
            /// <returns></returns>
            public byte[] Stm32SendPack(byte cmd , byte[] senddata, byte sendlen)
            {
                int CopyIndex = 0;
                byte[] databuf = new byte[Stm32LocalFunPackLen];
                Array.Clear(databuf, 0, databuf.Length);

                //填入Head
                databuf[0] = PC_NETWORK_MSG_HEAD;
                CopyIndex++;

                //填入Cmd
                databuf[1] = cmd;
                CopyIndex++;

                //填入Datalen
                databuf[2] = sendlen;
                CopyIndex++;

                //填入Databuf
                Array.Copy(senddata, 0, databuf, CopyIndex, sendlen);
                CopyIndex += sendlen;

                databuf[Stm32LocalFunPackLen - 1] = PC_NETWORK_MSG_TAIL;

                return databuf;
            }
        }

        public class Stm32ModCtrlFun
        {
            public static byte[] Stm32ModCtrlCmdForLocal(byte cmd, byte[] cmddata, byte cmdlen)
            {
                //返回构造的协议包
                byte[] databuf = new byte[Stm32Data.Stm32LocalFunPackLen]; //68字节
                Array.Clear(databuf, 0, databuf.Length);

                //获取组装好的命令数据
                Stm32Data pack = new Stm32Data();
                switch (cmd)
                {
                    case Stm32Data.Stm32GetTrainSite_FuncCode:
                        {
                            databuf = pack.Stm32SendPack(Stm32Data.Stm32GetTrainSite_FuncCode, cmddata, cmdlen);
                        }
                        break;

                    case Stm32Data.Stm32SetRobotStartVoice_FuncCode:
                        {
                            databuf = pack.Stm32SendPack(Stm32Data.Stm32SetRobotStartVoice_FuncCode, cmddata, cmdlen);
                        }
                        break;

                    case Stm32Data.Stm32SetRobotStopVoice_FuncCode:
                        {
                            databuf =  pack.Stm32SendPack(Stm32Data.Stm32SetRobotStopVoice_FuncCode, cmddata, cmdlen);
                        }
                        break;
                }

                return databuf;
            }
        }
    }
}

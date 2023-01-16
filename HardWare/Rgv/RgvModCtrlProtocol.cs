using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PcMainCtrl.HardWare.Rgv
{
    public class RgvModCtrlProtocol
    {
        /// <summary>
        /// Modbus协议控制字
        /// </summary>
        public class MoubusProtocolFuncPack
        {
            //请求：MBAP报文头(7 Byte)  功能码（ModBus协议功能码 1Byte）  数据域(起始地址、寄存器数量) 校验
            //应答：MBAP报文头(7 Byte)  功能码（ModBus协议功能码 1Byte）  数据(字节个数 XX XX XX XX) 校验

            #region MBAP报文头(7 Byte) 
            public const short ModbusHeaderMaxCnt = unchecked((short)0xffff);

            //MBAP报文头：事务元标识符,2Bytes
            public static short ModbusHeaderCurrCnt = 0x0001;

            //MBAP报文头：协议标识符,2Bytes
            public const short ModbusProtocolFlag = 0x0000;

            //MBAP报文头：Modbus功能字03报文头指示的数据长度,2Bytes
            public const short ModbusFunc03Code_MABPLen = 0x0006;

            //MBAP报文头：Modbus功能字06报文头指示的数据长度,2Bytes
            public const short ModbusFunc06Code_MABPLen = 0x0006;

            //MBAP报文头：Modbus功能字10报文头指示的数据长度,2Bytes
            public const short ModbusFunc10Code_MABPLen = 0x000b;

            //MBAP报文头：Slave从机地址,1Byte
            public const byte ModbusSlaveAddr = 0x01;

            //MBAP报文头长度
            public byte ModbusMBAPHeadLength = 0x06;
            #endregion

            #region 功能码（ModBus协议功能码 1Byte）
            //Modbus功能字:0x03
            public const byte ModbusFunc03Code = 0x03;

            //Modbus功能字:0x06
            public const byte ModbusFunc06Code = 0x06;

            //Modbus功能字:0x10
            public const byte ModbusFunc10Code = 0x10;
            #endregion

            /// <summary>
            /// Modbus功能字03报文
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public byte[] ToAraayWithModbus_03SendPack(short dataAddr, short dataLen)
            {
                int CopyIndex = 0;

                //Modbus报文数据缓冲区
                short ModbusFunc03Code_BufLen = 0x06;
                byte[] SendDataBuf = new byte[ModbusMBAPHeadLength + ModbusFunc03Code_BufLen];

                //MBAP报文头
                if (ModbusHeaderCurrCnt == ModbusHeaderMaxCnt)
                {
                    ModbusHeaderCurrCnt = 0x0000;

                }
                else
                {
                    ModbusHeaderCurrCnt++;
                }
                short num = IPAddress.HostToNetworkOrder(ModbusHeaderCurrCnt);
                byte[] ModbusHeaderCurrCntBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusHeaderCurrCntBy, 0, SendDataBuf, CopyIndex, ModbusHeaderCurrCntBy.Length);
                CopyIndex += ModbusHeaderCurrCntBy.Length;

                num = IPAddress.HostToNetworkOrder(ModbusProtocolFlag);
                byte[] ModbusProtocolFlagBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusProtocolFlagBy, 0, SendDataBuf, CopyIndex, ModbusProtocolFlagBy.Length);
                CopyIndex += ModbusProtocolFlagBy.Length;

                num = IPAddress.HostToNetworkOrder(ModbusFunc03Code_MABPLen);
                byte[] ModbusFunc03Code_MABPLenBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusFunc03Code_MABPLenBy, 0, SendDataBuf, CopyIndex, ModbusFunc03Code_MABPLenBy.Length);
                CopyIndex += ModbusFunc03Code_MABPLenBy.Length;

                //SendDataBuf.Append(ModbusSlaveAddr);
                SendDataBuf[CopyIndex] = ModbusSlaveAddr;
                CopyIndex += 1;

                //Modbus报文功能字
                //SendDataBuf.Append(ModbusFunc03Code);
                SendDataBuf[CopyIndex] = ModbusFunc03Code;
                CopyIndex += 1;

                //Modbus报文数据域
                num = IPAddress.HostToNetworkOrder(dataAddr);
                byte[] dataAddrBy = BitConverter.GetBytes(num);
                Array.Copy(dataAddrBy, 0, SendDataBuf, CopyIndex, dataAddrBy.Length);
                CopyIndex += dataAddrBy.Length;

                num = IPAddress.HostToNetworkOrder(dataLen);
                byte[] dataLenBy = BitConverter.GetBytes(num);
                Array.Copy(dataLenBy, 0, SendDataBuf, CopyIndex, dataLenBy.Length);
                CopyIndex += dataLenBy.Length;

                //返回拼接的数据包
                return SendDataBuf;
            }

            /// <summary>
            /// Modbus功能字06报文
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public byte[] ToAraayWithModbus_06SendPack(short dataAddr, short dataLen)
            {
                int     CopyIndex = 0;

                //Modbus报文数据缓冲区
                short ModbusFunc06Code_BufLen = 0x06;
                byte[] SendDataBuf = new byte[ModbusMBAPHeadLength + ModbusFunc06Code_BufLen];

                //MBAP报文头
                //MBAP报文头
                if (ModbusHeaderCurrCnt == ModbusHeaderMaxCnt)
                {
                    ModbusHeaderCurrCnt = 0x0000;

                }
                else
                {
                    ModbusHeaderCurrCnt++;
                }
                short num = IPAddress.HostToNetworkOrder(ModbusHeaderCurrCnt);
                byte[] ModbusHeaderCurrCntBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusHeaderCurrCntBy, 0, SendDataBuf, CopyIndex, ModbusHeaderCurrCntBy.Length);
                CopyIndex += ModbusHeaderCurrCntBy.Length;

                num = IPAddress.HostToNetworkOrder(ModbusProtocolFlag);
                byte[] ModbusProtocolFlagBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusProtocolFlagBy, 0, SendDataBuf, CopyIndex, ModbusProtocolFlagBy.Length);
                CopyIndex += ModbusProtocolFlagBy.Length;

                num = IPAddress.HostToNetworkOrder(ModbusFunc06Code_MABPLen);
                byte[] ModbusFunc06Code_MABPLenBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusFunc06Code_MABPLenBy, 0, SendDataBuf, CopyIndex, ModbusFunc06Code_MABPLenBy.Length);
                CopyIndex += ModbusFunc06Code_MABPLenBy.Length;

                //SendDataBuf.Append(ModbusSlaveAddr);
                SendDataBuf[CopyIndex] = ModbusSlaveAddr;
                CopyIndex += 1;

                //Modbus报文功能字
                //SendDataBuf.Append(ModbusFunc06Code);
                SendDataBuf[CopyIndex] = ModbusFunc06Code;
                CopyIndex += 1;

                //Modbus报文数据域
                num = IPAddress.HostToNetworkOrder(dataAddr);
                byte[] dataAddrBy = BitConverter.GetBytes(num);
                Array.Copy(dataAddrBy, 0, SendDataBuf, CopyIndex, dataAddrBy.Length);
                CopyIndex += dataAddrBy.Length;

                num = IPAddress.HostToNetworkOrder(dataLen);
                byte[] dataLenBy = BitConverter.GetBytes(num);
                Array.Copy(dataLenBy, 0, SendDataBuf, CopyIndex, dataLenBy.Length);
                CopyIndex += dataLenBy.Length;

                //返回拼接的数据包
                return SendDataBuf;
            }

            /// <summary>
            /// Modbus功能字10报文
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public byte[] ToAraayWithModbus_10SendPack(short dataAddr, short registerNum, byte[] databuf, byte dataLen)
            {
                int CopyIndex = 0;

                //Modbus报文数据缓冲区
                short ModbusFunc10Code_BufLen = 0x0b;
                byte[] SendDataBuf = new byte[ModbusMBAPHeadLength + ModbusFunc10Code_BufLen];

                //MBAP报文头
                if (ModbusHeaderCurrCnt == ModbusHeaderMaxCnt)
                {
                    ModbusHeaderCurrCnt = 0x0000;

                }
                else
                {
                    ModbusHeaderCurrCnt++;
                }
                short num = IPAddress.HostToNetworkOrder(ModbusHeaderCurrCnt);
                byte[] ModbusHeaderCurrCntBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusHeaderCurrCntBy, 0, SendDataBuf, CopyIndex, ModbusHeaderCurrCntBy.Length);
                CopyIndex += ModbusHeaderCurrCntBy.Length;

                num = IPAddress.HostToNetworkOrder(ModbusProtocolFlag);
                byte[] ModbusProtocolFlagBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusProtocolFlagBy, 0, SendDataBuf, CopyIndex, ModbusProtocolFlagBy.Length);
                CopyIndex += ModbusProtocolFlagBy.Length;

                num = IPAddress.HostToNetworkOrder(ModbusFunc10Code_MABPLen);
                byte[] ModbusFunc10Code_MABPLenBy = BitConverter.GetBytes(num);
                Array.Copy(ModbusFunc10Code_MABPLenBy, 0, SendDataBuf, CopyIndex, ModbusFunc10Code_MABPLenBy.Length);
                CopyIndex += ModbusFunc10Code_MABPLenBy.Length;

                //SendDataBuf.Append(ModbusSlaveAddr);
                SendDataBuf[CopyIndex] = ModbusSlaveAddr;
                CopyIndex += 1;

                //Modbus报文功能字
                SendDataBuf[CopyIndex] = ModbusFunc10Code;
                CopyIndex += 1;

                //Modbus报文数据域
                num = IPAddress.HostToNetworkOrder(dataAddr);
                byte[] dataAddrBy = BitConverter.GetBytes(num);
                Array.Copy(dataAddrBy, 0, SendDataBuf, CopyIndex, dataAddrBy.Length);
                CopyIndex += dataAddrBy.Length;

                num = IPAddress.HostToNetworkOrder(registerNum);
                byte[] registerNumBy = BitConverter.GetBytes(num);
                Array.Copy(registerNumBy, 0, SendDataBuf, CopyIndex, registerNumBy.Length);
                CopyIndex += registerNumBy.Length;

                SendDataBuf[CopyIndex] = dataLen;
                CopyIndex += 1;

                //拷贝数据
                //Array.Copy(databuf, 0, SendDataBuf, CopyIndex, dataLen);
                //CopyIndex += dataLen;

                SendDataBuf[CopyIndex] = databuf[3];
                CopyIndex += 1;
                SendDataBuf[CopyIndex] = databuf[2];
                CopyIndex += 1;
                SendDataBuf[CopyIndex] = databuf[1];
                CopyIndex += 1;
                SendDataBuf[CopyIndex] = databuf[0];
                CopyIndex += 1;

                //返回拼接的数据包
                return SendDataBuf;
            }
        }

        /// <summary>
        /// RGV控制命令
        /// </summary>
        public class RgvModCtrlFun
        {
            public static byte[] RgvModCtrlCmd_NormalStop()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_06SendPack(0x0001, 0x0000);
            }
            public static byte[] RgvModCtrlCmd_ForwardMotor()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_06SendPack(0x0001, 0x0001);
            }
            public static byte[] RgvModCtrlCmd_BackwardMotor()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_06SendPack(0x0001, 0x0002);
            }
            public static byte[] RgvModCtrlCmd_ClearAlarm()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_06SendPack(0x0001, 0x0007);
            }
            public static byte[] RgvModCtrlCmd_StartPowerCharge()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_06SendPack(0x0001, 0x000b);
            }
            public static byte[] RgvModCtrlCmd_StopPowerCharge()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_06SendPack(0x0001, 0x000c);
            }
            
            //查询运行状态
            public static byte[] RgvModCtrlCmd_QureyStat()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_03SendPack(0x0012, 0x000b);
            }

            //运动到指定距离
            public static byte[] RgvModCtrlCmd_RunAppointDistance()
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();
                return pack.ToAraayWithModbus_06SendPack(0x0001, 0x0005);
            }

            //设置目标运行速度
            public static byte[] RgvModCtrlCmd_SetTargetSpeed(int speed)
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();

                byte[] databuf = BitConverter.GetBytes(speed);
                return pack.ToAraayWithModbus_10SendPack(0x000a, 0x0002, databuf, 4);
            }

            //设置目标运行距离
            public static byte[] RgvModCtrlCmd_SetTargetDistance(int diatance)
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();

                byte[] databuf = BitConverter.GetBytes(diatance);
                return pack.ToAraayWithModbus_10SendPack(0x0004, 0x0002, databuf, 4);
            }

            //设置轨道长度
            public static byte[] RgvModCtrlCmd_SetTrackLength(int length)
            {
                MoubusProtocolFuncPack pack = new MoubusProtocolFuncPack();

                byte[] databuf = BitConverter.GetBytes(length);
                return pack.ToAraayWithModbus_10SendPack(0x0002, 0x0002, databuf, 4);
            }
        }
    }
}

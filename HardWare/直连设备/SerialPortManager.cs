using System;
using System.IO.Ports;
using System.Text;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.HardWare
{
    public class SerialPortManager
    {
        private SerialPort serialPort;
        public Action<string> ReadValue;

        public SerialPortManager(string com, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            try
            {
                serialPort = new SerialPort(com, baudRate, parity, dataBits, stopBits);
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.ErrorReceived += SerialPort_ErrorReceived;
                serialPort.PinChanged += SerialPort_PinChanged;
                serialPort.Open();
            }
            catch (Exception e)
            {
                Common.GlobalValues.AddLog(e.Message);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[serialPort.BytesToRead];
            int length = serialPort.Read(buffer, 0, buffer.Length);
            if (length > 0)
            {
                string value = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                TaskRun(() => { ReadValue?.Invoke(value); });
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            
        }

        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            
        }
    }
}

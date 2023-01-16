using PylonC.NET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Basler
{
    public class CameraManager
    {
        #region Propeties
        private int _numberOfCameras;
        private List<ICamera> _cameras;

        public int NumberOfCameras
        {
            get { return _numberOfCameras; }
            set { _numberOfCameras = value; }
        }
        public List<ICamera> Cameras
        {
            get { return _cameras; }
            set { _cameras = value; }
        }
        #endregion

        #region Methods
        public CameraManager()
        {

        }

        private List<ICamera> EnumerateDevices()
        {
            List<BaslerCamera> list = new List<BaslerCamera>();
            uint count = Pylon.EnumerateDevices();

            for (uint i = 0; i < count; ++i)
            {
                BaslerCamera _device = new BaslerCamera(i);
                PYLON_DEVICE_INFO_HANDLE hDi = Pylon.GetDeviceInfoHandle(i);
                _device.CameraID = (int)i;
                _device.Name = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoUserDefinedNameKey);
                _device.FullName = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoFullNameKey);
                _device.ModelName = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoModelNameKey);
                _device.SerialNumber = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoSerialNumberKey);
                _device.VendorName = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoVendorNameKey);
                _device.MacAddress = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoMacAddressKey);
                _device.IPAddress = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoIpAddressKey);
                _device.Port = Int32.Parse(Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoPortNrKey));

                string tooltip = "";
                uint propertyCount = Pylon.DeviceInfoGetNumProperties(hDi);

                if (propertyCount > 0)
                {
                    for (uint j = 0; j < propertyCount; j++)
                    {
                        tooltip += Pylon.DeviceInfoGetPropertyName(hDi, j) + ": " + Pylon.DeviceInfoGetPropertyValueByIndex(hDi, j);
                        if (j != propertyCount - 1)
                        {
                            tooltip += "\n";
                        }
                    }
                }

                list.Add(_device);
            }
            if (list != null && list.Count > 0)
            {
                list = list.OrderBy(m => m.Name).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CameraID = i;
                }
            }
            return list.ToList<ICamera>();
        }

        public void Initialise()
        {
            Environment.SetEnvironmentVariable("PYLON_GIGE_HEARTBEAT", "300000");
            Pylon.Initialize();
            DetectCameras();
        }

        public void DetectCameras()
        {
            _cameras = EnumerateDevices();
            _numberOfCameras = _cameras.Count;
        }

        public void Close()
        {
            Pylon.Terminate();
        }
        #endregion
    }
}

using PylonC.NET;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Basler
{
    public class BaslerImageProvider
    {
        protected class GrabResult
        {
            public RawImageData ImageData;
            public PYLON_STREAMBUFFER_HANDLE Handle;
        }

        protected bool _converterOutputFormatIsColor = false;
        protected PYLON_IMAGE_FORMAT_CONVERTER_HANDLE _hConverter;
        protected Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> _convertedBuffers;
        protected PYLON_DEVICE_HANDLE _hDevice;
        protected PYLON_STREAMGRABBER_HANDLE _hGrabber;
        protected PYLON_DEVICECALLBACK_HANDLE _hRemovalCallback;
        protected PYLON_WAITOBJECT_HANDLE _hWait;
        protected uint _numberOfBuffersUsed = 50;
        protected bool _grabThreadRun = false;
        private object _grabThreadRunLock = new object();
        protected bool _open = false;
        protected bool _grabOnce = false;
        protected bool _removed = false;
        protected bool _triggered = false;
        protected Thread _grabThread;
        protected Object _lockObject;
        protected Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<byte>> _buffers;
        protected List<GrabResult> _grabbedBuffers;
        protected DeviceCallbackHandler _callbackHandler;
        public int CameraID;

        public event DeviceEventHandler DeviceEvent;
        public event ImageReadyEventHandler ImageReadyEvent;
        public event GrabErrorEventHandler GrabErrorEvent;
        public event Action<Exception> ErrorEvent;
        public event Action<RawImageData> BackImageEvent;

        public BaslerImageProvider()
        {
            _grabThread = new Thread(Grab);
            _lockObject = new Object();
            _buffers = new Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>>();
            _grabbedBuffers = new List<GrabResult>();
            _hGrabber = new PYLON_STREAMGRABBER_HANDLE();
            _hDevice = new PYLON_DEVICE_HANDLE();
            _hRemovalCallback = new PYLON_DEVICECALLBACK_HANDLE();
            _hConverter = new PYLON_IMAGE_FORMAT_CONVERTER_HANDLE();
            _callbackHandler = new DeviceCallbackHandler();
            _callbackHandler.CallbackEvent += new DeviceCallbackHandler.DeviceCallback(RemovalCallbackHandler);
        }

        public bool IsOpen
        {
            get { return _open; }
        }

        public void Open(uint index)
        {
            Open(Pylon.CreateDeviceByIndex(index));
        }

        public void Close()
        {
            try
            {
                Thread.Sleep(20);
                if (_grabThread != null && _grabThread.ThreadState == ThreadState.Running)
                {
                    _grabThread.Abort();
                }
                OnDeviceClosingEvent();

                _removed = false;

                if (_hGrabber.IsValid)
                {
                    try
                    {
                        Pylon.StreamGrabberClose(_hGrabber);
                    }
                    catch (Exception e)
                    {
                        ErrorEvent?.Invoke(e);
                    }
                }

                if (_hDevice.IsValid)
                {
                    try
                    {
                        if (_hRemovalCallback.IsValid)
                        {
                            Pylon.DeviceDeregisterRemovalCallback(_hDevice, _hRemovalCallback);
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorEvent?.Invoke(e);
                    }

                    try
                    {
                        if (Pylon.DeviceIsOpen(_hDevice))
                        {
                            Pylon.DeviceClose(_hDevice);
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorEvent?.Invoke(e);
                    }

                    try
                    {
                        Pylon.DestroyDevice(_hDevice);
                    }
                    catch (Exception e)
                    {
                        ErrorEvent?.Invoke(e);
                    }
                }

                _hGrabber.SetInvalid();
                _hRemovalCallback.SetInvalid();
                _hDevice.SetInvalid();
                OnDeviceClosedEvent();
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex);
            }
        }

        public PYLON_DEVICE_HANDLE GetDeviceHandle()
        {
            return _hDevice;
        }

        public void SetTriggerMode(bool isTriggerMode, bool IsMultiframe)
        {
            try
            {
                _triggered = isTriggerMode;
                if (_triggered)
                {
                    if (IsMultiframe == true)
                    {
                        SetTriggerSelector(TriggerSelector.AcquisitionStart);
                    }
                    Pylon.DeviceFeatureFromString(_hDevice, "TriggerMode", "On");
                }
                else
                {
                    SetTriggerSelector(TriggerSelector.AcquisitionStart);
                    Pylon.DeviceFeatureFromString(_hDevice, "TriggerMode", "Off");
                    SetTriggerSelector(TriggerSelector.FrameStart);
                    Pylon.DeviceFeatureFromString(_hDevice, "TriggerMode", "Off");
                }
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex);
            }
        }

        public void SetTriggerSelector(TriggerSelector value)
        {
            Pylon.DeviceFeatureFromString(_hDevice, "TriggerSelector", Enum.GetName(typeof(TriggerSelector), value));
        }

        public void SetTriggerSource(TriggerSource value)
        {
            Pylon.DeviceFeatureFromString(_hDevice, "TriggerSource", Enum.GetName(typeof(TriggerSource), value));
        }

        public void SetAcquisitionFrameCount(int acquisitionFrameCount)
        {
            Pylon.DeviceFeatureFromString(_hDevice, "AcquisitionFrameCount", acquisitionFrameCount.ToString());
        }

        public void SetGammaEnable(bool enable)
        {
            Pylon.DeviceFeatureFromString(_hDevice, "GammaEnable", enable ? "1" : "0");
        }

        private bool GetGrabThreadRun()
        {
            lock (_grabThreadRunLock)
            {
                return _grabThreadRun;
            }
        }

        private void SetGrabThreadRun(bool b)
        {
            lock (_grabThreadRunLock)
            {
                _grabThreadRun = b;
            }
        }

        public void OneShot()
        {
            if (_open && !_grabThread.IsAlive)
            {
                _numberOfBuffersUsed = 1;
                _grabOnce = true;
                SetGrabThreadRun(true);
                _grabThread = new Thread(Grab);
                _grabThread.Start();
            }
        }

        public void ContinuousShot()
        {
            if (_open && !_grabThread.IsAlive)
            {
                _numberOfBuffersUsed = 5;
                _grabOnce = false;
                SetGrabThreadRun(true);
                _grabThread = new Thread(Grab);
                _grabThread.Start();
            }
        }

        public void Stop()
        {
            SetGrabThreadRun(false);
            if (_open && _grabThread.IsAlive)
            {
                _grabThread.Abort();
            }
        }

        public void TriggerShot()
        {
            if (_open && _grabThread.IsAlive)
            {
                Pylon.DeviceExecuteCommandFeature(_hDevice, "TriggerSoftware");
            }
        }

        public RawImageData GetCurrentImage()
        {
            lock (_lockObject)
            {
                if (_grabbedBuffers.Count > 0)
                {
                    return (_grabbedBuffers[0].ImageData);
                }
            }
            return null;
        }

        public RawImageData GetLatestImage()
        {
            lock (_lockObject)
            {
                while (_grabbedBuffers.Count > 1)
                {
                    ReleaseImage();
                }

                if (_grabbedBuffers.Count > 0)
                {
                    return (_grabbedBuffers[0].ImageData);
                }
            }
            return null;
        }

        public bool ReleaseImage()
        {
            lock (_lockObject)
            {
                if (_grabbedBuffers.Count > 0)
                {
                    if (GetGrabThreadRun())
                    {
                        Pylon.StreamGrabberQueueBuffer(_hGrabber, _grabbedBuffers[0].Handle, 0);
                    }
                    _grabbedBuffers.RemoveAt(0);
                    return true;
                }
            }
            return false;
        }

        private string GetLastErrorText()
        {
            string lastErrorMessage = GenApi.GetLastErrorMessage();
            string lastErrorDetail = GenApi.GetLastErrorDetail();

            string lastErrorText = lastErrorMessage;
            if (lastErrorDetail.Length > 0)
            {
                lastErrorText += "\n\nDetails:\n";
            }
            lastErrorText += lastErrorDetail;
            return lastErrorText;
        }

        public NODE_HANDLE GetNodeFromDevice(string name)
        {
            if (_open && !_removed)
            {
                NODEMAP_HANDLE hNodemap = Pylon.DeviceGetNodeMap(_hDevice);
                return GenApi.NodeMapGetNode(hNodemap, name);
            }
            return new NODE_HANDLE();
        }

        protected void Open(PYLON_DEVICE_HANDLE device)
        {
            try
            {
                _hDevice = device;
                Pylon.DeviceOpen(_hDevice, Pylon.cPylonAccessModeControl | Pylon.cPylonAccessModeStream);
                _hRemovalCallback = Pylon.DeviceRegisterRemovalCallback(_hDevice, _callbackHandler);

                if (Pylon.DeviceGetNumStreamGrabberChannels(_hDevice) < 1)
                {
                    ErrorEvent?.Invoke(new Exception("The transport layer doesn't support image streams."));
                    return;
                }

                _hGrabber = Pylon.DeviceGetStreamGrabber(_hDevice, 0);
                Pylon.StreamGrabberOpen(_hGrabber);
                _hWait = Pylon.StreamGrabberGetWaitObject(_hGrabber);
                PYLON_DEVICECALLBACK_HANDLE hCb = Pylon.DeviceRegisterRemovalCallback(_hDevice, _callbackHandler);
                PYLON_DEVICE_INFO_HANDLE hDi = Pylon.DeviceGetDeviceInfoHandle(_hDevice);
                string deviceClass = Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoDeviceClassKey);
                bool isGigECamera = deviceClass == "BaslerGigE";

                if (isGigECamera)
                {
                    SetHeartbeatTimeout(_hDevice, 10000);
                }

                OnDeviceOpenedEvent();
            }
            catch (Exception e)
            {
                try
                {
                    Close();
                }
                catch (Exception ex)
                {
                    ErrorEvent?.Invoke(ex);
                }
                ErrorEvent?.Invoke(e);
            }
        }

        public void SetupGrab()
        {
            lock (_lockObject)
            {
                _grabbedBuffers.Clear();
            }

            if (_grabOnce)
            {
                Pylon.DeviceFeatureFromString(_hDevice, "AcquisitionMode", "SingleFrame");
            }
            else
            {
                Pylon.DeviceFeatureFromString(_hDevice, "AcquisitionMode", "Continuous");
            }

            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<byte>> pair in _buffers)
            {
                pair.Value.Dispose();
            }
            _buffers.Clear();

            uint payloadSize = checked((uint)Pylon.DeviceGetIntegerFeature(_hDevice, "PayloadSize"));
            Pylon.StreamGrabberSetMaxNumBuffer(_hGrabber, _numberOfBuffersUsed);
            Pylon.StreamGrabberSetMaxBufferSize(_hGrabber, payloadSize);
            Pylon.StreamGrabberPrepareGrab(_hGrabber);

            for (uint i = 0; i < _numberOfBuffersUsed; ++i)
            {
                PylonBuffer<byte> buffer = new PylonBuffer<byte>(payloadSize, true);
                PYLON_STREAMBUFFER_HANDLE handle = Pylon.StreamGrabberRegisterBuffer(_hGrabber, ref buffer);
                _buffers.Add(handle, buffer);
            }

            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<byte>> pair in _buffers)
            {
                Pylon.StreamGrabberQueueBuffer(_hGrabber, pair.Key, 0);
            }

            _hConverter.SetInvalid();
            Pylon.DeviceExecuteCommandFeature(_hDevice, "AcquisitionStart");
        }

        private static long SetHeartbeatTimeout(PYLON_DEVICE_HANDLE hDev, long timeout_ms)
        {
            NODEMAP_HANDLE hNodemap;
            NODE_HANDLE hNode;
            long oldTimeout;
            hNodemap = Pylon.DeviceGetTLNodeMap(hDev);

            if (!hNodemap.IsValid)
            {
                return -1;
            }
            hNode = GenApi.NodeMapGetNode(hNodemap, "HeartbeatTimeout");
            if (!hNode.IsValid)
            {
                return -1;
            }

            oldTimeout = GenApi.IntegerGetValue(hNode);
            GenApi.IntegerSetValue(hNode, timeout_ms);
            return oldTimeout;
        }

        protected void Grab()
        {
            OnGrabbingStartedEvent();

            try
            {
                SetupGrab();

                while (GetGrabThreadRun())
                {
                    if (!Pylon.WaitObjectWait(_hWait, 1000))
                    {
                        lock (_lockObject)
                        {
                            if (!_triggered && _grabOnce)
                            {
                                ErrorEvent?.Invoke(new Exception("A grab timeout occurred."));
                                return;
                            }
                            continue;
                        }
                    }

                    PylonGrabResult_t grabResult; 
                    if (!Pylon.StreamGrabberRetrieveResult(_hGrabber, out grabResult))
                    {
                        ErrorEvent?.Invoke(new Exception("Failed to retrieve a grab result."));
                        return;
                    }

                    if (grabResult.Status == EPylonGrabStatus.Grabbed)
                    {
                        EnqueueTakenImage(grabResult);
                        
                        if (!_triggered)
                            OnGrabbingStoppedEvent();

                        new Thread(new ThreadStart(OnImageReadyEvent)).Start();

                        if (_grabOnce == true)
                        {
                            SetGrabThreadRun(false);
                            break;
                        }
                    }
                    else if (grabResult.Status == EPylonGrabStatus.Failed && _grabOnce)
                    {
                        ErrorEvent?.Invoke(new Exception(string.Format("A grab failure occurred. See the method ImageProvider::Grab for more information. The error code is {0:X08}.", grabResult.ErrorCode)));
                        return;
                    }
                    else if (grabResult.Status == EPylonGrabStatus.Failed)
                    {
                        ErrorEvent?.Invoke(new Exception("Status：Failed"));
                        return;
                    }
                }
                CleanUpGrab();
            }
            catch (Exception e)
            {
                SetGrabThreadRun(false);
                string lastErrorMessage = GetLastErrorText();

                try
                {
                    CleanUpGrab();
                }
                catch (Exception ex)
                {
                    ErrorEvent?.Invoke(e);
                    ErrorEvent?.Invoke(ex);
                    return;
                }

                OnGrabbingStoppedEvent();

                if (!_removed)
                {
                    OnGrabErrorEvent(e, lastErrorMessage);
                }
                ErrorEvent?.Invoke(e);
            }
        }

        protected void EnqueueTakenImage(PylonGrabResult_t grabResult)
        {
            PylonBuffer<byte> buffer;

            if (!_buffers.TryGetValue(grabResult.hBuffer, out buffer))
            {
                ErrorEvent?.Invoke(new Exception("Failed to find the buffer associated with the handle returned in grab result."));
                return;
            }

            GrabResult newGrabResultInternal = new GrabResult();
            newGrabResultInternal.Handle = grabResult.hBuffer;

            if (grabResult.PixelType == EPylonPixelType.PixelType_Mono8 || grabResult.PixelType == EPylonPixelType.PixelType_RGBA8packed || grabResult.PixelType == EPylonPixelType.PixelType_BayerRG8 || grabResult.PixelType == EPylonPixelType.PixelType_BayerBG8 || grabResult.PixelType == EPylonPixelType.PixelType_BayerGB8)
            {
                newGrabResultInternal.ImageData = new RawImageData(grabResult.SizeX, grabResult.SizeY, buffer.Array, grabResult.PixelType == EPylonPixelType.PixelType_RGBA8packed);
            }
            else if (grabResult.PixelType == EPylonPixelType.PixelType_Mono12 || grabResult.PixelType == EPylonPixelType.PixelType_Mono16 || grabResult.PixelType == EPylonPixelType.PixelType_BayerRG12 || grabResult.PixelType == EPylonPixelType.PixelType_BayerBG12)
            {
                newGrabResultInternal.ImageData = new RawImageData(grabResult.SizeX, grabResult.SizeY, buffer.Array, true);
            }
            else
            {
                if (!_hConverter.IsValid)
                {
                    _convertedBuffers = new Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<byte>>();
                    _hConverter = Pylon.ImageFormatConverterCreate();
                    _converterOutputFormatIsColor = !Pylon.IsMono(grabResult.PixelType) || Pylon.IsBayer(grabResult.PixelType);
                }

                PylonBuffer<byte> convertedBuffer = null;
                bool bufferListed = _convertedBuffers.TryGetValue(grabResult.hBuffer, out convertedBuffer);
                Pylon.ImageFormatConverterSetOutputPixelFormat(_hConverter, _converterOutputFormatIsColor ? EPylonPixelType.PixelType_BGRA8packed : EPylonPixelType.PixelType_Mono8);
                Pylon.ImageFormatConverterConvert(_hConverter, ref convertedBuffer, buffer, grabResult.PixelType, (uint)grabResult.SizeX, (uint)grabResult.SizeY, (uint)grabResult.PaddingX, EPylonImageOrientation.ImageOrientation_TopDown);
                if (!bufferListed)
                {
                    _convertedBuffers.Add(grabResult.hBuffer, convertedBuffer);
                }
                newGrabResultInternal.ImageData = new RawImageData(grabResult.SizeX, grabResult.SizeY, convertedBuffer.Array, _converterOutputFormatIsColor);
            }

            BackImageEvent?.Invoke(newGrabResultInternal.ImageData);
            lock (_lockObject)
            {
                _grabbedBuffers.Add(newGrabResultInternal);
            }
        }

        protected void CleanUpGrab()
        {
            Pylon.DeviceExecuteCommandFeature(_hDevice, "AcquisitionStop");

            if (_hConverter.IsValid)
            {
                Pylon.ImageFormatConverterDestroy(_hConverter);
                _hConverter.SetInvalid();
                foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> pair in _convertedBuffers)
                {
                    pair.Value.Dispose();
                }
                _convertedBuffers = null;
            }

            Pylon.StreamGrabberCancelGrab(_hGrabber);
            bool isReady;
            do
            {
                PylonGrabResult_t grabResult;
                isReady = Pylon.StreamGrabberRetrieveResult(_hGrabber, out grabResult);

            } while (isReady);

            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<byte>> pair in _buffers)
            {
                Pylon.StreamGrabberDeregisterBuffer(_hGrabber, pair.Key);
            }

            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<byte>> pair in _buffers)
            {
                pair.Value.Dispose();
            }
            _buffers.Clear();
            Pylon.StreamGrabberFinishGrab(_hGrabber);
        }

        protected void RemovalCallbackHandler(PYLON_DEVICE_HANDLE hDevice)
        {        
            OnDeviceRemovedEvent();
        }

        protected void OnDeviceOpenedEvent()
        {
            _open = true;
            if (DeviceEvent != null)
            {
                DeviceEvent(CameraStatusEvent.DEVICE_OPENED);
            }
        }

        protected void OnDeviceClosingEvent()
        {
            _open = false;
            if (DeviceEvent != null)
            {
                DeviceEvent(CameraStatusEvent.DEVICE_CLOSING);
            }
        }

        protected void OnDeviceClosedEvent()
        {
            _open = false;
            if (DeviceEvent != null)
            {
                DeviceEvent(CameraStatusEvent.DEVICE_CLOSED);
            }
        }

        protected void OnGrabbingStartedEvent()
        {
            if (DeviceEvent != null)
            {
                DeviceEvent(CameraStatusEvent.GRABBING_STARTED);
            }
        }

        protected void OnGrabbingStoppedEvent()
        {
            if (DeviceEvent != null)
            {
                DeviceEvent(CameraStatusEvent.GRABBING_STOPPED);
            }
        }

        protected void OnDeviceRemovedEvent()
        {
            _removed = true;
            SetGrabThreadRun(false);
            if (DeviceEvent != null)
            {
                DeviceEvent(CameraStatusEvent.DEVICE_REMOVED);
            }
        }

        protected void OnImageReadyEvent()
        {
            ImageReadyEvent?.Invoke(this, new CameraEventArgs());
        }

        protected void OnGrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            if (GrabErrorEvent != null)
            {
                GrabErrorEvent(grabException, additionalErrorMessage);
            }
        }

        private void GetDeviceInfo(PYLON_DEVICE_HANDLE hDevice, out Dictionary<string, string> deviceDetails)
        {
            PYLON_DEVICE_INFO_HANDLE hDi = Pylon.DeviceGetDeviceInfoHandle(hDevice);
            deviceDetails = new Dictionary<string, string>();
            deviceDetails.Add(BaslerDeviceDetails.InterfaceKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoInterfaceKey));
            deviceDetails.Add(BaslerDeviceDetails.IpAddressKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoIpAddressKey));
            deviceDetails.Add(BaslerDeviceDetails.ConfigCurrentKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoIpConfigCurrentKey));
            deviceDetails.Add(BaslerDeviceDetails.IpConfigOptionsKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoIpConfigOptionsKey));
            deviceDetails.Add(BaslerDeviceDetails.MacAddressKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoMacAddressKey));
            deviceDetails.Add(BaslerDeviceDetails.ModelNameKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoModelNameKey));
            deviceDetails.Add(BaslerDeviceDetails.PortNrKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoPortNrKey));
            deviceDetails.Add(BaslerDeviceDetails.SubnetAddressKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoSubnetAddressKey));
            deviceDetails.Add(BaslerDeviceDetails.SubnetMaskKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoSubnetMaskKey));
            deviceDetails.Add(BaslerDeviceDetails.UserDefinedNameKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoUserDefinedNameKey));
            deviceDetails.Add(BaslerDeviceDetails.VendorNameKey.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoVendorNameKey));
            deviceDetails.Add(BaslerDeviceDetails.Serial.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoSerialNumberKey));
            deviceDetails.Add(BaslerDeviceDetails.Version.ToString(), Pylon.DeviceInfoGetPropertyValueByName(hDi, Pylon.cPylonDeviceInfoDeviceVersionKey));
        }

        public void SaveUserSetting()
        {
            if (Pylon.DeviceFeatureIsAvailable(_hDevice, "UserSetSelector"))
            {
                Pylon.DeviceFeatureFromString(_hDevice, "UserSetSelector", "UserSet1");
                Pylon.DeviceExecuteCommandFeature(_hDevice, "UserSetSave");
            }
        }
    }
}

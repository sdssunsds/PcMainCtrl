using PylonC.NET;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Basler
{
    public class BaslerCamera : ICamera
    {
        private string _ipAddress;
        private int _port;
        private string _fullName;
        private string _modelName;
        private string _serialNumber;
        private string _vendorName;
        private string _macAddress;
        private int _cameraID;
        private uint _baslerID;
        protected BaslerImageProvider _imageProvider;
        private bool _opened;
        BaslerImage _image;
        
        private int _width;
        private int _widthMax;
        private int _height;
        private int _heightMax;
        private int _offsetX;
        private int _offsetY;
        private int _gain;
        private int _gainMax;
        private double _exposureTime;
        private double _exposureTimeMax;
        private string _format = "Mono8";
        private int _bitDepth = 8;
        private BaslerImage.ImageRotation _rotation = BaslerImage.ImageRotation.None;
        private bool _triggerMode = false;
        private bool _isMultiframe = false;
        private int _acquisitionFrameCount = 1;
        private TriggerSelector _triggerSelector = TriggerSelector.FrameStart;
        private TriggerSource _triggerSource = TriggerSource.Line1;
        private bool _gammaEnable = false;
        private int _packetSize;

        #region Camera Information
        [Category("Camera Information")]
        public string Name { get; set; }

        [Category("Camera Information")]
        public string IPAddress { get => _ipAddress; set => _ipAddress = value; }

        [DefaultValue(0)]
        [Category("Camera Information")]
        public int Port { get => _port; set => _port = value; }

        [DefaultValue(0)]
        [Category("Camera Information")]
        public DeviceStatus Status { get; set; }

        [Category("Camera Information")]
        public int CameraID
        {
            get { return _cameraID; }
            set { _cameraID = value; }
        }

        [Category("Camera Information")]
        public uint BaslerID
        {
            get { return _baslerID; }
            set { _baslerID = value; }
        }

        [Category("Camera Information")]
        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; }
        }

        [Category("Camera Information")]
        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

        [Category("Camera Information")]
        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; }
        }

        [Category("Camera Information")]
        public string VendorName
        {
            get { return _vendorName; }
            set { _vendorName = value; }
        }

        [Category("Camera Information")]
        public string MacAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; }
        }

        [Category("Camera Information")]
        public bool TriggerMode
        {
            get { return _triggerMode; }
            set
            {
                _triggerMode = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                {
                    _imageProvider.SetTriggerMode(_triggerMode, IsMultiframe);
                    _triggerSelector = (IsMultiframe == true && _triggerMode == true) ? TriggerSelector.AcquisitionStart : TriggerSelector.FrameStart;
                }
                if (value)
                {
                    ContinuousShot();
                }
                else
                {
                    Stop();
                }
            }
        }

        [Browsable(false)]
        [Category("Camera Information")]
        public bool IsMultiframe
        {
            get { return _isMultiframe; }
            set
            {
                _isMultiframe = value;
            }
        }

        [Category("Camera Information")]
        public int AcquisitionFrameCount
        {
            get { return _acquisitionFrameCount; }
            set
            {
                if (!_triggerMode)
                {
                    _acquisitionFrameCount = value;
                    if (_imageProvider != null && _imageProvider.IsOpen)
                    {
                        _imageProvider.SetAcquisitionFrameCount(value);
                    }
                }
            }
        }

        [Category("Camera Information")]
        public TriggerSelector TriggerSelector
        {
            get { return _triggerSelector; }
            set
            {
                _triggerSelector = value;
            }
        }

        [Category("Camera Information")]
        public TriggerSource TriggerSource
        {
            get { return _triggerSource; }
            set
            {
                _triggerSource = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetTriggerSource(value);
            }

        }

        [Category("Camera Information")]
        public bool Opened
        {
            get { return _opened; }
            set
            {
                _opened = value;
            }
        }

        [Category("Camera Information")]
        public string OfflineImageFolder
        {
            get { return null; }
            set { }
        }

        [Category("Camera Information")]
        public string BackImagePath { get; set; }
        #endregion

        #region Image Properties
        [Category("Image Properties")]
        public BaslerImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
            }
        }

        [Category("Image Properties")]
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetWidth(value);
            }
        }

        [Category("Image Properties")]
        public int WidthMax
        {
            get { return _widthMax; }
            set
            {
                _widthMax = value;
            }
        }

        [Category("Image Properties")]
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetHeight(value);
            }
        }

        [Category("Image Properties")]
        public int HeightMax
        {
            get { return _heightMax; }
            set
            {
                _heightMax = value;
            }
        }

        [Category("Image Properties")]
        public int OffsetX
        {
            get { return _offsetX; }
            set
            {
                _offsetX = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetOffsetX(value);
            }
        }

        [Category("Image Properties")]
        public int OffsetY
        {
            get { return _offsetY; }
            set
            {
                _offsetY = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetOffsetY(value);
            }
        }

        [Category("Image Properties")]
        public int Gain
        {
            get { return _gain; }
            set
            {
                _gain = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetGain(value);
            }
        }

        [Category("Image Properties")]
        public int GainMax
        {
            get { return _gainMax; }
            set
            {
                _gainMax = value;
            }
        }

        [Category("Image Properties")]
        public double ExposureTime
        {
            get { return _exposureTime; }
            set
            {
                _exposureTime = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetExposureTimeAbs(value);
            }
        }

        [Category("Image Properties")]
        public double ExposureTimeMax
        {
            get { return _exposureTimeMax; }
            set
            {
                _exposureTimeMax = value;
            }
        }

        [Category("Image Properties")]
        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                    SetImageFormat(_format);
            }
        }

        [Category("Image Properties")]
        public int BitDepth
        {
            get { return _bitDepth; }
            set { _bitDepth = value; }
        }

        [Category("Image Properties")]
        public BaslerImage.ImageRotation Rotation { get => _rotation; set => _rotation = value; }

        [Category("Image Properties")]
        public int PacketSize
        {
            get { return _packetSize; }
            set
            {
                _packetSize = value;
                if (_imageProvider != null && _imageProvider.IsOpen)
                {
                    SetPackageSize(_packetSize);
                }
            }
        }

        [Category("Image Properties")]
        public bool GammaEnable
        {
            get { return _gammaEnable; }
            set
            {
                if (_imageProvider != null && _imageProvider.IsOpen)
                {
                    _gammaEnable = value;
                    _imageProvider.SetGammaEnable(value);
                }
            }
        }
        #endregion 

        #region Events  
        public event ImageReadyEventHandler ImageReady;
        public event CameraErrorEventHandler CameraError;
        public event CameraStatusChangedEventHandler CameraStatusChanged;
        #endregion

        #region Metholds      
        public BaslerCamera()
        {
            RobotCtrl.Lib.Manager.Init();
        }

        public BaslerCamera(uint index) : this()
        {
            _baslerID = index;
        }

        public BaslerCamera(ICamera device) : this()
        {
            
        }

        public override string ToString()
        {
            return Name;
        }

        public void Initialise()
        {
            Status = DeviceStatus.Warning;

            try
            {
                ImageProviderInitialise();
                Status = DeviceStatus.Ready;
            }
            catch (Exception ex)
            {
                Status = DeviceStatus.Error;
                CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = ex.Message });
            }
        }

        public void ImageProviderInitialise()
        {
            _imageProvider = new BaslerImageProvider();
            _image = new BaslerImage();
            _imageProvider.GrabErrorEvent += new GrabErrorEventHandler(OnGrabErrorEventCallback);
            _imageProvider.DeviceEvent += new DeviceEventHandler(OnDeviceEventCallback);
            _imageProvider.ImageReadyEvent += new ImageReadyEventHandler(OnImageReadyEventCallback);
            _imageProvider.ErrorEvent += new Action<Exception>(OnErrorEventCallback);
            _imageProvider.BackImageEvent += new Action<RawImageData>(OnBackImageCallback);
            _imageProvider.CameraID = CameraID;
        }

        private void OnBackImageCallback(RawImageData data)
        {
            new Thread(new ThreadStart(() => 
            {
                if (!string.IsNullOrEmpty(BackImagePath))
                {
                    string savePath = BackImagePath + ".bmp";
                    string filePath = BackImagePath + ".buffer";

                    byte[] buffer = new byte[data._buffer.Length];
                    data._buffer.CopyTo(buffer, 0);
                    RawImageData rawImageCopy = new RawImageData(data._width, data._height, buffer, data._color);

                    if (_triggerMode)
                        _image = new BaslerImage();
                    if (_format == "Mono12" || _format == "Mono16")
                        _image.CreateImage(data._width, data._height, data._buffer, _rotation, BaslerImage.ImagePixelFormat.GRAY12);
                    else if (_format == "BayerBG8")
                        _image.CreateImage(data._width, data._height, data._buffer, _rotation, BaslerImage.ImagePixelFormat.BAYERBG8);
                    else if (_format == "BayerRG8")
                        _image.CreateImage(data._width, data._height, data._buffer, _rotation, BaslerImage.ImagePixelFormat.BAYERRG8);
                    else if (_format == "BayerGB8")
                        _image.CreateImage(data._width, data._height, data._buffer, _rotation, BaslerImage.ImagePixelFormat.BAYERGB8);
                    else if (_format == "Mono8")
                        _image.CreateImage(data._width, data._height, data._buffer, _rotation);
                    else
                    {
                        _image.PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                        _image.CreateImage(data._width, data._height, data._buffer, _rotation);
                    }

                SaveBack:
                    try
                    {
                        _image.Bitmap.Save(savePath);
                    }
                    catch
                    {
                        goto SaveBack;
                    }

                DeleteBack:
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    catch
                    {
                        goto DeleteBack;
                    }

                BufferBack:
                    try
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(buffer, 0, buffer.Length);
                        }
                    }
                    catch
                    {
                        goto BufferBack;
                    }
                }
            })).Start();
        }

        private void OnErrorEventCallback(Exception e)
        {
            CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = e.Message });
        }

        private void OnImageReadyEventCallback(object sender, CameraEventArgs e)
        {
            try
            {
                RawImageData rawImage;
                rawImage = _imageProvider.GetLatestImage();

                if (rawImage != null)
                {
                    byte[] buffer = new byte[rawImage._buffer.Length];
                    rawImage._buffer.CopyTo(buffer, 0);
                    RawImageData rawImageCopy = new RawImageData(rawImage._width, rawImage._height, buffer, rawImage._color);

                    if (_triggerMode)
                        _image = new BaslerImage();
                    if (_format == "Mono12" || _format == "Mono16")
                        _image.CreateImage(rawImage._width, rawImage._height, rawImage._buffer, _rotation, BaslerImage.ImagePixelFormat.GRAY12);
                    else if (_format == "BayerBG8")
                        _image.CreateImage(rawImage._width, rawImage._height, rawImage._buffer, _rotation, BaslerImage.ImagePixelFormat.BAYERBG8);
                    else if (_format == "BayerRG8")
                        _image.CreateImage(rawImage._width, rawImage._height, rawImage._buffer, _rotation, BaslerImage.ImagePixelFormat.BAYERRG8);
                    else if (_format == "BayerGB8")
                        _image.CreateImage(rawImage._width, rawImage._height, rawImage._buffer, _rotation, BaslerImage.ImagePixelFormat.BAYERGB8);
                    else if (_format == "Mono8")
                        _image.CreateImage(rawImage._width, rawImage._height, rawImage._buffer, _rotation);
                    else
                    {
                        _image.PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                        _image.CreateImage(rawImage._width, rawImage._height, rawImage._buffer, _rotation);
                    }

                    ImageReady?.Invoke(this, new CameraEventArgs() { Image = _image, Buffer = buffer });
                }

                _imageProvider.ReleaseImage();
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = ex.Message });
            }
        }

        private void OnGrabErrorEventCallback(Exception grabException, string additionalErrorMessage)
        {
            CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = grabException.Message + additionalErrorMessage });
        }

        private void OnDeviceEventCallback(CameraStatusEvent status)
        {
            if (Status == DeviceStatus.Offline)
                return;

            switch (status)
            {
                case CameraStatusEvent.GRABBING_STARTED:
                    Console.WriteLine("GRABBING_STARTED");
                    break;
                case CameraStatusEvent.GRABBING_STOPPED:
                    Console.WriteLine("GRABBING_STOPPED");
                    break;
                case CameraStatusEvent.DEVICE_REMOVED:
                    Stop();
                    Close();
                    break;
                case CameraStatusEvent.DEVICE_CLOSED:
                    _opened = false;
                    break;
            }

            CameraStatusChanged?.Invoke(this, new CameraStatusEventArgs() { Status = status });
        }

        public BaslerImage GetLastImage()
        {
            return _image;
        }

        public void Open()
        {
            int i = 0;
            try
            {
                if (_imageProvider != null && !_imageProvider.IsOpen)
                {
                    _imageProvider.Open(_baslerID);
                    i = 1;
                    _width = GetWidth();
                    i = 2;
                    _widthMax = GetMaxWidth();
                    i = 3;
                    _height = GetHeight();
                    i = 4;
                    _heightMax = GetMaxHeight();
                    i = 5;
                    _offsetX = GetOffsetX();
                    i = 6;
                    _offsetY = GetOffsetY();
                    i = 7;
                    _exposureTime = GetExposureTimeAbs();
                    i = 8;
                    _gain = GetGain();
                    i = 9;
                    _gainMax = GetMaxGain();
                    i = 10;
                    _format = GetImageFormat();
                    i = 11;
                    _triggerMode = GetTriggerMode();
                    i = 12;
                    _triggerSelector = GetTriggerSelector();
                    i = 13;
                    _triggerSource = GetTriggerSource();
                    i = 14;
                    _acquisitionFrameCount = GetAcquisitionFrameCount();
                    i = 15;
                    _packetSize = GetPacketSize();
                    i = 16;
                    _gammaEnable = GetGammaEnable();
                    _opened = true;
                    Status = DeviceStatus.Ready;
                }
            }
            catch (Exception ex)
            {
                Status = DeviceStatus.Error;
                CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = "[" + i + "]ImageProvider opening error. " + ex.Message });
            }
        }

        public void Close()
        {
            try
            {
                if (_imageProvider != null)
                {
                    _imageProvider.Stop();
                    _imageProvider.Close();
                }
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = ex.Message });
            }
            finally
            {
                _opened = false;
                Status = DeviceStatus.Offline;
            }
        }

        public void OneShot(BaslerImage image = null)
        {
            if (image != null)
                _image = image;
            else
                _image = new BaslerImage();

            if (Status == DeviceStatus.Ready)
                _imageProvider.OneShot();
            else
            {
                CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = "Camera is offline." });
                return;
            }
        }

        public void ContinuousShot()
        {
            try
            {
                if (_imageProvider == null)
                {
                    return;
                }

                if (Status == DeviceStatus.Ready)
                {
                    _imageProvider.ContinuousShot();
                }
                else
                {
                    CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = "Camera is offline." });
                    return;
                }
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, new CameraErrorEventArgs() { Message = ex.Message });
            }
        }

        public void Stop()
        {
            if (Status == DeviceStatus.Ready)
            {
                if (_imageProvider != null)
                {
                    _imageProvider.Stop();
                }
            }

        }
        #endregion

        #region Camera Settings
        public int GetMaxHeight()
        {
            return (int)GenApi.IntegerGetMax(_imageProvider.GetNodeFromDevice("Height"));
        }

        public int GetHeight()
        {
            return (int)GenApi.IntegerGetValue(_imageProvider.GetNodeFromDevice("Height"));
        }

        public void SetHeight(int value)
        {
            int maxValue = GetMaxHeight();
            if (value > maxValue)
                value = maxValue;
            if (value < 0)
                value = 0;
            GenApi.IntegerSetValue(_imageProvider.GetNodeFromDevice("Height"), value);
            _height = value;
        }

        public int GetMaxWidth()
        {
            return (int)GenApi.IntegerGetMax(_imageProvider.GetNodeFromDevice("Width"));
        }

        public int GetWidth()
        {
            return (int)GenApi.IntegerGetValue(_imageProvider.GetNodeFromDevice("Width"));
        }

        public void SetWidth(int value)
        {
            int maxValue = GetMaxWidth();
            if (value > maxValue)
                value = maxValue;
            if (value < 0)
                value = 0;
            GenApi.IntegerSetValue(_imageProvider.GetNodeFromDevice("Width"), value);
            _width = value;
        }

        public int GetOffsetX()
        {
            return (int)GenApi.IntegerGetValue(_imageProvider.GetNodeFromDevice("OffsetX"));
        }

        public void SetOffsetX(int value)
        {
            int maxValue = (int)GenApi.IntegerGetMax(_imageProvider.GetNodeFromDevice("OffsetX"));
            if (value > maxValue)
                value = maxValue;
            if (value < 0)
                value = 0;
            GenApi.IntegerSetValue(_imageProvider.GetNodeFromDevice("OffsetX"), value);
            _offsetX = value;
        }

        public int GetOffsetY()
        {
            return (int)GenApi.IntegerGetValue(_imageProvider.GetNodeFromDevice("OffsetY"));
        }

        public void SetOffsetY(int value)
        {
            int maxValue = (int)GenApi.IntegerGetMax(_imageProvider.GetNodeFromDevice("OffsetY"));
            if (value > maxValue)
                value = maxValue;
            if (value < 0)
                value = 0;
            GenApi.IntegerSetValue(_imageProvider.GetNodeFromDevice("OffsetY"), value);
            _offsetY = value;
        }

        public double GetMaxExposureTimeAbs()
        {
            return GenApi.FloatGetMax(_imageProvider.GetNodeFromDevice("ExposureTimeAbs"));
        }

        public double GetMinExposureTimeAbs()
        {
            return GenApi.FloatGetMin(_imageProvider.GetNodeFromDevice("ExposureTimeAbs"));
        }

        public double GetExposureTimeAbs()
        {
            return GenApi.FloatGetValue(_imageProvider.GetNodeFromDevice("ExposureTimeAbs"));
        }

        public void SetExposureTimeAbs(double value)
        {
            double maxValue = GetMaxExposureTimeAbs();
            double minValue = GetMinExposureTimeAbs();
            if (value > maxValue)
                value = maxValue;
            if (value < minValue)
                value = minValue;
            GenApi.FloatSetValue(_imageProvider.GetNodeFromDevice("ExposureTimeAbs"), value);
            _exposureTime = value;
        }

        public int GetMaxExposureTimeRaw()
        {
            return (int)GenApi.IntegerGetMax(_imageProvider.GetNodeFromDevice("ExposureTimeRaw"));
        }

        public int GetMinExposureTimeRaw()
        {
            return (int)GenApi.IntegerGetMin(_imageProvider.GetNodeFromDevice("ExposureTimeRaw"));
        }

        public int GetExposureTimeRaw()
        {
            return (int)GenApi.IntegerGetValue(_imageProvider.GetNodeFromDevice("ExposureTimeRaw"));
        }

        public void SetExposureTimeRaw(int value)
        {
            int maxValue = GetMaxExposureTimeRaw();
            int minValue = GetMinExposureTimeRaw();
            if (value > maxValue)
                value = maxValue;
            if (value < minValue)
                value = minValue;
            GenApi.IntegerSetValue(_imageProvider.GetNodeFromDevice("ExposureTimeRaw"), value);
        }

        public void SetTriggerSelector(TriggerSelector value)
        {
            _imageProvider.SetTriggerSelector(value);
            _triggerSelector = value;
        }

        public void SetTriggerSource(TriggerSource value)
        {
            _imageProvider.SetTriggerSource(value);
            _triggerSource = value;
        }

        public int GetMaxGain()
        {
            return (int)GenApi.IntegerGetMax(_imageProvider.GetNodeFromDevice("GainRaw"));
        }

        public int GetGain()
        {
            return (int)GenApi.IntegerGetValue(_imageProvider.GetNodeFromDevice("GainRaw"));
        }

        public void SetGain(int value)
        {
            int maxValue = GetMaxGain();
            if (value > maxValue)
                value = maxValue;
            if (value < 0)
                value = 0;
            GenApi.IntegerSetValue(_imageProvider.GetNodeFromDevice("GainRaw"), value);
            _gain = value;
        }

        public string GetImageFormat()
        {
            return GenApi.NodeToString(_imageProvider.GetNodeFromDevice("PixelFormat"));
        }

        public bool GetTriggerMode()
        {
            bool is_trigger = GenApi.NodeToString(_imageProvider.GetNodeFromDevice("TriggerMode")) == "On";
            return is_trigger;
        }

        public TriggerSelector GetTriggerSelector()
        {
            string str = GenApi.NodeToString(_imageProvider.GetNodeFromDevice("TriggerSelector"));
            return (TriggerSelector)Enum.Parse(typeof(TriggerSelector), str);
        }

        public TriggerSource GetTriggerSource()
        {
            string str = GenApi.NodeToString(_imageProvider.GetNodeFromDevice("TriggerSource"));
            return (TriggerSource)Enum.Parse(typeof(TriggerSource), str);
        }

        public int GetAcquisitionFrameCount()
        {
            return int.Parse(GenApi.NodeToString(_imageProvider.GetNodeFromDevice("AcquisitionFrameCount")));
        }

        public bool GetGammaEnable()
        {
            return GenApi.NodeToString(_imageProvider.GetNodeFromDevice("GammaEnable")) == "1";
        }

        public void SetImageFormat(string value)
        {
            NODE_HANDLE hNode = _imageProvider.GetNodeFromDevice("PixelFormat");
            NODE_HANDLE hEntry = GenApi.EnumerationGetEntryByName(hNode, value);
            if (hEntry.IsValid && GenApi.NodeIsAvailable(hEntry))
                GenApi.NodeFromString(hNode, value);
            _format = value;
        }

        public int GetPacketSize()
        {
            return (int)GenApi.IntegerGetValue(_imageProvider.GetNodeFromDevice("GevSCPSPacketSize"));
        }

        public int GetPackageSizeMax()
        {
            return (int)GenApi.IntegerGetMax(_imageProvider.GetNodeFromDevice("GevSCPSPacketSize"));
        }

        public int GetPackageSizeMin()
        {
            return (int)GenApi.IntegerGetMin(_imageProvider.GetNodeFromDevice("GevSCPSPacketSize"));
        }

        public void SetPackageSize(int value)
        {
            int maxValue = GetPackageSizeMax();
            int minValue = GetPackageSizeMin();
            if (value > maxValue)
                value = maxValue;
            if (value < minValue)
                value = minValue;
            GenApi.IntegerSetValue(_imageProvider.GetNodeFromDevice("GevSCPSPacketSize"), value);
            _packetSize = value;
        }
        #endregion
    }
}

namespace Basler
{
    public interface ICamera
    {
        TriggerSource TriggerSource { get; set; }
        int GainMax { get; set; }
        int Gain { get; set; }
        int OffsetY { get; set; }
        int OffsetX { get; set; }
        int HeightMax { get; set; }
        int Height { get; set; }
        int WidthMax { get; set; }
        int Width { get; set; }
        BaslerImage Image { get; set; }
        string Format { get; set; }
        TriggerSelector TriggerSelector { get; set; }
        int AcquisitionFrameCount { get; set; }
        bool IsMultiframe { get; set; }
        bool TriggerMode { get; set; }
        BaslerImage.ImageRotation Rotation { get; set; }
        DeviceStatus Status { get; set; }
        bool Opened { get; set; }
        int Port { get; set; }
        string IPAddress { get; set; }
        string Name { get; set; }
        int CameraID { get; set; }
        int BitDepth { get; set; }
        int PacketSize { get; set; }
        string OfflineImageFolder { get; set; }
        string BackImagePath { get; set; }
        double ExposureTime { get; set; }
        double ExposureTimeMax { get; set; }

        event ImageReadyEventHandler ImageReady;
        event CameraErrorEventHandler CameraError;
        event CameraStatusChangedEventHandler CameraStatusChanged;

        void Close();
        void ContinuousShot();
        void Initialise();
        void OneShot(BaslerImage image = null);
        void Open();
        void Stop();
    }
}

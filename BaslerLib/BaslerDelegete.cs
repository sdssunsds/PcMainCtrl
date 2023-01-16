using System;

namespace Basler
{
    public delegate void DeviceEventHandler(CameraStatusEvent status);
    public delegate void GrabErrorEventHandler(Exception grabException, string additionalErrorMessage);
    public delegate void ImageReadyEventHandler(object sender, CameraEventArgs e);
    public delegate void CameraErrorEventHandler(object sender, CameraErrorEventArgs e);
    public delegate void CameraStatusChangedEventHandler(object sender, CameraStatusEventArgs e);
}

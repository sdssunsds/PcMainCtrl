using System;

namespace Basler
{
    public class CameraStatusEventArgs : EventArgs
    {
        public CameraStatusEvent Status;
    }

	public enum CameraStatusEvent
	{
		DEVICE_OPENED,
		DEVICE_CLOSING,
		DEVICE_CLOSED,
		GRABBING_STARTED,
		GRABBING_STOPPED,
		DEVICE_REMOVED
	}
}

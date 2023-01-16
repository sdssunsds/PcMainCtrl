using System;

namespace Basler
{
    public class CameraEventArgs : EventArgs
    {
        public BaslerImage Image;
        public byte[] Buffer;
    }
}

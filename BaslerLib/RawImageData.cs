namespace Basler
{
    public class RawImageData
    {
        public readonly int _width;
        public readonly int _height;
        public readonly byte[] _buffer;
        public readonly bool _color;

        public RawImageData(int newWidth, int newHeight, byte[] newBuffer, bool color)
        {
            _width = newWidth;
            _height = newHeight;
            _buffer = newBuffer;
            _color = color;
        }
    }
}

using PcMainCtrl.HardWare.BaslerCamera;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PcMainCtrl.Converter
{
    /// <summary>
    /// 路径转Image的值转换器
    /// </summary>
    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = (string)value;
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    return new BitmapImage(new Uri(path, UriKind.Absolute));
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

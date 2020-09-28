using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using Zorbo.Core;

namespace Zorbo.Server.WPF.Converters
{
    /// <summary>
    /// A converter used to convert an IAvatar object to an ImageSource for DataBinding
    /// </summary>
    public class AvatarToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes) {
                if (bytes.Length > 0) {

                    BitmapImage img = new BitmapImage();
                    MemoryStream stream = new MemoryStream(bytes);

                    img.BeginInit();
                    img.StreamSource = stream;
                    img.EndInit();
                    img.Freeze();

                    return img;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

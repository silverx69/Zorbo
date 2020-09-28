using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Zorbo.Core;

namespace Zorbo.Server.WPF.Converters
{
    public class ReadableTopicConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Emotes.ToReadableColorCodes(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Emotes.ToAresColorCodes(value.ToString());
        }
    }
}

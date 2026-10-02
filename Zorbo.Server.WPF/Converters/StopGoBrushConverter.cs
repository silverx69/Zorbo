using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Zorbo.Server.WPF.Converters
{
    /// <summary>
    /// A converter used to return a simple Red or Green result based on the Boolean value input. 
    /// Used in <see cref="Main"/>.xaml to change the color of the Firewall Status Indicator.
    /// </summary>
    public class StopGoBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if (System.Convert.ToBoolean(value))
                return new SolidColorBrush(Colors.LimeGreen);

            else return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }
    }
}

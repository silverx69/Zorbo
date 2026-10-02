using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Zorbo.Ares.Server.Users;
using Zorbo.Core;

namespace Zorbo.Server.WPF.Converters
{
    /// <summary>
    /// A converter used to convert an Enum value to and from it's associated index.
    /// </summary>
    public sealed class EnumToInt32Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum @enum) {
                var values = Enum.GetValues(@enum.GetType());
                for (int i = 0; i < values.Length; i++) {
                    var val = values.GetValue(i);
                    if (val.Equals(value)) return i;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int @int && targetType.IsEnum) {
                var values = Enum.GetValues(targetType);
                if (@int > 0 && @int < values.Length)
                    return values.GetValue(@int);
            }
            return null;
        }
    }
}
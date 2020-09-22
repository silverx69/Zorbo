using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Zorbo.Ares.Server;
using Zorbo.Core;
using Zorbo.Core.Interfaces;

namespace Zorbo.Server.WPF.Converters
{
    /// <summary>
    /// A converter used to convert a Language enumeration value to and from it's LanguageValues index.
    /// </summary>
    public sealed class LanguageToInt32Converter : IValueConverter
    {
        readonly Language[] values;

        public LanguageToInt32Converter()
        {
            values = AresServerConfig.LanguageValues.ToArray();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Language language)
                return values.FindIndex((s) => s == language);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int @int)
                return values[@int];
            return null;
        }
    }
}

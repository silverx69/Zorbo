using System;
using System.Globalization;
using System.Windows.Data;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Zorbo.Server.WPF.Converters
{
    /// <summary>
    /// A converter used for converting a ClientId object to the user's name that it represents.
    /// </summary>
    public class ClientIdToUsernameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {

            if (values.Length == 2) {
                if (values[0] is ClientId id) {
                    if (id is null || !(values[1] is IHistory history))
                        return string.Empty;

                    var record = history.Records.Find((s) => s.ClientId.Equals(id));
                    if (record != null) return record.Name;
                }
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return null;
        }
    }
}

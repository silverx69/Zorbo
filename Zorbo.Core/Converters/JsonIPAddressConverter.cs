using Newtonsoft.Json;
using System;
using System.Net;

namespace Zorbo.Core.Converters
{
    public sealed class JsonIPAddressConverter : FlexibleJsonConverter<IPAddress> 
    {
        private JsonIPAddressConverter()
            : base(
                  (writer, value, serializer) => serializer.Serialize(writer, value.ToString()), 

                  (reader, objectType, existingValue, hasValue, serializer) => 
                  {
                      string value = serializer.Deserialize<string>(reader);
                      if (IPAddress.TryParse(value, out IPAddress address))
                          return address;
                      return IPAddress.Any;
                  }) { }

        public static readonly JsonIPAddressConverter Default = new JsonIPAddressConverter();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Zorbo.Core.Converters
{
    public sealed class JsonByteArrayConverter : FlexibleJsonConverter<byte[]>
    {
        private JsonByteArrayConverter()
            : base(
                  (writer, value, serializer) => 
                        serializer.Serialize(writer, Convert.ToBase64String(value)),

                  (reader, objectType, existingValue, hasValue, serializer) => 
                        Convert.FromBase64String(serializer.Deserialize<string>(reader))) { }

        public static readonly JsonByteArrayConverter Default = new JsonByteArrayConverter();
    }
}

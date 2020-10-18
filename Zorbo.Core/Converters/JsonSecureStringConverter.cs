using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Zorbo.Core.Converters
{
    public sealed class JsonSecureStringConverter : FlexibleJsonConverter<SecureString>
    {
        private JsonSecureStringConverter()
            : base(
                  (writer, value, serializer) => {
                      byte[] tmp = Encoding.UTF8.GetBytes(value.ToNativeString());
                      serializer.Serialize(writer, Convert.ToBase64String(Hashlinks.E67(tmp, 34567)));
                  },
                  (reader, objectType, existingValue, hasValue, serializer) => {
                      string ret = serializer.Deserialize<string>(reader);
                      if (!string.IsNullOrEmpty(ret))
                          return Encoding.UTF8.GetString(Hashlinks.D67(Convert.FromBase64String(ret), 34567)).ToSecureString();
                      return null;
                  })
        { }

        public static readonly JsonSecureStringConverter Default = new JsonSecureStringConverter();
    }
}

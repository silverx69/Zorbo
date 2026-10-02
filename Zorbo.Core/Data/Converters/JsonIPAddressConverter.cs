using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zorbo.Data.Converters
{
    public class JsonIPAddressConverter : JsonConverter<IPAddress>
    {
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.String) {
                if (IPAddress.TryParse(reader.GetString(), out IPAddress ip))
                    return ip;
                return IPAddress.Any;
            }
            return IPAddress.Any;
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options) {
            value ??= IPAddress.Any;
            writer.WriteStringValue(value.ToString());
        }
    }
}

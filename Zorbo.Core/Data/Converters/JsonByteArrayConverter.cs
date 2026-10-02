using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zorbo.Data.Converters
{
    public sealed class JsonByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TryGetBytesFromBase64(out byte[] ret))
                return ret;
            return null;
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options) {
            writer.WriteBase64StringValue(value);
        }
    }
}

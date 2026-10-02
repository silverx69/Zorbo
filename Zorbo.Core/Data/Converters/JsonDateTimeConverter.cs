using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zorbo.Data.Converters
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.String) {
                if (DateTime.TryParse(reader.GetString(), out DateTime time))
                    return time;
                return default;
            }
            return new DateTime(reader.GetInt64(), DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
            if (value.Kind == DateTimeKind.Utc)
                writer.WriteNumberValue(value.Ticks);
            else
                writer.WriteNumberValue(value.ToUniversalTime().Ticks);
        }
    }
}

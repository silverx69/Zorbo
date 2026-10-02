namespace Zorbo.Data.Converters
{
    public class BinaryDateTimeConverter : BinaryConverter<DateTime>
    {
        public override DateTime Read(ZBinaryReader reader) {
            return new DateTime(reader.ReadInt64(), DateTimeKind.Utc).ToLocalTime();
        }

        public override async Task<DateTime> ReadAsync(ZBinaryReader reader) {
            return new DateTime(await reader.ReadInt64Async(), DateTimeKind.Utc).ToLocalTime();
        }

        public override void Write(ZBinaryWriter writer, DateTime value) {
            if (value.Kind != DateTimeKind.Utc)
                value = value.ToUniversalTime();
            writer.Write(value.Ticks);
        }

        public override Task WriteAsync(ZBinaryWriter writer, DateTime value) {
            if (value.Kind != DateTimeKind.Utc)
                value = value.ToUniversalTime();
            return writer.WriteAsync(value.Ticks);
        }
    }
}

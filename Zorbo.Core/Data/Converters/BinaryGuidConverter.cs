namespace Zorbo.Data.Converters
{
    public class BinaryGuidConverter : BinaryConverter<Guid>
    {
        public override Guid Read(ZBinaryReader reader) {
            return new Guid(reader.ReadBytes(16));
        }

        public override async Task<Guid> ReadAsync(ZBinaryReader reader) {
            return new Guid(await reader.ReadBytesAsync(16));
        }

        public override void Write(ZBinaryWriter writer, Guid value) {
            writer.Write(value.ToByteArray());
        }

        public override async Task WriteAsync(ZBinaryWriter writer, Guid value) {
            await writer.WriteAsync(value.ToByteArray());
        }
    }
}
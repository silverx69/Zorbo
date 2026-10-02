using System.Net;

namespace Zorbo.Data.Converters
{
    public class BinaryIPAddressConverter : BinaryConverter<IPAddress>
    {
        public override IPAddress Read(ZBinaryReader reader) {
            return new IPAddress(reader.ReadBytes(reader.ReadByte()));
        }

        public override async Task<IPAddress> ReadAsync(ZBinaryReader reader) {
            return new IPAddress(await reader.ReadBytesAsync(reader.ReadByte()));
        }

        public override void Write(ZBinaryWriter writer, IPAddress value) {
            value ??= IPAddress.Any;
            byte[] b = value.GetAddressBytes();
            writer.Write((byte)b.Length);
            writer.Write(b);
        }

        public override async Task WriteAsync(ZBinaryWriter writer, IPAddress value) {
            value ??= IPAddress.Any;
            byte[] b = value.GetAddressBytes();
            await writer.WriteAsync((byte)b.Length);
            await writer.WriteAsync(b);
        }
    }
}

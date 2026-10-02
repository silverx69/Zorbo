using System.Buffers.Binary;

namespace Zorbo
{
    public static partial class PrimitiveExtensions
    {
        public static short AsBigEndian(this short value) {
            if (BitConverter.IsLittleEndian)
                return BinaryPrimitives.ReverseEndianness(value);
            return value;
        }

        public static ushort AsBigEndian(this ushort value) {
            if (BitConverter.IsLittleEndian)
                return BinaryPrimitives.ReverseEndianness(value);
            return value;
        }

        public static int AsBigEndian(this int value) {
            if (BitConverter.IsLittleEndian)
                return BinaryPrimitives.ReverseEndianness(value);
            return value;
        }

        public static uint AsBigEndian(this uint value) {
            if (BitConverter.IsLittleEndian)
                return BinaryPrimitives.ReverseEndianness(value);
            return value;
        }

        public static long AsBigEndian(this long value) {
            if (BitConverter.IsLittleEndian)
                return BinaryPrimitives.ReverseEndianness(value);
            return value;
        }

        public static ulong AsBigEndian(this ulong value) {
            if (BitConverter.IsLittleEndian)
                return BinaryPrimitives.ReverseEndianness(value);
            return value;
        }
    }
}

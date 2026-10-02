using System.Text;

namespace Zorbo.Data
{
    public class ZBinaryReader : BinaryReader
    {
        Encoding encoding = Encoding.UTF8;

        public long Length {
            get { return BaseStream.Length; }
        }

        public long Position {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public long Remaining {
            get { return Length - Position; }
        }

        public Encoding Encoding {
            get { return encoding; }
            private set { encoding = value; }
        }

        public ZBinaryReader(byte[] input)
            : this(new MemoryStream(input)) { }

        public ZBinaryReader(byte[] input, Encoding encoding)
            : this(new MemoryStream(input), encoding) { }

        public ZBinaryReader(byte[] input, int offset, int count)
            : this(new MemoryStream(input, offset, count)) { }

        public ZBinaryReader(byte[] input, int offset, int count, Encoding encoding)
            : this(new MemoryStream(input, offset, count), encoding) { }

        public ZBinaryReader(Stream input, bool leaveOpen = false)
            : this(input, Encoding.UTF8, leaveOpen) { }

        public ZBinaryReader(Stream input, Encoding encoding, bool leaveOpen = false)
            : base(input, encoding, leaveOpen) {
            Encoding = encoding;
        }

        public virtual void Clear() {
            BaseStream.SetLength(0);
        }

        public virtual byte[] ReadBytes(long count) {
            return base.ReadBytes((int)count);
        }

        public virtual async Task<byte[]> ReadBytesAsync(int count) {
            byte[] tmp = new byte[count];
            await BaseStream.ReadExactlyAsync(tmp, 0, count);
            return tmp;
        }

        public virtual Task<byte[]> ReadBytesAsync(long count) {
            return ReadBytesAsync((int)count);
        }

        public virtual async Task<bool> ReadBooleanAsync() => ReadBoolean();

        public virtual async Task<sbyte> ReadSByteAsync() => ReadSByte();

        public virtual async Task<byte> ReadByteAsync() => ReadByte();

        public virtual async Task<short> ReadInt16Async() => ReadInt16();

        public virtual async Task<ushort> ReadUInt16Async() => ReadUInt16();

        public virtual async Task<int> ReadInt32Async() => ReadInt32();

        public virtual async Task<uint> ReadUInt32Async() => ReadUInt32();

        public virtual async Task<long> ReadInt64Async() => ReadInt64();

        public virtual async Task<ulong> ReadUInt64Async() => ReadUInt64();

        public virtual async Task<float> ReadSingleAsync() => ReadSingle();

        public virtual async Task<double> ReadDoubleAsync() => ReadDouble();

        public virtual async Task<decimal> ReadDecimalAsync() => ReadDecimal();

        public virtual async Task<char> ReadCharAsync() => ReadChar();

        public virtual async Task<char[]> ReadCharsAsync(int count) => ReadChars(count);


        //public override string ReadString() => ReadString(false);

        public override string ReadString() {
            return ReadString(Read7BitEncodedInt());
        }

        public virtual string ReadString(bool prefix) {
            if (prefix)
                return ReadString(Read7BitEncodedInt());
            else
                return ReadString(Remaining);
        }

        public virtual string ReadString(int count) {
            return Encoding.GetString(ReadBytes((int)Math.Min(count, Remaining)));
        }

        public virtual string ReadString(long count) {
            return Encoding.GetString(ReadBytes((int)Math.Min(count, Remaining)));
        }

        public virtual async Task<string> ReadStringAsync(bool prefix = true) {
            if (prefix)
                return await ReadStringAsync(Read7BitEncodedInt());
            else
                return await ReadStringAsync(Remaining);
        }

        public virtual async Task<string> ReadStringAsync(int count) {
            return Encoding.GetString(await ReadBytesAsync((int)Math.Min(count, Remaining)));
        }

        public virtual async Task<string> ReadStringAsync(long count) {
            return Encoding.GetString(await ReadBytesAsync((int)Math.Min(count, Remaining)));
        }

        public byte[] ToArray() {
            byte[] bytes;
            long i = Position;
            if (!BaseStream.CanSeek)
                bytes = ReadBytes((int)Length);
            else {
                Position = 0;
                bytes = ReadBytes((int)Length);
                Position = i;
            }
            return bytes;
        }

        public async Task<byte[]> ToArrayAsync() {
            byte[] bytes;
            long i = Position;
            if (!BaseStream.CanSeek)
                bytes = await ReadBytesAsync((int)Length);
            else {
                Position = 0;
                bytes = await ReadBytesAsync((int)Length);
                Position = i;
            }
            return bytes;
        }
    }
}

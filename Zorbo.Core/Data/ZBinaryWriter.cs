using System.Text;

namespace Zorbo.Data
{
    public class ZBinaryWriter : BinaryWriter
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


        public ZBinaryWriter()
            : this(new MemoryStream()) { }

        public ZBinaryWriter(Encoding encoding)
            : this(new MemoryStream(), encoding) { }

        public ZBinaryWriter(Stream output, bool leaveOpen = false)
            : this(output, Encoding.UTF8, leaveOpen) { }

        public ZBinaryWriter(Stream output, Encoding encoding, bool leaveOpen = false)
            : base(output, encoding, leaveOpen) {
            Encoding = encoding;
        }


        public virtual void Clear() {
            BaseStream.SetLength(0);
        }

        public virtual Task FlushAsync() {
            return BaseStream.FlushAsync();
        }


        public override void Write(byte[] buffer) {
            Write(buffer, 0, buffer.Length);
        }

        public override void Write(byte[] buffer, int index, int count) {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
            BaseStream.Write(buffer, index, count);
        }

        public virtual Task WriteAsync(byte[] buffer) {
            return WriteAsync(buffer, 0, buffer.Length);
        }

        public virtual async Task WriteAsync(byte[] buffer, int index, int count) {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
            await BaseStream.WriteAsync(buffer.AsMemory(index, count));
        }


        public virtual async Task WriteAsync(bool value) => Write(value);

        public virtual async Task WriteAsync(sbyte value) => Write(value);

        public virtual async Task WriteAsync(byte value) => Write(value);

        public virtual async Task WriteAsync(short value) => Write(value);

        public virtual async Task WriteAsync(ushort value) => Write(value);

        public virtual async Task WriteAsync(int value) => Write(value);

        public virtual async Task WriteAsync(uint value) => Write(value);

        public virtual async Task WriteAsync(long value) => Write(value);

        public virtual async Task WriteAsync(ulong value) => Write(value);

        public virtual async Task WriteAsync(float value) => Write(value);

        public virtual async Task WriteAsync(double value) => Write(value);

        public virtual async Task WriteAsync(decimal value) => Write(value);

        public virtual async Task WriteAsync(char value) => Write(value);

        public override void Write(string value) {
            Write(value, true);
        }

        public virtual void Write(string value, bool prefix) {
            value ??= string.Empty;
            if (prefix) {
                Write7BitEncodedInt(Encoding.GetByteCount(value));
                Write(Encoding.GetBytes(value));
            }
            else {
                Write(Encoding.GetBytes(value));
                //Write((byte)0);
            }
        }

        public virtual async Task WriteAsync(string value, bool prefix = true) {
            value ??= string.Empty;
            if (prefix) {
                Write7BitEncodedInt(Encoding.GetByteCount(value));
                await WriteAsync(Encoding.GetBytes(value));
            }
            else {
                await WriteAsync(Encoding.GetBytes(value));
                //await WriteAsync((byte)0);
            }
        }

        public byte[] ToArray() {
            long i = Position;
            byte[] b = new byte[Length];
            if (!BaseStream.CanSeek)
                BaseStream.ReadExactly(b, 0, (int)Length);
            else {
                Position = 0;
                BaseStream.ReadExactly(b, 0, (int)Length);
                Position = i;
            }
            return b;
        }

        public async Task<byte[]> ToArrayAsync() {
            long i = Position;
            byte[] b = new byte[Length];
            if (!BaseStream.CanSeek)
                await BaseStream.ReadExactlyAsync(b.AsMemory(0, (int)Length));
            else {
                Position = 0;
                await BaseStream.ReadExactlyAsync(b.AsMemory(0, (int)Length));
                Position = i;
            }
            return b;
        }
    }
}
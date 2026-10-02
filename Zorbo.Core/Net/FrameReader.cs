using Zorbo.Data;

namespace Zorbo.Net
{
    public static class FrameReader
    {
        static readonly Random Random = new();

        public static async Task<byte[]> WriteAsync(Frame frame) {
            using var writer = new ZBinaryWriter();
            await WriteAsync(writer, frame);
            return await writer.ToArrayAsync();
        }

        public static async Task WriteAsync(ZBinaryWriter writer, Frame frame) {
            await WriteHeaderAsync(writer, frame);
            await WritePayloadAsync(writer, frame);
        }

        public static async Task WriteHeaderAsync(ZBinaryWriter writer, Frame frame) {
            int length = frame.Payload.Length;

            byte first = (byte)(frame.IsFinal ? 0b1_0_0_0_0000 : 0b0_0_0_0_0000);

            writer.Write((byte)(first + frame.OpCode));

            byte second = (byte)(frame.IsMasked ? 0b1_0000000 : 0b0_0000000);

            if (length <= 125)
                writer.Write((byte)(second | length));

            else if (length <= ushort.MaxValue) {
                writer.Write((byte)(second | 126));
                writer.Write(((ushort)length).AsBigEndian());
            }
            else {
                writer.Write((byte)(second | 127));
                writer.Write(((ulong)length).AsBigEndian());
            }
        }

        public static async Task WritePayloadAsync(ZBinaryWriter writer, Frame frame) {
            int length = frame.Payload.Length;
            byte[] input = frame.Payload;

            if (frame.IsMasked) {
                byte[] mask = new byte[4];
                Random.NextBytes(mask);

                for (int i = 0; i < length; i++)
                    input[i] = (byte)(input[i] ^ mask[i % 4]);

                await writer.WriteAsync(mask);
            }

            await writer.WriteAsync(input, 0, length);
        }

        public static Task<Frame> ReadAsync(byte[] input) {
            return ReadAsync(input, 0, input.Length);
        }

        public static Task<Frame> ReadAsync(byte[] input, int index, int count) {
            using var reader = new ZBinaryReader(input, index, count);
            return ReadAsync(reader);
        }

        public static async Task<Frame> ReadAsync(ZBinaryReader reader) {
            return await ReadPayloadAsync(reader, await ReadHeaderAsync(reader));
        }

        public static async Task<Frame> ReadHeaderAsync(ZBinaryReader reader) {
            var frame = new Frame();

            byte first = reader.ReadByte();
            frame.IsFinal = first >= 128;

            frame.OpCode = (OpCode)(first & 15);
            if (frame.OpCode == OpCode.Close)
                return frame;

            byte second = reader.ReadByte();

            frame.Length = second & 127;
            frame.IsMasked = second >= 128;

            if (frame.Length == 126) {
                if (reader.Remaining < 2) {
                    frame.Incomplete = true;
                    return frame;
                }
                frame.Length = reader.ReadUInt16().AsBigEndian();
            }
            else if (frame.Length == 127) {
                if (reader.Remaining < 8) {
                    frame.Incomplete = true;
                    return frame;
                }
                frame.Length = (long)reader.ReadUInt64().AsBigEndian();
            }

            return frame;
        }

        public static async Task<Frame> ReadPayloadAsync(ZBinaryReader reader, Frame frame) {
            if (frame.IsMasked) {
                if (reader.Remaining < frame.Length + 4) {
                    frame.Incomplete = true;
                    return frame;
                }

                byte[] mask = reader.ReadBytes(4);
                byte[] payload = await reader.ReadBytesAsync(frame.Length);

                for (int i = 0; i < payload.Length; i++)
                    payload[i] ^= mask[i % 4];

                frame.Payload = payload;
            }
            else {
                if (reader.Remaining < frame.Length) {
                    frame.Incomplete = true;
                    return frame;
                }

                frame.Payload = await reader.ReadBytesAsync(frame.Length);
            }

            return frame;
        }
    }
}

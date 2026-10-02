namespace Zorbo.Net
{
    public sealed class Frame
    {
        public long Length { get; set; }

        public OpCode OpCode { get; set; }

        public bool IsMasked { get; set; }

        public bool IsFinal { get; set; } = true;

        public bool Incomplete { get; set; }

        public byte[] Payload { get; set; }

        public Frame() { }

        public Frame(OpCode opcode, byte[] payload, bool masked, bool final = true) {
            OpCode = opcode;
            Payload = payload;
            IsMasked = masked;
            IsFinal = final;
        }
    }
}
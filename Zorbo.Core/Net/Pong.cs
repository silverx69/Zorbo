namespace Zorbo.Net
{
    public sealed class Pong : ControlMessage
    {
        public Pong() { }
        public Pong(byte[] bytes) : base(bytes) { }

        public static explicit operator Pong(byte[] bytes) => new(bytes);
    }
}

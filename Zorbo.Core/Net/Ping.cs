namespace Zorbo.Net
{
    public sealed class Ping : ControlMessage
    {
        public Ping() { }
        public Ping(byte[] bytes) : base(bytes) { }

        public static explicit operator Ping(byte[] bytes) => new(bytes);
    }
}
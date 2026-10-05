namespace Zorbo.Net
{
    public abstract class ControlMessage
    {
        public byte[] Data { get; set; }

        public ControlMessage() { }

        public ControlMessage(byte[] bytes) {
            ArgumentNullException.ThrowIfNull(bytes, nameof(bytes));
            Data = bytes;
        }
    }
}

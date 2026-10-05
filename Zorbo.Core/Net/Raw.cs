namespace Zorbo.Net
{
    public class Raw : ControlMessage
    {
        public int Index { get; set; }
        public int Count { get; set; }

        public Raw(byte[] bytes) 
            : base(bytes) {
            Index = 0;
            Count = bytes.Length;
        }

        public Raw(byte[] bytes, int index, int count) 
            : base(bytes) {
            Index = index;
            Count = count;
        }

        public static explicit operator Raw(byte[] bytes) => new(bytes);
    }
}
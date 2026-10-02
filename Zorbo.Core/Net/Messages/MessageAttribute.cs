namespace Zorbo.Net.Messages
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MessageAttribute(ushort id) : Attribute
    {
        public ushort Id { get; private set; } = id;
    }
}

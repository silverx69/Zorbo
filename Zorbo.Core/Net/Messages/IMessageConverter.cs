using Zorbo.Data;

namespace Zorbo.Net.Messages
{
    public class MessageResult
    {
        public ushort Id { get; set; }
        public object Message { get; set; }

        public MessageResult() { }
        public MessageResult(ushort id, object message) {
            Id = id;
            Message = message;
        }
    }

    public interface IMessageConverter
    {
        Task WriteAsync(ZBinaryWriter writer, object value, MessageType type);
        Task<MessageResult> ReadAsync(ZBinaryReader reader, MessageType type);
    }
}

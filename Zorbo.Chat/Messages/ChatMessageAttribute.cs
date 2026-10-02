using Zorbo.Net.Messages;

namespace Zorbo.Chat.Messages
{
    public class ChatMessageAttribute : MessageAttribute
    {
        public new MessageId Id {
            get { return (MessageId)base.Id; }
        }

        public ChatMessageAttribute(MessageId id)
            : base((ushort)id) { }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.CLIENT_PUBLIC)]
    public class ClientPublic
    {
        [StringLength(1024)]
        public string Message { get; set; }
    }
}
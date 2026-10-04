using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_ANNOUNCE)]
    public class ServerAnnounce
    {
        [StringLength(1024)]
        public string Message { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.CLIENT_PRIVATE)]
    public class ClientPrivate
    {
        [StringLength(128, MinimumLength = 2)]
        public string Target { get; set; }

        [StringLength(1024)]
        public string Message { get; set; }
    }
}
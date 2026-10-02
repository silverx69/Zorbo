using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_PUBLIC)]
    public class ServerPublic
    {
        [StringLength(128, MinimumLength = 2)]
        public string Sender { get; set; }

        [StringLength(1024)]
        public string Message { get; set; }
    }
}
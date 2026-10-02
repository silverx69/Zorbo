using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_DETAILS)]
    public class ServerDetails
    {
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Topic { get; set; }
    }
}
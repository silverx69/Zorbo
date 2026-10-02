using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_JOINED)]
    public class ServerJoined
    {
        [StringLength(128, MinimumLength = 2)]
        public string UserName { get; set; }

        public ServerJoined() { }
        public ServerJoined(string userName) { UserName = userName; }
    }
}
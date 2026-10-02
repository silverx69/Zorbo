using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_PARTED)]
    public class ServerParted
    {
        [StringLength(128, MinimumLength = 2)]
        public string UserName { get; set; }

        public ServerParted() { }
        public ServerParted(string userName) { UserName = userName; }
    }
}
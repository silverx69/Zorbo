using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_USERLIST)]
    public class ServerUserlist
    {
        public List<ServerUserlistItem> Users { get; set; } = [];
    }

    public class ServerUserlistItem 
    {
        [StringLength(128, MinimumLength = 2)]
        public string UserName { get; set; }
    }
}

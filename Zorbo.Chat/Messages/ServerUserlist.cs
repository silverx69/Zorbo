using Zorbo.Chat.Server.Database;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_USERLIST)]
    public class ServerUserlistItem : ServerJoined 
    {
        public ServerUserlistItem() { }

        public ServerUserlistItem(string username) : base(username) { }

        public ServerUserlistItem(Profile profile) : base(profile) { }
    }
}

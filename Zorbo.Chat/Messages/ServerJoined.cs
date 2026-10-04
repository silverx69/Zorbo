using System.ComponentModel.DataAnnotations;
using Zorbo.Chat.Server.Database;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_JOINED)]
    public class ServerJoined
    {
        [StringLength(128, MinimumLength = 2)]
        public string Username { get; set; }

        public ServerJoined() { }
        
        public ServerJoined(string username) { 
            Username = username;
        }

        public ServerJoined(Profile profile) {
            Username = profile.Username;
        }
    }
}
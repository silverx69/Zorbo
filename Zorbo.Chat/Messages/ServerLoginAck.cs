using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_LOGIN_ACK)]
    public class ServerLoginAck
    {
        [StringLength(32)]
        public string Version { get; set; }
        
        [StringLength(128, MinimumLength = 2)]
        public string Username { get; set; }

        public ServerSupportFlags Flags { get; set; }

        public ServerLoginAck() { }

        public ServerLoginAck(string username, ServerSupportFlags flags) {
            Username = username;
            Flags = flags;
        }

        public bool HasFlag(ServerSupportFlags flag) {
            return Flags.HasFlag(flag);
        }
    }
}
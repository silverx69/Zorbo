using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.CLIENT_LOGIN)]
    public class ClientLogin
    {
        [GuidNotEmpty]
        public Guid Guid { get; set; }

        [StringLength(128, MinimumLength = 2)]
        public string UserName { get; set; }

        public byte Age { get; set; }

        public Country Country { get; set; }

        [StringLength(64)]
        public string Region { get; set; }

        [StringLength(1024)]
        public string Status { get; set; }

        public ClientSupportFlags Flags { get; set; }

        public bool HasFlag(ClientSupportFlags flag) {
            return Flags.HasFlag(flag);
        }
    }
}
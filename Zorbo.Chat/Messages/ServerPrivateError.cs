using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    public enum PrivateError : byte
    {
        Offline,
        Ignored
    }

    [ChatMessage(MessageId.SERVER_PRIVATE_ERROR)]
    public class ServerPrivateError
    {
        [StringLength(128, MinimumLength = 2)]
        public string Target { get; set; }

        public PrivateError Code { get; set; }
    }
}

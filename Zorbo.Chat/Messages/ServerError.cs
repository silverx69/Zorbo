using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    [ChatMessage(MessageId.SERVER_ERROR)]
    public class ServerError
    {
        [StringLength(1024)]
        public string Message { get; set; }

        public ServerError() { }
        public ServerError(string message) { Message = message; }
    }
}
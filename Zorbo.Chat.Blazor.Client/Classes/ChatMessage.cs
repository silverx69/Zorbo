using Newtonsoft.Json;
using System;
using Zorbo.Core;

namespace Zorbo.Chat.Blazor.Client.Classes
{
    [JsonObject]
    public class ChatMessage
    {
        [JsonProperty("message")]
        public IPacket Message { get; private set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; private set; }

        public ChatMessage(IPacket msg) {
            Message = msg;
            Timestamp = DateTime.Now;
        }
    }
}

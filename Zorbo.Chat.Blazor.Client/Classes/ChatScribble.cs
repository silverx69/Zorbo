using cb0tProtocol;
using cb0tProtocol.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Chat.Blazor.Client.Classes
{
    public class ChatScribble : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_SERVER_ROOM_SCRIBBLE; }
        }

        [JsonProperty("base64scribble")]
        [PacketItem(0)]
        public string Base64Scribble { get; set; }

        public ChatScribble() { }

        public ChatScribble(string base64scribble) {
            Base64Scribble = base64scribble;
        }
    }
}

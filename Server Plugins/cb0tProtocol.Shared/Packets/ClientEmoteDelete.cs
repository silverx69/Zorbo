using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ClientEmoteDelete : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE; }
        }

        [JsonProperty("shortcut", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Shortcut { get; set; }
    }
}

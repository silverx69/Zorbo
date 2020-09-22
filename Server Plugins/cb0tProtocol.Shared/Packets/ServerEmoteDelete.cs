using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;
namespace cb0tProtocol.Packets
{
    public class ServerEmoteDelete : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Username { get; set; }

        [JsonProperty("shortcut", Required = Required.AllowNull)]
        [PacketItem(1)]
        public string Shortcut { get; set; }
    }
}

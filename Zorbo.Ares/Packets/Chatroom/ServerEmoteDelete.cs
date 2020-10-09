using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;
namespace Zorbo.Ares.Packets.Chatroom
{
    public class ServerEmoteDelete : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("shortcut", Required = Required.AllowNull)]
        [PacketItem(1)]
        public string Shortcut { get; set; }
    }
}

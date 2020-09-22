using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ServerEmoteItem : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTES_UPLOAD_ITEM; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Username { get; set; }

        [JsonProperty("shortcut", Required = Required.AllowNull)]
        [PacketItem(1)]
        public string Shortcut { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        [PacketItem(2)]
        public byte Size { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(3)]
        public byte[] Image { get; set; }
    }
}

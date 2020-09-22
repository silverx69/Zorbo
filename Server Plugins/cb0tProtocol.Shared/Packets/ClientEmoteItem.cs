using Newtonsoft.Json;
using System;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ClientEmoteItem : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTES_UPLOAD_ITEM; }
        }

        [JsonProperty("shortcut", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Shortcut { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        [PacketItem(1)]
        public byte Size { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(2)]
        public byte[] Image { get; set; }
    }
}

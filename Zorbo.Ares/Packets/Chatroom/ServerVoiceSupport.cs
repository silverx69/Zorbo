using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ServerVoiceSupport : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_SERVER_VC_SUPPORTED; }
        }

        [JsonProperty("enabled", Required = Required.Always)]
        [PacketItem(0)]
        public bool Enabled { get; set; }

        [JsonProperty("high_quality", Required = Required.Always)]
        [PacketItem(1)]
        public bool HighQuality { get; set; }
    }
}

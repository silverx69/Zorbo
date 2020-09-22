using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ServerEmoteSupport : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_SERVER_SUPPORTS_CUSTOM_EMOTES; }
        }

        [JsonProperty("flag", Required = Required.Always)]
        [PacketItem(0)]
        public byte Flag { get; set; }

        public ServerEmoteSupport() { }

        public ServerEmoteSupport(byte flag) {
            this.Flag = flag;
        }
    }
}

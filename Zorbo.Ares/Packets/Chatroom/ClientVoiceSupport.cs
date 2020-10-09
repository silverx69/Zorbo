using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ClientVoiceSupport : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_VC_SUPPORTED; }
        }

        [JsonProperty("public", Required = Required.Always)]
        [PacketItem(0)]
        public bool Public { get; set; }

        [JsonProperty("private", Required = Required.Always)]
        [PacketItem(1)]
        public bool Private { get; set; }
    }
}

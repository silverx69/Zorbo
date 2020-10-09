using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ClientVoiceFirst : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_VC_FIRST; }
        }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(0)]
        public byte[] Data { get; set; }
    }
}

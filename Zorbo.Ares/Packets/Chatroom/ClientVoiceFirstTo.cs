using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ClientVoiceFirstTo : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_VC_FIRST_TO; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] Data { get; set; }
    }
}

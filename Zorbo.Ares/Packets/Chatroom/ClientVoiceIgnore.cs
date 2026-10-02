using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ClientVoiceIgnore : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_VC_IGNORE; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }
    }
}

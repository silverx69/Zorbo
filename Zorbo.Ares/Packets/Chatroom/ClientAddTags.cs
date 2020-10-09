using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ClientAddTags : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_ADD_TAGS; }
        }

        [JsonProperty("tag", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Tag { get; set; }
    }
}

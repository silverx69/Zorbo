using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ClientRemTags : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_REM_TAGS; }
        }

        [JsonProperty("tag", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Tag { get; set; }
    }
}

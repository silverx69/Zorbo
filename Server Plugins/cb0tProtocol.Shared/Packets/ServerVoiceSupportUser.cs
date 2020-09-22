using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ServerVoiceSupportUser : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_SERVER_VC_USER_SUPPORTED; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Username { get; set; }

        [JsonProperty("public", Required = Required.Always)]
        [PacketItem(1)]
        public bool Public { get; set; }

        [JsonProperty("private", Required = Required.Always)]
        [PacketItem(2)]
        public bool Private { get; set; }
    }
}

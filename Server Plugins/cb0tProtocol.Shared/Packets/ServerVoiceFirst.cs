using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ServerVoiceFirst : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_SERVER_VC_FIRST; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Username { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] Data { get; set; }
    }
}

using Newtonsoft.Json;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ServerVoiceChunkFrom : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_SERVER_VC_CHUNK_FROM; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] Data { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;
namespace cb0tProtocol.Packets
{
    public class ClientVoiceChunkTo : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK_TO; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Username { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] Data { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ClientScribbleChunk : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_CHUNK; }
        }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 4094)]
        public byte[] Data { get; set; }
    }
}

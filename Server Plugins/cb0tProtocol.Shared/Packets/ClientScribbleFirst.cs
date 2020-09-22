using Newtonsoft.Json;
using System;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace cb0tProtocol.Packets
{
    public class ClientScribbleFirst : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_FIRST; }
        }

        [JsonProperty("size", Required = Required.Always)]
        [PacketItem(0)]
        public uint Size { get; set; }

        [JsonProperty("chunks", Required = Required.Always)]
        [PacketItem(1)]
        public ushort Chunks { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(2, MaxLength = 4090)]
        public byte[] Data { get; set; }
    }
}

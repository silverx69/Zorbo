using Newtonsoft.Json;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.WebSockets
{
    /// <summary>
    /// A packet used internally to signal higher-level classes that a WebSocket ping has been received. 
    /// This allows implementing classes more control over the response. Not meant to be seen by the plugins.
    /// </summary>
    public sealed class PingPongPacket : IPacket
    {
        byte IPacket.Id { get { return 0; } }

        [JsonIgnore]
        public bool IsPing { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        [PacketItem(0)]
        public byte[] RawBytes { get; set; }


        public PingPongPacket() { RawBytes = new byte[0]; }

        public PingPongPacket(byte[] bytes) { RawBytes = bytes; }
    }
}

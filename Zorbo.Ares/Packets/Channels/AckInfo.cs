using Newtonsoft.Json;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Channels
{
    sealed class AckInfo : AresUdpPacket
    {
        public override AresUdpId Id {
            get { return AresUdpId.OP_SERVERLIST_ACKINFO; }
        }

        [JsonProperty("port", Required = Required.Always)]
        [PacketItem(0)]
        public ushort Port { get; set; }

        [JsonProperty("users", Required = Required.Always)]
        [PacketItem(1)]
        public ushort Users { get; set; }

        [JsonProperty("servername", Required = Required.AllowNull)]
        [PacketItem(2, LengthPrefix = true)]
        public string Name { get; set; }

        [JsonProperty("topic", Required = Required.AllowNull)]
        [PacketItem(3, LengthPrefix = true)]
        public string Topic { get; set; }

        [JsonProperty("language", Required = Required.Always)]
        [PacketItem(4)]
        public Language Language { get; set; }

        [JsonProperty("version", Required = Required.AllowNull)]
        [PacketItem(5, LengthPrefix = true)]
        public string Version { get; set; }

        [JsonProperty("num_servers", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(6)]
        public byte ServersLen { get; set; }

        [JsonProperty("data", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(7)]
        public byte[] Servers { get; set; }
    }
}

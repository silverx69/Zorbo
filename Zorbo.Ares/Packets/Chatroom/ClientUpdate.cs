using Newtonsoft.Json;
using System.Net;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ClientUpdate : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_UPDATE_STATUS; }
        }

        [JsonProperty("files", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(0)]
        public ushort FileCount { get; set; }

        [JsonProperty("skipped", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(1)]
        public byte Skipped { get; set; }

        [JsonProperty("node_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(2)]
        public IPAddress NodeIp { get; set; }

        [JsonProperty("node_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(3)]
        public ushort NodePort { get; set; }

        [JsonProperty("external_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(4)]
        public IPAddress ExternalIp { get; set; }

        [JsonProperty("uploads", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5, Optional = true)]
        public byte Uploads { get; set; }

        [JsonProperty("maxuploads", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(6, Optional = true)]
        public byte MaxUploads { get; set; }

        [JsonProperty("queued", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(7, Optional = true)]
        public byte Queued { get; set; }
    }
}

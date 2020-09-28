using Newtonsoft.Json;
using System;
using System.Net;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class SearchHit : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_SEARCHHIT; }
        }

        [JsonProperty("search_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort SearchId { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [PacketItem(1)]
        public byte Type { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        [PacketItem(2)]
        public uint Size { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(3)]
        public byte[] Content { get; set; }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(4, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("external_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5)]
        public IPAddress ExternalIp { get; set; }

        [JsonProperty("direct_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(6)]
        public ushort DCPort { get; set; }

        [JsonProperty("node_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(7)]
        public IPAddress NodeIp { get; set; }

        [JsonProperty("node_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(8)]
        public ushort NodePort { get; set; }

        [JsonProperty("local_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(9)]
        public IPAddress LocalIp { get; set; }

        [JsonProperty("uploads", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(10)]
        public byte Uploads { get; set; }

        [JsonProperty("max_uploads", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(11)]
        public byte MaxUploads { get; set; }

        [JsonProperty("queued", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(12)]
        public byte Queued { get; set; }

        [JsonIgnore]
        [PacketItem(13)]
        public byte Skipped {
            get { return 1; }
            set { }
        }


        public SearchHit() { }

        public SearchHit(ushort searchid, IClient user, ISharedFile file) {
            SearchId = searchid;
            Type = file.Type;
            Size = file.Size;
            Content = file.Content;
            Username = user.Name;
            ExternalIp = user.ExternalIp;
            DCPort = user.ListenPort;
            NodeIp = user.NodeIp;
            NodePort = user.NodePort;
            LocalIp = user.LocalIp;
        }
    }
}

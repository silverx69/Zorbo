using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using Zorbo.Core.Data.Packets;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Login : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_LOGIN; }
        }

        [JsonProperty("guid", Required = Required.AllowNull)]
        [PacketItem(0)]
        public Guid Guid { get; set; }

        [JsonProperty("files", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(1)]
        public ushort FileCount { get; set; }

        [JsonProperty("encryption", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(2)]
        public byte Encryption { get; set; }

        [JsonProperty("direct_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(3)]
        public ushort ListenPort { get; set; }

        [JsonProperty("node_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(4)]
        public IPAddress NodeIp { get; set; }

        [JsonProperty("node_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5)]
        public ushort NodePort { get; set; }
        /// <summary>
        /// I've hijacked these 4 bytes. They've been 'skipped' for years and I need new flags
        /// </summary>
        [JsonProperty("modern", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(6)]
        public int ModernFlags { get; set; }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(7, MaxLength = 60)]
        public string Username { get; set; }

        [JsonProperty("version", Required = Required.AllowNull)]
        [PacketItem(8, MaxLength = 20)]
        public string Version { get; set; }

        [JsonProperty("local_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(9)]
        public IPAddress LocalIp { get; set; }

        [JsonProperty("external_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(10)]
        public IPAddress ExternalIp { get; set; }

        [JsonProperty("flags", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(11)]
        public SupportFlags SupportFlag { get; set; }

        [JsonProperty("uploads", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(12, Optional = true)]
        public byte Uploads { get; set; }

        [JsonProperty("max_uploads", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(13, Optional = true)]
        public byte MaxUploads { get; set; }

        [JsonProperty("queued", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(14, Optional = true)]
        public byte Queued { get; set; }

        [JsonProperty("age", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(15, Optional = true)]
        public byte Age { get; set; }

        [JsonProperty("gender", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(16, Optional = true)]
        public Gender Gender { get; set; }

        [JsonProperty("country", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(17, Optional = true)]
        public Country Country { get; set; }

        [JsonProperty("region", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(18, Optional = true, MaxLength = 30)]
        public string Region { get; set; }

        [JsonProperty("features", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(19, Optional = true, OptionalValue = ClientFlags.NONE)]
        public ClientFlags Features { get; set; }
    }
}

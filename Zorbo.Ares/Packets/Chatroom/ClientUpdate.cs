using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel;
using Zorbo.Core.Interfaces.Server;

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

        [JsonProperty("flags", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(1)]
        public SupportFlags SupportFlag { get; set; }

        [JsonProperty("node_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(2)]
        public IPAddress NodeIp { get; set; }

        [JsonProperty("node_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(3)]
        public ushort NodePort { get; set; }

        [JsonProperty("external_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(4)]
        public IPAddress ExternalIp { get; set; }

        [JsonProperty("age", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5, Optional = true)]
        public byte Age { get; set; }

        [JsonProperty("gender", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(6, Optional = true)]
        public Gender Gender { get; set; }

        [JsonProperty("country", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(7, Optional = true)]
        public Country Country { get; set; }

        [JsonProperty("region", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(8, Optional = true, MaxLength = 30)]
        public string Region { get; set; }

        [JsonIgnore]
        [PacketItem(9, Optional = true)]
        public byte Skipped2 { get; set; }
    }
}

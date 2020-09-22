using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Channels
{
    sealed class AckNodes : AresUdpPacket
    {
        public override AresUdpId Id {
            get { return AresUdpId.OP_SERVERLIST_ACKNODES; }
        }

        [JsonProperty("port", Required = Required.Always)]
        [PacketItem(0)]
        public ushort Port { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] Servers { get; set; }
    }
}

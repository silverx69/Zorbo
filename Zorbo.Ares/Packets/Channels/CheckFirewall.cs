using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Channels
{
    sealed class CheckFirewall : AresUdpPacket
    {
        public override AresUdpId Id {
            get { return AresUdpId.OP_SERVERLIST_PROCEEDCHECKFIREWALL; }
        }

        [JsonProperty("port", Required = Required.Always)]
        [PacketItem(0)]
        public ushort Port { get; set; }

        [JsonProperty("token", Required = Required.Always)]
        [PacketItem(1)]
        public uint Token { get; set; }
    }
}

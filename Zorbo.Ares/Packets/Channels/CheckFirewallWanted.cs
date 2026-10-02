using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Channels
{
    sealed class CheckFirewallWanted : AresUdpPacket
    {
        public override AresUdpId Id {
            get { return AresUdpId.OP_SERVERLIST_WANTCHECKFIREWALL; }
        }

        [JsonProperty("port", Required = Required.Always)]
        [PacketItem(0)]
        public ushort Port { get; set; }


        public CheckFirewallWanted() { }

        public CheckFirewallWanted(ushort port) {
            Port = port;
        }
    }
}

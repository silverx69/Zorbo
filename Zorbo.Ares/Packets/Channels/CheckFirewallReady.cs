using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;
using Newtonsoft.Json;

namespace Zorbo.Ares.Packets.Channels
{
    sealed class CheckFirewallReady : AresUdpPacket
    {
        public override AresUdpId Id {
            get { return AresUdpId.OP_SERVERLIST_READYTOCHECKFIREWALL; }
        }

        [JsonProperty("token", Required = Required.Always)]
        [PacketItem(0)]
        public uint Token { get; set; }

        [JsonProperty("target", Required = Required.AllowNull)]
        [PacketItem(1)]
        public IPAddress Target { get; set; }


        public CheckFirewallReady() { }

        public CheckFirewallReady(uint cookie, IPAddress ip) {
            Token = cookie;
            Target = ip;
        }
    }
}

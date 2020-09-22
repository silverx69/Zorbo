using System.Linq;
using System.Text;
using Zorbo.Ares.Packets.Channels;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;

namespace Zorbo.Ares.Packets.Formatters
{
    /// <summary>
    /// An implementation of PacketFormatter&lt;AresUdpSerializer&gt; used for formatting packets to and from an Ares Udp node.
    /// </summary>
    public class ChannelFormatter : PacketFormatter<AresUdpSerializer>
    {
        public override IPacket Unformat(byte id, string data, bool ib0t = false)
        {
            return ((AresUdpId)id) switch
            {
                AresUdpId.OP_SERVERLIST_ACKINFO => Json.Deserialize<AckInfo>(data),
                AresUdpId.OP_SERVERLIST_ACKIPS => Json.Deserialize<AckIps>(data),
                AresUdpId.OP_SERVERLIST_ACKNODES => Json.Deserialize<AckNodes>(data),
                AresUdpId.OP_SERVERLIST_ADDIPS => Json.Deserialize<AddIps>(data),
                AresUdpId.OP_SERVERLIST_CHECKFIREWALLBUSY => Json.Deserialize<CheckFirewallBusy>(data),
                AresUdpId.OP_SERVERLIST_PROCEEDCHECKFIREWALL => Json.Deserialize<CheckFirewall>(data),
                AresUdpId.OP_SERVERLIST_READYTOCHECKFIREWALL => Json.Deserialize<CheckFirewallReady>(data),
                AresUdpId.OP_SERVERLIST_SENDINFO => Json.Deserialize<SendInfo>(data),
                AresUdpId.OP_SERVERLIST_SENDNODES => Json.Deserialize<SendNodes>(data),
                AresUdpId.OP_SERVERLIST_WANTCHECKFIREWALL => Json.Deserialize<CheckFirewallWanted>(data),
                _ => new UnknownJson(id, data),
            };
        }

        public override IPacket Unformat(byte id, byte[] data, int index, int count) {

            return ((AresUdpId)id) switch
            {
                AresUdpId.OP_SERVERLIST_ACKINFO => Serializer.Deserialize<AckInfo>(data, index, count),
                AresUdpId.OP_SERVERLIST_ACKIPS => Serializer.Deserialize<AckIps>(data, index, count),
                AresUdpId.OP_SERVERLIST_ACKNODES => Serializer.Deserialize<AckNodes>(data, index, count),
                AresUdpId.OP_SERVERLIST_ADDIPS => Serializer.Deserialize<AddIps>(data, index, count),
                AresUdpId.OP_SERVERLIST_CHECKFIREWALLBUSY => Serializer.Deserialize<CheckFirewallBusy>(data, index, count),
                AresUdpId.OP_SERVERLIST_PROCEEDCHECKFIREWALL => Serializer.Deserialize<CheckFirewall>(data, index, count),
                AresUdpId.OP_SERVERLIST_READYTOCHECKFIREWALL => Serializer.Deserialize<CheckFirewallReady>(data, index, count),
                AresUdpId.OP_SERVERLIST_SENDINFO => Serializer.Deserialize<SendInfo>(data, index, count),
                AresUdpId.OP_SERVERLIST_SENDNODES => Serializer.Deserialize<SendNodes>(data, index, count),
                AresUdpId.OP_SERVERLIST_WANTCHECKFIREWALL => Serializer.Deserialize<CheckFirewallWanted>(data, index, count),
                _ => new Unknown(id, data.Skip(index).Take(count).ToArray()),
            };
        }
    }
}

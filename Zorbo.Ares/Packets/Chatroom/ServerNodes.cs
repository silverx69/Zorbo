using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerNodes : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_HERE_SUPERNODES; }
        }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(0)]
        public byte[] Nodes { get; set; }
    }
}

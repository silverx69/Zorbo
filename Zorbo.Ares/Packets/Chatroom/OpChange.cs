using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class OpChange : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_OPCHANGE; }
        }

        [JsonProperty("admin", Required = Required.Always)]
        [PacketItem(0)]
        public bool IsAdmin { get; set; }

        [JsonProperty("ignored", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(1)]
        public byte Ignored { get; set; }


        public OpChange() { }

        public OpChange(bool admin) {
            IsAdmin = admin;
        }
    }
}

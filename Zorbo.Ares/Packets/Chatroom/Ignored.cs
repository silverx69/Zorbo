using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Ignored : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_IGNORELIST; }
        }

        [JsonProperty("ignored", Required = Required.AllowNull)]
        [PacketItem(0)]
        public bool Ignore { get; set; }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(1)]
        public string Username { get; set; }
    }
}

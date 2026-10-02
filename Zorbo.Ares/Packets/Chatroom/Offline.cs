using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Offline : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_OFFLINEUSER; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        public Offline() { }

        public Offline(string name) { Username = name; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class IgnoringYou : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_ISIGNORINGYOU; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }


        public IgnoringYou() { }

        public IgnoringYou(string username) {
            Username = username;
        }
    }
}

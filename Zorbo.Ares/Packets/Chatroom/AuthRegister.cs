using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class AuthRegister : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_AUTHREGISTER; }
        }

        [JsonProperty("sha1pass", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 255)]
        public string Password { get; set; }
    }
}

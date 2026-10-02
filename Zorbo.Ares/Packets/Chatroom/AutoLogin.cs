using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class AutoLogin : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_AUTOLOGIN; }
        }

        [JsonProperty("sha1pass", Required = Required.AllowNull)]
        [PacketItem(0)]
        public byte[] Sha1Password { get; set; }
    }
}

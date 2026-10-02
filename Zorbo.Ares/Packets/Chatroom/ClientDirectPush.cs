using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ClientDirectPush : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_DIRCHATPUSH; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("text_sync", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] TextSync { get; set; }
    }
}

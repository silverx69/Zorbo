using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Browse : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_BROWSE; }
        }

        [JsonProperty("browse_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort BrowseId { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [PacketItem(1)]
        public byte Type { get; set; }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(2, MaxLength = 20)]
        public string Username { get; set; }
    }
}

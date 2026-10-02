using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Search : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_SEARCH; }
        }

        [JsonProperty("search_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort SearchId { get; set; }

        [JsonIgnore]
        [PacketItem(1)]
        public byte Skipped { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [PacketItem(2)]
        public byte Type { get; set; }

        [JsonProperty("search", Required = Required.AllowNull)]
        [PacketItem(3, LengthPrefix = true)]
        public string SearchWords { get; set; }
    }
}

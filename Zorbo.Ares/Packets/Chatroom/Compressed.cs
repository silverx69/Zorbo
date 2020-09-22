using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Compressed : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENTCOMPRESSED; }
        }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(0)]
        public byte[] Data { get; set; }
    }
}

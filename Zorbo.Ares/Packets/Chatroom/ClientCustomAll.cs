using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ClientCustomAll : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_CUSTOM_DATA_ALL; }
        }

        [JsonProperty("msg", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string CustomId { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] Data { get; set; }

        public ClientCustomAll() { }

        public ClientCustomAll(string customId, byte[] data)
        {
            CustomId = customId;
            Data = data;
        }
    }
}

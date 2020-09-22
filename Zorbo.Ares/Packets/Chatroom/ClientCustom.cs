using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ClientCustom : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_CUSTOM_DATA; }
        }

        [JsonProperty("msg", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string CustomId { get; set; }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(1, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(2)]
        public byte[] Data { get; set; }

        public ClientCustom() { }

        public ClientCustom(string name, string ident, byte[] data) {
            CustomId = ident;
            Username = name;
            Data = data;
        }

        public ClientCustom(string name, ClientCustomAll custom) {
            CustomId = custom.CustomId;
            Username = name;
            Data = custom.Data;
        }
    }
}

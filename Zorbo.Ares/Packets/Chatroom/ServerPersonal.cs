using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerPersonal : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_PERSONAL_MESSAGE; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("message", Required = Required.AllowNull)]
        [PacketItem(1, MaxLength = 255, NullTerminated = false)]
        public string Message { get; set; }


        public ServerPersonal() { }

        public ServerPersonal(string name, string text) {
            Username = name;
            Message = text;
        }
    }
}

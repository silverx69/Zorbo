using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerEmote : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_EMOTE; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("message", Required = Required.AllowNull)]
        [PacketItem(1, MaxLength = 1024, NullTerminated = false)]
        public string Message { get; set; }

        public ServerEmote() { }

        public ServerEmote(string username, string text) {
            Username = username;
            Message = text;
        }
    }
}

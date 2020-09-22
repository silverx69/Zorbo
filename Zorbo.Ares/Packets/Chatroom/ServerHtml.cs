using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerHtml : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_HTML; }
        }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(0, NullTerminated = false)]
        public string Content { get; set; }

        public ServerHtml() { }

        public ServerHtml(string content) {
            Content = content;
        }
    }
}

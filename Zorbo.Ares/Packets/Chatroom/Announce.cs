using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Announce : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_NOSUCH; }
        }

        [JsonProperty("message", Required = Required.AllowNull)]
        [PacketItem(0, NullTerminated = false, MaxLength = 1024)]
        public string Message { get; set; }


        public Announce() { }

        public Announce(string text) { 
            Message = text;
        }
    }
}

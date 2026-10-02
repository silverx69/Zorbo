using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Error : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_ERROR; }
        }

        [JsonProperty("message", Required = Required.AllowNull)]
        [PacketItem(0, NullTerminated = false)]
        public string Message { get; set; }


        public Error() { }

        public Error(string message) {
            Message = message;
        }
    }
}

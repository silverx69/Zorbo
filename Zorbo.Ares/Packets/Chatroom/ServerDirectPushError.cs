using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class DirectPushError : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_DIRCHATPUSH; }
        }

        [JsonProperty("error", Required = Required.Always)]
        [PacketItem(0)]
        public byte Error { get; set; }


        public DirectPushError() { }

        public DirectPushError(byte code) {
            Error = code;
        }
    }
}

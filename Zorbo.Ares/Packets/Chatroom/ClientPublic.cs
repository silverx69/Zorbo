﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ClientPublic : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_PUBLIC; }
        }

        [JsonProperty("message", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 1024)]
        public string Message { get; set; }
    }
}

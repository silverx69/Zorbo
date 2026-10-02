using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class BrowseError : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_BROWSEERROR; }
        }

        [JsonProperty("browse_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort BrowseId { get; set; }


        public BrowseError() { }

        public BrowseError(ushort browseid) {
            BrowseId = browseid;
        }
    }
}

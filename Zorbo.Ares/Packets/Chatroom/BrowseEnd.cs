using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class BrowseEnd : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_ENDOFBROWSE; }
        }

        [JsonProperty("browse_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort BrowseId { get; set; }

        public BrowseEnd() { }

        public BrowseEnd(ushort browseid) {
            BrowseId = browseid;
        }
    }
}

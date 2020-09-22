using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class BrowseStart : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_STARTOFBROWSE; }
        }

        [JsonProperty("browse_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort BrowseId { get; set; }

        [JsonProperty("count", Required = Required.Always)]
        [PacketItem(1)]
        public ushort FileCount { get; set; }


        public BrowseStart() { }

        public BrowseStart(ushort browseid, ushort filecount) {
            BrowseId = browseid;
            FileCount = filecount;
        }
    }
}

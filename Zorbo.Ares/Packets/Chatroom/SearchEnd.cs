using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class SearchEnd : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_ENDOFSEARCH; }
        }

        [JsonProperty("search_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort SearchId { get; set; }


        public SearchEnd() { }

        public SearchEnd(ushort searchid) {
            SearchId = searchid;
        }
    }
}

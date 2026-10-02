using Newtonsoft.Json;
using Zorbo.Core.Data.Packets;
using Zorbo.Core;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Packets.Chatroom.ib0t
{
    public class ScribbleBlock : ModelBase, IPacket
    {
        byte IPacket.Id { get { return 0; } }

        [JsonProperty("block", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Block { get; set; }

        public ScribbleBlock() { }

        public ScribbleBlock(string block) { Block = block; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Data.Packets;
using Zorbo.Core;
using Zorbo.Core.Models;

namespace cb0tProtocol.Packets.ib0t
{
    public class ScribbleHead : ModelBase, IPacket
    {
        byte IPacket.Id { get { return 0; } }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Name { get; set; }

        [JsonProperty("height", Required = Required.AllowNull)]
        [PacketItem(1)]
        public int Height { get; set; }

        [JsonProperty("blocks", Required = Required.AllowNull)]
        [PacketItem(2)]
        public int BlockCount { get; set; }

        public ScribbleHead() { }

        public ScribbleHead(string name, int height, int blocks) {
            Name = name;
            Height = height;
            BlockCount = blocks;
        }
    }
}

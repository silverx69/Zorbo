using Newtonsoft.Json;
using System;
using System.Dynamic;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Models;

namespace cb0tProtocol.Packets
{
    public class Advanced : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL; }
        }

        [JsonProperty("payload", Required = Required.AllowNull)]
        [PacketItem(0)]
        public IPacket Payload { get; set; }

        public Advanced() { }

        public Advanced(IPacket packet) { Payload = packet; }
    }

    public class AdvancedJson : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL; }
        }

        [JsonProperty("payload", Required = Required.AllowNull)]
        public string Payload { get; set; }
    }

    [JsonObject]
    [Serializable]
    public abstract class AdvancedPacket : ModelBase, IPacket
    {
        [JsonProperty("id", Required = Required.Always)]
        public abstract AdvancedId Id { get; }

        byte IPacket.Id { get { return (byte)Id; } }
    }
}

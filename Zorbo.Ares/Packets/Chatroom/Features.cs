using Newtonsoft.Json;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Features : AresPacket  
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_MYFEATURES; }
        }

        [JsonProperty("version", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 30)]
        public string Version { get; set; }

        [JsonProperty("flags", Required = Required.Always)]
        [PacketItem(1)]
        public SupportFlags SupportFlag { get; set; }

        [JsonProperty("shared_flags", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(2)]
        public byte SharedTypes { get; set; }

        [JsonProperty("language", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(3)]
        public Language Language { get; set; }

        [JsonProperty("token", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(4)]
        public uint Token { get; set; }

        [JsonProperty("avatars", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5)]
        public bool Avatars { get; set; } = true;
    }
}

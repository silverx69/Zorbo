using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;
using Zorbo.Core;

namespace cb0tProtocol.Packets
{
    public class ClientFont : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_CUSTOM_FONT; }
        }

        [JsonProperty("size", Required = Required.AllowNull)]
        [PacketItem(0)]
        public byte Size { get; set; } = 11;

        [JsonProperty("name", Required = Required.AllowNull)]
        [PacketItem(1)]
        public string Name { get; set; } = "Verdana";

        [JsonProperty("name_color", Required = Required.Always)]
        [PacketItem(2, Optional = true, OptionalValue = (byte)255)]
        public ColorCode NameColor { get; set; } = ColorCode.None;

        [JsonProperty("text_color", Required = Required.Always)]
        [PacketItem(3, Optional = true, OptionalValue = (byte)255)]
        public ColorCode TextColor { get; set; } = ColorCode.None;

        [JsonProperty("name_color2", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(4, Optional = true)]
        public string NameColor2 { get; set; }

        [JsonProperty("text_color2", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5, Optional = true)]
        public string TextColor2 { get; set; }
    }
}

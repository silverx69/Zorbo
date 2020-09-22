using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;

namespace cb0tProtocol.Packets
{
    public class ServerFont : AdvancedPacket
    {
        byte size = 11;

        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_SERVER_CUSTOM_FONT; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0)]
        public string Username { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        [PacketItem(1)]
        public byte Size {
            get { return size; }
            set {
                if (value < 8)
                    size = 8;
                else if (value > 18)
                    size = 18;
                else
                    size = value;
            }
        }

        [JsonProperty("name", Required = Required.AllowNull)]
        [PacketItem(2)]
        public string Name { get; set; } = "Verdana";

        [JsonProperty("name_color", Required = Required.Always)]
        [PacketItem(3, Optional = true, OptionalValue = (byte)255)]
        public ColorCode NameColor { get; set; } = ColorCode.None;

        [JsonProperty("text_color", Required = Required.Always)]
        [PacketItem(4, Optional = true, OptionalValue = (byte)255)]
        public ColorCode TextColor { get; set; } = ColorCode.None;

        [JsonProperty("name_color2", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5, Optional = true)]
        public string NameColor2 { get; set; }

        [JsonProperty("text_color2", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(6, Optional = true)]
        public string TextColor2 { get; set; }

        public ServerFont() { }

        public ServerFont(IClient user) {
            Username = user.Name;
        }
    }
}

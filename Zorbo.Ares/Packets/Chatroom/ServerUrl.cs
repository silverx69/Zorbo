using Newtonsoft.Json;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ServerUrl : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_URL; }
        }

        [JsonProperty("address", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 255)]
        public string Address { get; set; }

        [JsonProperty("caption", Required = Required.AllowNull)]
        [PacketItem(1, MaxLength = 255)]
        public string Caption { get; set; }

        public ServerUrl() { }

        public ServerUrl(string address, string caption) {
            Address = address;
            Caption = caption;
        }
    }
}

using Newtonsoft.Json;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Website : AresPacket
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


        public Website() { }

        public Website(string address, string caption) {
            Address = address;
            Caption = caption;
        }
    }
}

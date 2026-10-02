using Newtonsoft.Json;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class AuthLogin : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_AUTHLOGIN; }
        }

        [JsonProperty("password", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 255)]
        public string Password { get; set; }
    }
}

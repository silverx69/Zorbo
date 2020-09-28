using Newtonsoft.Json;
using System;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerAvatar : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_AVATAR; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(1)]
        public byte[] AvatarBytes { get; set; }

        public ServerAvatar() { }

        public ServerAvatar(IClient user) {
            Username = user.Name;
            AvatarBytes = user.Avatar ?? new byte[0];
        }

        public ServerAvatar(string name, byte[] avatar) {
            Username = name;
            AvatarBytes = avatar ?? new byte[0];
        }
    }
}

using Newtonsoft.Json;
using System;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class SharedFile : AresPacket, ISharedFile
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_ADDSHARE; }
        }

        [JsonProperty("type", Required = Required.Always)]
        [PacketItem(0)]
        public byte Type { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        [PacketItem(1)]
        public uint Size { get; set; }

        [JsonProperty("search", Required = Required.AllowNull)]
        [PacketItem(2, LengthPrefix = true)]
        public string SearchWords { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(3)]
        public byte[] Content { get; set; }
    }
}

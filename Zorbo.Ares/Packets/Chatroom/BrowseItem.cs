using Newtonsoft.Json;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class BrowseItem : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_BROWSEITEM; }
        }

        [JsonProperty("browse_id", Required = Required.Always)]
        [PacketItem(0)]
        public ushort BrowseId { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [PacketItem(1)]
        public byte Type { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        [PacketItem(2)]
        public uint Size { get; set; }

        [JsonProperty("data", Required = Required.AllowNull)]
        [PacketItem(3)]
        public byte[] Content { get; set; }

        public BrowseItem() { }

        public BrowseItem(ushort browseid, ISharedFile file) {
            BrowseId = browseid;
            Type = file.Type;
            Size = file.Size;
            Content = file.Content;
        }
    }
}

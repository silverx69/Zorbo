using Newtonsoft.Json;
using System;
using System.Net;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerDirectPush : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_DIRCHATPUSH; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("external_ip", Required = Required.AllowNull)]
        [PacketItem(1)]
        public IPAddress ExternalIp { get; set; }

        [JsonProperty("direct_port", Required = Required.Always)]
        [PacketItem(2)]
        public ushort ListenPort { get; set; }

        [JsonProperty("local_ip", Required = Required.AllowNull)]
        [PacketItem(3)]
        public IPAddress LocalIp { get; set; }

        [JsonProperty("text_sync", Required = Required.AllowNull)]
        [PacketItem(4)]
        public byte[] TextSync { get; set; }

        public ServerDirectPush() { }

        public ServerDirectPush(IClient client, ClientDirectPush push) {
            Username = client.Name;
            ExternalIp = client.ExternalIp;
            ListenPort = client.ListenPort;
            LocalIp = client.LocalIp;
            TextSync = push.TextSync;
        }
    }
}

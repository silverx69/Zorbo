using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;
using Zorbo.Core;
using Newtonsoft.Json;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Redirect : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_REDIRECT; }
        }

        [JsonProperty("external_ip", Required = Required.AllowNull)]
        [PacketItem(0)]
        public IPAddress ExternalIp { get; set; }

        [JsonProperty("port", Required = Required.Always)]
        [PacketItem(1)]
        public ushort Port { get; set; }

        [JsonProperty("local_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(2)]
        public IPAddress LocalIp { get; set; }

        [JsonProperty("servername", Required = Required.AllowNull)]
        [PacketItem(3, MaxLength = 20)]
        public string Name { get; set; }

        [JsonProperty("message", Required = Required.AllowNull)]
        [PacketItem(4, MaxLength = 255)]
        public string Message { get; set; }


        public Redirect() { }

        public Redirect(IChannel hash, String message) {
            Name = hash.Name;
            Port = hash.Port;
            LocalIp = hash.LocalIp;
            ExternalIp = hash.ExternalIp;
            Message = message;
        }
    }
}

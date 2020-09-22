using Newtonsoft.Json;
using System.Net;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerUpdate : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_UPDATE_USER_STATUS; }
        }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 20)]
        public string Username { get; set; }

        [JsonProperty("files", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(1)]
        public ushort FileCount { get; set; }

        [JsonProperty("browsable", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(2)]
        public bool Browsable { get; set; }

        [JsonProperty("node_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(3)]
        public IPAddress NodeIp { get; set; }

        [JsonProperty("node_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(4)]
        public ushort NodePort { get; set; }

        [JsonProperty("external_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5)]
        public IPAddress ExternalIp { get; set; }

        [JsonProperty("level", Required = Required.Always)]
        [PacketItem(6)]
        public AdminLevel Level { get; set; }

        [JsonProperty("age", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(7)]
        public byte Age { get; set; }

        [JsonProperty("gender", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(8)]
        public Gender Gender { get; set; }

        [JsonProperty("country", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(9)]
        public Country Country { get; set; }

        [JsonProperty("region", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(10, MaxLength = 30)]
        public string Region { get; set; }


        public ServerUpdate() { }

        public ServerUpdate(IClient client) {
            Username = client.Name;
            FileCount = client.FileCount;
            Browsable = client.Browsable;
            NodeIp = client.NodeIp;
            NodePort = client.NodePort;
            ExternalIp = client.ExternalIp;
            Level = client.Admin;
            Age = client.Age;
            Gender = client.Gender;
            Country = client.Country;
            Region = client.Region;
        }
    }
}

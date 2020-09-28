using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    #region " JoinBase "

    public abstract class JoinBase : AresPacket
    {
#pragma warning disable IDE0044 // Add readonly modifier
        string username = "";
        string region = "";

        ushort files = 0;
        bool browsable = false;
        AdminLevel level = AdminLevel.User;

        byte age = 0;
        Gender gender = Gender.Unknown;
        Country country = Country.Unknown;

        IPAddress externalIp = null;
        IPAddress internalIp = null;

        ClientFlags features = ClientFlags.NONE;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("files", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(0)]
        public ushort FileCount {
            get { return files; }
            set { OnPropertyChanged(() => files, value); }
        }

        [JsonIgnore]
        [PacketItem(1)]
        public int Skipped1 { get; set; }

        [JsonProperty("external_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(2)]
        public IPAddress ExternalIp {
            get { return externalIp; }
            set { OnPropertyChanged(() => externalIp, value); }
        }

        [JsonProperty("direct_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(3)]
        public ushort DCPort { get; set; }

        [JsonProperty("node_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(4)]
        public IPAddress NodeIp { get; set; }

        [JsonProperty("node_port", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(5)]
        public ushort NodePort { get; set; }

        [JsonIgnore]
        [PacketItem(6)]
        public byte Skipped2 { get; set; }

        [JsonProperty("username", Required = Required.AllowNull)]
        [PacketItem(7, MaxLength = 20)]
        public string Username {
            get { return username; }
            set { OnPropertyChanged(() => username, value); }
        }

        [JsonProperty("local_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(8)]
        public IPAddress LocalIp {
            get { return internalIp; }
            set { OnPropertyChanged(() => internalIp, value); }
        }

        [JsonProperty("browsable", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(9)]
        public bool Browsable {
            get { return browsable; }
            set { OnPropertyChanged(() => browsable, value); }
        }

        [JsonProperty("level", Required = Required.Always)]
        [PacketItem(10)]
        public AdminLevel Level {
            get { return level; }
            set { OnPropertyChanged(() => level, value); }
        }

        [JsonProperty("age", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(11)]
        public byte Age {
            get { return age; }
            set { OnPropertyChanged(() => age, value); }
        }

        [JsonProperty("gender", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(12)]
        public Gender Gender {
            get { return gender; }
            set { OnPropertyChanged(() => gender, value); }
        }

        [JsonProperty("country", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(13)]
        public Country Country {
            get { return country; }
            set { OnPropertyChanged(() => country, value); }
        }

        [JsonProperty("region", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(14, MaxLength = 30)]
        public string Region {
            get { return region; }
            set { OnPropertyChanged(() => region, value); }
        }

        [JsonProperty("features", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [PacketItem(15, Optional = true, OptionalValue = ClientFlags.NONE)]
        public ClientFlags Features {
            get { return features; }
            set { OnPropertyChanged(() => features, value); }
        }

        public JoinBase() { }

        public JoinBase(IClient user)
        {
            FileCount = user.FileCount;
            ExternalIp = user.Server.Config.HideIPs ? IPAddress.Any : user.ExternalIp;
            DCPort = user.ListenPort;
            NodeIp = user.NodeIp;
            NodePort = user.NodePort;
            Username = user.Name;
            LocalIp = user.Server.Config.HideIPs ? IPAddress.Any : user.LocalIp;
            Browsable = user.Browsable;
            Level = user.Admin;
            Age = user.Age;
            Gender = user.Gender;
            Country = user.Country;
            Region = user.Region;
            Features = user.Features;
        }
    }

    #endregion

    public sealed class Join : JoinBase
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_JOIN; }
        }

        public Join() { }

        public Join(IClient user) : base(user) { }
    }
}

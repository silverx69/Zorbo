using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Zorbo.Ares.Mars;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Models;

namespace Zorbo.Ares
{
    [JsonObject]
    public class AresChannel : 
        ModelBase, 
        IChannel, 
        IEquatable<AresChannel>,
        IEquatable<MarsChannel>
    {
#pragma warning disable IDE0044 // Add readonly modifier
        string name = "";
        string topic = "";
        string version = "";

        ushort port = 0;
        ushort users = 0;

        uint ackcount = 0;
        uint trycount = 0;

        bool json = false;
        bool websockets = false;
        
        DateTime asked = DateTime.MinValue;
        DateTime lastAck = DateTime.MinValue;
        DateTime lastTry = DateTime.MinValue;
        DateTime lastInfo = DateTime.MinValue;

        IPAddress localIp = null;
        IPAddress externalIp = null;

        Language language = Language.Unknown;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Name {
            get { return name; }
            set { OnPropertyChanged(() => name, value); }
        }

        [JsonProperty("topic", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Topic {
            get { return topic; }
            set { OnPropertyChanged(() => topic, value); }
        }

        [JsonProperty("version", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Version {
            get { return version; }
            set { OnPropertyChanged(() => version, value); }
        }

        [JsonProperty("port", Required = Required.Always)]
        public ushort Port {
            get { return port; }
            set { OnPropertyChanged(() => port, value); }
        }

        [JsonProperty("users", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ushort Users {
            get { return users; }
            set { OnPropertyChanged(() => users, value); }
        }

        [JsonProperty("local_ip", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IPAddress LocalIp {
            get { return localIp; }
            set { OnPropertyChanged(() => localIp, value); }
        }

        [JsonProperty("external_ip", Required = Required.Always)]
        public IPAddress ExternalIp {
            get { return externalIp; }
            set { OnPropertyChanged(() => externalIp, value); }
        }

        [JsonProperty("websockets", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool WebSockets {
            get { return websockets; }
            set { OnPropertyChanged(() => websockets, value); }
        }

        [JsonProperty("json", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool SupportJson {
            get { return json; }
            set { OnPropertyChanged(() => json, value); }
        }

        [JsonProperty("language", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Language Language {
            get { return language; }
            set { OnPropertyChanged(() => language, value); }
        }

        [JsonProperty("acks", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public uint AckCount {
            get { return ackcount; }
            set { OnPropertyChanged(() => ackcount, value); }
        }

        [JsonProperty("tries", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public uint TryCount {
            get { return trycount; }
            set { OnPropertyChanged(() => trycount, value); }
        }

        [JsonProperty("last_ack", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime LastAcked {
            get { return lastAck; }
            set { OnPropertyChanged(() => lastAck, value); }
        }

        [JsonProperty("last_try", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime LastSendIPs {
            get { return lastTry; }
            set { OnPropertyChanged(() => lastTry, value); }
        }

        [JsonProperty("last_info", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime LastSendInfo {
            get { return lastInfo; }
            set { OnPropertyChanged(() => lastInfo, value); }
        }

        [JsonProperty("last_fw", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime LastAskedFirewall {
            get { return asked; }
            set { OnPropertyChanged(() => asked, value); }
        }

        public static readonly byte[] HASH_HEADER = new byte[20];

        /// <summary>
        /// constructor used by serializer
        /// </summary>
        public AresChannel() {
            DateTime now = DateTime.Now;
            this.lastAck = now.Subtract(TimeSpan.FromMinutes(15));
            this.lastTry = now.Subtract(TimeSpan.FromMinutes(1));
        }

        public AresChannel(uint ip, ushort port)
            : this(ip.ToIPAddress(), port) { }

        public AresChannel(IPAddress ip, ushort port)
            : this(ip, port, 0) { }

        public AresChannel(IPAddress ip, ushort port, uint ackcount) {
            this.externalIp = ip;
            this.port = port;
            this.ackcount = ackcount;

            DateTime now = DateTime.Now;

            this.lastAck = now.Subtract(TimeSpan.FromMinutes(15));
            this.lastTry = now.Subtract(TimeSpan.FromMinutes(1));
        }

        public void CopyFrom(MarsChannel other)
        {
            Port = (ushort)other.Port;
            ExternalIp = IPAddress.Parse(other.ExternalIP);
            LocalIp = IPAddress.Parse(other.LocalIP);
            Name = other.Name;
            Topic = other.Topic;
            WebSockets = true;
        }

        public bool Equals(AresChannel other)
        {
            if (other == null)
                return false;
            return ExternalIp.Equals(other.ExternalIp) && Port == other.Port;
        }

        public bool Equals(MarsChannel other)
        {
            return ExternalIp.ToString() == other.ExternalIP && Port == (ushort)other.Port;
        }

        public override bool Equals(object obj)
        {
            if (obj is AresChannel channel)
                return Equals(channel);

            else if (obj is MarsChannel mchan)
                return Equals(mchan);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExternalIp, Port);
        }

        public byte[] ToByteArray() {

            using PacketWriter writer = new PacketWriter();

            writer.Write(HASH_HEADER);
            writer.Write("CHATCHANNEL");
            writer.Write(ExternalIp ?? IPAddress.Any);
            writer.Write(Port);
            writer.Write(LocalIp ?? IPAddress.Any);
            writer.Write(Name);
            writer.Write(SupportJson);

            return writer.ToArray();
        }

        public void FromByteArray(byte[] bytes) {

#pragma warning disable IDE0017 // Simplify object initialization
            using PacketReader reader = new PacketReader(bytes);
#pragma warning restore IDE0017 // Simplify object initialization

            reader.Position = 20;

            Debug.Assert(reader.ReadString().ToUpper() == "CHATCHANNEL");

            ExternalIp = reader.ReadIPAddress();
            Port = reader.ReadUInt16();
            LocalIp = reader.ReadIPAddress();
            Name = reader.ReadString();
            SupportJson = WebSockets = reader.ReadBoolean();
        }

        public string ToPlainText() {
            return string.Format("Chatroom:{0}:{1}|{2}", ExternalIp, Port, Name);
        }

        public void FromPlainText(string text)
        {
            var match = Regex.Match(text, "Chatroom:(.+?):([0-9]+)\\|([^\\x20]+)");
            if (match.Success) {
                if (IPAddress.TryParse(match.Groups[1].Value, out IPAddress ip))
                    ExternalIp = ip;
                
                if (int.TryParse(match.Groups[2].Value, out int port))
                    Port = (ushort)port;

                Name = match.Groups[3].Value;
            }
        }
    }
}

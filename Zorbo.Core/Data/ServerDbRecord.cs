using Newtonsoft.Json;
using System;
using System.Net;
using System.Text.RegularExpressions;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Core.Data
{
    public partial class ServerDbRecord : 
        IEquatable<ServerDbRecord>, 
        IHashlink
    {
        string topic;
        string hashlinkUri = "";

        //Database model properties

        public long Id { get; set; }

        public string Name { get; set; }

        public string Topic {
            get { return topic; }
            set { topic = value; BareTopic = value.StripColor(); }
        }

        public string Version { get; set; }

        public ushort Users { get; set; }

        public byte Language { get; set; }

        public bool WebSockets { get; set; }

        public bool SupportJson { get; set; }

        public string Domain { get; set; }

        public string ExternalIp { get; set; }

        public string InternalIp { get; set; }

        public ushort Port { get; set; }

        [JsonIgnore]
        public DateTime LastUpdate { get; set; }

        //Not part of database model
        public string BareTopic { get; private set; }

        [JsonIgnore]
        public string HashlinkUri {
            get {
                if (string.IsNullOrEmpty(hashlinkUri))
                    hashlinkUri = Hashlinks.ToHashlinkString(this).ToURLSafeBase64();
                return hashlinkUri;
            }
        }


        public ServerDbRecord() { }

        public byte[] ToByteArray()
        {
            using PacketWriter writer = new PacketWriter();
            writer.Write(new byte[20]);
            writer.Write("CHATCHANNEL");
            writer.Write(IPAddress.Parse(string.IsNullOrEmpty(ExternalIp) ? "0.0.0.0" : ExternalIp) ?? IPAddress.Any);
            writer.Write((ushort)Port);
            writer.Write(IPAddress.Parse(string.IsNullOrEmpty(InternalIp) ? "0.0.0.0" : InternalIp) ?? IPAddress.Any);
            writer.Write(Name);
            writer.Write(SupportJson);
            if (!string.IsNullOrEmpty(Domain))
                writer.Write(Domain);
            return writer.ToArray();
        }

        public void FromByteArray(byte[] bytes)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            using PacketReader reader = new PacketReader(bytes);
#pragma warning restore IDE0017 // Simplify object initialization
            reader.Position = 32;
            ExternalIp = reader.ReadIPAddress().ToString();
            Port = reader.ReadUInt16();
            InternalIp = reader.ReadIPAddress().ToString();
            Name = reader.ReadString();
            SupportJson = reader.ReadBoolean();
            Domain = reader.ReadString();
        }

        public string ToPlainText()
        {
            return string.Format("Chatroom:{0}:{1}|{2}", ExternalIp, Port, Name);
        }

        public void FromPlainText(string text)
        {
            var match = Regex.Match(text, "Chatroom:(.+?):([0-9]+)\\|([^\\x20]+)", RegexOptions.IgnoreCase);
            if (match.Success) {
                ExternalIp = match.Groups[1].Value;

                if (int.TryParse(match.Groups[2].Value, out int port))
                    Port = (ushort)port;

                Name = match.Groups[3].Value;
            }
        }

        public bool Equals(ServerDbRecord other)
        {
            return ExternalIp == other.ExternalIp && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is ServerDbRecord record)
                return Equals(record);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExternalIp, Name);
        }
    }
}

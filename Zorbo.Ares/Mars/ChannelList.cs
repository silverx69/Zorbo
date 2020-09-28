using Newtonsoft.Json;
using System;
using System.Net;
using System.Text.RegularExpressions;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Mars
{
    public class ChannelList
    {
        public int Count { get; set; }

        public MarsChannel[] Items { get; set; }

        public static int SortByNameAscending(MarsChannel a, MarsChannel b)
        {
            return string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public static int SortByNameDescending(MarsChannel a, MarsChannel b)
        {
            return string.Compare(b.Name, a.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class MarsChannel : IEquatable<MarsChannel>, IHashlink
    {
        string topic = string.Empty;
        string hashlinkUri = string.Empty;

        public string Name { get; set; }

        public string Topic {
            get { return topic; }
            set { topic = value; BareTopic = topic.StripColor(); }
        }

        public string BareTopic { get; set; }

        public string ExternalIP { get; set; } = string.Empty;

        public string LocalIP { get; set; } = string.Empty;

        public int Port { get; set; }

        public int Time { get; set; }

        [JsonIgnore]
        public string HashlinkUri { 
            get {
                if (string.IsNullOrEmpty(hashlinkUri))
                    hashlinkUri = Hashlinks.ToHashlinkString(this).ToURLSafeBase64();
                return hashlinkUri;
            }
        }

        public void CopyFrom(MarsChannel channel)
        {
            Name = channel.Name;
            Topic = channel.Topic;
            ExternalIP = channel.ExternalIP;
            LocalIP = channel.LocalIP;
            Port = channel.Port;
            Time = channel.Time;
        }

        public byte[] ToByteArray()
        {
            using PacketWriter writer = new PacketWriter();

            writer.Write(new byte[20]);
            writer.Write("CHATCHANNEL");
            writer.Write(IPAddress.Parse(ExternalIP) ?? IPAddress.Any);
            writer.Write((ushort)Port);
            writer.Write(IPAddress.Parse(LocalIP) ?? IPAddress.Any);
            writer.Write(Name);
            writer.Write((byte)0);

            return writer.ToArray();
        }

        public void FromByteArray(byte[] bytes)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            using PacketReader reader = new PacketReader(bytes);
#pragma warning restore IDE0017 // Simplify object initialization

            reader.Position = 32;
            ExternalIP = reader.ReadIPAddress().ToString();
            Port = reader.ReadUInt16();
            LocalIP = reader.ReadIPAddress().ToString();
            Name = reader.ReadString();
        }

        public string ToPlainText()
        {
            return string.Format("Chatroom:{0}:{1}|{2}", ExternalIP, Port, Name);
        }

        public void FromPlainText(string text)
        {
            var match = Regex.Match(text, "Chatroom:(.+?):([0-9]+)\\|([^\\x20]+)");
            if (match.Success) {
                if (IPAddress.TryParse(match.Groups[1].Value, out _))
                    ExternalIP = match.Groups[1].Value;

                if (int.TryParse(match.Groups[2].Value, out int port))
                    Port = (ushort)port;

                Name = match.Groups[3].Value;
            }
        }

        public bool Equals(MarsChannel other)
        {
            if (other == null)
                return false;
            return ExternalIP == other.ExternalIP && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (obj is MarsChannel channel)
                return Equals(channel);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExternalIP, Port);
        }
    }
}

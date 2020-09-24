using Newtonsoft.Json;
using System;
using System.Net;
using System.Security;
using System.Text;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Core.Interfaces
{
    [JsonObject]
    public class Password : ModelBase, IPassword
    {
#pragma warning disable IDE0044 // Add readonly modifier
        string sha1text;

        ClientId clientid;
        AdminLevel level;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("id", Required = Required.Always)]
        public ClientId ClientId {
            get { return clientid; }
            set { OnPropertyChanged(() => clientid, value); }
        }

        [JsonProperty("level", Required = Required.Always)]
        public AdminLevel Level {
            get { return level; }
            set { OnPropertyChanged(() => level, value); }
        }

        [JsonProperty("hash", Required = Required.Always)]
        public string Sha1Text {
            get { return sha1text; }
            set { OnPropertyChanged(() => sha1text, value); }
        }

        public Password() { }

        public Password(IClient client, string text) {
            this.clientid = new ClientId(client);
            this.level = client.Admin;
            this.sha1text = CreateSha1Text(text);
        }

        public Password(Record record, AdminLevel level, SecureString pass) {
            this.clientid = record.ClientId;
            this.level = level;
            this.sha1text = CreateSha1Text(pass);
        }

        public Password(Guid guid, IPAddress ip, string sha1text, AdminLevel level) {
            this.clientid = new ClientId(guid, ip);
            this.level = level;
            this.sha1text = sha1text;
        }

        public Password(Guid guid, IPAddress ip, SecureString pass, AdminLevel level) {
            this.clientid = new ClientId(guid, ip);
            this.level = level;
            this.sha1text = CreateSha1Text(pass);
        }

        public bool Equals(IPassword other)
        {
            return ClientId.Equals(other.ClientId) && Sha1Text == other.Sha1Text;
        }

        public override bool Equals(object obj)
        {
            if (obj is IPassword pass)
                return Equals(pass);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, Sha1Text);
        }

        public static string CreateSha1Text(string password) {
            return Convert.ToBase64String(Utils.SHA1.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        public static string CreateSha1Text(SecureString password) {
            return Convert.ToBase64String(Utils.MD5.ComputeHash(Encoding.UTF8.GetBytes(password.ToNativeString())));
        }
    }
}

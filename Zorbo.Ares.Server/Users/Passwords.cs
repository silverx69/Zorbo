using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server.Users
{
    [JsonArray]
    public class Passwords : ModelList<IPassword>, IPasswords
    {
        protected internal IServer Server {
            get;
            set;
        }

        public Passwords(IServer server, IEnumerable<IPassword> passwords)
            : base(passwords)
        {
            Server = server;
        }

        public new bool Add(IPassword password) {

            int index = this.FindIndex((s) => s.ClientId.Equals(password.ClientId));

            if (index > -1) {
                this[index].Sha1Text = password.Sha1Text;
                return true;
            }

            base.Add(password);
            return true;
        }

        public bool Add(IClient client, string password) {

            int index = this.FindIndex((s) => s.ClientId.Equals(client));

            if (index > -1) {
                this[index].Sha1Text = Password.CreateSha1Text(password);
                return true;
            }

            base.Add(new Password(client, password));
            return true;
        }


        public bool Remove(string password) {
            int index = this.FindIndex((s) => s.Sha1Text == password);
            if (index > -1) RemoveAt(index);
            return index > -1;
        }

        public IPassword CheckSha1(IClient client, byte[] password)
        {
            if (password.Length != 20)
                throw new ArgumentOutOfRangeException("password", "SHA1 password must be 20 bytes in length!");

            foreach (var pass in this) {
                if (!string.IsNullOrEmpty(pass.Sha1Text)) {
                    IPAddress ip = Server.ExternalIp;

                    if (client.LocalHost) {
                        if (IPAddress.IsLoopback(client.ExternalIp))
                            ip = IPAddress.Loopback;
                        else
                            ip = Server.InternalIp;
                    }

                    using var writer = new PacketWriter();

                    writer.Write(client.Token);
                    writer.Write(ip);
                    writer.Write(Convert.FromBase64String(pass.Sha1Text));

                    byte[] c = writer.ToArray();
                    c = Utils.SHA1.ComputeHash(c);

                    if (password.SequenceEqual(c))
                        return pass;
                }
            }

            return null;
        }
    }
}

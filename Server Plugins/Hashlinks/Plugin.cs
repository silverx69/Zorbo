using System;
using Zorbo.Ares;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Core;
using Zorbo.Core.Server;
using Zorbo.Core.Plugins.Server;

namespace Hashlinks
{
    public class Hashlinks : ServerPlugin
    {
        public override void OnPluginLoaded() {
            Server.SendAnnounce("Hashlinks plugin has been loaded!");
        }

        public  override void OnPluginKilled() {
            Server.SendAnnounce("Hashlinks plugin has been unloaded!");
        }

        public override bool OnTextCommand(IClient client, string cmd, string args)
        {
            switch (cmd) {
                case "hashlink": {
                    Server.SendAnnounce("\\\\arlnk://" +
                        Zorbo.Core.Hashlinks.ToHashlinkString(new AresChannel() {
                            Name = Server.Config.Name,
                            Port = Server.Config.Port,
                            InternalIp = Server.InternalIp,
                            ExternalIp = Server.ExternalIp,
                            WebSockets = Server.Config.UseWebSockets,
                            SupportJson = Server.Config.UseWebSockets,
                            Domain = Server.Config.Domain,
                            TlsPort = Server.Config.TlsPort
                        }));
                    break;
                }
                case "decrypt": {
                    var hash = Zorbo.Core.Hashlinks.FromHashlinkString<AresChannel>(args);
                    Server.SendAnnounce(string.Format("Name: {0}", hash.Name));
                    Server.SendAnnounce(string.Format("Port: {0}", hash.Port));
                    Server.SendAnnounce(string.Format("Domain: {0}", string.IsNullOrEmpty(hash.Domain) ? "Not configured" : hash.Domain));
                    Server.SendAnnounce(string.Format("External Ip: {0}", hash.ExternalIp));
                    Server.SendAnnounce(string.Format("Local Ip: {0}", hash.InternalIp));
                    Server.SendAnnounce(string.Format("JSON: {0}", hash.SupportJson));
                    break;
                }
            }
            return true;
        }
    }
}

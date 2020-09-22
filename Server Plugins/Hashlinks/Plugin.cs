using System;
using Zorbo.Ares;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
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

        public override bool OnBeforePacket(IClient client, IPacket packet) {

            switch ((AresId)packet.Id) {
                case AresId.MSG_CHAT_CLIENT_PUBLIC:
                    ClientPublic pub = (ClientPublic)packet;

                    if (pub.Message.StartsWith("#hashlink")) {

                        Server.SendAnnounce("\\\\arlnk://" +
                            Zorbo.Core.Hashlinks.ToHashlinkString(new AresChannel() {
                                Name = Server.Config.Name,
                                Port = Server.Config.Port,
                                LocalIp = Server.InternalIp,
                                ExternalIp = Server.ExternalIp,
                                SupportJson = true
                            }));
                    }
                    else if (pub.Message.Length > 9 && pub.Message.StartsWith("#decrypt ")) {

                        var hash = Zorbo.Core.Hashlinks.FromHashlinkString<AresChannel>(pub.Message.Substring(9));

                        Server.SendAnnounce(string.Format("Name: {0}", hash.Name));
                        Server.SendAnnounce(string.Format("Port: {0}", hash.Port));
                        Server.SendAnnounce(string.Format("External Ip: {0}", hash.ExternalIp));
                        Server.SendAnnounce(string.Format("Local Ip: {0}", hash.LocalIp));
                        Server.SendAnnounce(string.Format("JSON: {0}", hash.SupportJson));
                    }

                    break;
            }

            return true;
        }
    }
}

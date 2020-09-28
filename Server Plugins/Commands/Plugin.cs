using Commands.Resources;
using System;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Core;
using Zorbo.Core.Plugins.Server;
using Zorbo.Core.Server;

namespace Commands
{
    public class Commands : ServerPlugin
    {
        public override void OnPluginLoaded() {
            Processor.LoadHelp();
            Server.SendAnnounce("Zorbo Commands Plugin Loaded");
        }

        public override bool OnBeforePacket(IClient client, IPacket packet) {

            switch ((AresId)packet.Id) {

                case AresId.MSG_CHAT_CLIENT_PUBLIC:
                    ClientPublic pub = (ClientPublic)packet;

                    if (!String.IsNullOrEmpty(pub.Message)) {

                        if (pub.Message.StartsWith("#"))
                            if (!Processor.HandleCommand(Server, client, pub.Message.Substring(1)))
                                return false; //hide text
                    }
                    break;
                case AresId.MSG_CHAT_CLIENT_COMMAND:
                    Command command = (Command)packet;

                    if (!Processor.HandleCommand(Server, client, command.Message))
                        return false;

                    break;
            }
            return true;
        }

        public override void OnHelp(IClient client) {
            Processor.SendHelp(Server, client);
        }

        public override void OnVroomJoin(IClient client) {
            Server.SendAnnounce(client, string.Format(Strings.VroomNotice, client.Vroom));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class Dummy : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_DUMMY; }
        }
    }
}

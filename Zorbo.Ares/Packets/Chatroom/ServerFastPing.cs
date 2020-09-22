using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ServerFastPing : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_FASTPING; }
        }
    }
}

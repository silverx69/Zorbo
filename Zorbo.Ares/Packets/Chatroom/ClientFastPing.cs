using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public sealed class ClientFastPing : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_CLIENT_FASTPING; }
        }
    }
}

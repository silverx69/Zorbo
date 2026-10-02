using Zorbo.Ares.Packets;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class ClientEmoteSupport : AdvancedPacket
    {
        public override AdvancedId Id {
            get { return AdvancedId.MSG_CHAT_CLIENT_SUPPORTS_CUSTOM_EMOTES; }
        }
    }
}

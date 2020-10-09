namespace Zorbo.Ares.Packets.Chatroom
{
    public class FastPing : AresPacket
    {
        public override AresId Id {// client and server 14
            get { return AresId.MSG_CHAT_SERVER_FASTPING; }
        }
    }
}

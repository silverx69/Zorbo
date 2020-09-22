using Zorbo.Core.Interfaces;

namespace Zorbo.Core.Data.Packets
{
    public class Unknown : IPacket
    {
        public byte Id {
            get;
            set;
        }

        [PacketItem(0)]
        public byte[] Payload {
            get;
            set;
        }

        public Unknown() { }

        public Unknown(byte id, byte[] payload) {
            Id = id;
            Payload = payload;
        }
    }
}

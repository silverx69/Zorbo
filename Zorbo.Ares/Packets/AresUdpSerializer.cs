using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;

namespace Zorbo.Ares.Packets
{
    /// <summary>
    /// Used for serializing Udp packets to and from Ares Udp nodes. Serialize is overriden to not write the length of the packet.
    /// </summary>
    public sealed class AresUdpSerializer : PacketSerializer
    {
        public AresUdpSerializer() { }

        public AresUdpSerializer(IPacketFormatter formatter) :
            base(formatter) { }

        public override byte[] Serialize<T>(T obj) {

            using var writer = new PacketWriter();

            writer.Write(obj.Id);
            writer.WriteObject(obj);

            return writer.ToArray();
        }
    }
}

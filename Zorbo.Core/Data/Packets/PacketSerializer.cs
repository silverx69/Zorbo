using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Data.Packets
{
    public class PacketSerializer
    {
        public IPacketFormatter Formatter {
            get;
            set;
        }

        public PacketSerializer() { }

        public PacketSerializer(IPacketFormatter formatter)
        {
            Formatter = formatter;
        }

        public virtual byte[] Serialize<T>(T obj) where T : class, IPacket
        {
            using var writer = new PacketWriter();

            writer.Position = 2;

            writer.Write(obj.Id);
            if (obj is Unknown unk)
                writer.Write(unk.Payload);
            else
                writer.WriteObject(obj);

            writer.Position = 0;
            writer.Write((ushort)(writer.Length - 3));

            return writer.ToArray();
        }


        public virtual T Deserialize<T>(byte[] input) where T : class, IPacket
        {
            return Deserialize<T>(input, 0, input.Length);
        }

        public virtual T Deserialize<T>(byte[] input, int offset, int count) where T : class, IPacket
        {
            using var reader = new PacketReader(input, offset, count);
            return reader.ReadObject<T>(Formatter);//formatter provides the switch-case for id's
        }
    }
}
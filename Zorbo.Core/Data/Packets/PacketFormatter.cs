using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using Zorbo.Core.Interfaces;

namespace Zorbo.Core.Data.Packets
{
    public abstract class PacketFormatter<T> : IPacketFormatter where T : PacketSerializer, new()
    {
        protected T Serializer { get; set; }

        public PacketFormatter()
        {
            Serializer = new T();
            Serializer.Formatter = this;
        }

        protected PacketFormatter(T serializer)
        {
            Serializer = serializer;
        }

        public virtual byte[] Format(IPacket message)
        {
            return Serializer.Serialize(message);
        }

        public virtual byte[] FormatJson(IPacket message, bool ib0t = false)
        {
            return Encoding.UTF8.GetBytes(Json.Serialize(message));
        }

        public abstract IPacket Unformat(byte id, string data, bool ib0t = false);


        public virtual IPacket Unformat(byte id, byte[] data)
        {
            return Unformat(id, data, 0, data.Length);
        }

        public abstract IPacket Unformat(byte id, byte[] data, int index, int count);


        protected string[] Parseib0tMessage(string content)
        {
            try {
                string[] parts = content.Split(new[] { ':' }, 2);

                if (parts.Length >= 2) {
                    var ret = new List<string>();

                    string[] head = parts[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string payload = parts[1];

                    foreach (string len in head) {
                        if (int.TryParse(len, out int length)) {
                            ret.Add(payload.Substring(0, length));
                            payload = payload.Substring(length);
                        }
                        else return null;
                    }

                    return ret.ToArray();
                }
            }
            catch (Exception ex) {
                Logging.Error("PacketFormatter", ex);
            }
            return null;
        }
    }
}

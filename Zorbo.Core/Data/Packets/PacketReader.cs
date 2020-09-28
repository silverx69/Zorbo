using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Zorbo.Core.Data.Packets
{
    public class PacketReader : BinaryReader
    {
        readonly Encoding Encoding = Encoding.UTF8;

        public long Length {
            get { return BaseStream.Length; }
        }

        public long Position {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public long Remaining {
            get { return Length - Position; }
        }

        public PacketReader(byte[] input)
            : this(new MemoryStream(input)) { }

        public PacketReader(byte[] input, int offset, int count)
            : this(new MemoryStream(input, offset, count)) { }

        public PacketReader(Stream input)
            : base(input) { }


        public virtual Guid ReadGuid() {

            byte[] tmp = new byte[16];
            BaseStream.Read(tmp, 0, 16);

            return new Guid(tmp);
        }

        public virtual IPAddress ReadIPAddress() {

            byte[] tmp = new byte[4];
            BaseStream.Read(tmp, 0, 4);

            return new IPAddress(tmp);
        }

        public override string ReadString() {
            List<byte> bytes = new List<byte>();

            while (Remaining > 0) {
                int b = BaseStream.ReadByte();
                if (b == 0) break;

                bytes.Add((byte)b);
            }

            return Encoding.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        public virtual string ReadString(int count) {
            count = (int)Math.Min(count, Remaining);

            byte[] tmp = new byte[count];
            BaseStream.Read(tmp, 0, count);

            return Encoding.GetString(tmp);
        }


        public virtual T ReadObject<T>(IPacketFormatter formatter) {

            object ret = ReadObject(typeof(T), formatter);
            return (T)(ret ?? default);
        }

        private object ReadObject(Type type, IPacketFormatter formatter) {

            var members = TypeDictionary.GetRecord(type);

            object obj = Activator.CreateInstance(type, true);
            members.ForEach(prop => prop.Property.SetValue(obj, ReadProperty(prop, formatter), null));

            return obj;
        }

        private IPacket ReadPacket(IPacketFormatter formatter) {
            try {
                ushort len = ReadUInt16();

                if (Remaining < len + 1)
                    throw new EndOfStreamException();

                byte id = ReadByte();
                byte[] payload = ReadBytes(len);

                return formatter?.Unformat(id, payload);
            }
            catch { return null; }
        }

        private object ReadProperty(PropertyData prop, IPacketFormatter formatter) {

            switch (Type.GetTypeCode(prop.Property.PropertyType)) {
                case TypeCode.Boolean:
                    if (Remaining < 1 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (bool)prop.Attribute.OptionalValue;
                        else
                            return false;
                    }

                    return ReadBoolean();
                case TypeCode.Byte:
                    if (Remaining < 1 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (byte)prop.Attribute.OptionalValue;
                        else
                            return (byte)0;
                    }

                    return ReadByte();
                case TypeCode.SByte:
                    if (Remaining < 1 && prop.Attribute.Optional)
                        return (sbyte)0;

                    return ReadSByte();
                case TypeCode.Int16:
                    if (Remaining < 2 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (short)prop.Attribute.OptionalValue;
                        else
                            return (short)0;
                    }

                    return ReadInt16();
                case TypeCode.UInt16:
                    if (Remaining < 2 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (ushort)prop.Attribute.OptionalValue;
                        else
                            return (ushort)0;
                    }

                    return ReadUInt16();
                case TypeCode.Int32:
                    if (Remaining < 4 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (int)prop.Attribute.OptionalValue;
                        else
                            return (int)0;
                    }

                    return ReadInt32();
                case TypeCode.UInt32:
                    if (Remaining < 4 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (uint)prop.Attribute.OptionalValue;
                        else
                            return (uint)0;
                    }

                    return ReadUInt32();
                case TypeCode.Single:
                    if (Remaining < 4 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (float)prop.Attribute.OptionalValue;
                        else
                            return (float)0;
                    }

                    return ReadSingle();
                case TypeCode.Int64:
                    if (Remaining < 8 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (long)prop.Attribute.OptionalValue;
                        else
                            return (long)0;
                    }

                    return ReadInt64();
                case TypeCode.UInt64:
                    if (Remaining < 8 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (ulong)prop.Attribute.OptionalValue;
                        else
                            return (ulong)0;
                    }

                    return ReadUInt64();
                case TypeCode.Double:
                    if (Remaining < 8 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (double)prop.Attribute.OptionalValue;
                        else
                            return (double)0;
                    }

                    return ReadDouble();
                case TypeCode.Decimal:
                    if (Remaining < 16 && prop.Attribute.Optional) {
                        if (prop.Attribute.OptionalValue != null)
                            return (decimal)prop.Attribute.OptionalValue;
                        else
                            return (decimal)0;
                    }

                    return ReadDecimal();
                case TypeCode.String:
                    string ret;
                    if (prop.Attribute.Length > 0)
                        ret = ReadString(prop.Attribute.Length);
                    else if (prop.Attribute.LengthPrefix)
                        ret = ReadString(ReadUInt16());
                    else
                        ret = ReadString();

                    if (prop.Attribute.MaxLength > 0) {

                        byte[] b = Encoding.GetBytes(ret);
                        ret = Encoding.GetString(b, 0, Math.Min(b.Length, prop.Attribute.MaxLength));
                    }

                    return ret;
                case TypeCode.Object:
                    Type ptype = prop.Property.PropertyType;

                    if (ptype == typeof(byte[]))
                        return ReadBytes((int)Remaining);

                    else if (ptype == typeof(Guid)) {
                        if (Remaining < 16 && prop.Attribute.Optional) {
                            if (prop.Attribute.OptionalValue != null)
                                return (Guid)prop.Attribute.OptionalValue;
                            else
                                return Guid.Empty;
                        }

                        return ReadGuid();
                    }
                    else if (ptype == typeof(IPAddress)) {
                        if (Remaining < 4 && prop.Attribute.Optional) {
                            if (prop.Attribute.OptionalValue != null)
                                return (IPAddress)prop.Attribute.OptionalValue;
                            else
                                return IPAddress.Any;
                        }

                        return ReadIPAddress();
                    }
                    else if (typeof(IPacket).IsAssignableFrom(ptype))
                        if (Remaining >= 3) return ReadPacket(formatter);

                    throw new NotSupportedException("This data type is not supported by the serializer");
            }

            return null;
        }
    }
}

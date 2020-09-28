using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Data.Packets
{
    public class PacketWriter : BinaryWriter
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

        public PacketWriter()
            : this(new MemoryStream()) { }

        public PacketWriter(Stream input)
            : base(input, Encoding.UTF8) { }


        public byte[] ToArray() {
            long i = Position;
            byte[] b = new byte[Length];

            Position = 0;
            BaseStream.Read(b, 0, (int)Length);
            Position = i;

            return b;
        }


        public virtual void Write(Guid guid) {
            Write(guid.ToByteArray());
        }

        public virtual void Write(IPAddress ip) {
            Write((ip == null) ? new byte[4] : ip.GetAddressBytes());
        }

        public override void Write(byte[] buffer) {
            base.Write(buffer);
        }

        public override void Write(string value) {
            Write(value, true);
        }

        public virtual  void Write(string value, bool nullTerminate) {
            Write(value, nullTerminate, false);
        }

        public virtual void Write(string value, bool nullTerminate, bool prefix) {
            if (prefix)
                Write((value == null) ? 0 : Encoding.UTF8.GetByteCount(value));

            if (value != null)
                Write(Encoding.GetBytes(value));

            if (nullTerminate) Write((byte)0);
        }

        public void WriteObject(object value) {
            WriteObject(value, value?.GetType());
        }

        private void WriteObject(object value, Type valType) {
            if (valType == null)
                throw new ArgumentNullException("Type information cannot be null", "valType");

            var members = TypeDictionary.GetRecord(valType);
            members.ForEach(prop => WriteProperty(value, prop));
        }

        private void WritePacket(IPacket packet) {
            object obj = (object)packet;

            long index = Position;

            Position += 2;

            Write(packet.Id);
            WriteObject(obj);

            long endIndex = Position;

            Position = index;
            Write((ushort)(endIndex - index - 3));

            Position = endIndex;
        }

        private void WriteProperty(object value, PropertyData prop) {

            object propvalue = prop.Property.GetValue(value, null);

            if (prop.Attribute.Optional) {

                Type t = prop.Property.PropertyType;
                object opt = prop.Attribute.OptionalValue;

                if (t.IsValueType && opt != null && propvalue.Equals(opt))
                    return;

                else if (!t.IsValueType && opt == null)
                    return;
            }

            switch (Type.GetTypeCode(prop.Property.PropertyType)) {
                case TypeCode.Boolean:
                    Write((bool)propvalue);
                    break;
                case TypeCode.Byte:
                    Write((byte)propvalue);
                    break;
                case TypeCode.SByte:
                    Write((sbyte)propvalue);
                    break;
                case TypeCode.Int16:
                    Write((short)propvalue);
                    break;
                case TypeCode.UInt16:
                    Write((ushort)propvalue);
                    break;
                case TypeCode.Int32:
                    Write((int)propvalue);
                    break;
                case TypeCode.UInt32:
                    Write((uint)propvalue);
                    break;
                case TypeCode.Single:
                    Write((float)propvalue);
                    break;
                case TypeCode.Int64:
                    Write((long)propvalue);
                    break;
                case TypeCode.UInt64:
                    Write((ulong)propvalue);
                    break;
                case TypeCode.Double:
                    Write((double)propvalue);
                    break;
                case TypeCode.Decimal:
                    Write((decimal)propvalue);
                    break;
                case TypeCode.String:
                    string tmp = (string)propvalue;

                    byte[] b;
                    if (!string.IsNullOrEmpty(tmp))
                        b = Encoding.GetBytes(tmp);
                    else
                        b = new byte[0];

                    if (prop.Attribute.MaxLength > 0) 
                        Array.Resize(ref b, Math.Min(b.Length, prop.Attribute.MaxLength));

                    if (prop.Attribute.LengthPrefix) {
                        Write((ushort)b.Length);
                        Write(b);
                    }
                    else {
                        tmp = Encoding.GetString(b);
                        Write(tmp, prop.Attribute.NullTerminated);
                    }
                    break;
                case TypeCode.Object:
                    Type ptype = prop.Property.PropertyType;

                    if (ptype == typeof(byte[]))
                        Write((byte[])propvalue);

                    else if (ptype == typeof(Guid))
                        Write((Guid)propvalue);

                    else if (ptype == typeof(IPAddress))
                        Write((IPAddress)propvalue);

                    else if (typeof(IPacket).IsAssignableFrom(ptype))
                        WritePacket((IPacket)propvalue);

                    else throw new NotSupportedException("This data type is not supported by the serializer");

                    break;
            }
        }
    }
}

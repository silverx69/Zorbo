using Jurassic;
using Jurassic.Library;
using System;
using System.Net;
using System.Reflection;
using Zorbo.Core.Data.Packets;
using Zorbo.Core;

namespace Javascript.Objects
{
    public class Packet : ScriptObject
    {
        public class GetProperty : FunctionInstance
        {
            readonly IPacket packet = null;
            readonly PropertyInfo property = null;

            public GetProperty(Script script, IPacket packet, PropertyInfo prop)
                : base(script.Engine) {

                this.packet = packet;
                this.property = prop;
            }

            public override object CallLateBound(object thisObject, params object[] argumentValues) {
                if (!this.property.CanRead)
                    return Undefined.Value;

                object ret = this.property.GetValue(packet, null);
                //packets generally use unsigned shorts or ints

                if (ret is byte || ret is ushort || ret is uint)
                    ret = Convert.ToInt32(ret);

                else if (ret is byte[] v)
                    ret = v.ToJSArray(Engine);

                else if (ret is IPAddress address)
                    ret = address.ToString();

                return ret;
            }
        }

        public class SetProperty : FunctionInstance
        {
            readonly IPacket packet = null;
            readonly PropertyInfo property = null;

            public SetProperty(Script script, IPacket packet, PropertyInfo prop)
                : base(script.Engine) {

                this.packet = packet;
                this.property = prop;
            }

            public override object CallLateBound(object thisObject, params object[] argumentValues) {
                if (this.property.CanWrite) {
                    Object ret = argumentValues[0];
                    Type strtype = typeof(String);

                    //avatar, custom, unknown packets all use byte array members
                    if (property.PropertyType == typeof(byte[])) {
                        if (!(ret is ArrayInstance))
                            return Undefined.Value;

                        ret = ((ArrayInstance)ret).ToArray<byte>();
                    }
                    else if (property.PropertyType == typeof(IPAddress)) {
                        if (ret is ArrayInstance array) {
                            if (array.Length < 4) 
                                return Undefined.Value;

                            ret = new IPAddress(array.ToArray<byte>(4, true));
                        }
                        else if (ret is String || ret is ConcatenatedString) {
                            if (IPAddress.TryParse(ret.ToString(), out IPAddress ip))
                                ret = ip;

                            else return Undefined.Value;
                        }
                    }
                    else if (property.PropertyType == strtype)
                        if (ret.GetType() != strtype)
                            ret = ret.ToString();

                    this.property.SetValue(packet, ret, null);
                }
                return null;
            }
        }


        public Packet(Script script, IPacket packet)
            : base(script.Engine) {

            var props = TypeDictionary.GetRecord(packet.GetType());
            DefineProperty("id", new PropertyDescriptor(
                (int)packet.Id, 
                Jurassic.Library.PropertyAttributes.Enumerable | Jurassic.Library.PropertyAttributes.Sealed), 
                true
            );

            foreach (var prop in props) {
                var info = prop.Property;

                string name = info.Name;
                name = name[0].ToString().ToLower() + name.Substring(1);

                this.DefineProperty(
                    name,
                    new PropertyDescriptor(
                        new GetProperty(script, packet, info),
                        new SetProperty(script, packet, info),
                        Jurassic.Library.PropertyAttributes.Enumerable | Jurassic.Library.PropertyAttributes.Sealed),
                    true);
            }
        }
    }
}

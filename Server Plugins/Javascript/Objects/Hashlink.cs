using Jurassic;
using Jurassic.Library;
using System;
using System.Net;
using Zorbo.Ares;
using Zorbo.Core;

namespace Javascript.Objects
{
    public class Hashlink : ScriptObject, IHashlink
    {
        readonly AresChannel channel = null;

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly Script script = null;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, 
                      "Hashlink", 
                      new Hashlink(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public Hashlink Call(object a, object b, object c, object d)
            {
                return Construct(a, b, c, d);
            }

            [JSConstructorFunction]
            public Hashlink Construct(object a, object b, object c, object d)
            {
                return new Hashlink(script) {
                    Name = (!(a is Undefined)) ? TypeConverter.ConvertTo<String>(Engine, a) : "",
                    Port = (!(b is Undefined)) ? TypeConverter.ConvertTo<Int32>(Engine, b) : 0,
                    ExternalIp = (!(c is Undefined)) ? TypeConverter.ConvertTo<String>(Engine, c) : "",
                    InternalIp = (!(d is Undefined)) ? TypeConverter.ConvertTo<String>(Engine, d) : ""
                };
            }
        }

        #endregion

        [JSProperty(Name = "name", IsEnumerable = true)]
        public string Name {
            get { return channel.Name; }
            set { channel.Name = value; }
        }

        [JSProperty(Name = "port", IsEnumerable = true)]
        public int Port {
            get { return (int)channel.Port; }
            set { channel.Port = (ushort)value; }
        }

        [JSProperty(Name = "internalIp", IsEnumerable = true)]
        public String InternalIp {
            get { return channel.InternalIp.ToString(); }
            set {
                if (IPAddress.TryParse(value, out IPAddress ip))
                    channel.InternalIp = ip;
            }
        }

        [JSProperty(Name = "externalIp", IsEnumerable = true)]
        public String ExternalIp {
            get { return channel.ExternalIp.ToString(); }
            set {
                if (IPAddress.TryParse(value, out IPAddress ip))
                    channel.ExternalIp = ip;
            }
        }

        private Hashlink(Script script)
            : base(script.Engine) {

            this.PopulateFunctions();
        }

        protected Hashlink(Script script, ObjectInstance proto)
            : base(script.Engine, proto) {

            this.PopulateFunctions();
        }

        [JSFunction(Name = "encode", IsEnumerable = true, IsWritable = false)]
        public virtual string Encode()
        {
            return Zorbo.Core.Hashlinks.ToHashlinkString(this);
        }

        [JSFunction(Name = "decode", IsEnumerable = true, IsWritable = false)]
        public virtual object Decode(object a)
        {
            if (a is String || a is ConcatenatedString) {
                var str = a.ToString();
                return Zorbo.Core.Hashlinks.FromHashlinkString(str, this);
            }
            else if (a is ArrayInstance array) {
                byte[] tmp = array.ToArray<byte>();
                return Zorbo.Core.Hashlinks.FromHashlinkArray(tmp, this);
            }

            return null;
        }

        public virtual byte[] ToByteArray()
        {
            return channel.ToByteArray();
        }

        public virtual void FromByteArray(byte[] value)
        {
            channel.FromByteArray(value);
        }

        public virtual string ToPlainText() {
            return channel.ToPlainText();
        }

        public virtual void FromPlainText(string text) {
            channel.FromPlainText(text);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class JSHashlinks : ScriptObject
    {
        public JSHashlinks(Script script)
            : base(script.Engine) {

            this.PopulateFunctions();
        }

        [JSFunction(Name = "encode", IsEnumerable = true, IsWritable = false)]
        public string Encode(ObjectInstance a) {
            if (a is Hashlink hashlink)
                return hashlink.Encode();

            else if (a.Prototype is Hashlink)
                return TypeConverter.ConvertTo<string>(Engine, a.CallMemberFunction("encode", a));

            return string.Empty;
        }

        [JSFunction(Name = "decode", IsEnumerable = true, IsWritable = false)]
        public object Decode(object a, ObjectInstance b) {
            if (b is Hashlink hashlink)
                return hashlink.Decode(a);

            else if (b.Prototype is Hashlink)
                return b.CallMemberFunction("decode", a);

            return null;
        }

        [JSFunction(Name = "e67", IsEnumerable = true, IsWritable = false)]
        public ArrayInstance E67(object data, int b) {

            if (data is String || data is ConcatenatedString)
                return Zorbo.Core.Hashlinks.E67(System.Text.Encoding.UTF8.GetBytes(data.ToString()), b).ToJSArray(Engine);

            else if (data is ArrayInstance instance)
                return Zorbo.Core.Hashlinks.E67(instance.ToArray<byte>(), b).ToJSArray(Engine);

            return null;
        }

        [JSFunction(Name = "d67", IsEnumerable = true, IsWritable = false)]
        public ArrayInstance D67(object data, int b) {

            if (data is String || data is ConcatenatedString)
                return Zorbo.Core.Hashlinks.D67(System.Text.Encoding.UTF8.GetBytes(data.ToString()), b).ToJSArray(Engine);

            else if (data is ArrayInstance instance)
                return Zorbo.Core.Hashlinks.D67(instance.ToArray<byte>(), b).ToJSArray(Engine);

            return null;
        }
    }
}

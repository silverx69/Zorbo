using Jurassic;
using Jurassic.Library;
using System;
using System.Linq;

namespace Javascript.Objects
{
    public class Base64 : ScriptObject
    {
        public Base64(Javascript.Script script)
            : base(script.Engine) {

            this.PopulateFunctions();
        }

        [JSFunction(Name = "encode", IsEnumerable = true, IsWritable = false)]
        public string Encode(object a) {

            if (a is string || a is ConcatenatedString) {
                string str = a.ToString();

                byte[] tmp = System.Text.Encoding.ASCII.GetBytes(str);
                return Convert.ToBase64String(tmp);
            }
            else if (a is ArrayInstance array) {

                byte[] tmp = array.ToArray<byte>();
                return Convert.ToBase64String(tmp);
            }
            else return null;
        }

        [JSFunction(Name = "decode", IsEnumerable = true, IsWritable = false)]
        public ArrayInstance Decode(object a) {

            if (a is string || a is ConcatenatedString) {
                string str = a.ToString();
                byte[] tmp = Convert.FromBase64String(str);

                return Engine.Array.New(tmp.Select((s) => (object)(int)s).ToArray());
            }
            else if (a is ArrayInstance array) {

                byte[] tmp = array.ToArray<byte>();
                string str = System.Text.Encoding.ASCII.GetString(tmp);

                tmp = Convert.FromBase64String(str);

                return Engine.Array.New(tmp.Select((s) => (object)(int)s).ToArray());
            }
            else return null;
        }
    }
}

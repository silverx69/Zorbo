using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo;

using Jurassic;
using Jurassic.Library;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class EncodingInstance : ScriptObject
    {
        readonly JScript script;
        protected System.Text.Encoding encoding;

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly JScript script;

            public Constructor(JScript script) :
                base(script.Engine.Function.InstancePrototype, "EncodingInstance", new EncodingInstance(script)) {

                this.script = script;
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public EncodingInstance Call(object a) {

                return Construct(a);
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public EncodingInstance Construct(object a) {

                if (a is Undefined || a is Null)
                    return null;

                if (a is String || a is ConcatenatedString) {

                    switch (a.ToString().ToLower()) {
                        case "ascii":
                            return new ASCII(script);
                        case "utf7":
                            return new UTF7(script);
                        case "utf8":
                            return new UTF8(script);
                        case "utf16":
                            return new UTF16(script);
                        case "utf32":
                            return new UTF32(script);
                    }
                }

                return null;
            }
        }

        #endregion

        public EncodingInstance(JScript script)
            : base(script.Engine) {

            this.script = script;
            this.encoding = System.Text.Encoding.Default;

            this.PopulateFunctions();
        }

        public EncodingInstance(JScript script, System.Text.Encoding encoding)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["EncodingInstance"]).InstancePrototype) {

            this.script = script;
            this.encoding = encoding;

            this.PopulateFunctions();
        }

        [JSFunction(Name = "getBytes", IsWritable = true, IsEnumerable = true, Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
        public ArrayInstance GetBytes(object a) {

            if (a is String || a is ConcatenatedString)
                return encoding.GetBytes(a.ToString()).ToJSArray(script.Engine);

            else if (a is ArrayInstance instance)
                return encoding.GetBytes(instance.ToArray<char>(encoding)).ToJSArray(script.Engine);

            return null; 
        }

        [JSFunction(Name = "getChars", IsWritable = true, IsEnumerable = true, Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
        public ArrayInstance GetChars(object a) {

            if (a is String || a is ConcatenatedString)
                return encoding.GetChars(encoding.GetBytes(a.ToString())).ToJSArray(Engine);

            else if (a is ArrayInstance instance)
                return encoding.GetChars(instance.ToArray<byte>(encoding)).ToJSArray(Engine);

            return null;
        }

        [JSFunction(Name = "getString", IsWritable = true, IsEnumerable = true, Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
        public string GetString(ArrayInstance array, object index, object count) {

            if (array != null && (index is Undefined || count is Undefined))
                return encoding.GetString(array.ToArray<byte>(encoding));

            else if (array != null && index is int i && count is int c)
                return encoding.GetString(array.ToArray<byte>(encoding), i, c);

            return string.Empty;
        }
    }
}

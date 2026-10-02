using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class JSEncoding : ScriptObject
    {
        readonly Script script;

        public JSEncoding(Script script, bool init)
            : base(script.Engine) {

            this.script = script;
            this.PopulateFunctions();

            if (init) this.Init();
        }

        private void Init() {
            this.script.Engine.Global.DefineProperty("ASCII",
                new PropertyDescriptor(new JSASCII.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF7",
                new PropertyDescriptor(new JSUTF7.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF8",
                new PropertyDescriptor(new JSUTF8.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF16",
                new PropertyDescriptor(new JSUTF16.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF32",
                new PropertyDescriptor(new JSUTF32.Constructor(script), PropertyAttributes.Sealed), true);
        }

        [JSProperty(Name = "ASCII", IsEnumerable = true)]
        public JSASCII ASCII {
            get { return new JSASCII(script); }
        }

        [JSProperty(Name = "UTF7", IsEnumerable = true)]
        public JSUTF7 UTF7 {
            get { return new JSUTF7(script); }
        }

        [JSProperty(Name = "UTF8", IsEnumerable = true)]
        public JSUTF8 UTF8 {
            get { return new JSUTF8(script); }
        }

        [JSProperty(Name = "UTF16", IsEnumerable = true)]
        public JSUTF16 UTF16 {
            get { return new JSUTF16(script); }
        }

        [JSProperty(Name = "UTF32", IsEnumerable = true)]
        public JSUTF32 UTF32 {
            get { return new JSUTF32(script); }
        }
    }
}

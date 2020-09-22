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
    public class Encoding : ScriptObject
    {
        readonly JScript script;

        public Encoding(JScript script, bool init)
            : base(script.Engine) {

            this.script = script;
            this.PopulateFunctions();

            if (init) this.Init();
        }

        private void Init() {
            this.script.Engine.Global.DefineProperty("ASCII",
                new PropertyDescriptor(new ASCII.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF7",
                new PropertyDescriptor(new UTF7.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF8",
                new PropertyDescriptor(new UTF8.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF16",
                new PropertyDescriptor(new UTF16.Constructor(script), PropertyAttributes.Sealed), true);

            this.script.Engine.Global.DefineProperty("UTF32",
                new PropertyDescriptor(new UTF32.Constructor(script), PropertyAttributes.Sealed), true);
        }

        [JSProperty(Name = "ASCII", IsEnumerable = true)]
        public ASCII ASCII {
            get { return new ASCII(script); }
        }

        [JSProperty(Name = "UTF7", IsEnumerable = true)]
        public UTF7 UTF7 {
            get { return new UTF7(script); }
        }

        [JSProperty(Name = "UTF8", IsEnumerable = true)]
        public UTF8 UTF8 {
            get { return new UTF8(script); }
        }

        [JSProperty(Name = "UTF16", IsEnumerable = true)]
        public UTF16 UTF16 {
            get { return new UTF16(script); }
        }

        [JSProperty(Name = "UTF32", IsEnumerable = true)]
        public UTF32 UTF32 {
            get { return new UTF32(script); }
        }
    }
}

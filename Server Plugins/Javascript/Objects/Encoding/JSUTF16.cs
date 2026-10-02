using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class JSUTF16 : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly Script script;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, "UTF16", new JSUTF16(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public JSUTF16 Call() { return new JSUTF16(script); }

            [JSConstructorFunction]
            public JSUTF16 Construct() { return new JSUTF16(script); }
        }

        #endregion

        public JSUTF16(Script script)
            : base(script, System.Text.Encoding.Unicode) {
            this.PopulateFunctions();
        }
    }
}

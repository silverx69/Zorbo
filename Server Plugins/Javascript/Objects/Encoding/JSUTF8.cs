using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class JSUTF8 : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly Script script;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, "UTF8", new JSUTF8(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public JSUTF8 Call() { return new JSUTF8(script); }

            [JSConstructorFunction]
            public JSUTF8 Construct() { return new JSUTF8(script); }
        }

        #endregion

        public JSUTF8(Script script)
            : base(script, System.Text.Encoding.UTF8) {
            this.PopulateFunctions();
        }
    }
}

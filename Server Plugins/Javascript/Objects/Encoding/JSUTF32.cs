using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class JSUTF32 : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly Script script;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, "UTF32", new JSUTF32(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public JSUTF32 Call() { return new JSUTF32(script); }

            [JSConstructorFunction]
            public JSUTF32 Construct() { return new JSUTF32(script); }
        }

        #endregion

        public JSUTF32(Script script)
            : base(script, System.Text.Encoding.UTF32) {
            this.PopulateFunctions();
        }
    }
}

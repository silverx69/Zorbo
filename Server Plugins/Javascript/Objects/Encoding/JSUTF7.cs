using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class JSUTF7 : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly Script script;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, "UTF7", new JSUTF7(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public JSUTF7 Call() { return new JSUTF7(script); }

            [JSConstructorFunction]
            public JSUTF7 Construct() { return new JSUTF7(script); }
        }

        #endregion

        public JSUTF7(Script script)
            : base(script, System.Text.Encoding.UTF7) {
            this.PopulateFunctions();
        }
    }
}
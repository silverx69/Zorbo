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
    public class ASCII : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly JScript script;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "ASCII", new ASCII(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public ASCII Call() {
                return new ASCII(script);
            }

            [JSConstructorFunction]
            public ASCII Construct() {
                return new ASCII(script); 
            }
        }

        #endregion

        public ASCII(JScript script)
            : base(script, System.Text.Encoding.ASCII) {
            this.PopulateFunctions();
        }
    }
}

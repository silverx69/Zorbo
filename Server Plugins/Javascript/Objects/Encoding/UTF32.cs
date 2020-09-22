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
    public class UTF32 : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly JScript script;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "UTF32", new UTF32(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public UTF32 Call() { return new UTF32(script); }

            [JSConstructorFunction]
            public UTF32 Construct() { return new UTF32(script); }
        }

        #endregion

        public UTF32(JScript script)
            : base(script, System.Text.Encoding.UTF32) {
            this.PopulateFunctions();
        }
    }
}

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
    public class UTF16 : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly JScript script;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "UTF16", new UTF16(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public UTF16 Call() { return new UTF16(script); }

            [JSConstructorFunction]
            public UTF16 Construct() { return new UTF16(script); }
        }

        #endregion

        public UTF16(JScript script)
            : base(script, System.Text.Encoding.Unicode) {
            this.PopulateFunctions();
        }
    }
}

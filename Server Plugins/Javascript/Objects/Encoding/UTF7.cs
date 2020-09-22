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
    public class UTF7 : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly JScript script;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "UTF7", new UTF7(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public UTF7 Call() { return new UTF7(script); }

            [JSConstructorFunction]
            public UTF7 Construct() { return new UTF7(script); }
        }

        #endregion

        public UTF7(JScript script)
            : base(script, System.Text.Encoding.UTF7) {
            this.PopulateFunctions();
        }
    }
}
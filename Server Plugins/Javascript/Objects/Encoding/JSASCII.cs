using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class JSASCII : EncodingInstance
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly Script script;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, "ASCII", new JSASCII(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public JSASCII Call() {
                return new JSASCII(script);
            }

            [JSConstructorFunction]
            public JSASCII Construct() {
                return new JSASCII(script); 
            }
        }

        #endregion

        public JSASCII(Script script)
            : base(script, System.Text.Encoding.ASCII) {
            this.PopulateFunctions();
        }
    }
}

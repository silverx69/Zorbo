using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace Javascript.Objects
{
    public abstract class ScriptObject : ObjectInstance
    {
        public ScriptObject(ScriptEngine engine)
            : base(engine) { }

        public ScriptObject(ScriptEngine engine, ObjectInstance proto)
            : base(engine, proto) { }

        protected virtual string InternalClassName() {
            return this.GetType().Name;
        }

        public override string ToString()
        {
            return string.Format("[object {0}]", InternalClassName());
        }
    }
}

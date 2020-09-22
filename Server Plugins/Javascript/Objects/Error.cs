
using Jurassic;
using Jurassic.Library;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Error : ScriptObject
    {
        readonly JScript script = null;
        readonly JavaScriptException error = null;

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "Error", new Error(script)) {
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public User Call() {
                return null;
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public User Construct() {
                return null;
            }
        }

        #endregion

        public Error(JScript script)
            : base(script.Engine) {

            this.script = script;

            this.PopulateFields();
            this.PopulateFunctions();
        }

        public Error(JScript script, JavaScriptException ex)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["Error"]).InstancePrototype) {

            this.error = ex;
            this.script = script;

            this.PopulateFields();
            this.PopulateFunctions();
        }

        [JSProperty(Name = "line", IsEnumerable = true)]
        public int Line {
            get { return error != null ? error.LineNumber : 0; }
        }

        [JSProperty(Name = "script", IsEnumerable = true)]
        public string Script {
            get { return script.Name; }
        }

        [JSProperty(Name = "source", IsEnumerable = true)]
        public string Source {
            get { return error != null ? error.Source : string.Empty; }
        }

        [JSProperty(Name = "trace", IsEnumerable = true)]
        public string StackTrace {
            get { return error != null ? error.StackTrace : string.Empty; }
        }

        [JSProperty(Name = "message", IsEnumerable = true)]
        public string Message {
            get { return error != null ? error.Message : string.Empty; }
        }
    }
}

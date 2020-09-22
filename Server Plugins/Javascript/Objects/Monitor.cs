using Jurassic.Library;
using Zorbo.Core.Interfaces;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Monitor : ScriptObject
    {
        readonly IMonitor monitor = null;

        [JSProperty(Name = "speedIn", IsEnumerable = true)]
        public virtual double SpeedIn {
            get { return monitor != null ? monitor.SpeedIn : 0; }
        }

        [JSProperty(Name = "speedOut", IsEnumerable = true)]
        public virtual double SpeedOut {
            get { return monitor != null ? monitor.SpeedOut : 0; }
        }

        [JSProperty(Name = "totalBytesIn", IsEnumerable = true)]
        public virtual double TotalBytesIn {
            get { return monitor != null ? monitor.TotalBytesIn : 0; }
        }

        [JSProperty(Name = "totalBytesOut", IsEnumerable = true)]
        public virtual double TotalBytesOut {
            get { return monitor != null ? monitor.TotalBytesOut : 0; }
        }

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "Monitor", new Monitor(script)) {
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

        private Monitor(JScript script)
            : base(script.Engine) {
            this.PopulateFunctions();
        }

        protected Monitor(JScript script, ObjectInstance proto)
            : base(script.Engine, proto) {
            this.PopulateFunctions();
        }

        public Monitor(JScript script, IMonitor monitor)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["Monitor"]).InstancePrototype) {
            this.monitor = monitor;
            this.PopulateFunctions();
        }
    }
}

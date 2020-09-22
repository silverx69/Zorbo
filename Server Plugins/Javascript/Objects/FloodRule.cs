using Jurassic;
using Jurassic.Library;
using System;
using System.ComponentModel;
using Zorbo.Ares.Packets;
using Zorbo.Core;
using Zorbo.Core.Interfaces;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class FloodRule : ScriptObject, IFloodRule
    {
        readonly IFloodRule rule;

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly JScript script = null;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "FloodRule", new FloodRule(script)) {

                this.script = script;
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public FloodRule Call(object name, object id, object count, object timeout) {
                return Construct(name, id, count, timeout);
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public FloodRule Construct(object name, object id, object count, object timeout) {

                if (!(name is string || name is ConcatenatedString))
                    return null;

                if (!(id is int || id is double) || !(count is int || id is double) || !(timeout is int || id is double))
                    return null;

                return new FloodRule(script, InstancePrototype, name.ToString(),
                    Convert.ToInt32(id),
                    Convert.ToDouble(count),
                    Convert.ToDouble(timeout));
            }
        }

        #endregion

        [JSProperty(Name = "id", IsEnumerable = true)]
        public int Id {
            get { return rule != null ? rule.Id : 0; }
        }

        [JSProperty(Name = "name", IsEnumerable = true)]
        public string Name {
            get { return rule != null ? rule.Name : string.Empty; }
        }
        
        byte IFloodRule.Id {
            get { return (byte)(rule != null ? rule.Id : 0); }
        }

        [JSProperty(Name = "count", IsEnumerable = true)]
        public double Count {
            get { return rule != null ? rule.Count : 0; }
            set { if (rule != null) { rule.Count = value; } }
        }

        [JSProperty(Name = "timeout", IsEnumerable = true)]
        public double Timeout {
            get { return rule != null ? rule.Timeout : 0; }
            set { if (rule != null) rule.Timeout = value; }
        }

        private FloodRule(JScript script)
            : base(script.Engine) {

            this.PopulateFunctions();
        }

        public FloodRule(JScript script, IFloodRule rule)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["FloodRule"]).InstancePrototype) {

            this.rule = rule;
            this.PopulateFunctions();
        }

        internal FloodRule(JScript script, string name, int id, double count, double timeout)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["FloodRule"]).InstancePrototype) {


            this.rule = new Zorbo.Ares.FloodRule(name, (AresId)id, count, timeout);
            this.PopulateFunctions();
        }

        internal FloodRule(JScript script, ObjectInstance proto, string name, int id, double count, double timeout)
            : base(script.Engine, proto) {

            this.rule = new Zorbo.Ares.FloodRule(name, (AresId)id, count, timeout);
            this.PopulateFunctions();
        }

        public bool Equals(IFloodRule other) {
            if (other == null)
                return false;

            return this.Name == other.Name && 
                   this.Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            if (rule != null)
                return rule.Equals(obj);

            return false;
        }

        public override int GetHashCode() {
            if (rule != null)
                return rule.GetHashCode();

            return base.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged {
            add { if (rule != null) rule.PropertyChanged += value; }
            remove { if (rule != null) rule.PropertyChanged -= value; }
        }
    }
}

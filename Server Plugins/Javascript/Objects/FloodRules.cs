using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using Zorbo.Core;
using Zorbo.Core.Interfaces;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class FloodRules : Collection 
    {
        readonly IList<IFloodRule> rules;

        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return rules.Count; }
        }

        public FloodRules(JScript script, IList<IFloodRule> rules)
            : base(script, ((ClrFunction)script.Engine.Global["Collection"]).InstancePrototype) {

            this.rules = rules;
            this.PopulateFunctions();
        }

        [JSFunction(Name = "add", IsEnumerable = true, IsWritable = false)]
        public bool Add(object name, object id, object count, object timeout) {

            if (name is FloodRule rule) {
                rules.Add(rule);
                return true;
            }
            else if ((name is string || name is ConcatenatedString) && 
                     (id is int || id is double) && 
                     (count is int || count is double) && 
                     (timeout is int || timeout is double)) {

                rules.Add(new FloodRule(script, name.ToString(), 
                    Convert.ToInt32(id), 
                    Convert.ToDouble(count), 
                    Convert.ToDouble(timeout)));

                return true;
            }

            return false;
        }

        [JSFunction(Name = "remove", IsEnumerable = true, IsWritable = false)]
        public bool Remove(object a) {

            if (a is FloodRule rule) {
                return rules.Remove(rule);
            }
            else if (a is string || a is ConcatenatedString) {
                int idx = rules.FindIndex((s) => s.Name == a.ToString());
                if (idx > -1) {
                    rules.RemoveAt(idx);
                    return true;
                }
            }

            return false;
        }

        [JSFunction(Name = "removeAll", IsEnumerable = true, IsWritable = false)]
        public void RemoveAll(object a) {

            if (a is FunctionInstance func) {
                for (int i = (Count - 1); i >= 0; i--) {

                    object ret = func.Call(
                        Engine.Global,
                        new FloodRule(script, this.rules[i]));

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        this.rules.RemoveAt(i);
                }
            }
        }

        [JSFunction(Name = "removeAt", IsEnumerable = true, IsWritable = false)]
        public bool RemoveAt(int index) {

            if (index < 0 || index >= this.rules.Count)
                return false;

            this.rules.RemoveAt(index);
            return true;
        }


        [JSFunction(Name = "clear", IsEnumerable = true, IsWritable = false)]
        public void Clear() {
            this.rules.Clear();
        }

        [JSFunction(Name = "find", IsEnumerable = true, IsWritable = false)]
        public override object Find(object a) {

            if (a is FunctionInstance func) {

                for (int i = 0; i < this.Count; i++) {

                    var rule = new FloodRule(script, this.rules[i]);
                    object ret = func.Call(Engine.Global, rule);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        return rule;
                }
            }

            return null;
        }

        [JSFunction(Name = "findAll", IsEnumerable = true, IsWritable = false)]
        public override Collection FindAll(object a) {

            if (a is FunctionInstance func) {
                var list = new List(script);

                for (int i = 0; i < this.Count; i++) {

                    var rule = new FloodRule(script, this.rules[i]);
                    object ret = func.Call(Engine.Global, rule);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        list.Add(rule);
                }

                return list;
            }

            return null;
        }

        [JSFunction(Name = "indexOf", IsEnumerable = true, IsWritable = false)]
        public override int IndexOf(object a) {

            if (a is FunctionInstance func) {
                
                for (int i = 0; i < this.Count; i++) {

                    var rule = new FloodRule(script, this.rules[i]);
                    object ret = func.Call(Engine.Global, rule);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        return i;
                }
                return -1;
            }
            else if (a is FloodRule rule) {
                return this.rules.FindIndex((s) => s.Equals(rule));
            }

            return -1;
        }

        [JSFunction(Name = "contains", IsEnumerable = true, IsWritable = false)]
        public override bool Contains(object a) {
            return (IndexOf(a) > -1);
        }

        public override IEnumerable<PropertyNameAndValue> Properties {
            get {
                for (int i = 0; i < this.Count; i++) {
                    var rule = new FloodRule(script, this.rules[i]);
                    yield return new PropertyNameAndValue(i.ToString(), new PropertyDescriptor(rule, PropertyAttributes.FullAccess));
                }
            }
        }

        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index) {

            if (index < Count) {
                var rule = new FloodRule(script, this.rules[(int)index]);
                return new PropertyDescriptor(rule, PropertyAttributes.FullAccess);
            }

            return new PropertyDescriptor(null, PropertyAttributes.Sealed);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jurassic;
using Jurassic.Library;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Collection : BaseCollection
    {
        protected List<Object> items;

        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return items.Count; }
        }

        internal List<Object> Items {
            get { return items; }
        }

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "Collection", new Collection(script)) {
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public Collection Call() {
                return null;
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public Collection Construct() {
                return null;
            }
        }

        #endregion

        private Collection(JScript script)
            : base(script) {

            this.script = script;
            this.items = new List<Object>();

            this.PopulateFunctions();
        }

        public Collection(JScript script, ObjectInstance prototype)
            : base(script, prototype) {

            this.script = script;
            this.items = new List<Object>();

            this.PopulateFunctions();
        }

        [JSFunction(Name = "find", IsEnumerable = true, IsWritable = false)]
        public override object Find(object a) {

            if (a is FunctionInstance func) {
                for (int i = 0; i < items.Count; i++) {
                    object ret = func.Call(Engine.Global, items[i]);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        return items[i];
                }
            }

            return null;
        }

        [JSFunction(Name = "findAll", IsEnumerable = true, IsWritable = false)]
        public override Collection FindAll(object a) {

            if (a is FunctionInstance func) {
                var list = new List(script);

                for (int i = 0; i < items.Count; i++) {
                    object ret = func.Call(Engine.Global, items[i]);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        list.Add(items[i]);
                }

                return list;
            }

            return null;
        }

        [JSFunction(Name = "indexOf", IsEnumerable = true, IsWritable = false)]
        public override int IndexOf(object a) {

            if (a is FunctionInstance func) {
                for (int i = 0; i < items.Count; i++) {
                    object ret = func.Call(Engine.Global, items[i]);

                    if (TypeConverter.ConvertTo<bool>(Engine, ret))
                        return i;
                }
                return -1;
            }
            else return items.IndexOf(a);
        }

        [JSFunction(Name = "contains", IsEnumerable = true, IsWritable = false)]
        public override bool Contains(object a) {
            return (IndexOf(a) > -1);
        }

        protected override object GetMissingPropertyValue(object key)
        {
            return base.GetMissingPropertyValue(key);
        }

        public override IEnumerable<PropertyNameAndValue> Properties {
            get {
                for (int i = 0; i < items.Count; i++)
                    yield return new PropertyNameAndValue(i.ToString(), new PropertyDescriptor(items[i], PropertyAttributes.FullAccess));

                yield return new PropertyNameAndValue(null, new PropertyDescriptor(null, PropertyAttributes.Sealed));
            }
        }

        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index) {

            if (index < items.Count)
                return new PropertyDescriptor(items[(int)index], PropertyAttributes.FullAccess);

            return new PropertyDescriptor(null, PropertyAttributes.Sealed);
        }
    }
}

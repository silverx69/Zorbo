using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jurassic;
using Jurassic.Library;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class ReadOnlyList : Collection
    {
        #region " Constructor "

        public new class Constructor : ClrFunction
        {
            readonly JScript script = null;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "ReadOnlyList", new ReadOnlyList(script)) {

                this.script = script;
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public ReadOnlyList Call(object a) {
                return Construct(a);
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public ReadOnlyList Construct(object a) {

                if (a is List list)
                    return new ReadOnlyList(script, list, this.InstancePrototype);
                
                return null;
            }
        }

        #endregion

        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return items.Count; }
        }

        internal ReadOnlyList(JScript script)
            : base(script, ((ClrFunction)script.Engine.Global["Collection"]).InstancePrototype) {

            this.items = new List<Object>();
            this.PopulateFunctions();
        }

        public ReadOnlyList(JScript script, List list)
            : base(script, ((ClrFunction)script.Engine.Global["ReadOnlyList"]).InstancePrototype) {

            this.items = list.Items;
            this.PopulateFunctions();
        }

        public ReadOnlyList(JScript script, List list, ObjectInstance prototype)
            : base(script, prototype) {

            this.items = list.Items;
            this.PopulateFunctions();
        }


        public override IEnumerable<PropertyNameAndValue> Properties {
            get {
                for (int i = 0; i < items.Count; i++)
                    yield return new PropertyNameAndValue(i.ToString(), new PropertyDescriptor(items[i], PropertyAttributes.FullAccess));
            }
        }

        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index) {

            if (index < items.Count)
                return new PropertyDescriptor(items[(int)index], PropertyAttributes.FullAccess);

            return new PropertyDescriptor(null, PropertyAttributes.Sealed);
        }
    }
}

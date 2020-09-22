using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core;

using Jurassic;
using Jurassic.Library;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class List : Collection
    {
        [JSProperty(Name = "count", IsEnumerable = true)]
        public override int Count {
            get { return items.Count; }
        }

        #region " Constructor "

        public new  class Constructor : ClrFunction
        {
            readonly JScript script = null;

            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "List", new List(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public List Call(object a) {
                return Construct(a);
            }

            [JSConstructorFunction]
            public List Construct(object a) {
                object[] tmp = null;

                if (a is ArrayInstance array) {
                    tmp = new object[array.Length];

                    for (int i = 0; i < tmp.Length; i++)
                        tmp[i] = array[i];
                }

                if (tmp == null)
                    return new List(script);
                else
                    return new List(script, tmp);
            }
        }

        #endregion

        public List(JScript script)
            : base(script, ((ClrFunction)script.Engine.Global["Collection"]).InstancePrototype) {

            this.items = new List<Object>();
            this.PopulateFunctions();
        }

        public List(JScript script, object[] collection)
            : base(script, ((ClrFunction)script.Engine.Global["Collection"]).InstancePrototype) {

            this.items = new List<Object>();
            this.PopulateFunctions();

            collection.ForEach((s) => Add(s));
        }

        [JSFunction(Name = "add", IsEnumerable = true, IsWritable = false)]
        public int Add(object a) {
            if (TypeUtilities.IsPrimitiveOrObject(a))
                items.Add(a);

            return items.Count;
        }

        [JSFunction(Name = "addRange", IsEnumerable = true, IsWritable = false)]
        public void AddRange(object a) {
            if (a is ArrayInstance array) {
                for (int i = 0; i < array.Length; i++)
                    Add(array[i]);
            }
        }

        [JSFunction(Name = "insert", IsEnumerable = true, IsWritable = false)]
        public int Insert(int index, object a) {
            if (TypeUtilities.IsPrimitiveOrObject(a))
                items.Insert(index, a);

            return items.Count;
        }

        [JSFunction(Name = "insertRange", IsEnumerable = true, IsWritable = false)]
        public void InsertRange(int index, object a) {
            if (a is ArrayInstance array) {
                for (int i = 0; i < array.Length; i++)
                    Insert(index + i, array[i]);
            }
        }

        [JSFunction(Name = "remove", IsEnumerable = true, IsWritable = false)]
        public bool Remove(object a) {
            if (TypeUtilities.IsPrimitiveOrObject(a))
                return items.Remove(a);

            return false;
        }

        [JSFunction(Name = "removeAt", IsEnumerable = true, IsWritable = false)]
        public bool RemoveAt(int index) {

            if (index < 0 || index >= items.Count)
                return false;

            items.RemoveAt(index);
            return true;
        }

        [JSFunction(Name = "removeRange", IsEnumerable = true, IsWritable = false)]
        public void RemoveRange(int index, int count) {
            if (index < 0 || index >= items.Count)
                return;

            if (index + count > items.Count)
                count = (items.Count - index);

            items.RemoveRange(index, count);
        }

        [JSFunction(Name = "clear", IsEnumerable = true, IsWritable = false)]
        public void Clear() { items.Clear(); }

        public override void SetPropertyValue(uint index, object value, bool throwOnError) {
            if (index < items.Count)
                items[(int)index] = value;
        }

        [JSFunction(Name = "toArray", IsEnumerable = true, IsWritable = false)]
        public ArrayInstance ToArray() {
            var array = Engine.Array.New();

            for (int i = 0; i < Count; i++)
                array.Push(this[i]);

            return array;
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

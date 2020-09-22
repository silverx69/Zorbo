using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace Javascript.Objects
{
    public class BitConverter : ScriptObject
    {
        public BitConverter(Javascript.Script script)
            : base(script.Engine)
        {
            this.PopulateFunctions();
        }

        [JSFunction(Name = "getBytes", IsEnumerable = true, IsWritable = false, Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
        public ArrayInstance GetBytes(object a)
        {
            if (a.GetType().IsPrimitive) {
                if (a is sbyte || a is byte)
                    return System.BitConverter.GetBytes(Convert.ToByte(a)).ToJSArray(Engine);

                else if (a is short || a is ushort)
                    return System.BitConverter.GetBytes(Convert.ToInt16(a)).ToJSArray(Engine);

                else if (a is int || a is uint)
                    return System.BitConverter.GetBytes(Convert.ToInt32(a)).ToJSArray(Engine);

                else if (a is long || a is ulong)
                    return System.BitConverter.GetBytes(Convert.ToInt64(a)).ToJSArray(Engine);

                else if (a is float || a is double)
                    return System.BitConverter.GetBytes(Convert.ToInt64(a)).ToJSArray(Engine);
            }
            else if (a is NumberInstance num)
                return System.BitConverter.GetBytes(num.Value).ToJSArray(Engine);
            return null;
        }

        [JSFunction(Name = "toInt16", IsEnumerable = true, IsWritable = false)]
        public int ToInt16(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => System.BitConverter.ToInt16(tmp, index));
        }

        [JSFunction(Name = "toUInt16", IsEnumerable = true, IsWritable = false)]
        public int ToUInt16(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => System.BitConverter.ToUInt16(tmp, index));
        }

        [JSFunction(Name = "toInt32", IsEnumerable = true, IsWritable = false)]
        public int ToInt32(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => System.BitConverter.ToInt32(tmp, index));
        }

        [JSFunction(Name = "toUInt32", IsEnumerable = true, IsWritable = false)]
        public NumberInstance ToUInt32(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => Engine.Number.Construct(System.BitConverter.ToUInt32(tmp, index)));
        }

        [JSFunction(Name = "toInt64", IsEnumerable = true, IsWritable = false)]
        public NumberInstance ToInt64(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => Engine.Number.Construct(System.BitConverter.ToInt64(tmp, index)));
        }

        [JSFunction(Name = "toUInt64", IsEnumerable = true, IsWritable = false)]
        public NumberInstance ToUInt64(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => Engine.Number.Construct(System.BitConverter.ToUInt64(tmp, index)));
        }

        [JSFunction(Name = "toFloat", IsEnumerable = true, IsWritable = false)]
        public NumberInstance ToFloat(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => Engine.Number.Construct(System.BitConverter.ToSingle(tmp, index)));
        }

        [JSFunction(Name = "toDouble", IsEnumerable = true, IsWritable = false)]
        public NumberInstance ToDouble(object a, int index = 0)
        {
            return ToInternal(a, index, (tmp, index) => Engine.Number.Construct(System.BitConverter.ToDouble(tmp, index)));
        }

        private T ToInternal<T>(object a, int index, Func<byte[], int, T> action)
        {
            if (a is ArrayInstance array) {
                byte[] tmp = array.ToArray<byte>();
                return action(tmp, index);
            }
            return default;
        }
    }
}

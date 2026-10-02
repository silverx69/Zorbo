using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Zorbo;

using Jurassic;
using Jurassic.Library;

namespace Javascript.Objects
{
    static class Extensions
    {
        public static RegExpInstance ToRegExpInstance(this Regex regex, ScriptEngine engine) {
            string modifiers = "g";

            if ((regex.Options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase)
                modifiers += "i";

            if ((regex.Options & RegexOptions.Multiline) == RegexOptions.Multiline)
                modifiers += "m";

            return engine.RegExp.Construct(regex.ToString(), modifiers);
        }


        public static T[] ToArray<T>(this ArrayInstance array) {
            return ToArray<T>(array, System.Text.Encoding.UTF8);
        }

        public static T[] ToArray<T>(this ArrayInstance array, System.Text.Encoding encoding) {
            return ToArray<T>(array, (int)array.Length, encoding);
        }

        public static T[] ToArray<T>(this ArrayInstance array, int count, bool exact = false) {
            return ToArray<T>(array, count, System.Text.Encoding.UTF8, exact);
        }

        public static T[] ToArray<T>(this ArrayInstance array, int count, System.Text.Encoding encoding, bool exact = false) {

            Type toType = typeof(T);
            List<T> t = new List<T>();

            int i = 0;
            for (; i < array.Length && i < count; i++) {
                object val = array[i];

                if (val is String || val is ConcatenatedString) {
                    if (toType == typeof(byte)) {

                        var x = val.ToString();
                        byte[] tmp = encoding.GetBytes(x);

                        foreach (byte b in tmp)
                            try {
                                val = Convert.ChangeType(b, typeof(T));
                                t.Add((T)val);
                            }
                            catch { }
                    }
                    else if (toType == typeof(char)) {

                        var x = val.ToString();
                        char[] tmp = encoding.GetChars(encoding.GetBytes(x));

                        foreach(char c in tmp)
                            try {
                                val = Convert.ChangeType(c, typeof(T));
                                t.Add((T)val);
                            }
                            catch { }
                    }
                    else t.Add((T)val);
                }
                else if (val is NumberInstance instance)
                    t.Add((T)(object)instance.Value);
                
                else if (!(val is ObjectInstance))
                    try {
                        val = Convert.ChangeType(val, typeof(T));
                        t.Add((T)val);
                    }
                    catch { }
            }

            if (exact)
                while (i < count) t.Add(default);

            return t.ToArray();
        }


        public static bool SequenceEqual(this ArrayInstance array, ArrayInstance compare) {

            if (array.Length != compare.Length)
                return false;

            for (int i = 0; i < array.Length; i++)
                if (array[i] != compare[i]) return false;
            
            return true;
        }

        public static ArrayInstance ToJSArray<T>(this IEnumerable<T> collection, ScriptEngine engine) {

            var array = engine.Array.New();

            if (collection != null) {
                foreach (var item in collection) {
                    if (!(item is ObjectInstance))
                        try {
                            if (item is Char || item is String || item is ConcatenatedString)
                                array.Push(item.ToString());

                            else array.Push(Convert.ToInt32(item));
                        }
                        catch { }
                    else array.Push(item);
                }
            }

            return array;
        }
    }
}

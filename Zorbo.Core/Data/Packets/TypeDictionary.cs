using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zorbo.Core.Data.Packets
{
    public class PropertyData
    {
        public PropertyInfo Property { get; set; }

        public PacketItemAttribute Attribute { get; set; }


        public PropertyData(PropertyInfo prop, PacketItemAttribute attribute) {
            Property = prop;
            Attribute = attribute;
        }
    }

    public static class TypeDictionary
    {
        static readonly Dictionary<Type, List<PropertyData>> types;

        static TypeDictionary() {
            types = new Dictionary<Type, List<PropertyData>>();
        }

        public static List<PropertyData> GetRecord<T>() {
            return GetRecord(typeof(T));
        }

        public static List<PropertyData> GetRecord(Type t) {
            List<PropertyData> members = null;

            lock(types)
                if (types.ContainsKey(t))
                    members = types[t];

            if (members != null) return members;

            BindingFlags bf = (BindingFlags.Instance | BindingFlags.Public);

            members = new List<PropertyData>();
            var m = t.GetProperties(bf);

            List<PropertyData> props = new List<PropertyData>();

            for (int i = m.Length - 1; i >= 0; i--) {
                var p = m[i];
                var a = p.GetAttribute<PacketItemAttribute>();

                if (a == null || p.GetIndexParameters().Length > 0)
                    continue;

                members.Add(new PropertyData(p, a));
            }

            members.Sort(new Comparison<PropertyData>((a, b) => a.Attribute.Index.CompareTo(b.Attribute.Index)));

            lock (types) types[t] = members;

            return members;
        }


        public static T GetAttribute<T>(this PropertyInfo prop) where T : Attribute {
            var arr = prop.GetCustomAttributes(typeof(T), true);

            if (arr.Length > 0)
                return (T)arr[0];

            return null;
        }
    }
}

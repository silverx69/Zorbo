using System.Collections;
using System.Reflection;

namespace Zorbo.Data
{
    public sealed class ReflectedType
    {
        public Type Type { get; }

        public Type ElementType { get; }

        public bool IsArray { get; }

        public bool IsList { get; }

        public bool IsDictionary { get; }

        public bool IsGeneric { get; }

        public bool IsConstructable { get; }

        public List<object> Attributes { get; } = [];

        public List<Type> GenericArguments { get; } = [];

        public List<PropertyInfo> Properties { get; } = [];

        static readonly Type IListType = typeof(IList);
        static readonly Type IDictionaryType = typeof(IDictionary);

        public ReflectedType(Type type) {
            Type = type;

            Attributes = [.. type.GetCustomAttributes(true)];
            IsConstructable = !type.IsStatic() && type.HasDefaultConstructor();
            IsArray = type.IsArray && type.HasElementType;
            IsGeneric = type.IsConstructedGenericType;
            IsList = IListType.IsAssignableFrom(type) && IsGeneric;
            IsDictionary = IDictionaryType.IsAssignableFrom(type) && IsGeneric;

            if (IsArray)
                ElementType = type.GetElementType();

            if (IsGeneric)
                GenericArguments = [.. type.GetGenericArguments()];

            // attempt to keep properties in declaration order since reflection does not guarantee order across different environments and platforms
            foreach (var property in type.GetProperties().OrderBy(s => s.MetadataToken)) {
                if (property.GetCustomAttributes(typeof(BinaryIgnoreAttribute), true).Length == 0)
                    Properties.Add(property);
            }
        }

        public T GetAttribute<T>() where T : Attribute {
            return (T)Attributes.FirstOrDefault(s => s is T);
        }
    }
}

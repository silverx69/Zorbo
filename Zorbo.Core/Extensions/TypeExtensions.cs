namespace Zorbo
{
    public static partial class TypeExtensions
    {
        public static bool IsStatic(this Type type) {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool HasDefaultConstructor(this Type type) {
            return type.GetConstructor(Type.EmptyTypes) is not null;
        }
    }
}
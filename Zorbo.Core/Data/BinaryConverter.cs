using System.Reflection;

namespace Zorbo.Data
{
    public abstract class BinaryConverter<T> : BinaryConverter
    {
        Type toType;

        public override bool CanConvert(Type type) {
            toType ??= typeof(T);
            return toType == type;
        }

        public abstract T Read(ZBinaryReader reader);
        public abstract Task<T> ReadAsync(ZBinaryReader reader);

        public abstract void Write(ZBinaryWriter writer, T value);
        public abstract Task WriteAsync(ZBinaryWriter writer, T value);
    }

    public abstract class BinaryConverter
    {
        Type thisType;

        MethodInfo writeMethod;
        MethodInfo writeAsyncMethod;

        MethodInfo readMethod;
        MethodInfo readAsyncMethod;
        PropertyInfo readAsyncResult;

        public Type Type {
            get { return thisType ??= GetType(); }
        }

        internal MethodInfo ReadMethod {
            get { return readMethod ??= Type.GetMethod("Read"); }
        }

        internal MethodInfo ReadAsyncMethod {
            get { return readAsyncMethod ??= Type.GetMethod("ReadAsync"); }
        }

        internal PropertyInfo ReadAsyncResult {
            get { return readAsyncResult ??= ReadAsyncMethod.ReturnType.GetProperty("Result"); }
        }

        internal MethodInfo WriteMethod {
            get { return writeMethod ??= Type.GetMethod("Write"); }
        }

        internal MethodInfo WriteAsyncMethod {
            get { return writeAsyncMethod ??= Type.GetMethod("WriteAsync"); }
        }

        public abstract bool CanConvert(Type type);
    }
}

using System.Collections;
using System.Reflection;
using Zorbo.Data.Converters;

namespace Zorbo.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BinaryIgnoreAttribute : Attribute { }

    public static class BinarySerializer
    {
        static readonly BinaryConverter[] defaultConverters = [
                new BinaryDateTimeConverter(),
                new BinaryGuidConverter(),
                new BinaryIPAddressConverter()
            ];

        static readonly Lock dictionaryLock = new();
        static readonly Dictionary<Type, ReflectedType> knownTypes = [];

        public static byte[] Serialize(object value, BinaryConverter[] converters = null) {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            using var stream = new MemoryStream();
            Serialize(stream, value, converters);
            return stream.ToArray();
        }

        public static void Serialize(Stream stream, object value, BinaryConverter[] converters = null) {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            using var writer = new ZBinaryWriter(stream, true);
            Serialize(writer, value, converters);
        }

        public static void Serialize(ZBinaryWriter writer, object value, BinaryConverter[] converters = null) {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            Serialize(writer, value, value.GetType(), converters);
        }

        public static byte[] Serialize<T>(T value, BinaryConverter[] converters = null)
            where T : new() {
            using var stream = new MemoryStream();
            Serialize(stream, value, converters);
            return stream.ToArray();
        }

        public static void Serialize<T>(Stream stream, T value, BinaryConverter[] converters = null)
            where T : new() {
            using var writer = new ZBinaryWriter(stream, true);
            Serialize(writer, value, converters);
        }

        public static void Serialize<T>(ZBinaryWriter writer, T value, BinaryConverter[] converters = null)
            where T : new() {
            Serialize(writer, value, typeof(T), converters);
        }

        public static async Task<byte[]> SerializeAsync(object value, BinaryConverter[] converters = null) {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            using var stream = new MemoryStream();
            await SerializeAsync(stream, value, converters);
            return stream.ToArray();
        }

        public static Task SerializeAsync(Stream stream, object value, BinaryConverter[] converters = null) {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            using var writer = new ZBinaryWriter(stream, true);
            return SerializeAsync(writer, value, converters);
        }

        public static Task SerializeAsync(ZBinaryWriter writer, object value, BinaryConverter[] converters = null) {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            return SerializeAsync(writer, value, value.GetType(), converters);
        }

        public static async Task<byte[]> SerializeAsync<T>(T value, BinaryConverter[] converters = null)
            where T : new() {
            using var stream = new MemoryStream();
            await SerializeAsync(stream, value, converters);
            return stream.ToArray();
        }

        public static Task SerializeAsync<T>(Stream stream, T value, BinaryConverter[] converters = null)
            where T : new() {
            using var writer = new ZBinaryWriter(stream, true);
            return SerializeAsync(writer, value, converters);
        }

        public static Task SerializeAsync<T>(ZBinaryWriter writer, T value, BinaryConverter[] converters = null)
            where T : new() {
            return SerializeAsync(writer, value, typeof(T), converters);
        }


        public static object Deserialize(byte[] data, Type type, BinaryConverter[] converters = null) {
            return Deserialize(data, 0, data.Length, type, converters);
        }

        public static object Deserialize(byte[] data, int offset, int count, Type type, BinaryConverter[] converters = null) {
            using var stream = new MemoryStream(data, offset, count);
            return Deserialize(stream, type, converters);
        }

        public static object Deserialize(Stream stream, Type type, BinaryConverter[] converters = null) {
            using var reader = new ZBinaryReader(stream, true);
            return Deserialize(reader, type, converters);
        }

        public static T Deserialize<T>(byte[] data, BinaryConverter[] converters = null)
            where T : new() {
            return Deserialize<T>(data, 0, data.Length, converters);
        }

        public static T Deserialize<T>(byte[] data, int offset, int count, BinaryConverter[] converters = null)
            where T : new() {
            using var stream = new MemoryStream(data, offset, count);
            return Deserialize<T>(stream, converters);
        }

        public static T Deserialize<T>(Stream stream, BinaryConverter[] converters = null)
            where T : new() {
            using var reader = new ZBinaryReader(stream, true);
            return Deserialize<T>(reader, converters);
        }

        public static T Deserialize<T>(ZBinaryReader reader, BinaryConverter[] converters = null)
            where T : new() {
            return Deserialize(reader, typeof(T), converters) is T result ? result : default;
        }


        public static Task<T> DeserializeAsync<T>(byte[] data, BinaryConverter[] converters = null)
            where T : new() {
            return DeserializeAsync<T>(data, 0, data.Length, converters);
        }

        public static Task<T> DeserializeAsync<T>(byte[] data, int offset, int count, BinaryConverter[] converters = null)
            where T : new() {
            using var stream = new MemoryStream(data, offset, count);
            return DeserializeAsync<T>(stream, converters);
        }

        public static Task<T> DeserializeAsync<T>(Stream stream, BinaryConverter[] converters = null)
            where T : new() {
            using var reader = new ZBinaryReader(stream, true);
            return DeserializeAsync<T>(reader, converters);
        }

        public static async Task<T> DeserializeAsync<T>(ZBinaryReader reader, BinaryConverter[] converters = null)
            where T : new() {
            return await DeserializeAsync(reader, typeof(T), converters) is T result ? result : default;
        }


        #region " Private Serialize Methods "

        private static void Serialize(ZBinaryWriter writer, object obj, Type type, BinaryConverter[] converters = null) {

            if (converters is not null) {
                foreach (var converter in converters)
                    if (converter.CanConvert(type)) {
                        converter.WriteMethod.Invoke(converter, [writer, obj]);
                        return;
                    }
            }
            foreach (var converter in defaultConverters)
                if (converter.CanConvert(type)) {
                    converter.WriteMethod.Invoke(converter, [writer, obj]);
                    return;
                }

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    writer.Write((bool)obj);
                    break;
                case TypeCode.Byte:
                    writer.Write((byte)obj);
                    break;
                case TypeCode.SByte:
                    writer.Write((sbyte)obj);
                    break;
                case TypeCode.Int16:
                    writer.Write((short)obj);
                    break;
                case TypeCode.Int32:
                    writer.Write((int)obj);
                    break;
                case TypeCode.Int64:
                    writer.Write((long)obj);
                    break;
                case TypeCode.UInt16:
                    writer.Write((ushort)obj);
                    break;
                case TypeCode.UInt32:
                    writer.Write((uint)obj);
                    break;
                case TypeCode.UInt64:
                    writer.Write((ulong)obj);
                    break;
                case TypeCode.Single:
                    writer.Write((float)obj);
                    break;
                case TypeCode.Double:
                    writer.Write((double)obj);
                    break;
                case TypeCode.Decimal:
                    writer.Write((decimal)obj);
                    break;
                case TypeCode.Char:
                    writer.Write((char)obj);
                    break;
                case TypeCode.String:
                    writer.Write((string)obj);
                    break;
                default:
                    var knownType = GetReflectedType(type);
                    if (knownType.IsArray)
                        SerializeArray(writer, obj as Array, knownType.ElementType, converters);

                    else if (knownType.IsList)
                        SerializeList(writer, obj as IList, knownType.GenericArguments[0], converters);

                    else if (knownType.IsDictionary)
                        SerializeDictionary(writer, obj as IDictionary, [.. knownType.GenericArguments], converters);
                    else {
                        if (!knownType.IsConstructable)
                            throw new InvalidOperationException($"Type '{type.FullName}' is static or does not have a default constructor.");
                        SerializeObject(writer, obj, knownType, converters);
                    }
                    break;
            }
        }

        private static Task SerializeAsync(ZBinaryWriter writer, object obj, Type type, BinaryConverter[] converters) {

            if (converters is not null) {
                foreach (var converter in converters)
                    if (converter.CanConvert(type)) {
                        return (Task)converter.WriteAsyncMethod.Invoke(converter, [writer, obj]);
                    }
            }
            foreach (var converter in defaultConverters)
                if (converter.CanConvert(type)) {
                    return (Task)converter.WriteAsyncMethod.Invoke(converter, [writer, obj]);
                }

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    return writer.WriteAsync((bool)obj);
                case TypeCode.Byte:
                    return writer.WriteAsync((byte)obj);
                case TypeCode.SByte:
                    return writer.WriteAsync((sbyte)obj);
                case TypeCode.Int16:
                    return writer.WriteAsync((short)obj);
                case TypeCode.Int32:
                    return writer.WriteAsync((int)obj);
                case TypeCode.Int64:
                    return writer.WriteAsync((long)obj);
                case TypeCode.UInt16:
                    return writer.WriteAsync((ushort)obj);
                case TypeCode.UInt32:
                    return writer.WriteAsync((uint)obj);
                case TypeCode.UInt64:
                    return writer.WriteAsync((ulong)obj);
                case TypeCode.Single:
                    return writer.WriteAsync((float)obj);
                case TypeCode.Double:
                    return writer.WriteAsync((double)obj);
                case TypeCode.Decimal:
                    return writer.WriteAsync((decimal)obj);
                case TypeCode.Char:
                    return writer.WriteAsync((char)obj);
                case TypeCode.String:
                    return writer.WriteAsync((string)obj);
                default:
                    var knownType = GetReflectedType(type);
                    if (knownType.IsArray)
                        return SerializeArrayAsync(writer, obj as Array, knownType.ElementType, converters);

                    else if (knownType.IsList)
                        return SerializeListAsync(writer, obj as IList, knownType.GenericArguments[0], converters);

                    else if (knownType.IsDictionary)
                        return SerializeDictionaryAsync(writer, obj as IDictionary, [.. knownType.GenericArguments], converters);
                    else {
                        if (!knownType.IsConstructable)
                            throw new InvalidOperationException($"Type '{type.FullName}' is static or does not have a default constructor.");
                        return SerializeObjectAsync(writer, obj, knownType, converters);
                    }
            }
        }

        private static void SerializeArray(ZBinaryWriter writer, Array array, Type elementType, BinaryConverter[] converters) {
            if (array is null)
                writer.Write7BitEncodedInt(0);
            else {
                int length = array.GetLength(0);
                writer.Write7BitEncodedInt(length);
                for (int i = 0; i < length; i++)
                    Serialize(writer, array.GetValue(i), elementType, converters);
            }
        }

        private static async Task SerializeArrayAsync(ZBinaryWriter writer, Array array, Type elementType, BinaryConverter[] converters) {
            if (array is null)
                writer.Write7BitEncodedInt(0);
            else {
                int length = array.GetLength(0);
                writer.Write7BitEncodedInt(length);
                for (int i = 0; i < length; i++)
                    await SerializeAsync(writer, array.GetValue(i), elementType, converters);
            }
        }

        private static void SerializeList(ZBinaryWriter writer, IList list, Type elementType, BinaryConverter[] converters) {
            if (list is null)
                writer.Write7BitEncodedInt(0);
            else {
                writer.Write7BitEncodedInt(list.Count);
                foreach(var item in list)
                    Serialize(writer, item, elementType, converters);
            }
        }

        private static async Task SerializeListAsync(ZBinaryWriter writer, IList list, Type elementType, BinaryConverter[] converters) {
            if (list is null)
                writer.Write7BitEncodedInt(0);
            else {
                writer.Write7BitEncodedInt(list.Count);
                foreach (var item in list)
                    await SerializeAsync(writer, item, elementType, converters);
            }
        }

        private static void SerializeDictionary(ZBinaryWriter writer, IDictionary dict, Type[] elementTypes, BinaryConverter[] converters) {
            if (dict is null)
                writer.Write7BitEncodedInt(0);
            else {
                writer.Write7BitEncodedInt(dict.Count);
                foreach (var key in dict.Keys) {
                    Serialize(writer, key, elementTypes[0], converters);
                    Serialize(writer, dict[key], elementTypes[1], converters);
                }
            }
        }

        private static async Task SerializeDictionaryAsync(ZBinaryWriter writer, IDictionary dict, Type[] elementTypes, BinaryConverter[] converters) {
            if (dict is null)
                writer.Write7BitEncodedInt(0);
            else {
                writer.Write7BitEncodedInt(dict.Count);
                foreach (var key in dict.Keys) {
                    await SerializeAsync(writer, key, elementTypes[0], converters);
                    await SerializeAsync(writer, dict[key], elementTypes[1], converters);
                }
            }
        }

        private static void SerializeObject(ZBinaryWriter writer, object obj, ReflectedType type, BinaryConverter[] converters) {
            foreach (var property in type.Properties) {
                if (property.CanRead)
                    Serialize(writer, property.GetValue(obj), property.PropertyType, converters);
            }
        }

        private static async Task SerializeObjectAsync(ZBinaryWriter writer, object obj, ReflectedType type, BinaryConverter[] converters) {
            foreach (var property in type.Properties) {
                if (property.CanRead)
                    await SerializeAsync(writer, property.GetValue(obj), property.PropertyType, converters);
            }
        }

        #endregion

        #region " Private Deserialize Methods "

        public static object Deserialize(ZBinaryReader reader, Type type, BinaryConverter[] converters = null) {

            if (converters is not null) {
                foreach (var converter in converters)
                    if (converter.CanConvert(type))
                        return converter.ReadMethod.Invoke(converter, [reader]);
            }
            foreach (var converter in defaultConverters)
                if (converter.CanConvert(type))
                    return converter.ReadMethod.Invoke(converter, [reader]);

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    return reader.ReadBoolean();
                case TypeCode.Byte:
                    return reader.ReadByte();
                case TypeCode.SByte:
                    return reader.ReadSByte();
                case TypeCode.Int16:
                    return reader.ReadInt16();
                case TypeCode.Int32:
                    return reader.ReadInt32();
                case TypeCode.Int64:
                    return reader.ReadInt64();
                case TypeCode.UInt16:
                    return reader.ReadUInt16();
                case TypeCode.UInt32:
                    return reader.ReadUInt32();
                case TypeCode.UInt64:
                    return reader.ReadUInt64();
                case TypeCode.Single:
                    return reader.ReadSingle();
                case TypeCode.Double:
                    return reader.ReadDouble();
                case TypeCode.Decimal:
                    return reader.ReadDecimal();
                case TypeCode.Char:
                    return reader.ReadChar();
                case TypeCode.String:
                    return reader.ReadString();
                default:
                    var knownType = GetReflectedType(type);
                    if (knownType.IsArray)
                        return DeserializeArray(reader, knownType.ElementType, converters);

                    else if (knownType.IsList)
                        return DeserializeList(reader, type, knownType.GenericArguments[0], converters);

                    else if (knownType.IsDictionary)
                        return DeserializeDictionary(reader, type, [.. knownType.GenericArguments], converters);
                    else {
                        if (!knownType.IsConstructable)
                            throw new InvalidOperationException($"Type '{type.FullName}' is static or does not have a default constructor.");
                        return DeserializeObject(reader, knownType, converters);
                    }
            }
        }

        public static async Task<object> DeserializeAsync(ZBinaryReader reader, Type type, BinaryConverter[] converters = null) {

            if (converters is not null) {
                foreach (var converter in converters)
                    if (converter.CanConvert(type)) {
                        var task = (Task)converter.ReadAsyncMethod.Invoke(converter, [reader]);
                        await task.ConfigureAwait(false);
                        return converter.ReadAsyncResult.GetValue(task);
                    }
            }
            foreach (var converter in defaultConverters)
                if (converter.CanConvert(type)) {
                    var task = (Task)converter.ReadAsyncMethod.Invoke(converter, [reader]);
                    await task.ConfigureAwait(false);
                    return converter.ReadAsyncResult.GetValue(task);
                }

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    return await reader.ReadBooleanAsync();
                case TypeCode.Byte:
                    return await reader.ReadByteAsync();
                case TypeCode.SByte:
                    return await reader.ReadSByteAsync();
                case TypeCode.Int16:
                    return await reader.ReadInt16Async();
                case TypeCode.Int32:
                    return await reader.ReadInt32Async();
                case TypeCode.Int64:
                    return await reader.ReadInt64Async();
                case TypeCode.UInt16:
                    return await reader.ReadUInt16Async();
                case TypeCode.UInt32:
                    return await reader.ReadUInt32Async();
                case TypeCode.UInt64:
                    return await reader.ReadUInt64Async();
                case TypeCode.Single:
                    return await reader.ReadSingleAsync();
                case TypeCode.Double:
                    return await reader.ReadDoubleAsync();
                case TypeCode.Decimal:
                    return await reader.ReadDecimalAsync();
                case TypeCode.Char:
                    return await reader.ReadCharAsync();
                case TypeCode.String:
                    return await reader.ReadStringAsync();
                default:
                    var knownType = GetReflectedType(type);
                    if (knownType.IsArray)
                        return await DeserializeArrayAsync(reader, knownType.ElementType, converters);

                    else if (knownType.IsList)
                        return await DeserializeListAsync(reader, type, knownType.GenericArguments[0], converters);

                    else if (knownType.IsDictionary)
                        return await DeserializeDictionaryAsync(reader, type, [.. knownType.GenericArguments], converters);
                    else {
                        if (!knownType.IsConstructable)
                            throw new InvalidOperationException($"Type '{type.FullName}' is static or does not have a default constructor.");
                        return await DeserializeObjectAsync(reader, knownType, converters);
                    }
            }
        }

        private static Array DeserializeArray(ZBinaryReader reader, Type elementType, BinaryConverter[] converters) {
            int length = reader.Read7BitEncodedInt();
            Array array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
                array.SetValue(Deserialize(reader, elementType, converters), i);
            return array;
        }

        private static async Task<Array> DeserializeArrayAsync(ZBinaryReader reader, Type elementType, BinaryConverter[] converters) {
            int length = reader.Read7BitEncodedInt();
            Array array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
                array.SetValue(await DeserializeAsync(reader, elementType, converters), i);
            return array;
        }

        private static IList DeserializeList(ZBinaryReader reader, Type listType, Type elementType, BinaryConverter[] converters) {
            int length = reader.Read7BitEncodedInt();
            var list = (IList)Activator.CreateInstance(listType);
            for (int i = 0; i < length; i++)
                list.Add(Deserialize(reader, elementType, converters));
            return list;
        }

        private static async Task<IList> DeserializeListAsync(ZBinaryReader reader, Type listType, Type elementType, BinaryConverter[] converters) {
            int length = reader.Read7BitEncodedInt();
            var list = (IList)Activator.CreateInstance(listType);
            for (int i = 0; i < length; i++)
                list.Add(await DeserializeAsync(reader, elementType, converters));
            return list;
        }

        private static IDictionary DeserializeDictionary(ZBinaryReader reader, Type dictType, Type[] elementTypes, BinaryConverter[] converters) {
            int length = reader.Read7BitEncodedInt();
            var dict = (IDictionary)Activator.CreateInstance(dictType);
            for (int i = 0; i < length; i++) {
                dict.Add(
                    Deserialize(reader, elementTypes[0], converters),
                    Deserialize(reader, elementTypes[1], converters)
                );
            }
            return dict;
        }

        private static async Task<IDictionary> DeserializeDictionaryAsync(ZBinaryReader reader, Type dictType, Type[] elementTypes, BinaryConverter[] converters) {
            int length = reader.Read7BitEncodedInt();
            var dict = (IDictionary)Activator.CreateInstance(dictType);
            for (int i = 0; i < length; i++) {
                dict.Add(
                    await DeserializeAsync(reader, elementTypes[0], converters),
                    await DeserializeAsync(reader, elementTypes[1], converters)
                );
            }
            return dict;
        }

        private static object DeserializeObject(ZBinaryReader reader, ReflectedType type, BinaryConverter[] converters) {
            var obj = Activator.CreateInstance(type.Type);
            foreach (var property in type.Properties) {
                if (property.CanWrite)
                    property.SetValue(obj, Deserialize(reader, property.PropertyType, converters));
            }
            return obj;
        }

        private static async Task<object> DeserializeObjectAsync(ZBinaryReader reader, ReflectedType type, BinaryConverter[] converters) {
            var obj = Activator.CreateInstance(type.Type);
            foreach (var property in type.Properties) {
                if (property.CanWrite)
                    property.SetValue(obj, await DeserializeAsync(reader, property.PropertyType, converters));
            }
            return obj;
        }

        #endregion


        public static ReflectedType GetReflectedType(Type type) {
            lock (dictionaryLock) {
                if (!knownTypes.TryGetValue(type, out var knownType)) {
                    knownType = new ReflectedType(type);
                    knownTypes.Add(type, knownType);
                }
                return knownType;
            }
        }

        private static List<PropertyInfo> GetProperties(Type type) {
            lock (dictionaryLock) {
                if (!knownTypes.TryGetValue(type, out var knownType)) {
                    knownType = new ReflectedType(type);
                    knownTypes.Add(type, knownType);
                }
                return knownType.Properties;
            }
        }
    }
}
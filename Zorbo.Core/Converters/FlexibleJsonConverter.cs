using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zorbo.Core.Converters
{
    public class FlexibleJsonConverter<T> : JsonConverter<T> where T : class
    {
        readonly Action<JsonWriter, T, JsonSerializer> m_write;
        readonly Func<JsonReader, Type, T, bool, JsonSerializer, T> m_read;

        public FlexibleJsonConverter(
            Action<JsonWriter, T, JsonSerializer> writeAction,
            Func<JsonReader, Type, T, bool, JsonSerializer, T> readAction) 
            : base() {
            m_write = writeAction ?? throw new ArgumentNullException("writeAction");
            m_read = readAction ?? throw new ArgumentNullException("readAction");
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            m_write(writer, value, serializer);
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasValue, JsonSerializer serializer)
        {
            return m_read(reader, objectType, existingValue, hasValue, serializer);
        }
    }
}

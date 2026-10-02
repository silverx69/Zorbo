using System.Text.Json;
using Zorbo.Data;

namespace Zorbo.Net.Messages
{
    using JsonElement = System.Text.Json.JsonElement;
    using JsonSerializer = Data.JsonSerializer;

    public class MessageConverter : IMessageConverter
    {
        public class InputJsonMessage
        {
            public ushort Id { get; set; }
            public JsonElement Message { get; set; }
        }

        static readonly Lock dictionaryLock = new();
        static readonly Dictionary<ushort, ReflectedType> messageTypes = [];

        public virtual async Task WriteAsync(ZBinaryWriter writer, object value, MessageType msgType) {
            if (value is null) 
                return;

            var rtype = BinarySerializer.GetReflectedType(value.GetType());
            var attr = rtype.GetAttribute<MessageAttribute>();

            if (attr is null) return;

            lock(dictionaryLock)
                messageTypes[attr.Id] = rtype;

            switch (msgType) {
                case MessageType.Binary: {
                    writer.Write(attr.Id);
                    await BinarySerializer.SerializeAsync(writer, value);
                    break;
                }
                default: {
                    await JsonSerializer.SerializeAsync(writer.BaseStream, new MessageResult(attr.Id, value));
                    break;
                }
            }
        }

        public virtual async Task<MessageResult> ReadAsync(ZBinaryReader reader, MessageType msgType) {
            switch (msgType) {
                case MessageType.Binary: {
                    ushort id = reader.ReadUInt16();
                    var rtype = FindMessageType(id);

                    if (rtype is null) return null;

                    return new(id, await BinarySerializer.DeserializeAsync(reader, rtype.Type));
                }
                default: {
                    var jmsg = await JsonSerializer.DeserializeAsync<InputJsonMessage>(reader.BaseStream);
                    var rtype = FindMessageType(jmsg.Id);

                    if (rtype is null) return null;

                    return new(jmsg.Id, jmsg.Message.Deserialize(rtype.Type, JsonSerializer.Options));
                }
            }
        }

        public static ReflectedType FindMessageType(ushort id) {
            lock (dictionaryLock) {
                if (messageTypes.TryGetValue(id, out ReflectedType reflectedType))
                    return reflectedType;

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    foreach (var type in assembly.ExportedTypes) {

                        var rtype = BinarySerializer.GetReflectedType(type);
                        var attr = rtype.GetAttribute<MessageAttribute>();

                        if (attr is not null && attr.Id == id) {
                            messageTypes[id] = rtype;
                            return rtype;
                        }
                    }
                }
                return null;
            }
        }
    }
}
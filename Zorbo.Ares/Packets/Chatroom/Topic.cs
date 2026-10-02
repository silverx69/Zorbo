using Newtonsoft.Json;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Chatroom
{
    public abstract class TopicBase : AresPacket
    {
        [JsonProperty("topic", Required = Required.AllowNull)]
        [PacketItem(0, MaxLength = 1024, NullTerminated = false)]
        public string Message { get; set; }
    }

    public sealed class Topic : TopicBase
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_TOPIC; }
        }

        public Topic() { }

        public Topic(string topic) {
            Message = topic;
        }
    }

    public sealed class TopicFirst : TopicBase
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_TOPIC_FIRST; }
        }

        public TopicFirst() { }

        public TopicFirst(string topic) {
            Message = topic;
        }
    }
}

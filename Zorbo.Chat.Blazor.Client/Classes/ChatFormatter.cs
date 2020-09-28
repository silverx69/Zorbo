using cb0tProtocol;
using Zorbo.Ares.Packets;
using Zorbo.Core;

namespace Zorbo.Chat.Blazor.Client.Classes
{
    public class ChatFormatter : AdvancedServerFormatter
    {
        public override byte[] Format(IPacket message)
        {
            return base.Format(message);
        }

        public override byte[] FormatJson(IPacket message, bool ib0t = false)
        {
            return base.FormatJson(message, ib0t);
        }

        public override IPacket Unformat(byte id, string data, bool ib0t = false)
        {
            if (ib0t) {
                string[] values = data.Split(new[] { ':' }, 2);
                return UnformatBasic(values[0], values[1]);
            }

            if (id == (byte)AresId.MSG_CHAT_SERVER_JOIN)
                return Json.Deserialize<ChatJoin>(data);

            if (id == (byte)AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST)
                return Json.Deserialize<ChatUser>(data);

            else return base.Unformat(id, data, ib0t);
        }

        public override IPacket Unformat(byte id, byte[] data, int index, int length)
        {
            if (id == (byte)AresId.MSG_CHAT_SERVER_JOIN)
                return Serializer.Deserialize<ChatJoin>(data, index, length);

            if (id == (byte)AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST)
                return Serializer.Deserialize<ChatUser>(data, index, length);

            else return base.Unformat(id, data, index, length);
        }

        protected override IPacket UnformatBasic(string message, string content)
        {
            string[] values;
            switch (message.ToUpper()) {
                case "USERLIST":
                    values = Parseib0tMessage(content);
                    return new ChatUser() {
                        Username = values[0],
                        Level = (AdminLevel)byte.Parse(values[1])
                    };
                case "JOIN":
                    values = Parseib0tMessage(content);
                    return new ChatJoin() {
                        Username = values[0],
                        Level = (AdminLevel)byte.Parse(values[1])
                    };
            }

            return base.UnformatBasic(message, content);
        }
    }
}

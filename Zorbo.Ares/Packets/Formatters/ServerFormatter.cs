using System;
using System.Linq;
using System.Text;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;

namespace Zorbo.Ares.Packets.Formatters
{
    /// <summary>
    /// A CLIENT-side implementation of PacketFormatter&lt;PacketSerializer&gt; used for formatting packets from the server.
    /// </summary>
    public class ServerFormatter : 
        ServerFormatter<PacketSerializer> { }
    /// <summary>
    /// A CLIENT-side implementation of PacketFormatter&lt;TSerializer&gt; used for formatting packets from the server.
    /// </summary>
    public class ServerFormatter<TSerializer> : 
        PacketFormatter<TSerializer> 
        where TSerializer : PacketSerializer, new()
    {
        public override byte[] FormatJson(IPacket message, bool ib0t = false)
        {
            if (ib0t) {
                string ret = FormatBasic(message);
                if (!string.IsNullOrEmpty(ret))
                    return Encoding.UTF8.GetBytes(ret);
                return null;
            }

            return base.FormatJson(message, ib0t);
        }

        public override IPacket Unformat(byte id, string data, bool ib0t = false)
        {
            if (ib0t) {
                string[] values = data.Split(new[] { ':' }, 2);
                return UnformatBasic(values[0], values[1]);
            }

            return ((AresId)id) switch
            {
                AresId.MSG_CHAT_SERVER_NOSUCH => Json.Deserialize<Announce>(data),
                AresId.MSG_CHAT_SERVER_LOGIN_ACK => Json.Deserialize<LoginAck>(data),
                AresId.MSG_CHAT_SERVER_MYFEATURES => Json.Deserialize<Features>(data),
                AresId.MSG_CHAT_SERVER_TOPIC => Json.Deserialize<Topic>(data),
                AresId.MSG_CHAT_SERVER_TOPIC_FIRST => Json.Deserialize<TopicFirst>(data),
                AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST => Json.Deserialize<Userlist>(data),
                AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST_END => Json.Deserialize<UserlistEnd>(data),
                AresId.MSG_CHAT_SERVER_CHANNEL_USERLIST_CLEAR => Json.Deserialize<UserlistClear>(data),
                AresId.MSG_CHAT_SERVER_JOIN => Json.Deserialize<Join>(data),
                AresId.MSG_CHAT_SERVER_PART => Json.Deserialize<Parted>(data),
                AresId.MSG_CHAT_SERVER_AVATAR => Json.Deserialize<ServerAvatar>(data),
                AresId.MSG_CHAT_SERVER_PERSONAL_MESSAGE => Json.Deserialize<ServerPersonal>(data),
                AresId.MSG_CHAT_SERVER_OPCHANGE => Json.Deserialize<OpChange>(data),
                AresId.MSG_CHAT_SERVER_UPDATE_USER_STATUS => Json.Deserialize<ServerUpdate>(data),
                AresId.MSG_CHAT_SERVER_PUBLIC => Json.Deserialize<ServerPublic>(data),
                AresId.MSG_CHAT_SERVER_EMOTE => Json.Deserialize<ServerEmote>(data),
                AresId.MSG_CHAT_SERVER_PVT => Json.Deserialize<Private>(data),
                AresId.MSG_CHAT_SERVER_URL => Json.Deserialize<ServerUrl>(data),
                AresId.MSG_CHAT_SERVER_OFFLINEUSER => Json.Deserialize<Offline>(data),
                AresId.MSG_CHAT_SERVER_ISIGNORINGYOU => Json.Deserialize<IgnoringYou>(data),
                AresId.MSG_CHAT_SERVER_FASTPING => Json.Deserialize<FastPing>(data),
                AresId.MSG_CHAT_CLIENT_CUSTOM_DATA => Json.Deserialize<ClientCustom>(data),
                _ => new UnknownJson(id, data),
            };
        }

        public override IPacket Unformat(byte id, byte[] data, int index, int count)
        {
            return ((AresId)id) switch
            {
                AresId.MSG_CHAT_SERVER_NOSUCH => Serializer.Deserialize<Announce>(data, index, count),
                AresId.MSG_CHAT_SERVER_LOGIN_ACK => Serializer.Deserialize<LoginAck>(data, index, count),
                AresId.MSG_CHAT_SERVER_MYFEATURES => Serializer.Deserialize<Features>(data, index, count),
                AresId.MSG_CHAT_SERVER_TOPIC => Serializer.Deserialize<Topic>(data, index, count),
                AresId.MSG_CHAT_SERVER_TOPIC_FIRST => Serializer.Deserialize<TopicFirst>(data, index, count),
                AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST => Serializer.Deserialize<Userlist>(data, index, count),
                AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST_END => Serializer.Deserialize<UserlistEnd>(data, index, count),
                AresId.MSG_CHAT_SERVER_CHANNEL_USERLIST_CLEAR => Serializer.Deserialize<UserlistClear>(data, index, count),
                AresId.MSG_CHAT_SERVER_JOIN => Serializer.Deserialize<Join>(data, index, count),
                AresId.MSG_CHAT_SERVER_PART => Serializer.Deserialize<Parted>(data, index, count),
                AresId.MSG_CHAT_SERVER_AVATAR => Serializer.Deserialize<ServerAvatar>(data, index, count),
                AresId.MSG_CHAT_SERVER_PERSONAL_MESSAGE => Serializer.Deserialize<ServerPersonal>(data, index, count),
                AresId.MSG_CHAT_SERVER_OPCHANGE => Serializer.Deserialize<OpChange>(data, index, count),
                AresId.MSG_CHAT_SERVER_UPDATE_USER_STATUS => Serializer.Deserialize<ServerUpdate>(data, index, count),
                AresId.MSG_CHAT_SERVER_PUBLIC => Serializer.Deserialize<ServerPublic>(data, index, count),
                AresId.MSG_CHAT_SERVER_EMOTE => Serializer.Deserialize<ServerEmote>(data, index, count),
                AresId.MSG_CHAT_SERVER_PVT => Serializer.Deserialize<Private>(data, index, count),
                AresId.MSG_CHAT_SERVER_URL => Serializer.Deserialize<ServerUrl>(data, index, count),
                AresId.MSG_CHAT_SERVER_OFFLINEUSER => Serializer.Deserialize<Offline>(data,index,count),
                AresId.MSG_CHAT_SERVER_ISIGNORINGYOU => Serializer.Deserialize<IgnoringYou>(data, index, count),
                AresId.MSG_CHAT_SERVER_FASTPING => Serializer.Deserialize<FastPing>(data, index, count),
                AresId.MSG_CHAT_CLIENT_CUSTOM_DATA => Serializer.Deserialize<ClientCustom>(data, index, count),
                _ => new Unknown(id, data.Skip(index).Take(count).ToArray()),
            };
        }

        protected virtual string FormatBasic(IPacket packet)
        {
            switch((AresId)packet.Id) {
                case AresId.MSG_CHAT_CLIENT_LOGIN:
                    var login = (Login)packet;
                    return string.Format(
                            "LOGIN:4,32,{0},{1},{2}:2000{3}{4}{5}{6}", 
                            login.Username.Length,
                            8,
                            login.Version.Length,
                            login.Guid.ToString("N"),
                            login.Username,
                            "Internet",
                            login.Version
                        );
                case AresId.MSG_CHAT_CLIENT_DUMMY:
                case AresId.MSG_CHAT_CLIENT_FASTPING:
                case AresId.MSG_CHAT_CLIENT_UPDATE_STATUS:
                    return string.Format("PING:");
                case AresId.MSG_CHAT_CLIENT_PUBLIC:
                    return string.Format("PUBLIC:{0}", ((ClientPublic)packet).Message);
                case AresId.MSG_CHAT_CLIENT_EMOTE:
                    return string.Format("EMOTE:{0}", ((ClientEmote)packet).Message);
                case AresId.MSG_CHAT_CLIENT_COMMAND:
                    return string.Format("COMMAND:{0}", ((Command)packet).Message);
                case AresId.MSG_CHAT_CLIENT_PVT:
                    var priv = (Private)packet;
                    return string.Format("PM:{0},{1}:{2}{3}", priv.Username.Length, priv.Message.Length, priv.Username, priv.Message);
                case AresId.MSG_CHAT_CLIENT_IGNORELIST:
                    var ignore = (Ignored)packet;
                    return string.Format("IGNORE:{0},1:{1}{2}", ignore.Username.Length, ignore.Username, ignore.Ignore ? "1" : "0");
            }

            return null;
        }

        protected virtual IPacket UnformatBasic(string message, string content)
        {
            string[] values;
            switch(message.ToUpper()) {
                case "ACK":
                    values = Parseib0tMessage(content);
                    return new LoginAck() { Username = values[0] };
                case "USERLIST":
                    values = Parseib0tMessage(content);
                    return new Userlist() {
                        Username = values[0],
                        Level = (AdminLevel)byte.Parse(values[1])
                    };
                case "USERLIST_END":
                    return new UserlistEnd();
                case "TOPIC_FIRST":
                    values = Parseib0tMessage(content);
                    return new TopicFirst(values[0]);
                case "TOPIC":
                    values = Parseib0tMessage(content);
                    return new Topic(values[0]);
                case "AVATAR":
                    values = Parseib0tMessage(content);
                    return new ServerAvatar(values[0], Convert.FromBase64String(values[1]));
                case "PERSMSG":
                    values = Parseib0tMessage(content);
                    return new ServerPersonal(values[0], values[1]);
                case "JOIN":
                    values = Parseib0tMessage(content);
                    return new Join() {
                        Username = values[0],
                        Level = (AdminLevel)byte.Parse(values[1])
                    };
                case "PART":
                    values = Parseib0tMessage(content);
                    return new Parted(values[0]);
                case "UPDATE":
                    values = Parseib0tMessage(content);
                    return new ServerUpdate() {
                        Username = values[0],
                        Level = (AdminLevel)byte.Parse(values[1])
                    };
                case "NOSUCH":
                    values = Parseib0tMessage(content);
                    return new Announce(values[0]);
                case "PING":
                    return new FastPing();
                case "PUBLIC":
                    values = Parseib0tMessage(content);
                    return new ServerPublic(values[0], values[1]);
                case "EMOTE":
                    values = Parseib0tMessage(content);
                    return new ServerEmote(values[0], values[1]);
                case "PM":
                    values = Parseib0tMessage(content);
                    return new Private(values[0], values[1]);
                case "OFFLINE":
                    values = Parseib0tMessage(content);
                    return new Offline(values[0]);
                case "IGNORING":
                    values = Parseib0tMessage(content);
                    return new IgnoringYou(values[0]);
                case "URL":
                    values = Parseib0tMessage(content);
                    return new ServerUrl(values[0], values[1]);
            }

            return null;
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.WebSockets;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;

namespace Zorbo.Ares.Packets.Formatters
{
    /// <summary>
    /// A SERVER-side implementation of PacketFormatter&lt;PacketSerializer&gt; used for formatting packets to and from the client.
    /// </summary>
    public class ClientFormatter : PacketFormatter<PacketSerializer>
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
            else {
                return ((AresId)id) switch
                {
                    AresId.MSG_CHAT_CLIENT_LOGIN => Json.Deserialize<Login>(data),
                    AresId.MSG_CHAT_CLIENT_UPDATE_STATUS => Json.Deserialize<ClientUpdate>(data),
                    AresId.MSG_CHAT_CLIENT_PVT => Json.Deserialize<Private>(data),
                    AresId.MSG_CHAT_CLIENT_PUBLIC => Json.Deserialize<ClientPublic>(data),
                    AresId.MSG_CHAT_CLIENT_EMOTE => Json.Deserialize<ClientEmote>(data),
                    AresId.MSG_CHAT_CLIENT_FASTPING => Json.Deserialize<ClientFastPing>(data),
                    AresId.MSG_CHAT_CLIENT_COMMAND => Json.Deserialize<Command>(data),
                    AresId.MSG_CHAT_CLIENT_AUTHREGISTER => Json.Deserialize<AuthRegister>(data),
                    AresId.MSG_CHAT_CLIENT_AUTHLOGIN => Json.Deserialize<AuthLogin>(data),
                    AresId.MSG_CHAT_CLIENT_AUTOLOGIN => Json.Deserialize<AutoLogin>(data),
                    AresId.MSG_CHAT_CLIENT_AVATAR => Json.Deserialize<ClientAvatar>(data),
                    AresId.MSG_CHAT_CLIENT_PERSONAL_MESSAGE => Json.Deserialize<ClientPersonal>(data),
                    AresId.MSG_CHAT_CLIENT_ADDSHARE => Json.Deserialize<SharedFile>(data),
                    AresId.MSG_CHAT_CLIENT_SEARCH => Json.Deserialize<Search>(data),
                    AresId.MSG_CHAT_CLIENT_BROWSE => Json.Deserialize<Browse>(data),
                    AresId.MSG_CHAT_CLIENT_DIRCHATPUSH => Json.Deserialize<ClientDirectPush>(data),
                    AresId.MSG_CHAT_CLIENT_SEND_SUPERNODES => Json.Deserialize<ClientNodes>(data),
                    AresId.MSG_CHAT_CLIENT_IGNORELIST => Json.Deserialize<Ignored>(data),
                    AresId.MSG_CHAT_CLIENT_CUSTOM_DATA => Json.Deserialize<ClientCustom>(data),
                    AresId.MSG_CHAT_CLIENT_CUSTOM_DATA_ALL => Json.Deserialize<ClientCustomAll>(data),
                    AresId.MSG_CHAT_CLIENTCOMPRESSED => Json.Deserialize<Compressed>(data),
                    _ => new UnknownJson(id, data),
                };
            }
        }

        public override IPacket Unformat(byte id, byte[] data, int index, int count)
        {
            return ((AresId)id) switch
            {
                AresId.MSG_CHAT_CLIENT_LOGIN => Serializer.Deserialize<Login>(data, index, count),
                AresId.MSG_CHAT_CLIENT_UPDATE_STATUS => Serializer.Deserialize<ClientUpdate>(data, index, count),
                AresId.MSG_CHAT_CLIENT_PVT => Serializer.Deserialize<Private>(data, index, count),
                AresId.MSG_CHAT_CLIENT_PUBLIC => Serializer.Deserialize<ClientPublic>(data, index, count),
                AresId.MSG_CHAT_CLIENT_EMOTE => Serializer.Deserialize<ClientEmote>(data, index, count),
                AresId.MSG_CHAT_CLIENT_FASTPING => Serializer.Deserialize<ClientFastPing>(data, index, count),
                AresId.MSG_CHAT_CLIENT_COMMAND => Serializer.Deserialize<Command>(data, index, count),
                AresId.MSG_CHAT_CLIENT_AUTHREGISTER => Serializer.Deserialize<AuthRegister>(data, index, count),
                AresId.MSG_CHAT_CLIENT_AUTHLOGIN => Serializer.Deserialize<AuthLogin>(data, index, count),
                AresId.MSG_CHAT_CLIENT_AUTOLOGIN => Serializer.Deserialize<AutoLogin>(data, index, count),
                AresId.MSG_CHAT_CLIENT_AVATAR => Serializer.Deserialize<ClientAvatar>(data, index, count),
                AresId.MSG_CHAT_CLIENT_PERSONAL_MESSAGE => Serializer.Deserialize<ClientPersonal>(data, index, count),
                AresId.MSG_CHAT_CLIENT_ADDSHARE => Serializer.Deserialize<SharedFile>(data, index, count),
                AresId.MSG_CHAT_CLIENT_SEARCH => Serializer.Deserialize<Search>(data, index, count),
                AresId.MSG_CHAT_CLIENT_BROWSE => Serializer.Deserialize<Browse>(data, index, count),
                AresId.MSG_CHAT_CLIENT_DIRCHATPUSH => Serializer.Deserialize<ClientDirectPush>(data, index, count),
                AresId.MSG_CHAT_CLIENT_SEND_SUPERNODES => Serializer.Deserialize<ClientNodes>(data, index, count),
                AresId.MSG_CHAT_CLIENT_IGNORELIST => Serializer.Deserialize<Ignored>(data, index, count),
                AresId.MSG_CHAT_CLIENT_CUSTOM_DATA => Serializer.Deserialize<ClientCustom>(data, index, count),
                AresId.MSG_CHAT_CLIENT_CUSTOM_DATA_ALL => Serializer.Deserialize<ClientCustomAll>(data, index, count),
                AresId.MSG_CHAT_CLIENTCOMPRESSED => Serializer.Deserialize<Compressed>(data, index, count),
                _ => new Unknown(id, data.Skip(index).Take(count).ToArray()),
            };
        }

        protected virtual string FormatBasic(IPacket packet)
        {
            switch ((AresId)packet.Id) {
                case AresId.MSG_CHAT_SERVER_LOGIN_ACK:
                    var ack = (LoginAck)packet;
                    return string.Format("ACK:{0}:{1}", ack.Username.Length, ack.Username);
                case AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST:
                    var ul = (Userlist)packet;
                    return string.Format("USERLIST:{0},1:{1}{2}", ul.Username.Length, ul.Username, (byte)ul.Level);
                case AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST_END:
                    return "USERLIST_END:";
                case AresId.MSG_CHAT_SERVER_TOPIC:
                    var top = (TopicBase)packet;
                    return string.Format("TOPIC:{0}:{1}", top.Message.Length, top.Message);
                case AresId.MSG_CHAT_SERVER_TOPIC_FIRST:
                    top = (TopicBase)packet;
                    return string.Format("TOPIC_FIRST:{0}:{1}", top.Message.Length, top.Message);
                case AresId.MSG_CHAT_SERVER_AVATAR:
                    var ava = (ServerAvatar)packet;
                    if (ava.AvatarBytes.Length == 0)
                        return string.Format("AVATAR:{0}:{1}", ava.Username.Length, ava.Username);
                    else {
                        string avstring = Convert.ToBase64String(ava.AvatarBytes);
                        return string.Format("AVATAR:{0},{1}:{2}{3}", ava.Username.Length, avstring.Length, ava.Username, avstring);
                    }
                case AresId.MSG_CHAT_SERVER_PERSONAL_MESSAGE:
                    var pmsg = (ServerPersonal)packet;
                    return string.Format("PERSMSG:{0},{1}:{2}{3}", pmsg.Username.Length, pmsg.Message.Length, pmsg.Username, pmsg.Message);
                case AresId.MSG_CHAT_SERVER_JOIN:
                    var join = (Join)packet;
                    return string.Format("JOIN:{0},1:{1}{2}", join.Username.Length, join.Username, (byte)join.Level);
                case AresId.MSG_CHAT_SERVER_UPDATE_USER_STATUS:
                    var up = (ServerUpdate)packet;
                    return string.Format("UPDATE:{0},1:{1}{2}", up.Username.Length, up.Username, (byte)up.Level);
                case AresId.MSG_CHAT_SERVER_PART:
                    var part = (Parted)packet;
                    return string.Format("PART:{0}:{1}", part.Username.Length, part.Username);
                case AresId.MSG_CHAT_SERVER_NOSUCH:
                    var annouce = (Announce)packet;
                    return string.Format("NOSUCH:{0}:{1}", annouce.Message.Length, annouce.Message);
                case AresId.MSG_CHAT_SERVER_PUBLIC:
                    var pub = (ServerPublic)packet;
                    return string.Format("PUBLIC:{0},{1}:{2}{3}", pub.Username.Length, pub.Message.Length, pub.Username, pub.Message);
                case AresId.MSG_CHAT_SERVER_EMOTE:
                    var emo = (ServerEmote)packet;
                    return string.Format("EMOTE:{0},{1}:{2}{3}", emo.Username.Length, emo.Message.Length, emo.Username, emo.Message);
                case AresId.MSG_CHAT_SERVER_PVT:
                    var pvt = (Private)packet;
                    return string.Format("PM:{0},{1}:{2}{3}", pvt.Username.Length, pvt.Message.Length, pvt.Username, pvt.Message);
                case AresId.MSG_CHAT_SERVER_OFFLINEUSER:
                    var offline = (Offline)packet;
                    return string.Format("OFFLINE:{0}:{1}", offline.Username.Length, offline.Username);
                case AresId.MSG_CHAT_SERVER_ISIGNORINGYOU:
                    var ignored = (IgnoringYou)packet;
                    return string.Format("IGNORING:{0}:{1}", ignored.Username.Length, ignored.Username);
                case AresId.MSG_CHAT_SERVER_URL:
                    var url = (Website)packet;
                    return string.Format("URL:{0},{1}:{2}{3}", url.Address.Length, url.Caption.Length, url.Address, url.Caption);
            }

            return null;
        }

        protected virtual IPacket UnformatBasic(string message, string content)
        {
            string[] values;
            switch (message.ToUpper()) {
                case "LOGIN": {
                    values = Parseib0tMessage(content);

                    Login login = new Login();
                    byte[] guid = new byte[16];

                    for (int i = 0; i < 16; i++)
                        guid[i] = byte.Parse(values[1].Substring((i * 2), 2), NumberStyles.HexNumber);

                    login.Guid = new Guid(Utils.MD5.ComputeHash(guid));
                    login.Username = values[2].Trim();
                    login.Version = string.Format("{1} [{0}]", values[3], values[4]);
                    //login.Extended = int.Parse(values[0]) >= 2000;
                    return login;
                }
                case "PUBLIC":
                    return new ClientPublic { Message = content };
                case "EMOTE":
                    return new ClientEmote { Message = content };
                case "COMMAND":
                    return new Command { Message = content };
                case "PING":
                    return new PingPongPacket { IsPing = true };
                case "PM":
                    values = Parseib0tMessage(content);
                    return new Private(values[0], values[1]);
                case "IGNORE":
                    values = Parseib0tMessage(content);
                    return new Ignored {
                        Username = values[0],
                        Ignore = int.Parse(content.Substring(content.Length - 1)) == 1
                    };
            }

            return null;
        }
    }
}

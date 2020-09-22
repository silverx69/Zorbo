using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Ares.Packets.Chatroom
{
    public class Userlist : JoinBase
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST; }
        }

        public Userlist() { }

        public Userlist(IClient user) : base(user) { }
    }

    public class UserlistEnd : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST_END; }
        }

        [JsonIgnore]
        [PacketItem(0)]
        public byte Null {
            get { return 0; }
            set { }
        }
    }

    public class UserlistClear : AresPacket
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_CHANNEL_USERLIST_CLEAR; }
        }

        [JsonIgnore]
        [PacketItem(0)]
        public byte Null {
            get { return 0; }
            set { }
        }
    }
}

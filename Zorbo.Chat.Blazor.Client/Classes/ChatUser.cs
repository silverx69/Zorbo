using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Zorbo.Ares;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;

namespace Zorbo.Chat.Blazor.Client.Classes
{
    public class ChatJoin : ChatUser
    {
        public override AresId Id {
            get { return AresId.MSG_CHAT_SERVER_JOIN; }
        }
    }

    public class ChatUser : JoinBase, INotifyPropertyChanged
    {
        bool isWriting = false;
        DateTime lastWrite = DateTime.MinValue;

        string base64avatar = "";
        string personalMsg = "";
        byte[] avatar = null;

        public override AresId Id { 
            get { return  AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST; } 
        }

        public bool IsWriting {
            get { return isWriting; }
            set { OnPropertyChanged(() => isWriting, value); }
        }

        public DateTime LastWrite {
            get { return lastWrite; }
            set { OnPropertyChanged(() => lastWrite, value); }
        }

        [JsonIgnore]
        public byte[] Avatar {
            get { return avatar; }
            set {
                OnPropertyChanged(() => avatar, value);
                AvatarBase64 = Compute();
            }
        }

        [JsonIgnore]
        public string AvatarBase64 {
            get { return base64avatar; }
            set { OnPropertyChanged(() => base64avatar, value); }
        }

        [JsonIgnore]
        public string PersonalMessage {
            get { return personalMsg; }
            set { OnPropertyChanged(() => personalMsg, value); }
        } 


        private string Compute() {
            return Convert.ToBase64String(Avatar);
        }
    }
}

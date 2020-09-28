using cb0tProtocol;
using cb0tProtocol.Packets;
using cb0tProtocol.Packets.ib0t;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Chat.Blazor.Shared;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Models;

namespace Zorbo.Chat.Blazor.Client.Classes
{
    public class ChatInstance : ModelBase, IDisposable
    {
        Uri baseUri = null;

        string topic = "";
        string myname = "";
        string connString = "";

        int newMessages = 0;

        bool isWriting = false;
        bool isShowingUsers = true;

        RoomScribble scribble = null;

        ClientWebSocket socket = null;
        ChatFormatter formatter = null;
        ServerRecord channel = null;

        DateTime lastWrite = DateTime.MinValue;
        DateTime lastUpdate = DateTime.MinValue;
        DateTime lastConnect = DateTime.MinValue;

        volatile bool isSending = false;
        ConcurrentQueue<IPacket> outPackets;

        WebSocketMessageType prefMsgType = WebSocketMessageType.Binary;

        byte[] recv_buffer = new byte[8192];

        readonly ChatClient Client = ChatClient.Self;


        public bool IsConnected {
            get { return socket != null && socket.State == WebSocketState.Open; }
        }

        public bool IsWriting {
            get { return isWriting; }
            set { OnPropertyChanged(() => isWriting, value); }
        }

        public bool IsShowingUsers {
            get { return isShowingUsers; }
            set { OnPropertyChanged(() => isShowingUsers, value); } 
        }

        public ServerRecord Channel {
            get { return channel; }
            set { OnPropertyChanged(() => channel, value); } 
        }

        public string Topic {
            get { return topic; }
            set { OnPropertyChanged(() => topic, value); }
        }

        public string MyDisplayName {
            get { return myname; }
            set { OnPropertyChanged(() => myname, value); }
        }

        public DateTime LastUpdate {
            get { return lastUpdate; }
            set { OnPropertyChanged(() => lastUpdate, value); }
        }

        public DateTime LastConnectAttempt {
            get { return lastConnect; }
            set { OnPropertyChanged(() => lastConnect, value); }
        }

        public int NewMessages {
            get { return newMessages; }
            set { OnPropertyChanged(() => newMessages, value); }
        }


        public ModelList<ChatUser> Users { get; set; }

        public ModelList<ChatMessage> Messages { get; set; }

        public IEnumerable<string> UsersWriting {
            get {
                foreach (var user in Users)
                    if (user.IsWriting) yield return user.Username;
            }
        }


        private ChatFormatter Formatter {
            get { return formatter ??= new ChatFormatter(); }
        }


        public ChatInstance(ServerRecord channel, Uri baseUri)
        {
            Channel = channel;
            this.baseUri = baseUri;
            lastConnect = DateTime.Now;
            LastUpdate = DateTime.Now;
            Users = new ModelList<ChatUser>();
            Users.CollectionChanged += OnUsersChanged;
            Messages = new ModelList<ChatMessage>();
            Messages.CollectionChanged += OnMessagesChanged;
            scribble = new RoomScribble();
            outPackets = new ConcurrentQueue<IPacket>();
            if (!Channel.SupportJson) // use ib0t proto
                prefMsgType = WebSocketMessageType.Text;
        }


        public async Task CheckUpdate(DateTime now)
        {
            if (now.Subtract(lastWrite).TotalSeconds >= 3)
                await FinishedWriting();

            bool changed = false;
            
            Users.Where(s => s.IsWriting)
                 .ForEach(s => {
                     if (now.Subtract(s.LastWrite).TotalSeconds >= 10) {
                         changed = true;
                         s.IsWriting = false;
                     }
                 });

            if (changed) 
                RaisePropertyChanged(nameof(UsersWriting));

            if (now.Subtract(LastUpdate).TotalMinutes >= (Channel.SupportJson ? 3d : 1.25d)) {
                LastUpdate = now;
                await SendAsync(new ClientUpdate() {
                    Country = Country.Canada,
                    Gender = Gender.Male,
                    SupportFlag = (SupportFlags)7
                });
            }
        }

        public void AddMessage(IPacket message)
        {
            Messages.Add(new ChatMessage(message));
        }


        public async Task ConnectAsync()
        {
            if (socket != null) return;

            LastConnectAttempt = DateTime.Now;
            AddMessage(new Error("Connecting, please wait..."));

            var myip = await Client.GetAddress(baseUri);
            string toip = channel.ExternalIp;

            //temporary hard-coding so I can get into my own rooms.
            //if (myip.IsLocalAreaNetwork() && channel.ExternalIp == "108.168.8.37") {
            //    toip = myip.ToString();
            //}

            //connect via the channels local address?
            if (myip.ToString() == channel.ExternalIp && (!string.IsNullOrEmpty(channel.InternalIp) && channel.InternalIp != "0.0.0.0"))
                toip = channel.InternalIp;
            
            //Zorbo's chat server listens on ConfiguredPort + 1 for TLS
            bool secure = baseUri.Scheme.ToLower() == "https";

            connString = string.Format("{0}://{1}:{2}/", secure ? "wss" : "ws", toip, secure ? (Channel.Port + 1) : Channel.Port);

            try {
                socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri(connString), CancellationToken.None);
            }
            catch (Exception e) {
                OnException(e);
            }

            if (IsConnected) {
                LastUpdate = DateTime.Now;
                AddMessage(new Error("Connected, handshaking..."));

                await SendAsync(new Login() {
                    Guid = Guid.NewGuid(),
                    Username = ChatClient.Self.Username,
                    Version = "Zorbo Client 1.0a",
                    SupportFlag = (SupportFlags)7,
                    Gender = Gender.Unknown,
                    Country = Country.Unknown
                });

                await SendAsync(new ClientPersonal() {
                    Message = "Zorbo Web User"
                });

                ReceiveLoop();
            }
            else
                AddMessage(new Announce("Unable to connect."));
        }

        public async Task ReconnectAsync()
        {
            if (IsConnected)
                await DisconnectAsync();

            AddMessage(new Announce("Reconnecting..."));

            await ConnectAsync();
        }


        public async Task StartedWriting()
        {
            if (!isWriting) {
                isWriting = true;
                await SendAsync(new ClientCustomAll("cb0t_writing", new byte[] { 2 }));
            }
            lastWrite = DateTime.Now;
        }

        public async Task FinishedWriting()
        {
            if (isWriting) {
                isWriting = false;
                await SendAsync(new ClientCustomAll("cb0t_writing", new byte[] { 1 }));
            }
        }

        public async Task SendAsync(IPacket packet)
        {
            if (!IsConnected) return;

            if (isSending)
                outPackets.Enqueue(packet);
            else {
                isSending = true;
                try {
                    do {
                        byte[] tmp = null;

                        switch (prefMsgType) {
                            case WebSocketMessageType.Text:
                                tmp = Formatter.FormatJson(packet, !channel.SupportJson);
                                break;
                            case WebSocketMessageType.Binary:
                                tmp = Formatter.Format(packet);
                                tmp = tmp.Skip(2).ToArray();//dont need len for websocket
                                break;
                        }
                        //formatter could return null for unsupported or unknown packets
                        if (tmp != null && tmp.Length > 0)
                            await socket.SendAsync(tmp, prefMsgType, true, CancellationToken.None);
                    }
                    while (outPackets.TryDequeue(out packet));
                }
                catch (Exception e) {
                    OnException(e);
                }
                isSending = false;
            }
        }


        private async void ReceiveLoop()
        {
            var result = await socket.ReceiveAsync(recv_buffer, CancellationToken.None);

            if (!IsConnected) {
                await DisconnectAsync();
            }
            else if (result.Count > 0) {
                try {
                    if (result.EndOfMessage) {//for now we're not receiving multi-parts
                        IPacket packet = null;

                        switch (result.MessageType) {
                            case WebSocketMessageType.Text: {
                                int id = 0;

                                using var reader = new PacketReader(recv_buffer, 0, result.Count);
                                string tmp = reader.ReadString();

                                if (channel.SupportJson)
                                    id = tmp.ExtractId();

                                packet = Formatter.Unformat((byte)id, tmp, !channel.SupportJson);
                                break;
                            }
                            case WebSocketMessageType.Binary:
                                packet = Formatter.Unformat(recv_buffer[0], recv_buffer, 1, result.Count - 1);
                                break;
                            case WebSocketMessageType.Close:
                                await DisconnectAsync(WebSocketCloseStatus.NormalClosure);
                                break;
                        }

                        if (packet != null) OnPacketReceived(packet);
                        if (IsConnected) ReceiveLoop();
                    }
                }
                catch (Exception e) {
                    OnException(e);
                }
            }
            else {
                Console.WriteLine("Received nothing from socket. Trying again...");
                if (IsConnected) ReceiveLoop();
            }
        }


        private void OnPacketReceived(IPacket packet)
        {
            switch ((AresId)packet.Id) {
                case AresId.MSG_CHAT_SERVER_LOGIN_ACK:
                    var ack = (LoginAck)packet;
                    MyDisplayName = ack.Username;
                    Users.Clear();
                    AddMessage(new Error("Logged in, retrieving userlist..."));
                    break;
                case AresId.MSG_CHAT_SERVER_MYFEATURES:
                    AddMessage(packet);
                    break;
                case AresId.MSG_CHAT_SERVER_TOPIC_FIRST:
                    Topic = ((TopicBase)packet).Message;
                    break;
                case AresId.MSG_CHAT_SERVER_TOPIC:
                    Topic = ((TopicBase)packet).Message;
                    AddMessage(packet);
                    break;
                case AresId.MSG_CHAT_SERVER_JOIN:
                    Users.Add((ChatUser)packet);
                    AddMessage(packet);
                    break;
                case AresId.MSG_CHAT_SERVER_PART:
                    var part = (Parted)packet;
                    var user = Users.Find(s => s.Username == part.Username);
                    if (user != null) {
                        Users.Remove(user);
                        AddMessage(part);
                        if (user.IsWriting) {
                            user.IsWriting = false;
                            RaisePropertyChanged(nameof(UsersWriting));
                        }
                    }
                    break;
                case AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST:
                    Users.Add((ChatUser)packet);
                    break;
                case AresId.MSG_CHAT_SERVER_CHANNEL_USER_LIST_END:
                    //.... don't need this atm, this was 
                    //used for win32/gdi window flicker issues I'm pretty sure
                    break;
                case AresId.MSG_CHAT_SERVER_NOSUCH:
                    AddMessage(packet);
                    break;
                case AresId.MSG_CHAT_SERVER_PUBLIC:
                case AresId.MSG_CHAT_SERVER_EMOTE:
                    NewMessages++;
                    AddMessage(packet);
                    break;
                case AresId.MSG_CHAT_SERVER_UPDATE_USER_STATUS:
                    var upd = (ServerUpdate)packet;
                    Users.Where(s => s.Username == upd.Username)
                         .ForEach(s => s.Level = upd.Level);
                    break;
                case AresId.MSG_CHAT_SERVER_PERSONAL_MESSAGE: {
                    var msg = (ServerPersonal)packet;
                    Users.Where(s => s.Username == msg.Username)
                         .ForEach(s => s.PersonalMessage = msg.Message);
                    break;
                }
                case AresId.MSG_CHAT_SERVER_AVATAR: {
                    var avatar = (ServerAvatar)packet;
                    Users.Where(s => s.Username == avatar.Username)
                         .ForEach(s => s.Avatar = avatar.AvatarBytes);
                    break;
                }
                case AresId.MSG_CHAT_SERVER_CUSTOM_DATA: {
                    var custom = (ClientCustom)packet;
                    switch(custom.CustomId) {
                        case "cb0t_writing":
                            bool isWriting = custom.Data[0] == 2;
                            Users.Where(s => s.Username == custom.Username)
                                 .ForEach(s => { 
                                     s.IsWriting = isWriting;
                                     if (isWriting) s.LastWrite = DateTime.Now;
                                 });
                            RaisePropertyChanged(nameof(UsersWriting));
                            break;
                        //cb0t scribbles
                        case "cb0t_scribble_once":
                            scribble.Reset();
                            scribble.Chunks = 1;
                            scribble.Write(custom.Data);
                            break;
                        case "cb0t_scribble_first":
                            scribble.Reset();
                            scribble.Write(custom.Data);
                            break;
                        case "cb0t_scribble_chunk":
                            scribble.Write(custom.Data);
                            break;
                        case "cb0t_scribble_last":
                            scribble.Write(custom.Data);
                            OnRoomScribbleComplete(scribble);
                            break;
                    }
                    break;
                }
                default://ib0t room scribbles
                    if (packet.Id == 0) {
                        if (packet is ScribbleHead head) {
                            scribble.Reset();
                            scribble.Username = head.Name;
                            scribble.Chunks = (ushort)head.BlockCount;
                        }
                        else if (packet is ScribbleBlock block) {
                            scribble.Write(Convert.FromBase64String(block.Block));
                            if (scribble.IsComplete) {
                                AddMessage(new Announce(string.Format("\x000314--- From {0}", scribble.Username)));
                                OnRoomScribbleComplete(scribble);
                            }
                        }
                    }
                    break;
            }
            //room scribbles
            if (packet.Id == (byte)AdvancedId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL) {
                if (((Advanced)packet).Payload is AdvancedPacket advanced) {
                    switch (advanced.Id) {
                        case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_FIRST:
                            var p = (ClientScribbleFirst)advanced;
                            OnScribbleFirst((ClientScribbleFirst)advanced);
                            break;
                        case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_CHUNK:
                            OnScribbleChunk((ClientScribbleChunk)advanced);
                            break;
                    }
                }
            }
        }


        private void OnScribbleFirst(ClientScribbleFirst first)
        {
            scribble.Reset();
            scribble.Size = first.Size;
            scribble.Chunks = (ushort)(first.Chunks + 1);//scribble object counts first chunk
            scribble.Write(first.Data);
            if (scribble.IsComplete) 
                OnRoomScribbleComplete(scribble);
        }

        private void OnScribbleChunk(ClientScribbleChunk chunk)
        {
            scribble.Write(chunk.Data);
            if (scribble.IsComplete) 
                OnRoomScribbleComplete(scribble);
        }

        //Used for Sending scribble objects from another client
        //
        private void OnRoomScribbleComplete(RoomScribble scribble)
        {
            AddMessage(new ChatScribble(Convert.ToBase64String(Zlib.Decompress(scribble.RawImage()))));
        }

        private void OnUsersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Users));
        }

        private void OnMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Messages));
        }


        private async void OnException (Exception ex)
        {
            AddMessage(new Announce("An error has occured in the client."));
#if DEBUG
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null) {
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.InnerException.StackTrace);
            }
#endif
            await DisconnectAsync();
        }


        public Task DisconnectAsync(bool closing = false)
        {
            return DisconnectAsync(WebSocketCloseStatus.NormalClosure, closing: closing);
        }

        public async Task DisconnectAsync(WebSocketCloseStatus status, string message = "", bool closing = false)
        {
            if (socket == null) return;

            try {
                LastConnectAttempt = DateTime.Now;
                await socket.CloseAsync(status, message, CancellationToken.None);
            }
            catch { }

            OnDisconnectComplete(closing);
        }

        private void OnDisconnectComplete(bool closing = false)
        {
            if (!closing)
                AddMessage(new Announce("Disconnected."));

            try { socket.Dispose(); }
            catch { }

            socket = null;
            isSending = false;
        }

        public void Dispose()
        {
            Users.CollectionChanged -= OnUsersChanged;
            Messages.CollectionChanged -= OnMessagesChanged;
            try { socket?.Dispose(); }
            catch { }
            socket = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Zorbo.Ares.Formatters;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.Formatters;
using Zorbo.Ares.Packets.WebSockets;
using Zorbo.Ares.Resources;
using Zorbo.Ares.Sockets;
using Zorbo.Core;
using Zorbo.Core.Collections;
using Zorbo.Core.Models;
using Zorbo.Core.Plugins.Server;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Server
{
    public class AresServer : ModelBase, IServer
    {
#pragma warning disable IDE0044 // Add readonly modifier
        bool running = false;

        Timer timer;
        TimeSpan ticklength;

        AresTcpSocket listener;
        AresTcpSocket tlslistener;

        IPAddress externalip;
        IEnumerable<IPAddress> localAddresses;

        SortedStack<ushort> idpool;

        List<PendingConnection> pending;
        ModelList<IFloodRule> flood_rules;

        ServerPluginHost plugins = null;
#pragma warning restore IDE0044 // Add readonly modifier

        static readonly Comparison<IClient> sorter = (a, b) => (a.Id - b.Id);

        class PendingConnection : IDisposable
        {
            public AresTcpSocket Socket { get; set; }
            public DateTime Time { get; set; }

            public PendingConnection() { }

            public PendingConnection(AresTcpSocket socket, DateTime time) {
                this.Socket = socket;
                this.Time = time;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                else if (obj is PendingConnection conn)
                    return Socket == conn?.Socket;
                else if (obj is AresTcpSocket sock)
                    return ReferenceEquals(Socket, sock);
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Socket);
            }

            public void Dispose() {
                if (Socket != null)
                    Socket.Disconnect();
            }
        }

        public ISocket Socket {
            get { return listener; }
        }

        public ISocket TlsSocket {
            get { return tlslistener; }
        }

        public bool Running {
            get { return running; }
            set { OnPropertyChanged(() => running, value); }
        }

        public IPAddress ExternalIp {
            get { return externalip; }
            internal set {
                if (externalip == null && value != null && !Equals(externalip, value)) {
                    externalip = value;
                    OnPropertyChanged();
                    Logging.Info("AresServer", "Reported external ip: {0}", value);
                }
            }
        }

        public IPAddress InternalIp {
            get {
                if (Config.LocalIp.IsValidInternalIp())
                    return Config.LocalIp;
                return LocalAddresses
                        .Where(s => s.AddressFamily == AddressFamily.InterNetwork)
                        .FirstOrDefault();
            }
        }

        public IEnumerable<IPAddress> LocalAddresses {
            get { return localAddresses; }
            internal set {
                if (localAddresses == null && value != null && !Equals(localAddresses, value)) {
                    localAddresses = value;
                    OnPropertyChanged();
                }
            }
        }


        public AresServerStats Stats { get; }

        IServerStats IServer.Stats {
            get { return Stats; }
        }


        public AresServerConfig Config { get; }

        IServerConfig IServer.Config {
            get { return Config; }
        }


        public AresChannels Channels { get; }

        IChannelList IServer.Channels {
            get { return Channels; }
        }


        public AresUserHistory History { get; }

        IHistory IServer.History {
            get { return History; }
        }


        public ServerPluginHost PluginHost {
            get {
                if (plugins == null)
                    plugins = new ServerPluginHost(this);
                return plugins;
            }
            set { OnPropertyChanged(() => plugins, value); }
        }

        IServerPluginHost IServer.PluginHost {
            get { return PluginHost; }
        }


        public ModelList<IClient> Users { get; }

        IObservableCollection<IClient> IServer.Users {
            get { return Users; }
        }


        public IObservableCollection<IFloodRule> FloodRules {
            get { return flood_rules; }
        }

        public AresServer(AresServerConfig config) 
        {
            this.Config = config ?? 
                throw new ArgumentNullException("config", "Server configuration cannot be null.");

            ticklength = TimeSpan.FromSeconds(1);

            Stats = new AresServerStats();
            Users = new ModelList<IClient>();

            Channels = new AresChannels {
                Server = this,
                Channels = Persistence.LoadModel<ModelList<AresChannel>>(Path.Combine(config.Directories.AppData, "channels.json"))
            };

            if (Channels.Channels.Count == 0)
                Channels.LoadFromDatFile();

            idpool = new SortedStack<ushort>();
            idpool.SetSort((a, b) => (a - b));

            pending = new List<PendingConnection>();
            flood_rules = new ModelList<IFloodRule>();

            History = Persistence.LoadModel<AresUserHistory>(Path.Combine(config.Directories.AppData, "history.json"));
            History.Admin.Load(this);
            //History.Admin.

            Logging.WriteLines(
                LogLevel.Info,
                "AresServer",
                new[] {
                    "Chatroom configuration applied.",
                    "Listen Port: {0}",
                    "Using UDP Channels: {1}",
                    "Allow TCP Connections: {2}",
                    "Allow WebSocket Connections: {3}",
                    "Listen for TLS Connections: {4}"
                },
                config.Port,
                config.UseUdpSockets,
                config.UseTcpSockets,
                config.UseWebSockets,
                config.UseTlsSockets);
        }

        public void Start() {
            Stats.Start();

            LocalAddresses = Utils.GetLocalAddresses();

            Channels.PropertyChanged += OnChannelsPropertyChanged;
            Channels.Server = this;
            Channels.Start(Config.Port);

            for (ushort i = 0; i < Config.MaxClients; i++)
                idpool.Push(i);

            Config.PropertyChanged += Config_PropertyChanged;

            if (Config.UseTcpSockets) {
                listener = new AresTcpSocket(new AdvancedClientFormatter());
                listener.Accepted += ClientAccepted;
                listener.Bind(new IPEndPoint(Config.LocalIp, Config.Port));
                listener.Listen(25);
            }

            if (Config.UseTlsSockets) {
                tlslistener = new AresTcpSocket(new AdvancedClientFormatter());
                tlslistener.Accepted += TLSClientAccepted;
                tlslistener.Bind(new IPEndPoint(Config.LocalIp, Config.TlsPort));
                tlslistener.Listen(25);
            }

            Running = true;

            Stats.StartTime = DateTime.Now;
            timer = new Timer(new TimerCallback(OnTimer), null, ticklength, ticklength);

            Logging.Info("AresServer", "Chatroom server started.");
        }

        public void Stop()
        {
            Running = false;

            Logging.Info("AresServer", "Chatroom server shutting down.");

            timer.Change(-1, -1);
            timer.Dispose();

            Config.PropertyChanged -= Config_PropertyChanged;

            listener?.Dispose();
            listener = null;

            tlslistener?.Dispose();
            tlslistener = null;

            idpool.Clear();

            Stats.Reset();

            Channels.PropertyChanged -= OnChannelsPropertyChanged;
            Channels.Stop();
            Channels.Channels.SaveModel(Path.Combine(Config.Directories.AppData, "channels.json"));

            lock (pending) {
                foreach (var sock in pending)
                    sock.Dispose();//suppress disconnect event

                pending.Clear();
            }

            foreach (var user in Users) {
                History.Add(user);
                user.Dispose();//suppress disconnect event
            }
            Users.Clear();

            History.Admin.Save();//admin are saved separate from history
            History.SaveModel(Path.Combine(Config.Directories.AppData, "history.json"));

            Logging.Info("AresServer", "Chatroom server stopped.");
        }


        private void OnTimer(object state)
        {
            DateTime now = DateTime.Now;

            lock (pending) {
                foreach (var sock in pending) {
                    if (now.Subtract(sock.Time).TotalSeconds >= 30)
                        sock.Socket.Disconnect();
                }
            }

            foreach (var user in Users) {
                if (user.IsCaptcha) {
                    if (now.Subtract(user.CaptchaTime).TotalHours >= 2) {
                        Stats.CaptchaBanned++;
                        SendAnnounce(user, Strings.BannedCaptchaTimeout);
                        user.Ban();
                        continue;
                    }
                }
                else if (user.FastPing && now.Subtract(user.LastPing).TotalSeconds >= 10) {
                    user.LastPing = now;
                    user.SendPacket(new FastPing());
                }

                else if (now.Subtract(user.LastUpdate).TotalMinutes >= 5)
                    user.Disconnect();
            }

            if (now.Subtract(Channels.LastSaved).TotalMinutes >= 10) {
                Channels.LastSaved = now;
                Channels.Channels.SaveModel(Path.Combine(Config.Directories.AppData, "channels.json"));
            }

            if (now.Subtract(History.LastSaved).TotalMinutes >= 10) {
                History.LastSaved = now;
                History.Admin.Save();//admin are saved separate from the history
                History.SaveModel(Path.Combine(Config.Directories.AppData, "history.json"));
            }
        }

        private void OnChannelsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AresChannels.ExternalIp))
                ExternalIp = Channels.ExternalIp;
        }

        public void SendUserlist(IClient user)
        {
            Join join = new Join(user);

            if (Config.ShowBot) {
                user.SendPacket(new Userlist() {
                    Username = Config.BotName,
                    FileCount = 420,
                    Level = AdminLevel.Host,
                });
            }

            if (!user.IsCaptcha) {
                foreach (var s in Users) {
                    if (s != user &&
                        s.Connected &&
                        s.LoggedIn &&
                       !s.IsCaptcha &&
                        s.Vroom == user.Vroom) {
                        if (s.CanSee(user)) s.SendPacket(join);
                        if (user.CanSee(s)) user.SendPacket(new Userlist(s));
                    }
                }
            }

            user.SendPacket(new Userlist(user));
            user.SendPacket(new UserlistEnd());
        }

        public void SendAvatars(IClient user)
        {
            if (Config.ShowBot) {
                if (!Config.Avatar.IsEmpty())
                    user.SendPacket(new ServerAvatar(Config.BotName, Config.Avatar));

                user.SendPacket(new ServerPersonal(Config.BotName, Strings.VersionLogin));
            }
            foreach (var s in Users) {

                if (!s.IsCaptcha && s.Vroom == user.Vroom) {
                    if (s.CanSee(user)) {
                        if (!string.IsNullOrEmpty(user.Message))
                            s.SendPacket(new ServerPersonal(user.Name, user.Message));

                        if (!user.Avatar.IsEmpty() || !Config.Avatar.IsEmpty())
                            s.SendPacket(new ServerAvatar(user));
                    }
                    if (user.CanSee(s)) {
                        if (!string.IsNullOrEmpty(s.Message))
                            user.SendPacket(new ServerPersonal(s.Name, s.Message));

                        if (!s.Avatar.IsEmpty() || !Config.Avatar.IsEmpty())
                            user.SendPacket(new ServerAvatar(s));
                    }
                }
            }
        }

        public void SendPacket(IPacket packet) {
                foreach (var user in Users)
                    user.SendPacket((s) => !s.IsCaptcha, packet);
        }

        public void SendPacket(IClient user, IPacket packet) {
            if (user != null) user.SendPacket(packet);
        }

        public void SendPacket(Predicate<IClient> match, IPacket packet) {
                foreach (var user in Users)
                    user.SendPacket(match, packet);
        }

        public IClient FindUser(Predicate<IClient> match)
        {
            foreach (var user in Users)
                if (user.Connected &&
                    user.LoggedIn &&
                    match(user)) return user;

            return null;
        }


        protected virtual void ClientAccepted(object sender, AcceptEventArgs e) 
        {
            if (!(sender is ISocket)) return;

            var socket = (AresTcpSocket)e.Socket;

            if (Channels.TestingFirewall && 
                Channels.IsCheckingMyFirewall(socket.RemoteEndPoint)) {
                socket.Dispose();
            }
            else { 
                socket.Exception += ClientException;
                socket.Disconnected += ClientDisconnected;
                socket.PacketSent += ClientPacketSent;
                socket.PacketReceived += ClientPacketReceived;
                socket.RequestReceived += ClientHttpRequestReceived;
                lock(pending) pending.Add(new PendingConnection(socket, DateTime.Now));
                socket.ReceiveAsync();
            }
        }

        protected virtual void TLSClientAccepted(object sender, AcceptEventArgs e)
        {
            if (!(sender is ISocket)) return;

            var socket = (AresTcpSocket)e.Socket;

            if (Channels.TestingFirewall &&
                Channels.IsCheckingMyFirewall(socket.RemoteEndPoint)) {
                socket.Dispose();
            }
            else if (!string.IsNullOrEmpty(Config.Certificate)) {

                socket.Exception += ClientException;
                socket.Disconnected += ClientDisconnected;
                socket.PacketSent += ClientPacketSent;
                socket.PacketReceived += ClientPacketReceived;
                socket.RequestReceived += ClientHttpRequestReceived;
                lock (pending) pending.Add(new PendingConnection(socket, DateTime.Now));
                //Authenticate TLS as a Server
                socket.AuthenticateAsServer(Config.Certificate, Config.CertificatePassword);
                socket.ReceiveAsync();
            }
            else {
                Logging.Info(
                    "AresServer",
                    "Connection rejected from '{0}'. Chatroom does not have a certificate configured to allow TLS connections.",
                    socket.RemoteEndPoint.Address
                );
                socket.Dispose();
            }
        }

        protected virtual void ClientException(object sender, ExceptionEventArgs e) 
        {
            if (!(sender is ISocket socket)) 
                return;

            Logging.Error("AresServer", socket, e.Exception);

            socket.Disconnect();
        }

        protected virtual void ClientPacketSent(object sender, PacketEventArgs e) 
        {
            if (!(sender is ISocket)) return;

            Stats.PacketsSent++;
            Stats.AddOutput(e.Transferred);

            if (e.Packet is PingPongPacket)
                return;

            IClient user = Users.Find(s => s.Socket == sender);
            if (user != null) PluginHost.OnPacketSent(user, e.Packet);
        }

        protected virtual void ClientPacketReceived(object sender, PacketEventArgs e) 
        {
            if (!(sender is ISocket socket)) return;

            Stats.PacketsReceived++;
            Stats.AddInput(e.Transferred);

            if (e.Packet.Id == (byte)AresId.MSG_CHAT_CLIENT_LOGIN) {

                IClient user = Users.Find(s => s.Socket == socket);

                if (user == null) {
                    if (HandlePending(socket)) {
                        if (idpool.Count == 0) {
                            socket.SendAsync(new Announce(Errors.RoomFull));
                            socket.Disconnect();
                            Logging.Info(
                                "AresServer", 
                                "Chatroom has reached capacity ({0}). If this happens frequently consider raising Max Clients", 
                                Config.MaxClients
                            );
                        }
                        else {
                            int id = idpool.Pop();
                            var client = new AresClient(this, socket, (ushort)id);

                            if (IPAddress.IsLoopback(client.ExternalIp) ||
                                client.ExternalIp.Equals(ExternalIp) ||
                                LocalAddresses.Contains(client.ExternalIp) ||
                               (Config.LocalAreaIsHost && client.ExternalIp.IsLocalAreaNetwork()))
                                client.LocalHost = true;

                            lock (Users) {
                                Users.Add(client);
                                Users.Sort(sorter);
                                Stats.PeakUsers = Math.Max(Users.Count, Stats.PeakUsers);
                            }

                            client.HandleJoin(e);
                        }
                    }
                }
                else {
                    //sending too many login packets
                    SendAnnounce(user, Errors.LoginFlood);
                    Logging.Info("AresServer", "User '{0}' has been disconnected for login flooding.", user.Name);

                    user.Disconnect();
                }
            }
        }

        /// <summary>
        /// AresTcpSocket can also act as a miniature WebServer and can handle HEAD/GET/POST requests
        /// </summary>
        private void ClientHttpRequestReceived(object sender, HttpRequestEventArgs e)
        {
            if (!(sender is ISocket socket)) return;

            Stats.PacketsReceived++;
            Stats.AddInput(e.Transferred);

            PendingConnection connection;
            lock(pending) connection = pending.Find(s => s.Equals(sender));

            if (connection != null) {
                //upgrading to a websocket connection?
                if (e.Headers.TryGetValue("CONNECTION", out string upgrade) &&
                    e.Headers.TryGetValue("UPGRADE", out string upgrade_type)) {

                    if (Config.UseWebSockets) {

                        e.Headers.TryGetValue("ORIGIN", out string origin);
                        e.Headers.TryGetValue("SEC-WEBSOCKET-VERSION", out string version);
                        e.Headers.TryGetValue("SEC-WEBSOCKET-KEY", out string key);

                        var my_headers = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(origin)) {
                            my_headers.Add("Access-Control-Allow-Origin", origin);
                            my_headers.Add("Access-Control-Allow-Credentials", "true");
                            my_headers.Add("Access-Control-Allow-Headers", "content-type");
                        }

                        connection.Socket.IsWebSocket = true;
                        connection.Socket.SendBytes(Http.WebSocketAcceptHeaderBytes(key, my_headers.ToArray()));
                    }
                    else {//websockets disabled
                        Logging.Info(
                            "AresServer",
                            "Connection rejected from '{0}'. Chatroom is not configured to allow WebSocket connections.",
                            socket.RemoteEndPoint.Address
                        );

                        connection.Socket.Disconnect();
                    }
                }
                else if (PluginHost.OnHttpRequest(socket, e)) {
                    Logging.Info(
                        "AresServer",
                        "Http request from '{0}' for resource '{1}' denied.",
                        socket.RemoteEndPoint.Address,
                        e.RequestUri
                    );
                    connection.Socket.Disconnect();
                }
            }
            else {
                //Http request is not from a pending connection
                IClient user = Users.Find(s => s.Socket == sender);
                //if not pending, they have at least sent a login packet but are processing, 
                if (user != null && user.LoggedIn) {
                    if (PluginHost.OnHttpRequest(user, e)) {
                        Logging.Info(
                            "AresServer",
                            "Http request from '{0}' ({1}) for resource '{2}' denied.",
                            user.Name,
                            user.ExternalIp,
                            e.RequestUri
                        );
                    }
                }
                else {
                    //this doesn't appear to happen typically, it would just be racing against !LoggedIn if it did.
                    //dc for now unless it causes problems in the future.
                    connection.Socket.Disconnect();
                }
            }
        }

        protected virtual void ClientDisconnected(object sender, DisconnectEventArgs e)
        {
            if (!(sender is ISocket socket)) return;

            IClient user = null;

            lock (pending) pending.RemoveAll((s) => s.Equals(sender));

            int uindex = Users.FindIndex(s => s.Socket == sender);
            if (uindex == -1) return;

            lock (Users) {
                user = Users[uindex];
                Users.RemoveAt(uindex);
            }

            if (user.LoggedIn) {
                History.Add(user);

                SendPacket((s) =>
                    !s.Guid.Equals(user.Guid) && //ghost?
                     s.Vroom == user.Vroom &&
                     s.CanSee(user),
                     new Parted(user.Name));
            }

            Stats.Parted++;
            PluginHost.OnPart(user, e.UserToken);

            idpool.Push(user.Id);
            user.Dispose();
        }


        protected virtual bool HandlePending(ISocket socket) {
            lock (pending) {
                var conn = pending.Find((s) => s.Socket == socket);
                if (conn == null) {
                    socket.Disconnect();
                    return false;
                }
                pending.Remove(conn);
            }
            return true;
        }

        protected internal virtual bool CheckCounters(IClient user, IPacket packet) 
        {
            DateTime now = DateTime.Now;

            foreach (var rule in flood_rules.Where((s) => s.Id == packet.Id)) {

                if (!user.Counters.TryGetValue(packet.Id, out IFloodCounter counter)) {
                    counter = new FloodCounter(0, now);
                    user.Counters.Add(packet.Id, counter);
                }

                if (now.Subtract(counter.Last).TotalMilliseconds > rule.Timeout)
                    counter.Count = 0;

                if (++counter.Count >= rule.Count) {
                    Stats.FloodsTriggered++;

                    if (!PluginHost.OnFlood(user, rule, packet))
                        return false;
                }
                else counter.Last = now;
            }
            
            return true;
        }


        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e) {

            switch (e.PropertyName) {
                case nameof(Config.Topic):
                    SendPacket(new Topic(Config.Topic));
                    break;
                case nameof(Config.Avatar):
                    ServerAvatar allPacket = null;
                    if (Config.ShowBot)
                        allPacket = new ServerAvatar(Config.BotName, Config.Avatar);
                    foreach (var user in Users) {
                        if (user.OrgAvatar.IsEmpty()) {
                            SendPacket(
                                (s) => 
                                    s.Vroom == user.Vroom &&
                                    s.CanSee(user),
                                new ServerAvatar(user));
                        }
                        if (Config.ShowBot) SendPacket(user, allPacket);
                    }
                    break;
                case nameof(Config.Website):
                    SendWebsite(Config.Website.Address, Config.Website.Caption);
                    break;
            }
        }

        #region " IServer Methods "

        public void SendToAdmin(string message) {
            SendToAdmin(AdminLevel.Moderator, message);
        }

        public void SendToAdmin(IPacket packet) {
            SendToAdmin(AdminLevel.Moderator, packet);
        }

        public void SendToAdmin(AdminLevel minlevel, string message) {
            SendAnnounce((s) => s.Admin >= minlevel, message);
        }

        public void SendToAdmin(AdminLevel minlevel, IPacket packet) {
            SendPacket((s) => s.Admin >= minlevel, packet);
        }

        public void SendAnnounce(string text)
        {
            SendPacket(new Announce(text));
        }

        public void SendAnnounce(string target, string text)
        {
            SendPacket((s) => s.Name == target, new Announce(text));
        }

        public void SendAnnounce(IClient target, string text)
        {
            SendPacket((s) => s == target, new Announce(text));
        }

        public void SendAnnounce(Predicate<IClient> match, string text)
        {
            SendPacket(match, new Announce(text));
        }

        public void SendText(string target, string sender, string text) {
            SendPacket((s) => s.Name == target, new ServerPublic(sender, text));
        }

        public void SendText(IClient target, string sender, string text) {
            SendPacket((s) => s == target, new ServerPublic(sender, text));
        }

        public void SendText(IClient target, IClient sender, string text) {
            SendPacket((s) => s == target, new ServerPublic(sender.Name, text));
        }

        public void SendText(string sender, string text) {
            SendPacket(new ServerPublic(sender, text));
        }

        public void SendText(IClient sender, string text) {
            SendPacket(new ServerPublic(sender.Name, text));
        }

        public void SendText(Predicate<IClient> match, string sender, string text) {
            SendPacket(match, new ServerPublic(sender, text));
        }

        public void SendText(Predicate<IClient> match, IClient sender, string text) {
            SendPacket(match, new ServerPublic(sender.Name, text));
        }


        public void SendEmote(string target, string sender, string text) {
            SendPacket((s) => s.Name == target, new ServerEmote(sender, text));
        }

        public void SendEmote(IClient target, string sender, string text) {
            SendPacket((s) => s == target, new ServerEmote(sender, text));
        }

        public void SendEmote(IClient target, IClient sender, string text) {
            SendPacket((s) => s == target, new ServerEmote(sender.Name, text));
        }

        public void SendEmote(string sender, string text) {
            SendPacket(new ServerEmote(sender, text));
        }

        public void SendEmote(IClient sender, string text) {
            SendPacket(new ServerEmote(sender.Name, text));
        }

        public void SendEmote(Predicate<IClient> match, string sender, string text) {
            SendPacket(match, new ServerEmote(sender, text));
        }

        public void SendEmote(Predicate<IClient> match, IClient sender, string text) {
            SendPacket(match, new ServerEmote(sender.Name, text));
        }


        public void SendPrivate(string target, string sender, string text) {
            SendPacket((s) => s.Name == target, new Private(sender, text));
        }

        public void SendPrivate(IClient target, string sender, string text) {
            SendPacket((s) => s == target, new Private(sender, text));
        }

        public void SendPrivate(IClient target, IClient sender, string text) {
            SendPacket((s) => s == target, new Private(sender.Name, text));
        }

        public void SendPrivate(Predicate<IClient> match, string sender, string text) {
            SendPacket(match, new Private(sender, text));
        }

        public void SendPrivate(Predicate<IClient> match, IClient sender, string text) {
            SendPacket(match, new Private(sender.Name, text));
        }


        public void SendWebsite()
        {
            if (Config.Website != null)
                SendWebsite(Config.Website.Address, Config.Website.Caption);
        }

        public void SendWebsite(string address, string caption) {
            SendPacket(new ServerUrl(address, caption));
        }

        public void SendWebsite(string target, string address, string caption) {
            SendPacket((s) => s.Name == target, new ServerUrl(address, caption));
        }

        public void SendWebsite(IClient target, string address, string caption) {
            SendPacket((s) => s == target, new ServerUrl(address, caption));
        }

        public void SendWebsite(Predicate<IClient> match, string address, string caption) {
            SendPacket(match, new ServerUrl(address, caption));
        }

        #endregion
    }
}

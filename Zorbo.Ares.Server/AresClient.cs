using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.WebSockets;
using Zorbo.Ares.Resources;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server
{
    public class AresClient : ModelBase, IClient, IDisposable
    {
        #region " Variables "

        ISocket socket;

#pragma warning disable IDE0044 // Add readonly modifier

        AresServer server;
        AdminLevel admin;

        ushort id;
        Guid guid;

        int captchaTries = 5;
        int captchaAnswer;

        bool logged = false;
        bool local = false;
        bool browse = false;
        bool cloaked = false;
        bool muzzled = false;
        bool captcha = false;
        bool fastping = false;
        bool compression = false;
        bool encryption = false;
        
        byte age = 0;
        Gender gender = Gender.Unknown;
        Country country = Country.Unknown;

        IAvatar avatar = null;
        IAvatar orgavatar = null;

        string name;
        string orgname;
        string version;
        string region;
        string message;

        ushort vroom;
        ushort dcport;
        ushort nodeport;

        IPAddress nodeip;
        IPAddress localip;

        DateTime jointime;
        DateTime captchatime;
        DateTime lastupdate;

        ModelList<string> ignored;

        ClientFlags features = ClientFlags.NONE;

        Dictionary<byte, FloodCounter> counters;
        Dictionary<string, object> extended_props;

#pragma warning restore IDE0044 // Add readonly modifier

        #endregion

        #region " Properties "

        public ushort Id {
            get { return id; }
            set { id = value; }
        }

        public Guid Guid {
            get { return guid; }
            set { OnPropertyChanged(() => guid, value); }
        }

        public uint Token {
            get { return (uint)GetHashCode(); }
        }

        public ISocket Socket {
            get { return socket; }
        }

        public IServer Server {
            get { return server; }
        }

        public IMonitor Monitor {
            get {
                if (socket != null)
                    return socket.Monitor;

                return null;
            }
        }

        public ClientId ClientId {
            get { return new ClientId(Guid, ExternalIp); }
        }


        public bool Connected {
            get { return socket != null && socket.IsConnected; }
        }

        public bool IsCaptcha {
            get { return captcha; }
            set {
                if (captcha != value) {
                    captcha = value;
                    OnPropertyChanged();

                    var record = server.History.Records.Find((s) => s.Equals(this));

                    if (value) {
                        record.Trusted = false;

                        if (LoggedIn)
                            server.SendPacket((s) =>
                                s != this &&
                                s.Vroom == Vroom,
                                new Parted(Name));

                        vroom = ushort.MaxValue;
                        ShowCaptcha();
                    }
                    else {
                        vroom = 0;
                        captchaTries = 5;

                        record.Trusted = true;
                        server.PluginHost.OnCaptcha(this, CaptchaEvent.Exit);

                        FinishJoin();
                    }
                }
            }
        }

        public bool LoggedIn {
            get { return logged; }
            set { OnPropertyChanged(() => logged, value); }
        }


        public bool LocalHost {
            get { return local; }
            set { OnPropertyChanged(() => local, value); }
        }

        public bool Browsable {
            get { return browse; }
            set { OnPropertyChanged(() => browse, value); }
        }

        public bool Cloaked {
            get { return cloaked; }
            set {
                if (cloaked != value) {
                    cloaked = value;
                    OnPropertyChanged();
                    if (cloaked)
                        server.SendPacket((s) =>
                            s != this &&
                            s.Admin < Admin &&
                            s.Vroom == Vroom,
                            new Parted(Name));
                    else
                        server.SendPacket((s) =>
                            s != this &&
                            s.Admin < Admin &&
                            s.Vroom == Vroom,
                            new Join(this));
                }
            }
        }

        public bool Muzzled {
            get { return muzzled; }
            set {
                if (muzzled != value) {
                    muzzled = value;
                    OnPropertyChanged();
                    if (muzzled)
                        server.SendAnnounce(this, Strings.Muzzled);
                    else
                        server.SendAnnounce(this, Strings.Unmuzzled);
                }
            }
        }

        public bool FastPing {
            get { return fastping; }
            set { OnPropertyChanged(() => fastping, value); }
        }

        public bool Compression {
            get { return compression; }
            set { OnPropertyChanged(() => compression, value); }
        }

        public bool Encryption {
            get { return encryption; }
            set { OnPropertyChanged(() => encryption, value); }
        }


        public byte Age {
            get { return age; }
            set { OnPropertyChanged(() => age, value); }
        }

        public Gender Gender {
            get { return gender; }
            set { OnPropertyChanged(() => gender, value); }
        }

        public Country Country {
            get { return country; }
            set { OnPropertyChanged(() => country, value); }
        }

        public AdminLevel Admin {
            get { return admin; }
            set {
                if (admin != value) {

                    if (LocalHost && value < admin)
                        return;

                    if (value > AdminLevel.Host)
                        value = AdminLevel.Host;

                    admin = value;
                    OnPropertyChanged();

                    server.SendPacket(this, new OpChange(Admin > AdminLevel.User));
                    server.SendPacket((s) => s.Vroom == Vroom, new ServerUpdate(this));
                }
            }
        }


        public IAvatar Avatar {
            get {
                if (avatar == null)
                    return server.Config.Avatar;

                return avatar;
            }
            set {
                if (avatar == null || !avatar.Equals(value)) {

                    avatar = value;
                    OnPropertyChanged();

                    server.SendPacket((s) =>
                        s.Vroom == Vroom,
                        new ServerAvatar(this));
                }
            }
        }

        public IAvatar OrgAvatar {
            get { return orgavatar; }
            set { orgavatar = value; }
        }


        public string Name {
            get { return name; }
            set {
                if (name != value)
                    ChangeName(name, value);
            }
        }

        public string OrgName {
            get { return orgname; }
            set { orgname = value; }
        }

        public string Version {
            get { return version; }
            set { version = value; }
        }

        public string Region {
            get { return region; }
            set { OnPropertyChanged(() => region, value); }
        }

        public string Message {
            get { return message; }
            set {
                if (message != value) {
                    message = value;
                    OnPropertyChanged();

                    server.SendPacket((s) =>
                        s.Vroom == Vroom,
                        new ServerPersonal(Name, message));
                }
            }
        }


        public ushort Vroom {
            get { return vroom; }
            set {
                if (vroom != value)
                    ChangeVroom(vroom, value);
            }
        }

        public ushort FileCount => 0;

        public ushort ListenPort {
            get { return dcport; }
            set { dcport = value; }
        }

        public ushort NodePort {
            get { return nodeport; }
            set { nodeport = value; }
        }


        public IPAddress NodeIp {
            get { return nodeip; }
            set { OnPropertyChanged(() => nodeip, value); }
        }

        public IPAddress LocalIp {
            get { return localip; }
            set { OnPropertyChanged(() => localip, value); }
        }

        public IPAddress ExternalIp {
            get {
                if (socket != null)
                    try {
                        return socket.RemoteEndPoint.Address;
                    }
                    catch { }

                return null;
            }
        }

        public ClientFlags Features {
            get { return features; }
            set { OnPropertyChanged(() => features, value); }
        }


        public ModelList<string> Ignored {
            get { return ignored; }
        }

        IObservableCollection<string> IClient.Ignored {
            get { return Ignored; }
        }

        IDictionary<string, object> IClient.Extended {
            get { return extended_props; }
        }


        public DateTime JoinTime {
            get { return jointime; }
            set { jointime = value; }
        }

        public DateTime CaptchaTime {
            get { return captchatime; }
            set { captchatime = value; }
        }

        public DateTime LastUpdate {
            get { return lastupdate; }
            set { lastupdate = value; }
        }


        internal Dictionary<byte, FloodCounter> Counters {
            get { return counters; }
            set { counters = value; }
        }

        IDictionary<byte, IFloodCounter> IClient.Counters {
            get { return (IDictionary<byte,IFloodCounter>)Counters; }
        }

        #endregion


        internal AresClient(AresServer server, ISocket socket, ushort id)
        {
            this.id = id;

            this.server = server;
            this.socket = socket;
            this.socket.PacketReceived += OnPacketReceived;
            this.Socket.RequestReceived += OnHttpRequestReceived;

            this.jointime = DateTime.Now;
            this.lastupdate = this.jointime;

            this.ignored = new ModelList<string>();
            this.counters = new Dictionary<byte, FloodCounter>();
            this.extended_props = new Dictionary<string, object>();
        }

        public void SendPacket(IPacket packet)
        {
            if (Connected && LoggedIn)
                socket.SendAsync(packet);
        }

        public void SendPacket(Predicate<IClient> match, IPacket packet)
        {
            if (match(this))
                SendPacket(packet);
        }

        private void ShowCaptcha()
        {
            if (!LoggedIn)
                PerformLogin();

            else PerformQuickLogin();

            try {
                //this could except on client.Socket
                //SendPacket won't send out packets to IsCaptcha = true
                captchaAnswer = Captcha.Create(this);

                captchatime = DateTime.Now;
                server.PluginHost.OnCaptcha(this, CaptchaEvent.Enter);
            }
            catch { }
        }

        private void FinishCaptcha(string message)
        {
            if (int.TryParse(message, out int sum)) {
                if (sum == captchaAnswer) {
                    IsCaptcha = false;
                    return;
                }
            }

            if (--captchaTries <= 0) {
                server.Stats.CaptchaBanned++;

                server.SendAnnounce(this, Strings.BannedCaptchaAnswers);
                server.PluginHost.OnCaptcha(this, CaptchaEvent.Banned);

                Ban();
            }
            else {
                server.SendAnnounce(this, String.Format(Strings.InvalidCaptcha, captchaTries));
                server.PluginHost.OnCaptcha(this, CaptchaEvent.Failed);
            }
        }


        internal void PerformLogin()
        {
            LoggedIn = true;

            server.SendPacket(this, new LoginAck() {
                Username = Name,
                ServerName = server.Config.Name,
            });

            var features = server.PluginHost.OnSendFeatures(this, server.Config.GetFeatures());

            server.SendPacket(this, new Features() {
                Version = Strings.VersionLogin,
                SupportFlag = features,
                SharedTypes = 63,
                Language = server.Config.Language,
                Token = this.Token,
            });

            server.SendPacket(this, new TopicFirst(server.Config.Topic));
            server.SendUserlist(this);

            if (!IsCaptcha) {
                server.SendAvatars(this);
                server.SendPacket(this, new OpChange(Admin > AdminLevel.User));

                server.PluginHost.OnSendJoin(this);
            }
        }

        internal void PerformQuickLogin()
        {
            server.SendPacket(this, new LoginAck() {
                Username = Name,
                ServerName = server.Config.Name,
            });

            server.SendPacket(this, new TopicFirst(server.Config.Topic));
            server.SendUserlist(this);

            if (!IsCaptcha) {
                if (!String.IsNullOrEmpty(Message))
                    server.SendPacket((s) =>
                        s.Vroom == Vroom,
                        new ServerPersonal(Name, Message));

                server.SendAvatars(this);
                server.SendPacket(this, new OpChange(Admin > AdminLevel.User));

                server.PluginHost.OnSendJoin(this);
            }
        }


        internal void ChangeName(string oldname, string newname)
        {
            name = newname.FormatUsername();

            if (string.IsNullOrWhiteSpace(name))
                name = ExternalIp.AnonUsername();

            RaisePropertyChanged(nameof(Name));

            server.SendPacket((s) =>
                s != this &&
                s.Vroom == Vroom,
                new Parted() { Username = oldname });

            PerformQuickLogin();
        }

        internal void ChangeVroom(ushort oldvroom, ushort newvroom)
        {
            if (server.PluginHost.OnVroomJoinCheck(this, newvroom)) {

                server.PluginHost.OnVroomPart(this);

                vroom = newvroom;
                RaisePropertyChanged(nameof(Vroom));

                server.SendPacket((s) =>
                    s != this &&
                    s.Vroom == oldvroom,
                    new Parted(Name));

                PerformQuickLogin();
                server.PluginHost.OnVroomJoin(this);
            }
        }


        internal void HandleJoin(PacketEventArgs e)
        {
            Login login = (Login)e.Packet;

            Guid = login.Guid;
            Encryption = login.Encryption == 250;
            ListenPort = login.ListenPort;
            NodeIp = login.NodeIp;
            NodePort = login.NodePort;
            name = login.Username.FormatUsername();
            if (string.IsNullOrWhiteSpace(name))
                name = ExternalIp.AnonUsername();
            orgname = name;
            Version = login.Version;
            LocalIp = login.LocalIp;
            Browsable = (login.SupportFlag & SupportFlags.SHARING) == SupportFlags.SHARING;
            Compression = (login.SupportFlag & SupportFlags.COMPRESSION) == SupportFlags.COMPRESSION;
            Age = login.Age;
            Gender = login.Gender;
            Country = login.Country;
            Region = login.Region;
            Features = login.Features;

            if ((Features & ClientFlags.OPUS_VOICE) == ClientFlags.OPUS_VOICE)
                Features |= ClientFlags.VOICE;

            if ((Features & ClientFlags.PRIVATE_OPUS_VOICE) == ClientFlags.PRIVATE_OPUS_VOICE)
                Features |= ClientFlags.PRIVATE_VOICE;

            var record = server.History.Add(this);
            var autologin = server.History.Admin.Passwords.Find((s) => s.ClientId.Equals(record.ClientId));

            admin = (autologin != null) ? autologin.Level : admin;
            admin = (LocalHost) ? AdminLevel.Host : admin;

            if (admin != AdminLevel.User)
                RaisePropertyChanged(nameof(Admin));

            if (AllowedJoin(record)) {

                if (server.Config.BotProtection) {
                    if (!record.Trusted) {
                        IsCaptcha = true;
                        return;
                    }
                }

                FinishJoin();
            }
            else server.Stats.Rejected++;
        }

        private void FinishJoin()
        {
            PerformLogin();
            server.Stats.Joined++;
            server.PluginHost.OnJoin(this);
        }

        private void OnPacketReceived(object sender, PacketEventArgs e)
        {
            if (LoggedIn && server.CheckCounters(this, e.Packet)) {

                LastUpdate = DateTime.Now;

                if (Socket.IsWebSocket && e.Packet is PingPongPacket) 
                    return;

                if (!HandlePacket(e) && server.PluginHost.OnBeforePacket(this, e.Packet)) {

                    HandleOverridePacket(e);
                    server.PluginHost.OnAfterPacket(this, e.Packet);
                }
            }
        }

        private void OnHttpRequestReceived(object sender, RequestEventArgs e)
        {
            Console.WriteLine("--------- HTTP Request Received ---------");
            Console.WriteLine("");
        }

        /// <summary>
        /// Packets handled in this function are 'internal' and cannot be overriden.
        /// </summary>
        internal bool HandlePacket(PacketEventArgs e)
        {
            if (IsCaptcha) {
                switch ((AresId)e.Packet.Id) {
                    case AresId.MSG_CHAT_CLIENT_FASTPING:
                        fastping = true;
                        return true;
                    case AresId.MSG_CHAT_CLIENT_AUTOLOGIN:
                        AutoLogin login = (AutoLogin)e.Packet;
                        Commands.HandleAutoLogin(server, this, login.Sha1Password);
                        return true;
                    case AresId.MSG_CHAT_CLIENT_PUBLIC:
                        ClientPublic pub = (ClientPublic)e.Packet;
                        FinishCaptcha(pub.Message);
                        return true;
                    case AresId.MSG_CHAT_CLIENT_EMOTE:
                        ClientEmote emote = (ClientEmote)e.Packet;
                        FinishCaptcha(emote.Message);
                        return true;
                    case AresId.MSG_CHAT_CLIENT_ADDSHARE:
                        return true;
                    case AresId.MSG_CHAT_CLIENT_UPDATE_STATUS:
                        ClientUpdate update = (ClientUpdate)e.Packet;

                        lastupdate = DateTime.Now;

                        NodeIp = update.NodeIp;
                        NodePort = update.NodePort;
                        Age = (update.Age != 0) ? update.Age : Age;
                        Gender = (update.Gender != 0) ? update.Gender : Gender;
                        Country = (update.Country != 0) ? update.Country : Country;
                        //Region = !String.IsNullOrEmpty(update.Region) ? update.Region : Region;
                        return true;
                    default:
                        break;
                }
                return false;
            }
            else if (LoggedIn) {

                switch ((AresId)e.Packet.Id) {
                    case AresId.MSG_CHAT_CLIENT_FASTPING:
                        fastping = true;
                        return true;
                    case AresId.MSG_CHAT_CLIENT_DUMMY:
                        return true;
                    case AresId.MSG_CHAT_CLIENT_PUBLIC:
                        ClientPublic pub = (ClientPublic)e.Packet;

                        if (!String.IsNullOrEmpty(pub.Message))
                            if (pub.Message.StartsWith("#") && Commands.HandlePreCommand(server, this, pub.Message.Substring(1)))
                                return true;

                        if (Muzzled) {
                            server.SendAnnounce(this, Strings.AreMuzzled);
                            return true;
                        }

                        return false;
                    case AresId.MSG_CHAT_CLIENT_EMOTE:
                        ClientEmote emote = (ClientEmote)e.Packet;

                        if (!String.IsNullOrEmpty(emote.Message))
                            if (emote.Message.StartsWith("#") && Commands.HandlePreCommand(server, this, emote.Message.Substring(1)))
                                return true;

                        if (Muzzled) {
                            server.SendAnnounce(this, Strings.AreMuzzled);
                            return true;
                        }

                        return false;
                    case AresId.MSG_CHAT_CLIENT_COMMAND:
                        Command cmd = (Command)e.Packet;
                        Commands.HandlePreCommand(server, this, cmd.Message);
                        break;
                    case AresId.MSG_CHAT_CLIENT_PVT:
                        Private pvt = (Private)e.Packet;

                        if (Muzzled && !server.Config.MuzzledPMs) {

                            pvt.Message = "[" + Strings.AreMuzzled + "]";
                            server.SendPacket(this, pvt);

                            return true;
                        }

                        return false;
                    case AresId.MSG_CHAT_CLIENT_AUTHREGISTER: {
                        AuthRegister reg = (AuthRegister)e.Packet;
                        Commands.HandleRegister(server, this, reg.Password);
                    }
                    return true;
                    case AresId.MSG_CHAT_CLIENT_AUTHLOGIN: {
                        AuthLogin login = (AuthLogin)e.Packet;
                        Commands.HandleLogin(server, this, login.Password);
                    }
                    return true;
                    case AresId.MSG_CHAT_CLIENT_AUTOLOGIN: {
                        AutoLogin login = (AutoLogin)e.Packet;
                        Commands.HandleAutoLogin(server, this, login.Sha1Password);
                    }
                    return true;
                    case AresId.MSG_CHAT_CLIENT_ADDSHARE:
                        return true;
                    case AresId.MSG_CHAT_CLIENT_IGNORELIST:
                        Ignored ignore = (Ignored)e.Packet;

                        if (ignore.Ignore) {
                            lock (ignored) {
                                if (!ignored.Contains(ignore.Username)) {
                                    ignored.Add(ignore.Username);
                                    server.SendAnnounce(this, String.Format(Strings.Ignored, ignore.Username));
                                }
                            }
                        }
                        else {
                            lock (ignored) {
                                if (ignored.Contains(ignore.Username)) {
                                    ignored.Remove(ignore.Username);
                                    server.SendAnnounce(this, String.Format(Strings.Unignored, ignore.Username));
                                }
                            }
                        }
                        return true;
                    case AresId.MSG_CHAT_CLIENT_UPDATE_STATUS:
                        ClientUpdate update = (ClientUpdate)e.Packet;

                        lastupdate = DateTime.Now;

                        NodeIp = update.NodeIp;
                        NodePort = update.NodePort;
                        Age = (update.Age != 0) ? update.Age : Age;
                        Gender = (update.Gender != 0) ? update.Gender : Gender;
                        Country = (update.Country != 0) ? update.Country : Country;
                        //Region = !String.IsNullOrEmpty(update.Region) ? update.Region : Region;
                        server.SendPacket((s) => s.Vroom == Vroom, new ServerUpdate(this));

                        return true;
                    case AresId.MSG_CHAT_CLIENT_DIRCHATPUSH:
                        ClientDirectPush push = (ClientDirectPush)e.Packet;

                        if (Encoding.UTF8.GetByteCount(push.Username) < 2) {
                            server.SendPacket(this, new DirectPushError(4));
                            return true;
                        }

                        if (push.TextSync.Length < 16) {
                            server.SendPacket(this, new DirectPushError(3));
                            return true;
                        }

                        IClient target = server.FindUser(s => s.Name == push.Username);

                        if (target == null) {
                            server.SendPacket(this, new DirectPushError(1));
                            return true;
                        }

                        if (target.Ignored.Contains(Name)) {
                            server.SendPacket(this, new DirectPushError(2));
                            return true;
                        }

                        server.SendPacket(this, new DirectPushError(0));
                        server.SendPacket(target, new ServerDirectPush(this, push));

                        return true;
                    case AresId.MSG_CHAT_CLIENT_BROWSE:
                        server.SendPacket(this, new BrowseError(((Browse)e.Packet).BrowseId));
                        return true;
                    case AresId.MSG_CHAT_CLIENT_SEARCH:
                        server.SendPacket(this, new SearchEnd(((Search)e.Packet).SearchId));
                        return true;
                    case AresId.MSG_CHAT_CLIENTCOMPRESSED: {
                        Compressed packet = (Compressed)e.Packet;
                        byte[] payload = Zlib.Decompress(packet.Data);

                        var reader = new PacketReader(payload) {
                            Position = 0L
                        };

                        while (reader.Remaining >= 3) {
                            ushort count = reader.ReadUInt16();
                            byte id = reader.ReadByte();

                            IPacket msg = Socket.Formatter.Unformat(id, reader.ReadBytes(count));
                            OnPacketReceived(Socket, new PacketEventArgs(msg, WebSocketMessageType.Binary, 0));
                        }
                        break;
                    }
                    default:
                        break;
                }

                return false;//wasn't handled
            }
            else {
                //not captcha, not logged, error?
                Logging.Info("AresClient", "Client {0} sent {1} before logging in.", this.ExternalIp, e.Packet.Id);
                return true;
            }
        }

        /// <summary>
        /// Packets handled in this function could possibly be overriden by a plugin
        /// </summary>
        internal void HandleOverridePacket(PacketEventArgs e)
        {
            switch ((AresId)e.Packet.Id) {

                case AresId.MSG_CHAT_CLIENT_PUBLIC:
                    ClientPublic pub = (ClientPublic)e.Packet;

                    if (!string.IsNullOrEmpty(pub.Message)) {

                        server.SendPacket((s) =>
                            s.Vroom == Vroom &&
                           !s.Ignored.Contains(Name),
                            new ServerPublic(Name, pub.Message));
                    }
                    break;
                case AresId.MSG_CHAT_CLIENT_EMOTE:
                    ClientEmote emote = (ClientEmote)e.Packet;

                    if (!string.IsNullOrEmpty(emote.Message)) {

                        server.SendPacket((s) =>
                            s.Vroom == Vroom &&
                           !s.Ignored.Contains(Name),
                            new ServerEmote(Name, emote.Message));
                    }
                    break;
                case AresId.MSG_CHAT_CLIENT_PVT: {
                    Private priv = (Private)e.Packet;

                    if (string.IsNullOrEmpty(priv.Message)) return;

                    IClient target = server.FindUser((s) => s.Name == priv.Username);

                    if (target != null) {

                        if (target.Ignored.Contains(Name))
                            server.SendPacket(this, new IgnoringYou(priv.Username));
                        else {
                            priv.Username = Name;
                            server.SendPacket(target, priv);
                        }
                    }
                    else server.SendPacket(this, new Offline(priv.Username));
                }
                break;
                case AresId.MSG_CHAT_CLIENT_COMMAND:
                    //Command cmd = (Command)e.Packet;
                    //Not necessary to handle this here anymore
                    break;
                case AresId.MSG_CHAT_CLIENT_PERSONAL_MESSAGE:
                    ClientPersonal personal = (ClientPersonal)e.Packet;
                    Message = personal.Message;
                    break;
                case AresId.MSG_CHAT_CLIENT_AVATAR:
                    ClientAvatar avatar = (ClientAvatar)e.Packet;

                    if (avatar.AvatarBytes.Length == 0)
                        Avatar = null;
                    else {
                        Avatar = new AresAvatar(avatar.AvatarBytes);

                        if (OrgAvatar == null)
                            OrgAvatar = new AresAvatar(avatar.AvatarBytes);
                    }
                    break;
                case AresId.MSG_CHAT_CLIENT_CUSTOM_DATA: {
                    ClientCustom custom = (ClientCustom)e.Packet;

                    string username = string.IsNullOrEmpty(custom.Username) ? Name : custom.Username;
                    custom.Username = Name;

                    IClient target = server.FindUser((s) => s.Name == username);

                    if (target != null && !target.Ignored.Contains(Name))
                        server.SendPacket(target, custom);
                }
                break;
                case AresId.MSG_CHAT_CLIENT_CUSTOM_DATA_ALL:
                    ClientCustomAll customAll = (ClientCustomAll)e.Packet;

                    server.SendPacket((s) =>
                        s != this &&
                        s.Vroom == Vroom,
                        new ClientCustom(Name, customAll));

                    break;
            }
        }

        /*
         * NOTICE -- Occurs before "LoggedIn" is set to true. "AresServer.SendPacket" will fail
         */
        private bool AllowedJoin(Record record)
        {
            if (this.Guid.Equals(Guid.Empty))
                return Rejected(new Announce(Errors.InvalidLogin), RejectReason.InvalidLogin);

            //let onjoincheck run for host but ignore its return
            if (!server.PluginHost.OnJoinCheck(this) && !LocalHost)
                return Rejected(new Error(Errors.Rejected), RejectReason.Plugins);

            // check bans
            if (!LocalHost) {
                if (server.History.Bans.Contains((s) => s.Equals(record)))
                    return Rejected(new Error(Errors.Banned), RejectReason.Banned);

                if (server.History.RangeBans.Contains((s) => s.IsMatch(ExternalIp.ToString())))
                    return Rejected(new Error(Errors.RangeBanned), RejectReason.RangeBanned);
            }

            //check name hijacking
            int count = 1;
            foreach (var user in server.Users) {
                if (user != this) {

                    if (user.Name == Name)
                        return Rejected(new Announce(Errors.NameTaken), RejectReason.NameTaken);
                    
                    else if (user.ExternalIp.Equals(socket.RemoteEndPoint.Address))
                        count++;
                }

                if (count > server.Config.MaxClones)
                    return Rejected(new Error(String.Format(Errors.Clones, socket.RemoteEndPoint.Address)), RejectReason.TooManyBots);
            }
            //Disconnect ghosts after the Name hijack check
            //this prevents an infinite loop in case you open 2 tabs to the same room with the same name
            //the second join will not boot the first one, this can cause a delay in ghost detection but it's minimal
            foreach (var user in server.Users)
                if (user != this && user.Guid.Equals(this.Guid))
                    user.Disconnect();

            return true;
        }

        private bool Rejected(IPacket msg, RejectReason reason)
        {
            socket.SendAsync(msg);
            server.PluginHost.OnJoinRejected(this, reason);

            Disconnect();
            return false;
        }

        public void Ban()
        {
            Ban(null);
        }

        public void Ban(Object state)
        {
            server.Stats.Banned++;
            server.History.Bans.Add(this.ClientId);

            Disconnect(state);
        }


        public void Disconnect()
        {
            Disconnect(null);
        }

        public void Disconnect(Object state)
        {
            if (socket != null)
                socket.Disconnect(state);
        }


        internal void Dispose()
        {
            LoggedIn = false;

            socket.Dispose();
            socket = null;

            guid = Guid.Empty;
            name = null;
            orgname = null;
            version = null;
            region = null;
            message = null;
            avatar = null;
            orgavatar = null;
            localip = null;
            nodeip = null;

            counters.Clear();
            counters = null;

            ignored.Clear();
            ignored = null;

            extended_props.Clear();
            extended_props = null;
        }

        //we don't want a plugin to be able
        //to dispose us directly... bad
        void IDisposable.Dispose()
        {
            if (Connected) Disconnect();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Guid);
        }
    }
}
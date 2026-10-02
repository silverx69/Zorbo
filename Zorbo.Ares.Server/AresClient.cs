using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.WebSockets;
using Zorbo.Ares.Resources;
using Zorbo.Core;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Models;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Server
{
    public class AresClient : ModelBase, IClient, IDisposable
    {
        #region " Variables "

#pragma warning disable IDE0044 // Add readonly modifier

        Guid guid;

        AresServer server;
        AdminLevel admin;
        
        int captchaTries = 5;
        int captchaAnswer;

        bool logged = false;
        bool local = false;
        bool modern = false;
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

        byte[] avatar = null;
        byte[] orgavatar = null;

        string name;
        string version;
        string region;
        string message;

        ushort vroom;

        IPAddress nodeip;
        IPAddress localip;

        DateTime jointime;

        ClientFlags features = ClientFlags.NONE;
        Dictionary<string, object> extended_props;

#pragma warning restore IDE0044 // Add readonly modifier

        #endregion

        #region " Properties "

        public ushort Id { get; set; }

        public Guid Guid {
            get { return guid; }
            set { OnPropertyChanged(() => guid, value); }
        }

        public uint Token {
            get { return (uint)GetHashCode(); }
        }

        public ISocket Socket { get; private set; }

        public IServer Server {
            get { return server; }
        }

        public IMonitor Monitor {
            get {
                if (Socket != null)
                    return Socket.Monitor;

                return null;
            }
        }

        public ClientId ClientId {
            get;
            private set;
        }


        public bool Connected {
            get { return Socket != null && Socket.IsConnected; }
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
                                s.Vroom == Vroom &&
                                s.CanSee(this),
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

        public bool IsModern {
            get { return modern; }
            set { OnPropertyChanged(() => modern, value); }
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

                    SendPacket(new OpChange(Admin > AdminLevel.User));
                    server.SendPacket((s) => 
                        s.Vroom == Vroom &&
                        s.CanSee(this), 
                        new ServerUpdate(this));
                }
            }
        }


        public byte[] Avatar {
            get {
                if (avatar.IsEmpty())
                    return server.Config.Avatar;
                return avatar; 
            }
            set {
                if (avatar == null || !avatar.Equals(value)) {
                    avatar = value;
                    OnPropertyChanged();
                    server.SendPacket((s) =>
                        s.Vroom == Vroom && 
                        s.CanSee(this),
                        new ServerAvatar(this));
                }
            }
        }

        public byte[] OrgAvatar {
            get { return orgavatar; }
            set { OnPropertyChanged(() => orgavatar, value); }
        }


        public string Name {
            get { return name; }
            set { ChangeName(name, value); }
        }

        public string OrgName { get; set; }

        public string Version {
            get { return version; }
            set { OnPropertyChanged(() => version, value); }
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
                        s.Vroom == Vroom &&
                        s.CanSee(this),
                        new ServerPersonal(Name, message));
                }
            }
        }

        public ushort Vroom {
            get { return vroom; }
            set { ChangeVroom(vroom, value); }
        }

        public ushort FileCount => 0;

        public ushort ListenPort { get; set; }

        public ushort NodePort { get; set; }


        public IPAddress NodeIp {
            get { return nodeip; }
            set { OnPropertyChanged(() => nodeip, value); }
        }

        public IPAddress LocalIp {
            get { return localip; }
            set { OnPropertyChanged(() => localip, value); }
        }

        public IPAddress ExternalIp {
            get { return Socket?.RemoteEndPoint?.Address; }
        }

        public ClientFlags Features {
            get { return features; }
            set { OnPropertyChanged(() => features, value); }
        }

        public ModelList<string> Ignored { get; private set; }

        IObservableCollection<string> IClient.Ignored {
            get { return Ignored; }
        }

        IDictionary<string, object> IClient.Extended {
            get { return extended_props; }
        }


        public DateTime JoinTime { get; set; }

        public DateTime CaptchaTime { get; set; }

        public DateTime LastPing { get; set; }

        public DateTime LastUpdate { get; set; }

        public IDictionary<byte, IFloodCounter> Counters { 
            get; 
            private set; 
        }

        #endregion


        internal AresClient(AresServer server, ISocket socket, ushort id)
        {
            this.Id = id;

            this.server = server;
            this.Socket = socket;
            this.Socket.PacketReceived += OnPacketReceived;

            this.jointime = DateTime.Now;
            this.LastPing = this.jointime;
            this.LastUpdate = this.jointime;

            this.Ignored = new ModelList<string>();
            this.Counters = new Dictionary<byte, IFloodCounter>();
            this.extended_props = new Dictionary<string, object>();
        }


        public void SendPacket(IPacket packet)
        {
            if (Connected && LoggedIn)
                Socket.SendAsync(packet);
        }

        public void SendPacket(Predicate<IClient> match, IPacket packet)
        {
            if (match(this)) SendPacket(packet);
        }


        private void ShowCaptcha()
        {
            if (!LoggedIn)
                PerformLogin();

            else PerformQuickLogin();

            try {
                captchaAnswer = Captcha.Create(this);
                CaptchaTime = DateTime.Now;

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

            SendPacket(new LoginAck() {
                Username = Name,
                ServerName = server.Config.Name,
            });

            var features = server.PluginHost.OnSendFeatures(this, server.Config.GetFeatures());

            SendPacket(new Features() {
                Version = Strings.VersionLogin,
                SupportFlag = features,
                SharedTypes = 63,
                Language = server.Config.Language,
                Token = this.Token,
            });

            SendPacket(new TopicFirst(server.Config.Topic));
            server.SendUserlist(this);

            if (!IsCaptcha) {
                SendPacket(new OpChange(Admin > AdminLevel.User));
                server.SendWebsite();
                server.SendAvatars(this);
                server.PluginHost.OnSendJoin(this);
            }
        }

        internal void PerformQuickLogin()
        {
            SendPacket(new LoginAck() {
                Username = Name,
                ServerName = server.Config.Name,
            });

            SendPacket(new TopicFirst(server.Config.Topic));
            server.SendUserlist(this);

            if (!IsCaptcha) {
                SendPacket(new OpChange(Admin > AdminLevel.User));
                server.SendWebsite();
                if (!string.IsNullOrEmpty(Message))
                    server.SendPacket((s) =>
                        s.Vroom == Vroom &&
                        s.CanSee(this),
                        new ServerPersonal(Name, Message));
                server.SendAvatars(this);
                server.PluginHost.OnSendJoin(this);
            }
        }


        internal void ChangeName(string oldname, string newname)
        {
            newname = newname.FormatUsername();

            if (string.IsNullOrWhiteSpace(newname))
                newname = ExternalIp.AnonUsername();

            if (oldname != newname) {

                name = newname;
                RaisePropertyChanged(nameof(Name));

                server.SendPacket((s) =>
                    s != this &&
                    s.Vroom == Vroom &&
                    s.CanSee(this),
                    new Parted() { Username = oldname });

                PerformQuickLogin();
            }
        }

        internal void ChangeVroom(ushort oldvroom, ushort newvroom)
        {
            if (oldvroom != newvroom && server.PluginHost.OnVroomJoinCheck(this, newvroom)) {

                server.PluginHost.OnVroomPart(this);

                vroom = newvroom;
                RaisePropertyChanged(nameof(Vroom));

                server.SendPacket((s) =>
                    s != this &&
                    s.Vroom == oldvroom &&
                    s.CanSee(this),
                    new Parted(Name));

                PerformQuickLogin();
                server.PluginHost.OnVroomJoin(this);
            }
        }

        internal void HandleJoin(PacketEventArgs e)
        {
            Login login = (Login)e.Packet;

            Guid = login.Guid;
            ClientId = new ClientId(Guid, ExternalIp);
            Encryption = login.Encryption == 250;
            ListenPort = login.ListenPort;
            NodeIp = login.NodeIp;
            NodePort = login.NodePort;
            name = login.Username.FormatUsername();
            if (string.IsNullOrWhiteSpace(name))
                name = ExternalIp.AnonUsername();
            OrgName = name;
            Version = login.Version;
            LocalIp = login.LocalIp;
            Age = login.Age;
            Gender = login.Gender;
            Country = login.Country;
            Region = login.Region;

            HandleFeatures(login);
                    
            var record = server.History.Add(this);
            var autologin = server.History.Admin.Passwords.Find((s) => s.ClientId.Equals(record.ClientId));

            admin = (autologin != null) ? autologin.Level : admin;
            admin = (LocalHost) ? AdminLevel.Host : admin;

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

        private void HandleFeatures(Login login)
        {
            Browsable = (login.SupportFlag & SupportFlags.SHARING) == SupportFlags.SHARING;
            Compression = (login.SupportFlag & SupportFlags.COMPRESSION) == SupportFlags.COMPRESSION;

            Features = login.Features;

            if ((Features & ClientFlags.OPUS_VOICE) == ClientFlags.OPUS_VOICE)
                Features |= ClientFlags.VOICE;

            if ((Features & ClientFlags.PRIVATE_OPUS_VOICE) == ClientFlags.PRIVATE_OPUS_VOICE)
                Features |= ClientFlags.PRIVATE_VOICE;

            if (!server.Config.AllowVoice) {
                Features ^= ClientFlags.VOICE;
                Features ^= ClientFlags.OPUS_VOICE;
                Features ^= ClientFlags.PRIVATE_VOICE;
                Features ^= ClientFlags.PRIVATE_OPUS_VOICE;
            }
        }


        private void FinishJoin()
        {
            PerformLogin();
            server.Stats.Joined++;
            server.PluginHost.OnJoin(this);
        }


        private void OnPacketReceived(object sender, PacketEventArgs e)
        {
            if (server.CheckCounters(this, e.Packet) && LoggedIn) {

                LastUpdate = DateTime.Now;

                if (Socket.IsWebSocket && e.Packet is PingPongPacket) 
                    return;

                if (!HandlePacket(e) && server.PluginHost.OnBeforePacket(this, e.Packet)) {

                    HandleOverridePacket(e);
                    server.PluginHost.OnAfterPacket(this, e.Packet);
                }
            }
        }

        /// <summary>
        /// Packets handled in this function are 'internal' and cannot be overriden.
        /// </summary>
        internal bool HandlePacket(PacketEventArgs e)
        {
            if (IsCaptcha) {
                switch ((AresId)e.Packet.Id) {
                    case AresId.MSG_CHAT_CLIENT_FASTPING:
                        FastPing = true;
                        return true;
                    case AresId.MSG_CHAT_CLIENT_DUMMY:
                        return true;
                    case AresId.MSG_CHAT_CLIENT_AUTOLOGIN:
                        AutoLogin login = (AutoLogin)e.Packet;
                        AresCommands.HandleAutoLogin(server, this, login.Sha1Password);
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

                        LastUpdate = DateTime.Now;
                        NodeIp = update.NodeIp;
                        NodePort = update.NodePort;
                        return true;
                    default:
                        break;
                }
                return false;
            }
            else if (LoggedIn) {

                switch ((AresId)e.Packet.Id) {
                    case AresId.MSG_CHAT_CLIENT_FASTPING:
                        FastPing = true;
                        return true;
                    case AresId.MSG_CHAT_CLIENT_DUMMY:
                        return true;
                    case AresId.MSG_CHAT_CLIENT_PUBLIC:
                        ClientPublic pub = (ClientPublic)e.Packet;

                        if (AresCommands.HandlePreCommand(server, this, pub.Message))
                            return true;

                        if (Muzzled) {
                            server.SendAnnounce(this, Strings.AreMuzzled);
                            return true;
                        }
                        break;
                    case AresId.MSG_CHAT_CLIENT_EMOTE:
                        ClientEmote emote = (ClientEmote)e.Packet;

                        if (AresCommands.HandlePreCommand(server, this, emote.Message))
                            return true;

                        if (Muzzled) {
                            server.SendAnnounce(this, Strings.AreMuzzled);
                            return true;
                        }
                        break;
                    case AresId.MSG_CHAT_CLIENT_COMMAND:
                        Command cmd = (Command)e.Packet;
                        if (AresCommands.HandleCommand(server, this, cmd.Message))
                            return true;
                        break;
                    case AresId.MSG_CHAT_CLIENT_PVT:
                        Private pvt = (Private)e.Packet;

                        if (Muzzled && !server.Config.MuzzledPMs) {

                            pvt.Message = "[" + Strings.AreMuzzled + "]";
                            SendPacket(pvt);

                            return true;
                        }

                        break;
                    case AresId.MSG_CHAT_CLIENT_AUTHREGISTER: {
                        AuthRegister reg = (AuthRegister)e.Packet;
                        AresCommands.HandleRegister(server, this, reg.Password);
                        return true;
                    }
                    case AresId.MSG_CHAT_CLIENT_AUTHLOGIN: {
                        AuthLogin login = (AuthLogin)e.Packet;
                        AresCommands.HandleLogin(server, this, login.Password);
                        return true;
                    }
                    case AresId.MSG_CHAT_CLIENT_AUTOLOGIN: {
                        AutoLogin login = (AutoLogin)e.Packet;
                        AresCommands.HandleAutoLogin(server, this, login.Sha1Password);
                        return true;
                    }
                    case AresId.MSG_CHAT_CLIENT_ADDSHARE:
                        return true;
                    case AresId.MSG_CHAT_CLIENT_IGNORELIST:
                        Ignored ignore = (Ignored)e.Packet;
                        if (ignore.Ignore) {
                            lock (Ignored) {
                                if (!Ignored.Contains(ignore.Username)) {
                                    Ignored.Add(ignore.Username);
                                    server.SendAnnounce(this, String.Format(Strings.Ignored, ignore.Username));
                                }
                            }
                        }
                        else {
                            lock (Ignored) {
                                if (Ignored.Contains(ignore.Username)) {
                                    Ignored.Remove(ignore.Username);
                                    server.SendAnnounce(this, String.Format(Strings.Unignored, ignore.Username));
                                }
                            }
                        }
                        return true;
                    case AresId.MSG_CHAT_CLIENT_UPDATE_STATUS:
                        ClientUpdate update = (ClientUpdate)e.Packet;

                        LastUpdate = DateTime.Now;
                        NodeIp = update.NodeIp;
                        NodePort = update.NodePort;
                        server.SendPacket((s) => 
                            s.Vroom == Vroom &&
                            s.CanSee(this), 
                            new ServerUpdate(this));

                        return true;
                    case AresId.MSG_CHAT_CLIENT_DIRCHATPUSH:
                        ClientDirectPush push = (ClientDirectPush)e.Packet;

                        if (Encoding.UTF8.GetByteCount(push.Username) < 2) {
                            SendPacket(new DirectPushError(4));
                            return true;
                        }

                        if (push.TextSync.Length < 16) {
                            SendPacket(new DirectPushError(3));
                            return true;
                        }

                        IClient target = server.FindUser(s => s.Name == push.Username);

                        if (target == null) {
                            SendPacket(new DirectPushError(1));
                            return true;
                        }

                        if (target.Ignored.Contains(Name)) {
                            SendPacket(new DirectPushError(2));
                            return true;
                        }

                        SendPacket(new DirectPushError(0));
                        server.SendPacket(target, new ServerDirectPush(this, push));

                        return true;
                    case AresId.MSG_CHAT_CLIENT_BROWSE:
                        SendPacket(new BrowseError(((Browse)e.Packet).BrowseId));
                        return true;
                    case AresId.MSG_CHAT_CLIENT_SEARCH:
                        SendPacket(new SearchEnd(((Search)e.Packet).SearchId));
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
                            s.CanSee(this) &&
                           !s.Ignored.Contains(Name),
                            new ServerPublic(Name, pub.Message));
                    }
                    break;
                case AresId.MSG_CHAT_CLIENT_EMOTE:
                    ClientEmote emote = (ClientEmote)e.Packet;

                    if (!string.IsNullOrEmpty(emote.Message)) {

                        server.SendPacket((s) =>
                            s.Vroom == Vroom &&
                            s.CanSee(this) &&
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
                            SendPacket(new IgnoringYou(priv.Username));
                        else {
                            priv.Username = Name;
                            server.SendPacket(target, priv);
                        }
                    }
                    else SendPacket(new Offline(priv.Username));
                }
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
                        Avatar = avatar.AvatarBytes;
                        OrgAvatar ??= avatar.AvatarBytes;
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
                        s.Vroom == Vroom &&
                        s.CanSee(this),
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
                    
                    else if (user.ExternalIp.Equals(Socket.RemoteEndPoint.Address))
                        count++;
                }

                if (count > server.Config.MaxClones)
                    return Rejected(new Error(String.Format(Errors.Clones, Socket.RemoteEndPoint.Address)), RejectReason.TooManyBots);
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
            Socket.SendAsync(msg);
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
            if (Socket != null)
                Socket.Disconnect(state);
        }

        /// <summary>
        /// AresServer should be the only class to Dispose of an AresClient class. 
        /// The code below prevents calling Dispose from any other Assembly other than Zorbo.Ares.Server assembly. 
        /// This is so the AresServer class can use 'IClient' explicitly for all operations, allowing "Users" to be exposed to plugins without any casting.
        /// </summary>
        void IDisposable.Dispose()
        {
            var asm1 = Assembly.GetAssembly(this.GetType());
            var asm2 = Assembly.GetCallingAssembly();

            if (asm1 == asm2) Dispose();
        }

        internal void Dispose()
        {
            LoggedIn = false;

            Socket.Dispose();
            Socket = null;

            guid = Guid.Empty;
            name = null;
            OrgName = null;
            version = null;
            region = null;
            message = null;
            avatar = null;
            orgavatar = null;
            localip = null;
            nodeip = null;

            Counters.Clear();
            Counters = null;

            Ignored.Clear();
            Ignored = null;

            extended_props.Clear();
            extended_props = null;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Guid);
        }
    }
}
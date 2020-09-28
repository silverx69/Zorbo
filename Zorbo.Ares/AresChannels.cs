using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zorbo.Ares.Mars;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Channels;
using Zorbo.Ares.Packets.Formatters;
using Zorbo.Ares.Resources;
using Zorbo.Ares.Sockets;
using Zorbo.Core;
using Zorbo.Core.Data;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Models;
using Zorbo.Core.Server;

namespace Zorbo.Ares
{
    [JsonObject]
    public class AresChannels : ModelBase, IChannelList
    {
#pragma warning disable IDE0044 // Add readonly modifier
        IServer server = null;
        Timer udptimer = null;

        TimeSpan ticklength;
        DateTime lastmars;
        DateTime lastpurge;
        DateTime lastexpire;
        DateTime lastackinfo;

        AresUdpSocket socket = null;
        IPAddress externalIp = null;

        ModelList<AresChannel> channels = null;
        ModelList<SoftBan> banned = null;
        ModelList<FirewallTest> firewalltests = null;
        ModelList<AresChannel> myfirewalltests = null;

        uint addips = 0;
        uint ackips = 0;
        uint ackinfo = 0;
        uint requests = 0;
        uint checkfire = 0;

        bool firewall = false;
        bool firewallTest = false;
        bool isDownloading = false;

#pragma warning restore IDE0044 // Add readonly modifier

        volatile bool running = false;
        const int AddIpFrequency = 3; //seconds

        const string MarsList = "http://marsproject.net/ib0t/list.aspx";

        [JsonIgnore]
        public IServer Server {
            get { return server; }
            set { OnPropertyChanged(() => server, value); }
        }

        [JsonIgnore]
        public IPAddress ExternalIp {
            get { return externalIp; }
            set { OnPropertyChanged(() => externalIp, value); }
        }

        [JsonIgnore]
        public IOMonitor Monitor {
            get { return socket?.Monitor; }
        }

        IMonitor IChannelList.Monitor {
            get { return Monitor; }
        }

        public DateTime LastMars {
            get { return lastmars; }
            set { OnPropertyChanged(() => lastmars, value); }
        }

        [JsonProperty("addIpHits", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public uint AddIpHits {
            get { return addips; }
            set { OnPropertyChanged(() => addips, value); }
        }

        [JsonProperty("ackIpHits", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public uint AckIpHits {
            get { return ackips; }
            set { OnPropertyChanged(() => ackips, value); }
        }

        [JsonProperty("ackInfoHits", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public uint AckInfoHits {
            get { return ackinfo; }
            set { OnPropertyChanged(() => ackinfo, value); }
        }

        [JsonProperty("sendInfoHits", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public uint SendInfoHits {
            get { return requests; }
            set { OnPropertyChanged(() => requests, value); }
        }

        [JsonProperty("firewallHits", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public uint CheckFirewallHits {
            get { return checkfire; }
            set { OnPropertyChanged(() => checkfire, value); }
        }

        [JsonIgnore]
        public bool Listing {
            get { return Server?.Config.ShowChannel ?? false; }
            set {
                if (Server != null) {
                    Server.Config.ShowChannel = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public bool FirewallOpen {
            get { return firewall; }
            set { OnPropertyChanged(() => firewall, value); }
        }

        [JsonIgnore]
        public bool FinishedTestingFirewall {
            get { return firewallTest; }
            set { OnPropertyChanged(() => firewallTest, value); }
        }

        [JsonIgnore]
        public bool IsDownloading {
            get { return isDownloading; }
            set { OnPropertyChanged(() => isDownloading, value); }
        }

        [JsonIgnore]
        public ModelList<AresChannel> Channels {
            get { return channels; }
            set { OnPropertyChanged(() => channels, value); }
        }

        IObservableCollection<IChannel> IChannelList.Channels {
            get { return Channels.Cast<IChannel>(); }
        }

        [JsonIgnore]
        public DateTime LastSaved { get; set; }


        #region " Nested Classes "

        internal class SoftBan : IEquatable<IPAddress>
        {
            public IPAddress Address { get; set; }
            public DateTime BanTime { get; set; }

            public SoftBan() { }
            public SoftBan(IPAddress ip, DateTime time)
            {
                Address = ip;
                BanTime = time;
            }

            public bool Equals(IPAddress ip)
            {
                if (ip == null)
                    return false;

                return Address.Equals(ip);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as IPAddress);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Address);
            }
        }

        internal class FirewallTest
        {
            uint cookie;
            Socket socket;

            bool succeeded;


            public Boolean Succeeded {
                get { return succeeded; }
                private set { succeeded = value; }
            }

            public IPEndPoint RemoteEndPoint {
                get;
                private set;
            }

            public FirewallTest(IPEndPoint remoteEp)
            {
                RemoteEndPoint = remoteEp;
            }

            public FirewallTest(IPAddress address, ushort port)
                : this(new IPEndPoint(address, port)) { }


            public void Begin()
            {
                socket = SocketManager.CreateTcp();

                SocketConnectTask task = new SocketConnectTask(RemoteEndPoint);
                task.Completed += TestComplete;

                socket.QueueConnect(task);
            }

            private void TestComplete(object sender, IOTaskCompleteEventArgs<SocketConnectTask> e)
            {
                e.Task.Completed -= TestComplete;

                if (e.Task.Exception == null)
                    Succeeded = e.Task.Socket.Connected;

                Complete?.Invoke(this, EventArgs.Empty);

                socket.Destroy();
            }


            public override int GetHashCode()
            {
                if (cookie == 0)
                    cookie = (uint)base.GetHashCode();

                return (int)cookie;
            }

            public event EventHandler Complete;
        }

        #endregion


        public AresChannels()
        {
            LastSaved = DateTime.Now;
            LastMars = DateTime.Now.Subtract(TimeSpan.FromMinutes(8));

            udptimer = new Timer(new TimerCallback(OnTimer), null, -1, -1);
            ticklength = TimeSpan.FromSeconds(AddIpFrequency);

            banned = new ModelList<SoftBan>();
            myfirewalltests = new ModelList<AresChannel>();
            firewalltests = new ModelList<FirewallTest>();
        }


        void StartSocket(int port)
        {
            socket = new AresUdpSocket(new ChannelFormatter());

            socket.Exception += SocketException;
            socket.PacketReceived += UdpPacketReceived;

            socket.Bind(new IPEndPoint(IPAddress.Any, port));

            udptimer.Change(TimeSpan.Zero, ticklength);

            running = true;
            socket.ReceiveAsync();
        }


        public void Start(int port)
        {
            if (Channels.Count > 0)
                ticklength = TimeSpan.FromSeconds(AddIpFrequency);

            StartSocket(port);
        }

        public void Stop()
        {
            running = false;
            udptimer.Change(-1, -1);

            if (socket != null) {
                socket.Exception -= SocketException;
                socket.PacketReceived -= UdpPacketReceived;
                socket.Dispose();
                socket = null;
            }

            FirewallOpen = false;
            FinishedTestingFirewall = false;
        }


        public void DownloadFromMars() 
        {
            DownloadFromMarsAsync().RunSynchronously();
        }

        public async Task DownloadFromMarsAsync()
        {
            var request = WebRequest.CreateHttp(MarsList);

            request.Method = "GET";

            using var response = await request.GetResponseAsync();
            using var reader = new StreamReader(response.GetResponseStream());

            string temp = await reader.ReadToEndAsync();

            ChannelList list = Json.Deserialize<ChannelList>(temp);

            foreach (var channel in list.Items) {
                var c = Channels.Find(s => s.Equals(channel));
                if (c != null)
                    c.CopyFrom(channel);
                else
                    Channels.Add(new AresChannel() {
                        Name = channel.Name,
                        Topic = channel.Topic,
                        Port = (ushort)channel.Port,
                        ExternalIp = IPAddress.Parse(channel.ExternalIP),
                        LocalIp = IPAddress.Parse(channel.LocalIP),
                        WebSockets = true
                    });
            }
        }


        public void DownloadFromNetwork()
        {
            if (!IsDownloading) {
                IsDownloading = true;
                lastackinfo = DateTime.Now;
                ThreadPool.QueueUserWorkItem(DownloadThread, new AresUdpSocket(new ChannelFormatter()));
            }
        }

        private async void DownloadThread(object state)
        {
            var socket = (AresUdpSocket)state;

            socket.Exception += SocketException;
            socket.PacketReceived += UdpPacketReceived;

            socket.ReceiveAsync();

            int i = 0;
            while (IsDownloading) {
                DateTime now = DateTime.Now;
                Channels
                    .Skip(i)
                    .Take(10)
                    .ForEach(s => {
                        s.LastSendInfo = now;
                        socket.SendAsync(
                            new SendInfo(),
                            new IPEndPoint(s.ExternalIp, s.Port));
                    });

                i = Math.Min(Channels.Count, i + 10);
                await CheckDownloadTimeout(socket);
            }
        }

        private async Task CheckDownloadTimeout(AresUdpSocket socket)
        {
            DateTime now = DateTime.Now;

            if (now.Subtract(lastackinfo).TotalSeconds >= 30) {
                IsDownloading = false;
                try {
                    socket.Close();
                }
                catch {
                }
                socket.Dispose();
                DownloadComplete?.Invoke(this, EventArgs.Empty);
            }
            else await Task.Delay(1000);
        }


        private void OnTimer(object state)
        {
            if (running) {

                if (!FinishedTestingFirewall)
                    CheckFirewall();

                else if (Listing) {
                    AresChannel tosend = GetOldestContacted();

                    if (tosend != null) {
                        uint num = 6;
                        socket.SendAsync(
                            new AddIps(
                                (ushort)socket.LocalEndPoint.Port, 
                                GetSendServers(tosend.ExternalIp, ref num)),
                            new IPEndPoint(tosend.ExternalIp, tosend.Port));
                    }

                    DateTime now = DateTime.Now;

                    if (now.Subtract(lastmars).TotalMinutes >= 10) {
                        lastmars = now;
                        UpdateMars();
                    }

                    if (now.Subtract(lastexpire).TotalMinutes >= 1) {
                        lastexpire = now;
                        PurgeOld();
                    }

                    if (now.Subtract(lastpurge).TotalMinutes >= 10) {
                        lastpurge = now;
                        PurgeExceeding();
                    }
                }

                ExpireBans();
            }
        }

        private async void UpdateMars()
        {
            const string ib0t = "http://chatrooms.marsproject.net/ibot.aspx?proto=2";

            string data = string.Format(
                "local={0}&port={1}&name={2}&topic={3}",
                Server.InternalIp ?? ExternalIp,
                Server.Config.Port,
                Uri.EscapeDataString(Server.Config.Name),
                Uri.EscapeDataString(Server.Config.Topic));

            byte[] body = Encoding.UTF8.GetBytes(data);

            try {
                var request = WebRequest.CreateHttp(ib0t);
               
                request.Method = "POST";
                request.ContentLength = body.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                using var stream = await request.GetRequestStreamAsync();

                await stream.WriteAsync(body, 0, body.Length);
                await stream.FlushAsync();

                using var response = await request.GetResponseAsync();
            }
            catch(Exception ex) {//mars probably just timed out
                Logging.Error("AresChannels", ex);
                //try again in 5 minutes?
                lastmars = DateTime.Now.Subtract(TimeSpan.FromMinutes(5));
            }
        }

        private void CheckFirewall()
        {
            AresChannel channel = null;
            DateTime now = DateTime.Now;

            lock (Channels)
                channel = Channels.Find((s) => !s.ExternalIp.IsLocalAreaNetwork() && now.Subtract(s.LastAskedFirewall).TotalMinutes > 5);

            if (channel != null) {
                channel.LastAskedFirewall = now;
                IPAddress ip = channel.ExternalIp;

                lock (myfirewalltests) myfirewalltests.Add(channel);

                socket.SendAsync(
                    new CheckFirewallWanted(channel.Port),
                    new IPEndPoint(ip, channel.Port));
            }
            else { FinishedTestingFirewall = true; }
        }
        /*
        private bool CheckFloodCounter(IPacket packet, IPAddress address)
        {
            UdpFloodCounter counter;

            DateTime now = DateTime.Now;
            TimeSpan span = TimeSpan.FromMinutes(5);

            lock (counters) {
                counter = counters.Find(s => s.Address.Equals(address));

                if (counter == null) {
                    counter = new UdpFloodCounter(packet.Id, address, 0, now);
                    counters.Add(counter);
                }
            }

            if (now.Subtract(counter.Last) > span) {
                counter.Count = 0;
                counter.Last = now;
            }
            else if (++counter.Count >= 10) {
                Logging.Info("AresChannels",
                    "UDP Flood from '{0}'. Has sent packet '{1}' {2} times.",
                    address,
                    (AresUdpId)packet.Id,
                    counter.Count);
                //SOFT BAN SERVER
                lock (banned) banned.Add(new SoftBan(address, now));

                return false;
            }
            //else counter.Last = now;

            return true;
        }
        */
        private void PurgeOld()
        {
            DateTime now = DateTime.Now;

            lock (Channels)
                Channels.RemoveAll((s) =>
                    s.TryCount > 10 &&
                    now.Subtract(s.LastSendIPs).TotalMinutes >= 1 &&
                    now.Subtract(s.LastAcked).TotalHours >= 1);
        }

        private void PurgeExceeding()
        {
            if (Channels.Count < 1500)
                return;

            lock (Channels) {
                Channels.Sort((a, b) => (int)(b.AckCount - a.AckCount));

                while (Channels.Count > 1200) 
                    Channels.RemoveAt(Channels.Count - 1);
            }
        }


        private void ParseServers(byte[] input)
        {
            using var reader = new PacketReader(input);
            lock (Channels) {
                while (reader.Remaining >= 6) {

                    var ip = reader.ReadIPAddress();
                    ushort port = reader.ReadUInt16();

                    var channel = FindChannel(ip, port);

                    if (channel == null && CheckBanned(ip))
                        Channels.Add(new AresChannel(ip, port));
                }
            }
        }

        private bool CheckBanned(IPAddress ip)
        {
            lock (banned)
                if (banned.Contains((s) => s.Equals(ip)))
                    return true;
            return false;
        }

        private void ExpireBans()
        {
            DateTime now = DateTime.Now;

            lock (banned) {
                for (int i = (banned.Count - 1); i >= 0; i--) {
                    if (now.Subtract(banned[i].BanTime).TotalMinutes >= 40)
                        banned.RemoveAt(i);
                }
            }
        }


        private AresChannel GetOldestContacted()
        {
            AresChannel oldest = null;
            if (Channels.Count == 0) return oldest;

            DateTime now = DateTime.Now;

            foreach (var channel in Channels.ToArray()) {

                if (Equals(ExternalIp, channel.ExternalIp) || channel.ExternalIp.IsLocalAreaNetwork())
                    continue;

                double last = now.Subtract(channel.LastSendIPs).TotalMinutes;
                double old = now.Subtract(oldest?.LastSendIPs ?? now).TotalMinutes;

                if (last > 10 && last > old) oldest = channel;
            }

            if (oldest != null) {
                oldest.TryCount++;
                oldest.LastSendIPs = DateTime.Now;
            }

            return oldest;
        }

        private byte[] GetSendServers(IPAddress ip, ref uint count)
        {
            uint num = 0;
            using var writer = new PacketWriter();
            DateTime now = DateTime.Now;

            foreach (var s in Channels.ToArray()) {

                if (!s.ExternalIp.Equals(ip) &&
                    s.AckCount > 0 &&
                    now.Subtract(s.LastAcked).TotalMinutes < 15) {

                    num++;

                    writer.Write(s.ExternalIp);
                    writer.Write(s.Port);
                }

                if (num == count) break;
            }

            count = num;
            return writer.ToArray();
        }

        public bool IsCheckingMyFirewall(IPEndPoint endpoint)
        {
            if (!FirewallOpen) {
                lock (myfirewalltests) {

                    FinishedTestingFirewall = myfirewalltests.Contains((s) => s.ExternalIp.Equals(endpoint.Address));

                    if (FinishedTestingFirewall) {

                        FirewallOpen = true;
                        myfirewalltests.Clear();
                    }
                }

                return FirewallOpen;
            }
            else return false;
        }


        private void SocketException(object sender, ExceptionEventArgs e)
        {
            if (socket != null) socket.ReceiveAsync();
        }

        private void UdpPacketReceived(object sender, PacketEventArgs e)
        {
            try {
                HandleUdpPacket(e);
            }
            catch(Exception ex) {
                Logging.Error("AresChannels", ex);
            }
        }

        private void HandleUdpPacket(PacketEventArgs e)
        {
            if (CheckBanned(e.RemoteEndPoint.Address)) return;

            switch ((AresUdpId)e.Packet.Id) {
                case AresUdpId.OP_SERVERLIST_ACKINFO: {
                    AckInfo info = (AckInfo)e.Packet;

                    AckInfoHits++;
                    lastackinfo = DateTime.Now;

                    AresChannel channel = FindChannel(e.RemoteEndPoint.Address, info.Port);

                    if (channel == null)
                        if (e.RemoteEndPoint.Address.IsLocalMachine()) {
                            channel = Channels.Find(s => Equals(ExternalIp, s.ExternalIp));
                        }
                        else if (e.RemoteEndPoint.Address.IsLocalAreaNetwork()) {
                            channel = Channels.Find(s => Equals(ExternalIp, s.ExternalIp) && Equals(s.LocalIp, e.RemoteEndPoint.Address));
                        }

                    if (channel == null) {
                        //problem
                        throw new Exception("Bad things happened.");
                    }

                    channel.Port = info.Port;
                    channel.Users = info.Users;
                    channel.Name = info.Name;
                    channel.Topic = info.Topic;
                    channel.Language = info.Language;
                    channel.Version = info.Version;

                    ParseServers(info.Servers);
                }
                break;
                case AresUdpId.OP_SERVERLIST_ADDIPS: {
                    AddIps add = (AddIps)e.Packet;

                    AddIpHits++;

                    AresChannel channel = FindChannel(e.RemoteEndPoint.Address, add.Port);

                    if (channel != null)
                        channel.Port = add.Port;
                    else {
                        var address = e.RemoteEndPoint.Address;

                        if (address.IsLocalAreaNetwork())
                            address = ExternalIp ?? e.RemoteEndPoint.Address;
                        
                        channel = new AresChannel(address, add.Port);
                        Channels.Add(channel);
                    }

                    uint num = 6;
                    ParseServers(add.Servers);

                    socket.SendAsync(new AckIps() {
                        Port = channel.Port,
                        Servers = GetSendServers(server.ExternalIp, ref num),

                    }, e.RemoteEndPoint);
                }
                break;
                case AresUdpId.OP_SERVERLIST_ACKIPS: {
                    AckIps ips = (AckIps)e.Packet;

                    AckIpHits++;

                    AresChannel channel = FindChannel(e.RemoteEndPoint.Address, ips.Port);

                    if (channel != null) {
                        channel.Port = ips.Port;
                        channel.AckCount++;
                        channel.TryCount = 0;
                        channel.LastAcked = DateTime.Now;
                    }

                    ParseServers(ips.Servers);
                }
                break;
                case AresUdpId.OP_SERVERLIST_CHECKFIREWALLBUSY: {
                    CheckFirewallBusy busy = (CheckFirewallBusy)e.Packet;

                    lock (myfirewalltests)
                        myfirewalltests.RemoveAll((s) => s.ExternalIp.Equals(e.RemoteEndPoint.Address));

                    ParseServers(busy.Servers);
                }
                break;
                case AresUdpId.OP_SERVERLIST_PROCEEDCHECKFIREWALL: {
                    CheckFirewall check = (CheckFirewall)e.Packet;
                    FirewallTest test = null;

                    lock (firewalltests)
                        test = firewalltests.Find(s => s.GetHashCode() == check.Token);

                    if (test != null) {
                        test.RemoteEndPoint.Port = check.Port;
                        test.Begin();
                    }
                }
                break;
                case AresUdpId.OP_SERVERLIST_READYTOCHECKFIREWALL: {
                    CheckFirewallReady ready = (CheckFirewallReady)e.Packet;

                    if (!ready.Target.IsLocalAreaNetwork()) {
                        ExternalIp = ready.Target;
                        //Console.Write()
                        socket.SendAsync(new CheckFirewall() {
                            Port = (ushort)socket.LocalEndPoint.Port,
                            Token = ready.Token,

                        }, e.RemoteEndPoint);
                    }
                }
                break;
                case AresUdpId.OP_SERVERLIST_WANTCHECKFIREWALL: {
                    CheckFirewallWanted want = (CheckFirewallWanted)e.Packet;

                    CheckFirewallHits++;

                    if (firewalltests.Count < 5) {
                        FirewallTest test = new FirewallTest(e.RemoteEndPoint.Address, want.Port);

                        lock (firewalltests) firewalltests.Add(test);

                        socket.SendAsync(
                            new CheckFirewallReady((uint)test.GetHashCode(), e.RemoteEndPoint.Address),
                            e.RemoteEndPoint);
                    }
                    else {
                        uint num = 6;

                        socket.SendAsync(new CheckFirewallBusy() {
                            Port = (ushort)socket.LocalEndPoint.Port,
                            Servers = GetSendServers(e.RemoteEndPoint.Address, ref num)

                        }, e.RemoteEndPoint);
                    }
                }
                break;
                case AresUdpId.OP_SERVERLIST_SENDINFO: {
                    SendInfo sendInfo = (SendInfo)e.Packet;

                    SendInfoHits++;

                    if (Listing && Server != null) {

                        AckInfo ackinfo = new AckInfo() {
                            Language = Server.Config.Language,
                            Name = Server.Config.Name,
                            Topic = Server.Config.Topic,
                            Version = Strings.VersionChannels,
                            Port = (ushort)socket.LocalEndPoint.Port,
                            Users = (ushort)(Server.Users.Count + 1),
                        };

                        uint num = 6;

                        ackinfo.Servers = GetSendServers(e.RemoteEndPoint.Address, ref num);
                        ackinfo.ServersLen = (byte)num;

                        socket.SendAsync(ackinfo, e.RemoteEndPoint);
                    }
                }
                break;
                /* --- Used for P2P stuff - Ignore
                case AresUdpId.OP_SERVERLIST_SENDNODES: {
                    uint num = 20;
                    SendNodes sendnodes = (SendNodes)e.Packet;

                    SendNodeHits++;

                    if (Listing) {

                        AckNodes acknodes = new AckNodes() {
                            Port = (ushort)socket.LocalEndPoint.Port,
                            Servers = GetSendServers(e.RemoteEndPoint.Address, ref num)
                        };

                        socket.SendAsync(acknodes, e.RemoteEndPoint);
                    }
                }
                break;
                case AresUdpId.OP_SERVERLIST_ACKNODES: {
                    AckNodes ackNodes = (AckNodes)e.Packet;

                    AckNodeHits++;
                    ParseServers(ackNodes.Servers);
                }
                break;
                */
            }
        }

        private void FirewallTestComplete(object sender, EventArgs e)
        {
            FirewallTest test = (FirewallTest)sender;
            lock (firewalltests) firewalltests.Remove(test);
        }

        private AresChannel FindChannel(IPAddress ip, ushort port)
        {
            return Channels.Find((s) => s.ExternalIp.Equals(ip) && s.Port.Equals(port));
        }

        public void LoadFromDatFile() {
            Assembly asm = Assembly.GetExecutingAssembly();

            using var stream = asm.GetManifestResourceStream("Zorbo.Ares.Servers.dat");

            byte size = 6;
            while (stream.Position < stream.Length) {

                byte[] b = new byte[size];
                stream.Read(b, 0, size);

                uint ip = BitConverter.ToUInt32(b, 0);
                ushort port = BitConverter.ToUInt16(b, 4);
                uint ackcount = 1;

                if (size >= 8)
                    ackcount = BitConverter.ToUInt16(b, 6);

                if (ip == 0) {
                    size = b[4];
                    continue;
                }

                if (port == 0) continue;
                Channels.Add(new AresChannel(ip, port) { AckCount = ackcount });
            }
        }

        public event EventHandler DownloadComplete;
    }
}
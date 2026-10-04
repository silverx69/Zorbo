using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Zorbo.Chat.Messages;
using Zorbo.Chat.Server.Database;
using Zorbo.Collections;
using Zorbo.Data;
using Zorbo.Net;
using Zorbo.Net.Messages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Zorbo.Chat.Server
{
    public partial class ChatServer : Observable, IDisposable, IAsyncDisposable
    {
        ZorboSocket listener;
        X509Certificate2 certificate;

        List<IPAddress> localAddresses;

        readonly SortedStack<ushort> idPool;
        readonly ObservableList<Pending> pending;
        readonly ObservableList<ChatClient> clients;

        public IPEndPoint LocalEndPoint {
            get { return listener?.LocalEndPoint; }
        }

        protected PooledDbContextFactory<Database> DbFactory {
            get;
            set;
        }

        class Pending :
            IEquatable<Pending>,
            IEquatable<ZorboSocket>,
            IEquatable<Socket>,
            IDisposable,
            IAsyncDisposable 
        {
            public ZorboSocket Socket { get; set; }
            public DateTime JoinTime { get; set; }

            public Pending() {
                JoinTime = DateTime.Now;
            }

            public Pending(ZorboSocket socket)
                : this() {
                Socket = socket;
            }

            public bool Equals(Socket other) {
                return ReferenceEquals(Socket.Socket, other);
            }

            public bool Equals(ZorboSocket other) {
                return Equals(other?.Socket);
            }

            public bool Equals(Pending other) {
                return Equals(other?.Socket);
            }

            public override bool Equals(object obj) {
                if (obj is Socket socket)
                    return Equals(socket);

                if (obj is ZorboSocket zsocket)
                    return Equals(zsocket);

                return Equals(obj as Pending);
            }

            public override int GetHashCode() {
                return HashCode.Combine(Socket);
            }

            public void Dispose() {
                Socket?.Dispose();
            }

            public ValueTask DisposeAsync() {
                return Socket?.DisposeAsync() ?? ValueTask.CompletedTask;
            }
        }

        public ChatServer() {
            idPool = [];
            clients = [];
            pending = [];
        }

        public void Start() {
            var options = new DbContextOptionsBuilder<Database>()
                .UseSqlite("Data Source=Zorbo.Chat.db")
                .Options;

            DbFactory = new PooledDbContextFactory<Database>(options);
            OnPropertyChanged(nameof(DbFactory));

            using var ctx = DbFactory.CreateDbContext();
            ctx.Database.EnsureCreated();

            if (!ctx.Channels.Any())
                Channels.Add(ctx, new("general, true")).Wait();

            localAddresses = Network.GetLocalAddresses();

            CreateListener();
        }

        public async Task StartAsync() {
            var options = new DbContextOptionsBuilder<Database>()
                .UseSqlite("Data Source=Zorbo.Chat.db")
                .Options;

            DbFactory = new PooledDbContextFactory<Database>(options);
            OnPropertyChanged(nameof(DbFactory));

            using var ctx = DbFactory.CreateDbContext();
            await ctx.Database.EnsureCreatedAsync();

            if (!ctx.Channels.Any())
                await Channels.Add(ctx, new("general", true));

            localAddresses = await Network.GetLocalAddressesAsync();

            CreateListener();
        }

        private void CreateListener() {
            listener = new ZorboSocket();

            listener.IsSecureSocket = true;
            listener.Certificate = GenerateCertificate();

            listener.Accepted += this.OnListenerAccepted;
            listener.Rejected += this.OnListenerRejected;
            listener.Exception += this.OnListenerException;

            listener.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
            OnPropertyChanged(nameof(LocalEndPoint));

            listener.Listen();
        }

        public void Stop() {
            listener?.Dispose();
            listener = null;

            pending.ForEach(s => s.Dispose());
            pending.Clear();

            clients.ForEach(s => s.Dispose());
            clients.Clear();

            certificate?.Dispose();
            certificate = null;
        }

        public async Task StopAsync() {
            if (listener is not null) {
                await listener.DisposeAsync();
                listener = null;
            }
            
            foreach (var s in pending)
                await s.DisposeAsync();
            pending.Clear();
            
            foreach (var s in clients)
                await s.DisposeAsync();
            clients.Clear();

            certificate?.Dispose();
            certificate = null;
        }

        public void Send(Predicate<ChatClient> predicate, object message) {
            foreach (var client in clients)
                if (predicate(client)) client.Send(message);
        }

        private Task OnListenerAccepted(ZorboSocket sender, AcceptEventArgs e) {
            var client = e.Socket;

            client.Received += OnPendingReceived;
            client.HttpRequest += OnPendingHttpRequest;
            client.Exception += OnPendingException;
            client.Disconnected += OnPendingDisconnected;

            pending.Add(new Pending(client));

            client.Receive();
            return Task.CompletedTask;
        }

        private Task OnListenerRejected(ZorboSocket sender, RejectedEventArgs e) {
            // log?
            return Task.CompletedTask;
        }

        private Task OnListenerException(ZorboSocket sender, ExceptionEventArgs e) {
            //
            return Task.CompletedTask;
        }

        #region " Pending Connections "

        private async Task OnPendingReceived(ZorboSocket sender, MessageEventArgs e) {
            // must be a login message
            if (e.Message is not ClientLogin login) {
                sender.Disconnect(CloseStatus.PolicyViolation);
                return;
            }

            pending.Remove(s => s.Equals(sender));

            if (MessageValidator.Validate(login, out var results)) {
                sender.Received -= OnPendingReceived;
                sender.HttpRequest -= OnPendingHttpRequest;
                sender.Exception -= OnPendingException;
                sender.Disconnected -= OnPendingDisconnected;

                var client = new ChatClient(sender, e.MessageType);

                client.Received += OnClientReceived;
                client.HttpRequest += OnClientHttpRequest;
                client.Exception += OnClientException;
                client.Disconnected += OnClientDisconnected;

                await OnClientLogin(client, login);
            }
            else {
                sender.Send(new ServerError(results[0].ErrorMessage), e.MessageType);
                sender.Disconnect(CloseStatus.ProtocolError);
            }
        }

        private Task OnPendingHttpRequest(ZorboSocket sender, HttpRequestEventArgs e) {
            // here we would properly handle requests
            // access uri field with e.Resource
            switch(e.Method) {
                case "GET":
                case "HEAD":
                case "POST":
                    break;
            }

            //the byte[] overload will also send raw, but I think this shows intent
            sender.Send(new ZorboSocket.Raw(HttpHelper.ResponseHeaderBytes(HttpStatusCode.MethodNotAllowed)));
            sender.Disconnect();

            return Task.CompletedTask;
        }
        
        private Task OnPendingException(ZorboSocket sender, ExceptionEventArgs e) {
            // log?
            return Task.CompletedTask;
        }

        private async Task OnPendingDisconnected(ZorboSocket sender, DisconnectEventArgs e) {
            pending.Remove(s => s.Equals(sender));
            await sender.DisposeAsync();
        }

        #endregion

        private async Task OnClientLogin(ChatClient sender, ClientLogin login) {
            using var ctx = await DbFactory.CreateDbContextAsync();

            var candidate = new Profile() {
                Guid = login.Guid.ToString(),
                Username = login.Username,//still needs sanitizing
                Address = sender.RemoteEndPoint.Address.ToString()
            };

            var profile = await Profiles.Find(ctx, candidate);

            if (profile is null)
                profile = await Profiles.Add(ctx, candidate);

            else if (profile.Banned) {
                sender.Send(new ServerError("You are banned from this server."));
                sender.Disconnect(CloseStatus.PolicyViolation);
                return;
            }
            else {
                profile.Guid = candidate.Guid;
                profile.Username = candidate.Username;
                profile.Address = candidate.Address;
                profile = await Profiles.Update(ctx, profile);
            }

            sender.Profile = profile;
            sender.Channel = await Channels.Default(ctx);
            sender.IsLocalHost = localAddresses.Any(s => s.Equals(sender.RemoteEndPoint.Address));

            clients.Add(sender);

            sender.Send(new ServerLoginAck() {
                Username = sender.Profile.Username,
                Flags = ServerSupportFlags.ALL,
                Version = "Zorbo Server 1.0"
            });

            sender.Send(new ServerDetails() {
                Name = "Zorbo Test Server",
                Topic = "Welcome to my Zorbo server!"
            });

            foreach(var client in clients) {
                if (client.Channel.Equals(sender.Channel)) {
                    if (client != sender)
                        client.Send(new ServerJoined(sender.Profile));
                    sender.Send(new ServerUserlistItem(client.Profile));
                }
            }
        }

        private async Task OnClientReceived(ChatClient sender, ChatMessageEventArgs e) {
            switch (e.MessageId) {
                case MessageId.CLIENT_ADMIN:
                    break;
                case MessageId.CLIENT_UPDATE:
                    break;
                case MessageId.CLIENT_PUBLIC: {
                    var textMsg = e.MessageAs<ClientPublic>();

                    using var ctx = await DbFactory.CreateDbContextAsync();
                    Message msg = await Messages.Add(ctx, new() {
                        ChannelId = sender.Channel.Id,
                        SenderId = sender.Profile.Id,
                        Type = DbMessageType.Public,
                        Content = textMsg.Message
                    });

                    Send((s) => s.Channel == sender.Channel,
                        new ServerPublic() {
                            Id = msg.Id,
                            Sender = sender.Profile.Username,
                            Message = textMsg.Message
                        }
                    );
                    break;
                }
                case MessageId.CLIENT_PRIVATE: {
                    var privMsg = e.MessageAs<ClientPrivate>();
                    var receiver = clients.FirstOrDefault(s => s.Profile.Username == privMsg.Target);

                    if (receiver is null)
                        sender.Send(new ServerPrivateError() {  
                            Target = privMsg.Target,
                            Code = PrivateError.Offline
                        });
                    else {
                        using var ctx = await DbFactory.CreateDbContextAsync();
                        Message msg = await Messages.Add(ctx, new() {
                            SenderId = sender.Profile.Id,
                            ReceiverId = receiver.Profile.Id,
                            Type = DbMessageType.Private,
                            Content = privMsg.Message
                        });

                        receiver.Send(new ServerPrivate() {
                            Id = msg.Id,
                            Sender = sender.Profile.Username,
                            Message = privMsg.Message
                        });
                    }
                    break;
                }
                default:
                    // unknown client message, disconnect
                    sender.Disconnect(CloseStatus.InvalidMessageType);
                    break;
            }
            Console.WriteLine("{0} {1}", e.MessageId, JsonSerializer.Serialize(e.Message));
        }

        private Task OnClientHttpRequest(ChatClient sender, HttpRequestEventArgs e) {
            // why is a logged in chatclient sending http requests?
            // is this an error? should we just serve it?
            return Task.CompletedTask;
        }

        private Task OnClientException(ChatClient sender, ExceptionEventArgs e) {
            //
            return Task.CompletedTask;
        }

        private async Task OnClientDisconnected(ChatClient sender, DisconnectEventArgs e) {
            clients.Remove(sender);
            await sender.DisposeAsync();
        }

        public void Dispose() {
            Stop();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync() {
            await StopAsync();
            GC.SuppressFinalize(this);
        }

        private static X509Certificate2 GenerateCertificate() {
            return Certificates.Generate(new CertGenerationOptions() {
                Name = "ZorboTest",
                LocalAddresses = Network.GetLocalAddresses(),
                PrivateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.pfx"),
                PublicFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.pub.cer")
            });
        }
    }
}

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

        PooledDbContextFactory<Database> factory;

        public IPEndPoint LocalEndPoint {
            get { return listener?.LocalEndPoint; }
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

            factory = new PooledDbContextFactory<Database>(options);

            using var database = factory.CreateDbContext();
            database.Database.EnsureCreated();

            if (!database.Channels.Any()) {
                database.Channels.Add(new() { 
                    Name = "general",
                    Created = DateTime.UtcNow
                });
                database.SaveChanges();
            }

            localAddresses = Network.GetLocalAddresses();

            CreateListener();
        }

        public async Task StartAsync() {
            var options = new DbContextOptionsBuilder<Database>()
                .UseSqlite("Data Source=Zorbo.Chat.db")
                .Options;

            factory = new PooledDbContextFactory<Database>(options);

            using var database = factory.CreateDbContext();
            await database.Database.EnsureCreatedAsync();

            if (!database.Channels.Any()) {
                database.Channels.Add(new() {
                    Name = "general",
                    Default = true,
                    Created = DateTime.UtcNow
                });
                await database.SaveChangesAsync();
            }

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
            client.Exception += OnPendingException;
            client.Disconnected += OnPendingDisconnected;

            pending.Add(new Pending(client));
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
            Console.WriteLine("Chat server received:\r\n{0} {1}", (MessageId)e.Id, JsonSerializer.Serialize(e.Message));

            if (e.Message is not ClientLogin login) {
                sender.Disconnect(CloseStatus.PolicyViolation);
                return;
            }

            pending.Remove(s => s.Equals(sender));

            if (MessageValidator.Validate(login, out var results)) {
                sender.Received -= OnPendingReceived;
                sender.Exception -= OnPendingException;
                sender.Disconnected -= OnPendingDisconnected;

                var client = new ChatClient(sender, e.MessageType);

                client.Received += OnClientReceived;
                client.Exception += OnClientException;
                client.Disconnected += OnClientDisconnected;

                await OnClientLogin(client, login);
            }
            else {
                sender.Send(new ServerError(results[0].ErrorMessage), e.MessageType);
                sender.Disconnect(CloseStatus.ProtocolError);
            }

            return;
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
            //login.UserName = SanitizeUserName(login.UserName);

            using var database = await factory.CreateDbContextAsync();

            var newProfile = new DbProfile() {
                Guid = login.Guid.ToString(),
                UserName = login.UserName,
                Address = sender.RemoteEndPoint.Address.ToString(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            var profile = await Profiles.Find(database, newProfile);

            if (profile is null) {
                profile = newProfile;
                database.Profiles.Add(profile);
                await database.SaveChangesAsync();
            }
            else if (profile.Banned) {
                sender.Send(new ServerError("You are banned from this server."));
                sender.Disconnect(CloseStatus.PolicyViolation);
                return;
            }
            else {
                profile.Guid = newProfile.Guid;
                profile.UserName = newProfile.UserName;
                profile.Address = newProfile.Address;
                profile.Updated = DateTime.UtcNow;
                database.Profiles.Update(profile);
                await database.SaveChangesAsync();
            }

            sender.Profile = profile;
            sender.Channel = database.Channels.FirstOrDefault(s => s.Default);
            sender.Channel ??= database.Channels.First();

            sender.IsLocalHost = localAddresses.Any(s => s.Equals(sender.RemoteEndPoint.Address));

            clients.Add(sender);

            sender.Send(new ServerLoginAck() {
                UserName = sender.Profile.UserName,
                Flags = ServerSupportFlags.ALL,
                Version = "Zorbo Server 1.0"
            });

            sender.Send(new ServerDetails() {
                Name = "Zorbo Test Server",
                Topic = "Welcome to my Zorbo server!"
            });
        }

        private async Task OnClientReceived(ChatClient sender, ChatMessageEventArgs e) {

            Console.WriteLine("Chat server received:\r\n{0} {1}", e.MessageId, JsonSerializer.Serialize(e.Message));

            switch (e.MessageId) {
                case MessageId.CLIENT_ADMIN:
                    break;
                case MessageId.CLIENT_UPDATE:
                    break;
                case MessageId.CLIENT_PUBLIC: {
                    DbMessage msg;
                    var pub = (ClientPublic)e.Message;
                    using var database = await factory.CreateDbContextAsync();
                    database.Messages.Add(msg = new() {
                        ChannelId = sender.Channel.Id,
                        SenderId = sender.Profile.Id,
                        Content = sender.IsBinary ?
                            await e.Reader.ToArrayAsync() :
                            await BinarySerializer.SerializeAsync(e.Message),
                        Created = DateTime.UtcNow
                    });
                    await database.SaveChangesAsync();
                    Send(
                        (s) => s.Channel == sender.Channel,
                        new ServerPublic() {
                            Id = msg.Id,
                            Sender = sender.Profile.UserName,
                            Message = pub.Message
                        }
                    );
                    break;
                }
                case MessageId.CLIENT_PRIVATE: {
                    DbMessage msg;
                    var priv = (ClientPrivate)e.Message;
                    using var database = await factory.CreateDbContextAsync();
                    database.Messages.Add(msg = new() {
                        SenderId = sender.Profile.Id,
                        Content = sender.IsBinary ?
                            await e.Reader.ToArrayAsync() : 
                            await BinarySerializer.SerializeAsync(e.Message),
                        Created = DateTime.UtcNow
                    });
                    await database.SaveChangesAsync();
                    Send(
                        (s) => s.Profile.UserName == priv.Target,
                        new ServerPrivate() {
                            Id = msg.Id,
                            Sender = sender.Profile.UserName,
                            Message = priv.Message
                        }
                    );
                    break;
                }
                default:
                    // unknown client message, disconnect
                    sender.Disconnect(CloseStatus.InvalidMessageType);
                    break;
            }
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

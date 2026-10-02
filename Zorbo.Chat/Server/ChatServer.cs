using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Zorbo.Chat.Messages;
using Zorbo.Collections;
using Zorbo.Data;
using Zorbo.Net;
using Zorbo.Net.Messages;

namespace Zorbo.Chat.Server
{
    public class ChatServer : Observable 
    {
        ZorboSocket listener;
        X509Certificate2 certificate;

        SortedStack<ushort> idPool;
        ObservableList<Pending> pending;
        ObservableList<ChatClient> clients;

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
                return Equals(other.Socket);
            }

            public bool Equals(Pending other) {
                return Equals(other.Socket);
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
            listener = new ZorboSocket();

            listener.IsSecureSocket = true;
            listener.Certificate = GenerateCertificate();

            listener.Accepted += this.Listener_Accepted;
            listener.Rejected += this.Listener_Rejected;
            listener.Exception += this.Listener_Exception;

            listener.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
            listener.Listen();
        }

        public void Stop() {
            pending.ForEach(s => s.Dispose());
            pending.Clear();

            clients.ForEach(s => s.Dispose());
            clients.Clear();

            listener?.Dispose();
            listener = null;

            certificate?.Dispose();
            certificate = null;
        }

        public void Send(object message, Predicate<ChatClient> predicate) {
            foreach (var client in clients)
                if (predicate(client)) client.Send(message);
        }

        private void Listener_Accepted(ZorboSocket sender, AcceptEventArgs e) {
            var client = e.Socket;

            client.Received += Pending_Received;
            client.Exception += Pending_Exception;
            client.Disconnected += Pending_Disconnected;

            pending.Add(new Pending(client));
        }

        private void Listener_Rejected(ZorboSocket sender, RejectedEventArgs e) {
            // log?
        }

        private void Listener_Exception(ZorboSocket sender, ExceptionEventArgs e) {
            //
        }

        #region " Pending Connections "

        private void Pending_Received(ZorboSocket sender, MessageEventArgs e) {
            // must be a login message
            Console.WriteLine("Chat server received:\r\n{0} {1}", (MessageId)e.Id, JsonSerializer.Serialize(e.Message));

            if (e.Message is not ClientLogin login) {
                sender.Disconnect(CloseStatus.PolicyViolation);
                return;
            }

            if (!MessageValidator.Validate(login, out var results)) {
                sender.Send(new ServerError(results[0].ErrorMessage), e.MessageType);
                sender.Disconnect(CloseStatus.ProtocolError);
                return;
            }

            sender.Received -= Pending_Received;
            sender.Exception -= Pending_Exception;
            sender.Disconnected -= Pending_Disconnected;

            var client = new ChatClient(sender, e.MessageType);

            client.Received += Client_Received;
            client.Exception += Client_Exception;
            client.Disconnected += Client_Disconnected;

            clients.Add(client);
            pending.Remove(s => s.Equals(sender));

            Client_Login(client, login);
        }

        private void Pending_Exception(ZorboSocket sender, ExceptionEventArgs e) {
            // log?
        }

        private async void Pending_Disconnected(ZorboSocket sender, DisconnectEventArgs e) {
            pending.Remove(s => s.Equals(sender));
            await sender.DisposeAsync();
        }

        #endregion

        private void Client_Login(ChatClient sender, ClientLogin login) {
            sender.Guid = login.Guid;
            sender.UserName = login.UserName;

            // check bans etc.,

            sender.Send(new ServerLoginAck() {
                UserName = sender.UserName,
                Flags = ServerSupportFlags.ALL,
                Version = "Zorbo Server 1.0"
            });

            sender.Send(new ServerDetails() {
                Name = "Zorbo Test Server",
                Topic = "Welcome to my Zorbo server!"
            });
        }

        private void Client_Received(ChatClient sender, MessageEventArgs e) {

            Console.WriteLine("Chat server received:\r\n{0} {1}", (MessageId)e.Id, JsonSerializer.Serialize(e.Message));

            switch ((MessageId)e.Id) {
                case MessageId.CLIENT_ADMIN:
                    break;
                case MessageId.CLIENT_UPDATE:
                    break;
                case MessageId.CLIENT_PUBLIC:
                    break;
                case MessageId.CLIENT_PRIVATE:
                    break;
                default:
                    // unknown client message, disconnect
                    sender.Disconnect(CloseStatus.InvalidMessageType);
                    break;
            }
        }

        private void Client_Exception(ChatClient sender, ExceptionEventArgs e) {
            //
        }

        private async void Client_Disconnected(ChatClient sender, DisconnectEventArgs e) {
            clients.Remove(sender);
            await sender.DisposeAsync();
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

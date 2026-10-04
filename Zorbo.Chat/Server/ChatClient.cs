using System.ComponentModel.DataAnnotations;
using System.Net;
using Zorbo.Chat.Messages;
using Zorbo.Net;
using Zorbo.Net.Messages;

namespace Zorbo.Chat.Server
{
    public class ChatClient : IDisposable, IAsyncDisposable
    {
        public bool IsBinary {
            get { return MessageType == MessageType.Binary; }
        }

        public bool IsLocalHost {
            get;
            internal set;
        }

        public ZorboSocket Socket {
            get;
            private set;
        }

        public MessageType MessageType {
            get { return Socket?.MessageType ?? MessageType.Binary; }
        }

        public Database.Profile Profile {
            get;
            internal set;
        }

        public Database.Channel Channel {
            get;
            internal set;
        }

        public IPEndPoint LocalEndPoint {
            get { return Socket?.LocalEndPoint; }
        }

        public IPEndPoint RemoteEndPoint {
            get { return Socket?.RemoteEndPoint; }
        }

        public ChatClient(ZorboSocket socket, MessageType messageType) {
            ArgumentNullException.ThrowIfNull(socket, nameof(socket));
            Socket = socket;
            Socket.Received += OnReceived;
            Socket.HttpRequest += OnHttpRequest;
            Socket.Exception += OnException;
            Socket.Disconnected += OnDisconnected;
            Socket.MessageType = messageType;
        }

        public void Send(object message) {
            if (MessageValidator.Validate(message, out var results))
                Socket?.Send(message);
            else
                // if we're sending an invalid message, throw an exception
                throw new ValidationException(results[0].ErrorMessage);
        }
        
        protected virtual Task OnReceived(ZorboSocket sender, MessageEventArgs e) {
            Socket.MessageType = e.MessageType;

            if (!MessageValidator.Validate(e.Message, out var results)) {
                Send(new ServerError(results[0].ErrorMessage));
                Disconnect();
                return Task.CompletedTask;
            }

            return Received?.Invoke(this, new(e)) ?? Task.CompletedTask;
        }

        protected virtual Task OnHttpRequest(ZorboSocket sender, HttpRequestEventArgs e) {
            return HttpRequest?.Invoke(this, e) ?? Task.CompletedTask;
        }

        protected virtual Task OnException(ZorboSocket sender, ExceptionEventArgs e) {
            return Exception?.Invoke(this, e) ?? Task.CompletedTask;
        }

        protected virtual Task OnDisconnected(ZorboSocket sender, DisconnectEventArgs e) {
            return Disconnected?.Invoke(this, e) ?? Task.CompletedTask;
        }

        public void Disconnect(CloseStatus closeStatus = CloseStatus.NormalClosure) {
            Socket?.Disconnect(closeStatus);
        }

        public void Dispose() {
            if (Socket is not null) {
                Socket.Received -= OnReceived;
                Socket.Exception -= OnException;
                Socket.Disconnected -= OnDisconnected;
                Socket.Dispose();
                Socket = null;
            }
            Received = null;
            Exception = null;
            Disconnected = null;
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync() {
            if (Socket is not null) {
                Socket.Received -= OnReceived;
                Socket.Exception -= OnException;
                Socket.Disconnected -= OnDisconnected;
                await Socket.DisposeAsync();
                Socket = null;
            }
            Received = null;
            Exception = null;
            Disconnected = null;
            GC.SuppressFinalize(this);
        }

        public event ClientEventHandler<ChatMessageEventArgs> Received;
        public event ClientEventHandler<HttpRequestEventArgs> HttpRequest;
        public event ClientEventHandler<ExceptionEventArgs> Exception;
        public event ClientEventHandler<DisconnectEventArgs> Disconnected;
    }

    public class ChatMessageEventArgs : MessageEventArgs
    {
        public MessageId MessageId {
            get { return (MessageId)base.Id; }
        }

        public ChatMessageEventArgs(MessageEventArgs e)
            : base(e.Id, e.Message, e.MessageType) { }
    }

    public delegate Task ClientEventHandler<T>(ChatClient sender, T e) where T : SocketEventArgs;
}

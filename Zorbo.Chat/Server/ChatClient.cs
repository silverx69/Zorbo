using System.ComponentModel.DataAnnotations;
using Zorbo.Chat.Messages;
using Zorbo.Net;
using Zorbo.Net.Messages;

namespace Zorbo.Chat.Server
{
    public class ChatClient : IDisposable, IAsyncDisposable
    {
        public Guid Guid {
            get;
            set;
        }

        public string UserName {
            get;
            set;
        }

        public MessageType MessageType {
            get;
            set;
        }

        public ZorboSocket Socket {
            get;
            private set;
        }

        public ChatClient(ZorboSocket socket, MessageType messageType) {
            ArgumentNullException.ThrowIfNull(socket, nameof(socket));
            Socket = socket;
            Socket.Received += OnReceived;
            Socket.Exception += OnException;
            Socket.Disconnected += OnDisconnected;
            MessageType = messageType;
        }

        public void Send(object message) {
            if (MessageValidator.Validate(message, out var results))
                Socket?.Send(message, MessageType);
            else
                // if we're sending an invalid message, throw an exception
                throw new ValidationException(results[0].ErrorMessage);
        }
        
        protected virtual void OnReceived(ZorboSocket sender, MessageEventArgs e) {
            MessageType = e.MessageType;
            if (MessageValidator.Validate(e.Message, out var results))
                Received?.Invoke(this, e);
            else {
                Send(new ServerError(results[0].ErrorMessage));
                Disconnect();
            }
        }

        protected virtual void OnException(ZorboSocket sender, ExceptionEventArgs e) {
            Exception?.Invoke(this, e);
        }

        protected virtual void OnDisconnected(ZorboSocket sender, DisconnectEventArgs e) {
            Disconnected?.Invoke(this, e);
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

        public event ClientEventHandler<MessageEventArgs> Received;
        public event ClientEventHandler<ExceptionEventArgs> Exception;
        public event ClientEventHandler<DisconnectEventArgs> Disconnected;
    }

    public delegate void ClientEventHandler<T>(ChatClient sender, T e) where T : SocketEventArgs;
}

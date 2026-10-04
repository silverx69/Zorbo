using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Zorbo.Data;
using Zorbo.Net.Messages;

namespace Zorbo.Net
{
    public enum OpCode : byte
    {
        Continuation,
        Text = 0x1,
        Binary = 0x2,
        Close = 0x8,
        Ping = 0x9,
        Pong = 0xA
    }

    public enum MessageType : byte
    {
        Text,
        Binary
    }

    public enum CloseStatus
    {
        NormalClosure = 1000,
        EndpointUnavailable = 1001,
        ProtocolError = 1002,
        InvalidMessageType = 1003,
        Empty = 1005,
        InvalidPayloadData = 1007,
        PolicyViolation = 1008,
        MessageTooBig = 1009,
        MandatoryExtension = 1010,
        InternalServerError = 1011
    }

    public interface IZorboSocket : IDisposable, IAsyncDisposable
    {
        Socket Socket { get; }

        Uri RemoteUri { get; }

        bool IsConnected { get; }
        bool IsListening { get; }

        bool IsWebSocket { get; set; }
        bool IsSecureSocket { get; set; }

        int MaxFrameSize { get; set; }
        int MaxMessageSize { get; set; }

        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }

        IMonitor Monitor { get; }
        IMessageConverter Converter { get; }

        X509Certificate Certificate { get; set; }
        RemoteCertificateValidationCallback CertificateCallback { get; set; }

        void Bind(IPEndPoint endpoint);
        void Listen(int backlog = 100);

        void Connect(Uri uri);
        void Connect(string host, int port);
        void Connect(IPAddress ip, int port);
        void Connect(IPEndPoint endpoint);

        void Send(object message);
        void Send(object message, MessageType type);

        void Disconnect();
        void Disconnect(CloseStatus status);

        void Close();
        Task CloseAsync();

        event SocketEventHandler<AcceptEventArgs> Accepted;
        event SocketEventHandler<RejectedEventArgs> Rejected;
        event SocketEventHandler<ConnectEventArgs> Connected;
        event SocketEventHandler<MessageEventArgs> Received;
        event SocketEventHandler<HttpRequestEventArgs> HttpRequest;
        event SocketEventHandler<ExceptionEventArgs> Exception;
        event SocketEventHandler<DisconnectEventArgs> Disconnected;
    }

    public class SocketEventArgs : EventArgs
    {
        new public static readonly SocketEventArgs Empty = new();
    }

    public class AcceptEventArgs(ZorboSocket socket) : SocketEventArgs
    {
        public ZorboSocket Socket { get; private set; } = socket;
    }

    public class RejectedEventArgs(ZorboSocket socket, Exception ex) : SocketEventArgs
    {
        public ZorboSocket Socket { get; private set; } = socket;

        public Exception Exception { get; private set; } = ex;
    }

    public class ConnectEventArgs : SocketEventArgs
    {
        new public static readonly ConnectEventArgs Empty = new();
    }

    public class DisconnectEventArgs(CloseStatus status) : SocketEventArgs
    {
        public CloseStatus Status { get; private set; } = status;
    }

    public class ExceptionEventArgs(Exception ex) : SocketEventArgs
    {
        public Exception Exception { get; private set; } = ex;
    }

    public class MessageEventArgs : SocketEventArgs
    {
        public ushort Id { get; private set; }

        public object Message { get; private set; }

        public MessageType MessageType { get; private set; }

        public MessageEventArgs() { }

        public MessageEventArgs(ushort id, object message, MessageType msgType) {
            Id = id;
            Message = message;
            MessageType = msgType;
        }

        /// <summary>
        /// Simple helper method to cast the Message object to the specified type.
        /// </summary>
        /// <exception cref="InvalidCastException"></exception>
        public T MessageAs<T>() { return (T)Message; }
    }

    public class HttpRequestEventArgs : SocketEventArgs
    {
        readonly ZBinaryReader content;
        readonly RequestMetadata request;

        public string Method { get { return request.Method; } }

        public string Resource { get { return request.Resource; } }

        public string Protocol { get { return request.Protocol; } }

        public Dictionary<string, string> Headers { get { return request.Headers; } }

        public ZBinaryReader Content { get { return content; } }


        public HttpRequestEventArgs() { }

        public HttpRequestEventArgs(RequestMetadata request, ZBinaryReader content) {
            this.request = request;
            this.content = content;
        }
    }

    public delegate Task SocketEventHandler<T>(ZorboSocket sender, T e) where T : SocketEventArgs;
}

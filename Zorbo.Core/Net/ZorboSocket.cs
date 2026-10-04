using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Zorbo.Data;
using Zorbo.Net.Messages;

namespace Zorbo.Net
{
    public class ZorboSocket : IZorboSocket
    {
        Socket socket;

        Guid sessionGuid;
        Handshake handshake;

        MessageType defaultMsgType = MessageType.Binary;
        MessageType incomingMsgType;

        bool upgradeRequired;
        bool continueMessage;

        readonly bool isClient = true;
        volatile bool isReading;
        volatile bool isWriting;
        volatile bool isListening;

        readonly Lock writeLock = new();
        Queue<QueuedMessage> writeQueue;

        byte[] recvBuffer;
        SslStream sslStream;
        NetworkStream netStream;
        MemoryStream readStream;
        MemoryStream messageStream;

        protected enum Handshake : byte
        {
            Initial,
            Finished
        }

        protected enum FrameResult : byte
        {
            Finished,
            Incomplete,
            Close
        }

        public sealed class Raw : ControlMessage
        {
            public int Index { get; set; }
            public int Count { get; set; }

            public Raw(byte[] bytes) 
                : base(bytes) {
                Count = bytes.Length;
            }

            public Raw(byte[] bytes, int index, int count) 
                : base(bytes) {
                Index = index;
                Count = count;
            }
        }

        public sealed class Ping(byte[] bytes)
            : ControlMessage(bytes) { }

        public sealed class Pong(byte[] bytes)
            : ControlMessage(bytes) { }

        public abstract class ControlMessage
        {
            public byte[] Data { get; set; }

            public ControlMessage() { }
            public ControlMessage(byte[] data) { Data = data; }
        }

        protected class QueuedMessage(object msg, MessageType type)
        {
            public object Message { get; set; } = msg;
            public MessageType Type { get; set; } = type;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.Socket"/> associated with this ZorboSocket.
        /// </summary>
        public Socket Socket {
            get { return socket; }
        }

        /// <summary>
        /// Gets the URI representing the remote endpoint of the current connection.
        /// </summary>
        public Uri RemoteUri {
            get;
            protected set;
        }

        public bool IsConnected {
            get { return Socket is not null && Socket.Connected; }
        }

        public bool IsListening {
            get { return Socket is not null && isListening; }
        }

        public bool IsWebSocket { get; set; }

        public bool IsSecureSocket { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="MessageType"/> for sending data when an explicit message type is not supplied to Send(). 
        /// The socket will still receive either <see cref="MessageType.Text"/> or <see cref="MessageType.Binary"/>.
        /// </summary>
        public MessageType MessageType {
            get { return defaultMsgType; }
            set { defaultMsgType = value; } 
        }

        public IMonitor Monitor {
            get;
            protected set;
        }

        public IMessageConverter Converter {
            get;
            protected set;
        }

        public IPEndPoint LocalEndPoint {
            get { return Socket?.LocalEndPoint as IPEndPoint; }
        }

        public IPEndPoint RemoteEndPoint {
            get { return Socket?.RemoteEndPoint as IPEndPoint; }
        }

        /// <summary>
        /// The maximum size, in kilobytes, of a frame before messages should be fragmented into multiple frames.
        /// </summary>
        public long MaxFrameSize { get; set; } = 32 * 1024;

        /// <summary>
        /// The maximum size, in kilobytes, of any message (the combined size of all frames). 
        /// </summary>
        public long MaxMessageSize { get; set; } = 128 * 1024;

        /// <summary>
        /// The maximum size, in kilobytes, of an HTTP request header (includes WebSocket upgrade).
        /// </summary>
        public long MaxRequestHeaderSize { get; set; } = 8 * 1024;

        /// <summary>
        /// The maximum size, in kilobytes, of an HTTP request content buffer (default is 2GB).
        /// </summary>
        public long MaxRequestContentSize { get; set; } = 2L * 1024L * 1024L * 1024L;


        public X509Certificate Certificate { get; set; }

        public RemoteCertificateValidationCallback CertificateCallback { get; set; }


        public ZorboSocket() : this(new MessageConverter()) { }

        public ZorboSocket(IMessageConverter converter) {
            ArgumentNullException.ThrowIfNull(converter, nameof(converter));
            this.socket = SocketExtensions.CreateTcp();
            this.Converter = converter;
            this.Monitor = new IOMonitor(true);
        }

        // called by Accept()
        protected ZorboSocket(ZorboSocket listener, Socket socket) {
            ArgumentNullException.ThrowIfNull(socket, nameof(socket));
            this.socket = socket;
            this.Converter = listener.Converter;
            this.Monitor = listener.Monitor;
            this.isClient = false;
            this.netStream = new NetworkStream(Socket, false);
            this.writeQueue = [];
            this.recvBuffer = ArrayPool<byte>.Shared.Rent(8192);
            this.readStream = new MemoryStream();
            this.messageStream = new MemoryStream();
            this.sessionGuid = Guid.NewGuid();
        }

        #region " Accept "

        public virtual void Bind(IPEndPoint endpoint) {
            Socket.Bind(endpoint);
        }

        public virtual void Listen(int backlog = 100) {
            if (isListening)
                return;
            isListening = true;
            Socket.Listen(backlog);
            Accept();
        }

        protected virtual async void Accept() {
            Socket socket = null;
            ZorboSocket client = null;
            while (isListening) {
                try {
                    socket = await Socket.AcceptAsync();
                    client = new ZorboSocket(this, socket);
                    try {
                        client.IsWebSocket = IsWebSocket;
                        client.upgradeRequired = IsWebSocket;
                        client.IsSecureSocket = IsSecureSocket;

                        if (IsSecureSocket) {
                            client.Certificate = Certificate;
                            await client.AuthenticateAsServer();
                        }
                    }
                    catch (Exception ex) {
                        OnRejected(client, ex);
                        await client.DisposeAsync();
                        continue;
                    }

                    client.UpdateUri();
                    OnAccepted(client);
                }
                catch (Exception ex) {
                    if (client is not null)
                        await client.DisposeAsync();
                    else if (socket is not null)
                        await socket.DestroyAsync();
                    OnException(ex);
                }
            }
        }

        #endregion

        #region " Connect "

        public virtual void Connect(Uri uri) {
            ArgumentNullException.ThrowIfNull(uri, nameof(uri));

            RemoteUri = uri;
            switch (uri.Scheme) {
                case "tcp":
                    break;
                case "tcps":
                    IsSecureSocket = true;
                    break;
                case "ws":
                    IsWebSocket = true;
                    break;
                case "wss":
                    IsWebSocket = true;
                    IsSecureSocket = true;
                    break;
                default:
                    throw new InvalidOperationException("The supplied URI scheme is not supported.");
            }

            EndPoint endpoint = IPAddress.TryParse(uri.Host, out var ip) ?
                new IPEndPoint(ip, uri.Port) :
                new DnsEndPoint(uri.Host, uri.Port);

            Connect(endpoint);
        }

        public virtual void Connect(string host, int port) {
            ArgumentNullException.ThrowIfNull(host, nameof(host));

            EndPoint endpoint = IPAddress.TryParse(host, out var ip) ?
                new IPEndPoint(ip, port) :
                new DnsEndPoint(host, port);

            UpdateUri(endpoint);
            Connect(endpoint);
        }

        public virtual void Connect(IPAddress address, int port) {
            ArgumentNullException.ThrowIfNull(address, nameof(address));
            Connect(new IPEndPoint(address, port));
        }

        public virtual void Connect(IPEndPoint endpoint) {
            ArgumentNullException.ThrowIfNull(endpoint, nameof(endpoint));

            UpdateUri(endpoint);
            Connect((EndPoint)endpoint);
        }

        protected virtual async void Connect(EndPoint endpoint) {
            ArgumentNullException.ThrowIfNull(endpoint, nameof(endpoint));

            socket ??= SocketExtensions.CreateTcp();
            handshake = Handshake.Initial;
            writeQueue = [];
            recvBuffer = ArrayPool<byte>.Shared.Rent(8192);
            readStream = new MemoryStream();
            messageStream = new MemoryStream();
            sessionGuid = Guid.NewGuid();

            try {
                await Socket.ConnectAsync(endpoint);
                netStream = new NetworkStream(Socket, false);

                if (IsSecureSocket)
                    await AuthenticateAsClient();

                if (IsWebSocket)
                    SendWebSocketUpgrade();
            }
            catch (Exception ex) {
                OnRejected(this, ex);
                return;
            }

            OnConnected();
        }

        #endregion

        #region " Disconnect "

        public virtual void Disconnect() {
            Disconnect(CloseStatus.NormalClosure);
        }

        public virtual async void Disconnect(CloseStatus status) {

            if (IsConnected && status != CloseStatus.EndpointUnavailable) {
                try {
                    using var writer = new ZBinaryWriter();

                    await writer.WriteAsync((ushort)status);
                    await WriteFrame(writer, OpCode.Close);
                }
                catch (Exception e) {
                    OnException(e, false);
                }
            }

            await CloseAsync();
            OnDisconnected(status);
        }

        #endregion

        #region " SSL / TLS "

        protected virtual async Task AuthenticateAsServer() {
            if (sslStream is not null)
                throw new InvalidOperationException("TLS has already been activated on this Socket.");

            Socket.Blocking = true;

            sslStream = new SslStream(netStream, true);
            await sslStream.AuthenticateAsServerAsync(Certificate);
        }

        protected virtual async Task AuthenticateAsClient() {
            if (sslStream is not null)
                throw new InvalidOperationException("TLS has already been activated on this Socket.");

            Socket.Blocking = true;

            if (CertificateCallback is null)
                sslStream = new SslStream(netStream, true);
            else
                sslStream = new SslStream(netStream, true, CertificateCallback);

            await sslStream.AuthenticateAsClientAsync(RemoteUri.Host);
        }

        #endregion

        #region " WebSocket Handshake "

        protected virtual void SendWebSocketUpgrade(params KeyValuePair<string, string>[] headers) {
            Send(HttpHelper.UpgradeWebSocketHeaderBytes(RemoteUri, sessionGuid, headers));
        }

        protected virtual void SendWebSocketAccept(params KeyValuePair<string, string>[] headers) {
            Send(HttpHelper.AcceptWebSocketHeaderBytes(sessionGuid, [.. headers]));
        }

        #endregion

        #region " Send "

        public virtual void Send(byte[] rawbytes) {
            Send(new Raw(rawbytes));
        }

        public virtual void Send(byte[] rawbytes, int index, int count) {
            Send(new Raw(rawbytes, index, count));
        }

        public virtual void Send(object msg) {
            Send(msg, MessageType);
        }

        public virtual async void Send(object msg, MessageType type) {
            ArgumentNullException.ThrowIfNull(msg, nameof(msg));
            try {
                lock (writeLock) {
                    if (isWriting) {
                        writeQueue.Enqueue(new QueuedMessage(msg, type));
                        return;
                    }
                    isWriting = true;
                }

                while (isWriting) {
                    await Write(msg, type);

                    lock (writeLock) {
                        if (writeQueue.TryDequeue(out var queued)) {
                            msg = queued.Message;
                            type = queued.Type;
                        }
                        else isWriting = false;
                    }
                }
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        protected virtual async Task Write(object msg, MessageType type) {
            OpCode opCode;
            using var writer = new ZBinaryWriter();

            if (msg is Ping ping) {
                opCode = OpCode.Ping;
                await writer.WriteAsync(ping.Data);
            }
            else if (msg is Pong pong) {
                opCode = OpCode.Pong;
                await writer.WriteAsync(pong.Data);
            }
            else if (msg is Raw raw) {
                var stream = GetSocketStream();
                if (stream is not null)
                    await stream.WriteAsync(raw.Data.AsMemory(raw.Index, raw.Count));
                return;
            }
            else {
                try {
                    opCode = (type == MessageType.Binary) ? OpCode.Binary : OpCode.Text;
                    await Converter.WriteAsync(writer, msg, type);
                }
                catch(MessageConversionException mex) {
                    OnException(mex, false);
                    return;
                }
                catch (Exception ex) {
                    OnException(new MessageConversionException(ex), false);
                    return;
                }
            }

            await WriteFrame(writer, opCode);
        }

        protected virtual async Task WriteFrame(ZBinaryWriter writer, OpCode opCode) {
            writer.Position = 0;

            var frame = new Frame() {
                OpCode = opCode,
                IsMasked = isClient
            };

            var stream = GetSocketStream();
            if (stream is null) return;

            byte[] buffer;
            while (writer.Remaining > MaxFrameSize) {
                frame.IsFinal = false;
                frame.Payload ??= new byte[MaxFrameSize];

                await writer.BaseStream.ReadExactlyAsync(frame.Payload);

                buffer = await FrameReader.WriteAsync(frame);
                await stream.WriteAsync(buffer);

                Monitor.AddOutput(buffer.Length);
                frame.OpCode = OpCode.Continuation;
            }

            frame.IsFinal = true;
            frame.Payload = new byte[writer.Remaining];

            await writer.BaseStream.ReadExactlyAsync(frame.Payload);

            buffer = await FrameReader.WriteAsync(frame);
            await stream.WriteAsync(buffer);

            Monitor.AddOutput(buffer.Length);
        }

        #endregion

        #region " Receive "

        public virtual async void Receive() {
            if (isReading)
                return;
            isReading = true;
            try {
                await ReadFromStream();
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        protected virtual async Task ReadFromStream() {
            while (isReading) {
                var stream = GetSocketStream();
                if (stream is null) return;

                int count = await stream.ReadAsync(recvBuffer);

                if (count == 0) {
                    Disconnect(CloseStatus.EndpointUnavailable);
                    return;
                }

                Monitor.AddInput(count);

                await readStream.WriteAsync(recvBuffer.AsMemory(0, count));

                readStream.Position = 0;
                using var reader = new ZBinaryReader(readStream, true);

                do {
                    long start = reader.Position;
                    var state = await ReadFrame(reader);

                    if (state == FrameResult.Close)
                        return;

                    if (state == FrameResult.Incomplete) {
                        await RepositionStream(start);
                        goto NEXT_READ;
                    }
                }
                while (IsConnected && reader.Position < reader.Length);

                readStream?.SetLength(0);
            NEXT_READ:;
            }
        }

        protected virtual async Task RepositionStream(long position) {
            using var buffer = new MemoryStream();

            readStream.Position = position;
            await readStream.CopyToAsync(buffer);

            readStream.SetLength(0);

            buffer.Position = 0;
            await buffer.CopyToAsync(readStream);

            readStream.Position = 0;
        }

        protected virtual async Task<FrameResult> ReadFrame(ZBinaryReader reader) {
            if (handshake == Handshake.Initial) {
                if (upgradeRequired || !IsWebSocket) {
                    if (reader.Remaining < 2)
                        return FrameResult.Incomplete;

                    string tmp = (await reader.ReadStringAsync(2)).ToUpper();
                    reader.Position -= 2;

                    switch (tmp) {
                        case "GE":
                        case "HE":
                        case "PO": return await ReadHttpRequest(reader);
                    }
                }
                else return await ReadWebSocketAccept(reader);
            }

            // true if the listener is set to websocket only and a peer sends data before the upgrade
            if (upgradeRequired) {
                Disconnect(CloseStatus.ProtocolError);
                return FrameResult.Close;
            }

            // if we fall through from upgrade (not a GET) assume it's a bare socket
            handshake = Handshake.Finished;

            var frame = await FrameReader.ReadHeaderAsync(reader);

            if (frame.Incomplete)
                return FrameResult.Incomplete;

            // while the websocket RFC doesn't specify a maximum length
            // having an upper bound prevents a peer from sending an infinitely long message
            if (frame.Length > MaxFrameSize ||
                frame.Length + messageStream.Length > MaxMessageSize) {
                Disconnect(CloseStatus.MessageTooBig);
                return FrameResult.Close;
            }

            // shouldMask is false when the listener created the socket
            // so if !shouldMask then read data should be masked
            if (!isClient && !frame.IsMasked) {
                Disconnect(CloseStatus.ProtocolError);
                return FrameResult.Close;
            }

            await FrameReader.ReadPayloadAsync(reader, frame);

            if (frame.Incomplete)
                return FrameResult.Incomplete;

            switch (frame.OpCode) {
                case OpCode.Text:
                    incomingMsgType = MessageType.Text;
                    if (continueMessage) {
                        // error: opcode must be 0x00
                        Disconnect(CloseStatus.ProtocolError);
                        return FrameResult.Close;
                    }
                    else if (frame.IsFinal) {
                        continueMessage = false;
                        return await FinishFrame(frame.Payload);
                    }
                    else continueMessage = true;
                    break;
                case OpCode.Binary:
                    incomingMsgType = MessageType.Binary;
                    if (continueMessage) {
                        // error: opcode must be 0x00
                        Disconnect(CloseStatus.ProtocolError);
                        return FrameResult.Close;
                    }
                    else if (frame.IsFinal) {
                        continueMessage = false;
                        return await FinishFrame(frame.Payload);
                    }
                    else continueMessage = true;
                    break;
                case OpCode.Continuation:
                    if (!continueMessage) {
                        // error: must receive text/binary opcode first
                        Disconnect(CloseStatus.ProtocolError);
                        return FrameResult.Close;
                    }
                    else if (frame.IsFinal) {
                        continueMessage = false;
                        return await FinishFrame(frame.Payload);
                    }
                    break;
                case OpCode.Ping:
                    Send(new Pong(frame.Payload));
                    OnControlReceived(new Ping(frame.Payload));
                    return FrameResult.Finished;
                case OpCode.Pong:
                    OnControlReceived(new Pong(frame.Payload));
                    return FrameResult.Finished;
                case OpCode.Close:
                    Disconnect();
                    return FrameResult.Close;
                default://don't handle any other opcodes
                    Disconnect(CloseStatus.InvalidMessageType);
                    return FrameResult.Close;
            }

            await messageStream.WriteAsync(frame.Payload);
            return FrameResult.Finished;
        }

        protected virtual async Task<FrameResult> ReadHttpRequest(ZBinaryReader reader) {
            var state = ReadHttpHeader(reader, out var sb);
            if (state == FrameResult.Finished) {
                var request = HttpHelper.ParseRequestHeaders(sb.ToString());

                if (request.Headers.TryGetValue("CONNECTION", out _) &&
                    request.Headers.TryGetValue("UPGRADE", out _) &&
                    request.Headers.TryGetValue("SEC-WEBSOCKET-KEY", out string key)) {

                    return await ReadWebSocketUpgrade(reader, request, key);
                }

                var content = new ZBinaryReader();
                if (request.Headers.TryGetValue("CONTENT-LENGTH", out string len)) {

                    if (int.TryParse(len, out int length)) {

                        if (length > MaxRequestContentSize) {
                            Disconnect(CloseStatus.MessageTooBig);
                            return FrameResult.Close;
                        }

                        if (length < reader.Remaining)
                            return FrameResult.Incomplete;

                        await reader.BaseStream.CopyToAsync(content.BaseStream);
                    }
                    else {
                        Disconnect(CloseStatus.InvalidPayloadData);
                        return FrameResult.Close;
                    }
                }

                OnHttpRequestReceived(content, request);
            }

            return state;
        }

        protected virtual FrameResult ReadHttpHeader(ZBinaryReader reader, out StringBuilder sb) {
            sb = new StringBuilder();
            while (reader.Remaining > 0) {
                sb.Append(reader.ReadChar());

                if (sb.Length > MaxRequestHeaderSize) {
                    Disconnect(CloseStatus.MessageTooBig);
                    return FrameResult.Close;
                }
                else if (sb.EndsWith("\r\n\r\n"))
                    return FrameResult.Finished;
            }

            return FrameResult.Incomplete;
        }

        protected virtual async Task<FrameResult> ReadWebSocketUpgrade(ZBinaryReader reader, RequestMetadata request, string key) {
            if (string.IsNullOrEmpty(key)) {
                Disconnect(CloseStatus.ProtocolError);
                return FrameResult.Close;
            }
            try {
                sessionGuid = new Guid(Convert.FromBase64String(key));
            }
            catch {
                Disconnect(CloseStatus.ProtocolError);
                return FrameResult.Close;
            }

            if (sessionGuid == Guid.Empty) {
                Disconnect(CloseStatus.ProtocolError);
                return FrameResult.Close;
            }

            var my_headers = new Dictionary<string, string>();

            if (request.Headers.TryGetValue("ORIGIN", out string origin)) {
                my_headers.Add("Access-Control-Allow-Origin", origin);
                my_headers.Add("Access-Control-Allow-Credentials", "true");
                my_headers.Add("Access-Control-Allow-Headers", "content-type");
            }

            IsWebSocket = true;
            upgradeRequired = false;
            handshake = Handshake.Finished;

            UpdateUri();
            SendWebSocketAccept([.. my_headers]);

            return FrameResult.Finished;
        }

        protected virtual async Task<FrameResult> ReadWebSocketAccept(ZBinaryReader reader) {
            var state = ReadHttpHeader(reader, out var sb);
            if (state == FrameResult.Finished) {
                var response = HttpHelper.ParseResponseHeaders(sb.ToString());

                if (response.Headers.TryGetValue("CONNECTION", out _) &&
                    response.Headers.TryGetValue("UPGRADE", out _) &&
                    response.Headers.TryGetValue("SEC-WEBSOCKET-ACCEPT", out string hash)) {

                    if (hash == HttpHelper.GetAcceptKeyHash(sessionGuid)) {
                        handshake = Handshake.Finished;
                        return FrameResult.Finished;
                    }
                }
                Disconnect(CloseStatus.ProtocolError);
                return FrameResult.Close;
            }
            return state;
        }

        protected virtual async Task<FrameResult> FinishFrame(byte[] payload) {
            await messageStream.WriteAsync(payload);

            MessageResult message;
            using var frameReader = new ZBinaryReader(messageStream, true);
            
            try {
                frameReader.Position = 0;
                message = await Converter.ReadAsync(frameReader, incomingMsgType);
            }
            catch (MessageConversionException mex) {
                // assume if the converter throws an exception the data was bad
                OnException(mex, CloseStatus.InvalidPayloadData);
                return FrameResult.Close;
            }
            catch (Exception ex) {
                // assume if the converter throws an exception the data was bad
                OnException(new MessageConversionException(ex), CloseStatus.InvalidPayloadData);
                return FrameResult.Close;
            }

            OnMessageReceived(message);

            messageStream.SetLength(0);
            return FrameResult.Finished;
        }

        #endregion

        protected virtual Stream GetSocketStream() {
            return (sslStream is null) ? netStream : sslStream;
        }

        protected virtual void UpdateUri(EndPoint endpoint = null) {
            endpoint ??= RemoteEndPoint;
            if (IsWebSocket) {
                if (IsSecureSocket)
                    RemoteUri = new Uri($"wss://{endpoint}");
                else
                    RemoteUri = new Uri($"ws://{endpoint}");
            }
            else if (IsSecureSocket)
                RemoteUri = new Uri($"tcps://{endpoint}");
            else
                RemoteUri = new Uri($"tcp://{endpoint}");
        }

        protected virtual async void OnAccepted(ZorboSocket client) {
            await (Accepted?.Invoke(this, new(client)) ?? Task.CompletedTask);
        }

        protected virtual async void OnRejected(ZorboSocket client, Exception ex) {
            await (Rejected?.Invoke(this, new(client, ex)) ?? Task.CompletedTask);
        }

        protected virtual async void OnConnected() {
            await (Connected?.Invoke(this, ConnectEventArgs.Empty) ?? Task.CompletedTask);
        }

        protected virtual void OnControlReceived(object message) {
            //await (Received?.Invoke(this, new(0, message, MessageType.Binary)) ?? Task.CompletedTask);
        }

        protected virtual async void OnMessageReceived(MessageResult result) {
            await (Received?.Invoke(this, new(result.Id, result.Message, incomingMsgType)) ?? Task.CompletedTask);
        }

        protected virtual async void OnHttpRequestReceived(ZBinaryReader content, RequestMetadata request) {
            using var args = new HttpRequestEventArgs(request, content);
            await (HttpRequest?.Invoke(this, args) ?? Task.CompletedTask);
        }

        protected virtual void OnException(Exception ex, CloseStatus closeStatus) {
            OnException(ex, true, closeStatus);
        }

        protected virtual async void OnException(Exception ex, bool disconnect = true, CloseStatus closeStatus = CloseStatus.InternalServerError) {
            if (disconnect) Disconnect(closeStatus);
            await (Exception?.Invoke(this, new(ex)) ?? Task.CompletedTask);
        }

        protected virtual async void OnDisconnected(CloseStatus status) {
            await (Disconnected?.Invoke(this, new(status)) ?? Task.CompletedTask);
        }

        public virtual void Close() {
            isListening = false;
            if (isClient)
                Monitor.Reset();
            
            if (socket is not null) {
                socket?.Destroy();
                socket = null;
            }
            netStream?.Dispose();
            netStream = null;
            sslStream?.Dispose();
            sslStream = null;
            readStream?.Dispose();
            readStream = null;
            messageStream?.Dispose();
            messageStream = null;

            if (recvBuffer is not null) {
                ArrayPool<byte>.Shared.Return(recvBuffer);
                recvBuffer = null;
            }

            isReading = false;
            handshake = Handshake.Initial;
            continueMessage = false;

            lock (writeLock) {
                isWriting = false;
                writeQueue?.Clear();
                writeQueue = null;
            }
        }

        public virtual async Task CloseAsync() {
            isListening = false;
            if (isClient)
                Monitor.Reset();
            
            if (socket is not null) {
                await socket.DestroyAsync();
                socket = null;
            }

            if (netStream is not null)
                await netStream.DisposeAsync();
            netStream = null;

            if (sslStream is not null)
                await sslStream.DisposeAsync();
            sslStream = null;

            if (readStream is not null)
                await readStream.DisposeAsync();
            readStream = null;

            if (messageStream is not null)
                await messageStream.DisposeAsync();
            messageStream = null;

            if (recvBuffer is not null) {
                ArrayPool<byte>.Shared.Return(recvBuffer);
                recvBuffer = null;
            }

            isReading = false;
            handshake = Handshake.Initial;
            continueMessage = false;

            lock (writeLock) {
                isWriting = false;
                writeQueue?.Clear();
                writeQueue = null;
            }
        }

        public virtual void Dispose() { 
            Accepted = null;
            Rejected = null;
            Connected = null;
            Received = null;
            HttpRequest = null;
            Exception = null;
            Disconnected = null;
            Close();
            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsync() {
            Accepted = null;
            Rejected = null;
            Connected = null;
            Received = null;
            HttpRequest = null;
            Exception = null;
            Disconnected = null;
            await CloseAsync();
            GC.SuppressFinalize(this);
        }

        public event SocketEventHandler<AcceptEventArgs> Accepted;
        public event SocketEventHandler<RejectedEventArgs> Rejected;
        public event SocketEventHandler<ConnectEventArgs> Connected;
        public event SocketEventHandler<MessageEventArgs> Received;
        public event SocketEventHandler<HttpRequestEventArgs> HttpRequest;
        public event SocketEventHandler<ExceptionEventArgs> Exception;
        public event SocketEventHandler<DisconnectEventArgs> Disconnected;
    }
}
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Formatters;
using Zorbo.Ares.Packets.WebSockets;
using Zorbo.Core;
using Zorbo.Core.Data;
using Zorbo.Core.Data.Packets;
using Zorbo.Core.Interfaces;

namespace Zorbo.Ares.Sockets
{
    public class AresTcpSocket : ISocket, IDisposable
    {
        SslStream sslStream;
        NetworkStream stream;
        X509Certificate sslCert;

        readonly bool should_mask;

        volatile bool sending;
        volatile bool receiving;
        volatile bool disconnected;
        volatile bool disconnecting;
        volatile bool activating;

        IPacketFormatter formatter;
        ConcurrentQueue<IPacket> outQueue;

        readonly IPacketFormatter orgFormatter;


        public Socket Socket { get; private set; }

        public int HeaderLength { get; set; } = 2;


        public virtual bool IsConnected {
            get { return (Socket != null && Socket.Connected && !disconnecting && !disconnected); }
        }

        public virtual bool IsWebSocket { get; set; }

        [Obsolete("The ib0t protocol is currently deprecated and will be removed in the future.")]
        public virtual bool Isib0tSocket { get; set; }

        public virtual bool IsTLSEnabled { get; private set; }


        public IPEndPoint LocalEndPoint {
            get {
                if (Socket != null)
                    return Socket.LocalEndPoint as IPEndPoint;

                return null;
            }
        }

        public IPEndPoint RemoteEndPoint {
            get {

                if (Socket != null)
                    return Socket.RemoteEndPoint as IPEndPoint;

                return null;
            }
        }

        public IOMonitor Monitor { get; }

        IMonitor ISocket.Monitor { get { return Monitor; } }


        public IPacketFormatter Formatter {
            get { return formatter; }
            set {
                if (value == null)
                    formatter = orgFormatter;
                else
                    formatter = value;
            }
        }


        private ReceiveStatus State { get; set; }

        private WebSocketMessageType MessageType { get; set; } = WebSocketMessageType.Binary;


        public AresTcpSocket()
            : this(new ServerFormatter()) { }

        public AresTcpSocket(IPacketFormatter formatter) {
            Monitor = new IOMonitor();
            Monitor.Start();
            outQueue = new ConcurrentQueue<IPacket>();
            Socket = SocketManager.CreateTcp();
            Formatter = orgFormatter = formatter;
        }

        //called by Listener methods
        private AresTcpSocket(IPacketFormatter formatter, Socket socket)
        {
            should_mask = false;
            Monitor = new IOMonitor();
            Monitor.Start();
            outQueue = new ConcurrentQueue<IPacket>();
            this.Socket = socket;
            Formatter = orgFormatter = formatter;
        }

        #region " Listen "

        public virtual void Bind(IPEndPoint ep)
        {
            if (Socket == null) {
                Socket = SocketManager.CreateTcp();
                outQueue = new ConcurrentQueue<IPacket>();
                Monitor.Start();
            }

            Socket.Bind(ep);
        }

        public virtual void Listen()
        {
            Listen(25);
        }

        public virtual void Listen(int backlog)
        {
            if (Socket == null) {
                Socket = SocketManager.CreateTcp();
                outQueue = new ConcurrentQueue<IPacket>();
                Monitor.Start();
            }

            Socket.Listen(backlog);

            SocketAcceptTask task = new SocketAcceptTask();
            task.Completed += AcceptComplete;

            Socket.QueueAccept(task);
        }

        protected virtual void AcceptComplete(object sender, IOTaskCompleteEventArgs<SocketAcceptTask> e)
        {
            if (e.Task.Exception == null) {
                Accepted?.Invoke(this, new AcceptEventArgs(new AresTcpSocket(Formatter, e.Task.AcceptSocket)));
            }
            
            if (Socket != null) Socket.QueueAccept(e.Task);
        }

        #endregion

        #region " Connect "

        public virtual void Connect(string host, int port)
        {
            disconnected = false;

            if (Socket == null) {
                Socket = SocketManager.CreateTcp();
                outQueue = new ConcurrentQueue<IPacket>();
                Monitor.Start();
            }

            var task = new SocketConnectTask(host, port);

            task.Completed += ConnectCompleted;

            Socket.QueueConnect(task);
        }

        public virtual void Connect(IPAddress ip, int port)
        {
            Connect(ip.ToString(), port);
        }

        public virtual void Connect(IPEndPoint ep)
        {
            disconnecting = false;
            disconnected = false;

            if (Socket == null) {
                Socket = SocketManager.CreateTcp();
                outQueue = new ConcurrentQueue<IPacket>();
                Monitor.Start();
            }

            var task = new SocketConnectTask(ep);

            task.Completed += ConnectCompleted;

            Socket.QueueConnect(task);
        }

        protected virtual void ConnectCompleted(object sender, IOTaskCompleteEventArgs<SocketConnectTask> e)
        {
            e.Task.Completed -= ConnectCompleted;
            Connected?.Invoke(this, new ConnectEventArgs(e.Task.UserToken));
        }

        #endregion

        #region " Disconnect " 

        public virtual void Disconnect()
        {
            if (IsWebSocket)
                Disconnect(WebSocketCloseStatus.NormalClosure);
            else
                Disconnect(null);
        }

        public virtual async void Disconnect(WebSocketCloseStatus status) {
            if (!disconnecting) {
                disconnecting = true;
                await Task.Run(async () => {
                    Socket.QueueSend(EncodedPacket(WebSocketOpCode.Close, BitConverter.GetBytes((ushort)status)));
                    await Task.Delay(300);
                    DisconnectFromWebSocket();
                });
            }
        }

        private void DisconnectFromWebSocket()
        {
            Monitor.Stop();

            var task = new SocketDisconnectTask();

            task.Completed += OnDisconnected;
            Socket.QueueDisconnect(task);

        }

        public virtual void Disconnect(Object state)
        {
            if (!disconnecting) {
                disconnecting = true;

                Monitor.Stop();

                var task = new SocketDisconnectTask { UserToken = state };

                task.Completed += OnDisconnected;
                Socket.QueueDisconnect(task);
            }
        }

        protected virtual void OnDisconnected(object sender, IOTaskCompleteEventArgs<SocketDisconnectTask> e)
        {
            disconnecting = false;
            disconnected = true;
            e.Task.Completed -= OnDisconnected;
            Disconnected?.Invoke(this, new DisconnectEventArgs(e.Task.UserToken));
        }

        #endregion

        #region " Send "

        public virtual void SendAsync(IPacket packet)
        {
            if (disconnecting)
                return;

            if (sending || activating) {
                outQueue.Enqueue(packet);
                return;
            }

            sending = true;
            SendPacket(packet);
        }

        //meant for the udp adaptation of ISocket but usable anyway
        public virtual void SendAsync(IPacket packet, IPEndPoint endpoint)
        {
            SendAsync(packet);
        }

        protected virtual void SendQueue()
        {
            if (!activating && outQueue.TryDequeue(out IPacket packet)) {
                sending = true;
                SendPacket(packet);
            }
            else sending = false;
        }

        protected virtual void SendPacket(IPacket packet)
        {
            byte[] payload;

            var task = new SocketSendTask() { UserToken = packet };
            var ping = packet as PingPongPacket;
            
            switch (MessageType) {
                case WebSocketMessageType.Text: {

                    if (!IsWebSocket)
                        throw new InvalidOperationException("Can only send text packets on WebSockets.");

                    string id_ = $"|{packet.Id}|";

                    if (ping != null)
                        payload = ping.RawBytes;
                    else {
                        payload = Formatter.FormatJson(packet, Isib0tSocket);
                        if (payload == null) {
                            if (IsConnected) SendQueue();
                            return;
                        }
                    }

                    task.Data = EncodedPacket(ping != null ? WebSocketOpCode.Pong : WebSocketOpCode.Text, payload);
                    break;
                }
                case WebSocketMessageType.Binary: {
                    if (ping != null)
                        payload = ping.RawBytes;
                    else
                        payload = Formatter.Format(packet);

                    if (IsWebSocket) {
                        payload = payload.Skip(2).ToArray();
                        payload = EncodedPacket(ping != null ? WebSocketOpCode.Pong : WebSocketOpCode.Binary, payload);
                    }

                    task.Data = payload;
                    break;
                }
            }

            task.SslStream = sslStream;
            task.Count = task.Data.Length;
            task.Completed += SendComplete;

            if (IsConnected) Socket.QueueSend(task);
        }

        protected virtual byte[] EncodedPacket(WebSocketOpCode opCode, byte[] input)
        {
            int length = input.Length;

            using var writer = new PacketWriter();

            writer.Write((byte)(0b1_0_0_0_0000 + opCode));

            byte secondbyte = (byte)(should_mask ? 0b1_0000000 : 0b0_0000000);

            if (length <= 125)
                writer.Write((byte)(secondbyte | length));

            else if (length <= ushort.MaxValue) {
                writer.Write((byte)(secondbyte | 126));
                writer.Write(Utils.EnsureEndian((ushort)length));
            }
            else {
                writer.Write((byte)(secondbyte | 127)); //127
                writer.Write(Utils.EnsureEndian((long)length));
            }

            if (should_mask) {
                byte[] mask = new byte[4];
                Utils.Random.NextBytes(mask);

                for (int i = 0; i < input.Length; i++)
                    input[i] = (byte)(input[i] ^ mask[i % 4]);

                writer.Write(mask);
            }

            writer.Write(input);

            return writer.ToArray();
        }

        protected virtual void SendComplete(object sender, IOTaskCompleteEventArgs<SocketSendTask> e)
        {
            e.Task.Completed -= SendComplete;

            var msg = (IPacket)e.Task.UserToken;
            if (msg.Id == (byte)AresId.MSG_CHAT_SERVER_HTML) { 
                
            }

            if (e.Task.Exception == null) {
                Monitor.AddOutput(e.Task.Transferred);
                try {
                    
                    PacketSent?.Invoke(this, new PacketEventArgs(msg, MessageType, e.Task.Transferred));

                    if (IsConnected) SendQueue();
                }
                catch (Exception ex) {
                    OnException(ex);
                }
            }
            else OnException(e.Task.Exception);
        }

        #endregion

        #region " Receive "

        public virtual void ReceiveAsync()
        {
            if (receiving)
                throw new InvalidOperationException("Socket is already receiving.");
            receiving = true;

            var task = new SocketReceiveTask(HeaderLength);
            task.Completed += ReceiveCompleted;

            ReceiveAsyncInternal(task);
        }

        private void ReceiveAsyncInternal(SocketReceiveTask task) {
            State = ReceiveStatus.Header;
            task.Count = HeaderLength;
            task.SslStream = sslStream;
            if (IsConnected) Socket.QueueReceive(task);
        }

        protected virtual void ReceiveCompleted(object sender, IOTaskCompleteEventArgs<SocketReceiveTask> e)
        {
            if (e.Task.Exception == null) {

                if (e.Task.Transferred > 0) {

                    Monitor.AddInput(e.Task.Transferred);
                    try {
                        switch (State) {
                            case ReceiveStatus.Header:
                                HandlePacketHeader(e);
                                break;
                            case ReceiveStatus.Request_Head:
                                HandleRequestHeader(e);
                                break;
                            case ReceiveStatus.Request_Body:
                                HandleRequestBody(e);
                                break;
                            case ReceiveStatus.Decode_Length:
                                HandleWebSocketLength(e);
                                break;
                            case ReceiveStatus.Payload: {
                                HandlePayload(e);
                                break;
                            }
                        }
                    }
                    catch (Exception ex) { OnException(ex); }
                }
            }
            else OnException(e.Task.Exception);
        }

        protected virtual void HandlePacketHeader(IOTaskCompleteEventArgs<SocketReceiveTask> e) 
        {
            if (IsWebSocket) {//receive encoded message
                byte first = e.Buffer.Buffer[e.Buffer.Offset];

                /*support multi-part frames?
                bool fin =  (first & (1 << 7)) != 0;
                bool rsv1 = (first & (1 << 6)) != 0;
                bool rsv2 = (first & (1 << 5)) != 0;
                bool rsv3 = (first & (1 << 4)) != 0;
                */

                var opcode = (WebSocketOpCode)(first & ((1 << 4) - 1));
                if (opcode == WebSocketOpCode.Close) {
                    Disconnect();
                    return;
                }

                byte second = e.Buffer.Buffer[e.Buffer.Offset + 1];

                int length = second & 127;
                bool masked = (second & (1 << 7)) != 0;

                if (!masked && should_mask) {
                    //MASKED BIT NOT SET
                    Disconnect(WebSocketCloseStatus.ProtocolError);
                    return;
                }

                e.Task.UserToken = new WebSocketReceiveState {
                    OpCode = opcode,
                    IsMasking = masked
                };

                switch (length) {
                    case 126://ushort
                        State = ReceiveStatus.Decode_Length;
                        e.Task.Count = 2;
                        break;
                    case 127://ulong
                        State = ReceiveStatus.Decode_Length;
                        e.Task.Count = 8;
                        break;
                    default:
                        State = ReceiveStatus.Payload;
                        e.Task.Count = masked ? length + 4: length;
                        break;
                }
            }
            else {
                ushort length = BitConverter.ToUInt16(e.Buffer.Buffer, e.Buffer.Offset);

                // Ares has a hard message limit of 4k... enforce?
                // Zorbo uses a *default* socket buffer of 8k
                // Anything larger than this is either special or an error
                if (length == 17735 || //Numerical equivalent of "GE"
                    length == 17736 || //"HE"
                    length == 20304) { //"PO"

                    var token = new HttpRequestState();
                    token.Buffer.AddRange(e.Buffer.Buffer.Skip(e.Buffer.Offset).Take(2));

                    State = ReceiveStatus.Request_Head;
                    e.Task.UserToken = token;
                    e.Task.Count = length == 17735 ? 3 : 4;//need to receive rest of request from stream
                }
                else {
                    if (length >= SocketManager.BufferSize)
                        throw new SocketException((int)SocketError.NoBufferSpaceAvailable);

                    State = ReceiveStatus.Payload;
                    e.Task.Count = length + 1;
                }
            }
            //we got this far, keep receiving
            if (IsConnected) Socket.QueueReceive(e.Task);
        }

        protected virtual void HandleRequestHeader(IOTaskCompleteEventArgs<SocketReceiveTask> e) 
        {
            var token = (HttpRequestState)e.Task.UserToken;

            token.Buffer.AddRange(e.Buffer.Buffer.Skip(e.Buffer.Offset).Take(e.Task.Transferred));
            string temp = Encoding.UTF8.GetString(token.Buffer.ToArray());

            int terminator = temp.IndexOf("\r\n\r\n");
            if (terminator == -1) {
                //still need the rest of the header
                if (Socket.Available == 0) {
                    //no data available... wait longer?
                }
                e.Task.UserToken = token;
                e.Task.Count = Socket.Available;
                if (IsConnected) Socket.QueueReceive(e.Task);
            }
            else {
                string header = temp.Substring(0, terminator);
                string body = temp.Substring(terminator + 4);

                //remove up-to the body from the buffer
                token.Total = token.Buffer.Count;
                token.Buffer.RemoveRange(0, Encoding.UTF8.GetByteCount(header) + 4);

                int contentLen = 0;
                string[] lines = header.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                Match match = Regex.Match(lines[0], "(\\S+)\\s+/(\\S*)\\s+(\\S*)");
                if (match.Success) {

                    token.Method = match.Groups[1].Value;
                    token.RequestUri = match.Groups[2].Value;
                    token.Protocol = match.Groups[3].Value;

                    for (int i = 1; i < lines.Length; i++) {
                        match = Regex.Match(lines[i], "\\s*(\\S+)\\s*:\\s*(\\S+)");
                        if (match.Success) {
                            string key = match.Groups[1].Value.ToUpper();
                            string value = match.Groups[2].Value;
                            switch (key) {
                                case "CONTENT-LENGTH":
                                    int.TryParse(value, out contentLen);
                                    break;
                                case "SEC-WEBSOCKET-PROTOCOL":
                                    break;
                            }
                            token.Headers.Add(key, value);
                        }
                    }

                    if (contentLen > 0 && token.Buffer.Count < contentLen) {
                        //body is incomplete
                        State = ReceiveStatus.Request_Body;
                        e.Task.UserToken = token;
                        e.Task.Count = (contentLen - token.Buffer.Count);

                        if (IsConnected) Socket.QueueReceive(e.Task);
                    }
                    else {
                        token.Content = body;
                        OnHttpRequestReceived(token);
                        ReceiveAsyncInternal(e.Task);
                    }
                }
                else Disconnect();
            }
        }

        protected virtual void HandleRequestBody(IOTaskCompleteEventArgs<SocketReceiveTask> e) 
        {
            var token = (HttpRequestState)e.Task.UserToken;

            token.Total += e.Task.Transferred;
            token.Buffer.AddRange(e.Buffer.Buffer.Skip(e.Buffer.Offset).Take(e.Task.Transferred));

            token.Content = Encoding.UTF8.GetString(token.Buffer.ToArray());
            OnHttpRequestReceived(token);

            e.Task.UserToken = null;
            ReceiveAsyncInternal(e.Task);
        }

        protected virtual void HandleWebSocketLength(IOTaskCompleteEventArgs<SocketReceiveTask> e) 
        {
            var state = (WebSocketReceiveState)e.Task.UserToken;

            byte[] tmp = new byte[e.Task.Transferred];
            Array.Copy(e.Buffer.Buffer, e.Buffer.Offset, tmp, 0, tmp.Length);

            tmp = tmp.EnsureEndian();

            switch (e.Task.Transferred) {
                case 2: e.Task.Count = BitConverter.ToUInt16(tmp, 0); break;
                case 8: e.Task.Count = (int)BitConverter.ToUInt64(tmp, 0); break;
            }

            State = ReceiveStatus.Payload;

            if (state.IsMasking) e.Task.Count += 4;
            if (IsConnected) Socket.QueueReceive(e.Task);
        }

        protected virtual void HandlePayload(IOTaskCompleteEventArgs<SocketReceiveTask> e) 
        {
            int mask_length = 0;

            if (IsWebSocket) {
                var state = (WebSocketReceiveState)e.Task.UserToken;
                if (state.IsMasking) {
                    mask_length = 4;
                    //+1 for not copying the buffer to do the unmasking!
                    for (int i = 4; i < e.Task.Transferred; i++)
                        e.Buffer.Buffer[e.Buffer.Offset + i] ^= e.Buffer.Buffer[e.Buffer.Offset + (i % 4)];
                }

                switch (state.OpCode) {
                    case WebSocketOpCode.Text: {
                        
                        using var s = new MemoryStream(e.Buffer.Buffer, e.Buffer.Offset + mask_length, e.Task.Transferred - mask_length);
                        using var reader = new PacketReader(s);

                        string tmp = reader.ReadString();
                        Match match = Regex.Match(tmp, "[a-z]+?:", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                        if (match.Success) {
                            //is ib0t message
                            Isib0tSocket = true;
                            MessageType = WebSocketMessageType.Text;
                            Handleib0tMessage(tmp, e.Task.Transferred);
                            ReceiveAsyncInternal(e.Task);
                            return;
                        }
                        else {
                            int id = tmp.ExtractId();
                            if (id > -1) {
                                MessageType = WebSocketMessageType.Text;
                                OnTextPacketReceived((byte)id, tmp, e.Task.Transferred);
                                ReceiveAsyncInternal(e.Task);
                                return;
                            }
                        }

                        //this is a message type we don't recongnize.
                        Disconnect(WebSocketCloseStatus.InvalidPayloadData);
                        return;
                    }
                    case WebSocketOpCode.Ping:
                        //PingPongPacket was initially a private item and handled internally 
                        //however I decided that it can eventually get checked by AresServer for IO monitoring and flood inspecting
                        byte[] buffer = new byte[e.Task.Transferred - mask_length];
                        Array.Copy(e.Buffer.Buffer, e.Buffer.Offset, buffer, 0, buffer.Length);

                        var ping = new PingPongPacket(buffer);
                        
                        OnBinaryPacketReceived(ping, e.Task.Transferred);
                        SendAsync(ping);

                        ReceiveAsyncInternal(e.Task);
                        return;
                    case WebSocketOpCode.Pong:
                        ReceiveAsyncInternal(e.Task);
                        return;
                    case WebSocketOpCode.Binary:
                        MessageType = WebSocketMessageType.Binary;
                        break;//let binary fall through below
                    default://don't handle any of the other opcodes right now
                        Disconnect(WebSocketCloseStatus.InvalidMessageType);
                        return;
                }
            }

            OnBinaryPacketReceived(
                e.Buffer.Buffer[e.Buffer.Offset + mask_length], 
                e.Buffer.Buffer, 
                e.Buffer.Offset +  mask_length + 1, 
                e.Task.Transferred - mask_length - 1
            );

            ReceiveAsyncInternal(e.Task);
        }


        [Obsolete("The ib0t protocol is currently deprecated and will be removed in the future.")]
        private void Handleib0tMessage(string message, int transferred) 
        {
            OnTextPacketReceived(0, message, transferred, true);
        }

        protected virtual void OnException(Exception ex)
        {
            sending = false;
            receiving = false;
            Exception?.Invoke(this, new ExceptionEventArgs(ex, RemoteEndPoint));
        }

        protected virtual void OnTextPacketReceived(byte id, string json, int transferred, bool ib0t = false)
        {
            try {
                IPacket msg = Formatter.Unformat(id, json, ib0t);
                PacketReceived?.Invoke(this, new PacketEventArgs(msg, WebSocketMessageType.Text, transferred));
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        protected virtual void OnBinaryPacketReceived(IPacket packet, int transferred) {
            try {
                PacketReceived?.Invoke(this, new PacketEventArgs(packet, WebSocketMessageType.Binary, transferred));
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        protected virtual void OnBinaryPacketReceived(byte id, byte[] payload, int offset, int count)
        {
            try {
                IPacket msg = Formatter.Unformat(id, payload, offset, count);
                PacketReceived?.Invoke(this, new PacketEventArgs(msg, WebSocketMessageType.Binary, count + 1));
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        protected virtual void OnHttpRequestReceived(HttpRequestState state) {
            try {
                RequestReceived?.Invoke(this, new RequestEventArgs(state));
            }
            catch(Exception ex) {
                OnException(ex);
            }
        }

        #endregion

        #region " SSL / TLS "

        public void ActivateTLS(string pvt_file, string pub_file)
        {
            if (sslStream != null)
                throw new InvalidOperationException("TLS has already been activated on this Socket.");
            try {
                activating = true;

                stream = new NetworkStream(Socket, false);
                sslStream = new SslStream(stream, true, CertificateValidation);

                CreateCertificate(pvt_file, pub_file);

                sslCert = new X509Certificate2(pvt_file);
                sslStream.AuthenticateAsServer(sslCert, false, SslProtocols.None, false);

                activating = false;
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        private void CreateCertificate(string pvt_file, string pub_file)
        {
            if (!File.Exists(pvt_file)) {
                using RSA parent = RSA.Create(2048);

                CertificateRequest parentReq = new CertificateRequest(
                    $"CN={Path.GetFileNameWithoutExtension(pvt_file)}",
                    parent,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                using X509Certificate2 parentCert = parentReq.CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddDays(-45),
                    DateTimeOffset.UtcNow.AddDays(365));

                File.WriteAllBytes(pvt_file, parentCert.Export(X509ContentType.Pfx));
                File.WriteAllText(pub_file, string.Format(
                    "{0}\r\n{1}\r\n{2}",
                    "-----BEGIN CERTIFICATE-----",
                    Convert.ToBase64String(parentCert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks),
                    "-----END CERTIFICATE-----"));
            }
        }

        private bool CertificateValidation(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None ||
                sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) return true; //we don't have a proper certificate tree
            return false;
        }

        #endregion

        public virtual void Close()
        {
            Monitor.Stop();

            if (sslCert != null) {
                sslCert.Dispose();
                sslCert = null;
            }

            if (sslStream != null) {
                sslStream.Dispose();
                sslStream = null;
            }

            if (stream != null) {
                stream.Dispose();
                stream = null;
            }

            Socket.Destroy();

            sending = false;
            receiving = false;
        }

        public virtual void Dispose()
        {
            Close();
            Socket = null;
            Formatter = null;
            Exception = null;
            Accepted = null;
            Connected = null;
            Disconnected = null;
            PacketSent = null;
            PacketReceived = null;
            RequestReceived = null;
        }

        public event EventHandler<AcceptEventArgs>     Accepted;
        public event EventHandler<ConnectEventArgs>    Connected;
        public event EventHandler<DisconnectEventArgs> Disconnected;
        public event EventHandler<ExceptionEventArgs>  Exception;
        public event EventHandler<PacketEventArgs>     PacketSent;
        public event EventHandler<PacketEventArgs>     PacketReceived;
        public event EventHandler<RequestEventArgs>    RequestReceived;
    }
}

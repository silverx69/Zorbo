﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security;
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

namespace Zorbo.Ares.Sockets
{
    public class AresTcpSocket : ISocket, IDisposable
    {
        readonly bool should_mask;

        volatile bool sending;
        volatile bool receiving;
        volatile bool disconnected;
        volatile bool disconnecting;

        SocketTlsStreams tlsStreams;

        IPacketFormatter formatter;
        ConcurrentQueue<IPacket> outQueue;

        readonly MemoryStream readStream;
        readonly IPacketFormatter orgFormatter;

        EventHandler<IOTaskCompleteEventArgs<SocketSendTask>> sendHandler;
        EventHandler<IOTaskCompleteEventArgs<SocketReceiveTask>> recvHandler;

        public Socket Socket { get; private set; }

        public int HeaderLength { get; set; } = 2;


        public virtual bool IsConnected {
            get { return (Socket != null && Socket.Connected && !disconnecting && !disconnected); }
        }

        public virtual bool IsWebSocket { get; set; }

        [Obsolete("The ib0t protocol is currently deprecated and will be removed in the future.")]
        public virtual bool Isib0tSocket { get; set; }

        public virtual bool IsTLSEnabled => tlsStreams != null;


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

        private WebSocketMessageType MessageType { get; set; } = WebSocketMessageType.Binary;


        public AresTcpSocket(IPacketFormatter formatter) 
            : this() {
            Socket = SocketManager.CreateTcp();
            Formatter = orgFormatter = formatter;
        }

        //called by Listener methods
        protected AresTcpSocket(IPacketFormatter formatter, Socket socket)
            : this() {
            should_mask = false;//client sends masked
            Socket = socket;
            Formatter = orgFormatter = formatter;
        }

        protected AresTcpSocket() 
        {
            outQueue = new ConcurrentQueue<IPacket>();
            sendHandler = SendComplete;
            recvHandler = ReceiveCompleted2;
            readStream = new MemoryStream();
            Monitor = new IOMonitor();
            Monitor.Start();
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
            if (e.Task.Exception == null)
                Accepted?.Invoke(this, new AcceptEventArgs(new AresTcpSocket(Formatter, e.Task.AcceptSocket)));

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
                    SendBytes(EncodedPacket(WebSocketOpCode.Close, BitConverter.GetBytes((ushort)status)));
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

        public virtual void Disconnect(object state)
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

            if (sending) {
                outQueue.Enqueue(packet);
                return;
            }

            sending = true;
            SendPacket(packet);
        }

        public virtual void SendBytes(byte[] bytes)
        {
            var task = new SocketSendTask() {  };

            task.TlsStreams = tlsStreams;
            task.Data = bytes;
            task.Count = task.Data.Length;
            task.Completed += sendHandler;

            if (IsConnected) Socket.QueueSend(task);
        }

        //meant for the udp adaptation of ISocket but usable anyway
        public virtual void SendAsync(IPacket packet, IPEndPoint endpoint)
        {
            SendAsync(packet);
        }

        protected virtual void SendQueue()
        {
            if (outQueue.TryDequeue(out IPacket packet)) {
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
                    else {
                        payload = Formatter.Format(packet);
                        if (payload == null) {
                            if (IsConnected) SendQueue();
                            return;
                        }
                        if (IsWebSocket) 
                            payload = payload.Skip(2).ToArray();
                    }
                    if (IsWebSocket)
                        payload = EncodedPacket(ping != null ? WebSocketOpCode.Pong : WebSocketOpCode.Binary, payload);

                    task.Data = payload;
                    break;
                }
            }

            task.TlsStreams = tlsStreams;
            task.Count = task.Data.Length;
            task.Completed += sendHandler;

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
            e.Task.Completed -= sendHandler;
            var msg = (IPacket)e.Task.UserToken;

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

            var task = new SocketReceiveTask();
            task.Completed += recvHandler;
            ReceiveAsyncInternal(task);
        }

        private async void ReceiveAsyncInternal(SocketReceiveTask task) {
            if (tlsStreams != null) {//damn you sslstream
                try {
                    byte[] byt = new byte[1];
                    int ret = await tlsStreams.SslStream.ReadAsync(byt, 0, 1);
                    if (ret == 0) {
                        Disconnect();
                        return;
                    }
                    else readStream.Write(byt, 0, 1);
                }
                catch (Exception ex) {
                    OnException(ex);
                    return;
                }
            }
            task.Count = SocketManager.BufferSize;//bufferSize?
            task.TlsStreams = tlsStreams;
            if (IsConnected) Socket.QueueReceive(task);
        }

        protected virtual void ReceiveCompleted2(object sender, IOTaskCompleteEventArgs<SocketReceiveTask> e) 
        {
            if (e.Task.Exception == null) {

                Monitor.AddInput(e.Task.Transferred);
                
                try {
                    bool finished = true;

                    readStream.Write(e.Buffer.Buffer, e.Buffer.Offset, e.Task.Transferred);
                    readStream.Position = 0;

                    long start = 0;
                    using var reader = new PacketReader(readStream, true);

                    while (finished && reader.Remaining > 0) {
                        start = reader.Position;
                        finished = ReadPacketHeader(reader);
                    }

                    if (finished) 
                        readStream.SetLength(0);
                    else
                        RepositionStream(reader, start);
                }
                catch (Exception ex) { OnException(ex); }
            }
            else OnException(e.Task.Exception);

            if (IsConnected)
                ReceiveAsyncInternal(e.Task);
            else
                e.Task.Completed -= recvHandler;
        }
        
        protected virtual bool ReadPacketHeader(PacketReader reader) 
        {
            if (reader.Remaining < 2) 
                return false;

            if (IsWebSocket)//receive encoded message
                return ReadWebSocket(reader);
            else {
                ushort length = reader.ReadUInt16();
                // Ares has a hard message limit of 4k... enforce?
                // Zorbo uses a *default* socket buffer of 8k
                // Anything larger than this is either special or an error
                if (length == 17735 || //Numerical equivalent of "GE"
                    length == 17736 || //"HE"
                    length == 20304) { //"PO"
                    if (reader.Remaining >= (length == 17735 ? 16 : 17)) {
                        reader.Position -= 2;
                        return ReadHttpRequest(reader);
                    }
                }
                else {
                    if (length >= SocketManager.BufferSize)
                        throw new SocketException((int)SocketError.NoBufferSpaceAvailable);

                    if (reader.Remaining >= length + 1) {
                        byte id = reader.ReadByte();
                        byte[] payload = reader.ReadBytes(length);

                        OnBinaryPacketReceived(id, payload, 0, payload.Length);
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool ReadWebSocket(PacketReader reader)
        {
            byte first = reader.ReadByte();

            var opcode = (WebSocketOpCode)(first & ((1 << 4) - 1));
            if (opcode == WebSocketOpCode.Close) {
                Disconnect();
                return false;
            }

            byte second = reader.ReadByte();

            int length = second & 127;
            bool masked = (second & (1 << 7)) != 0;

            if (!masked && should_mask) {
                //MASKED BIT NOT SET
                Disconnect(WebSocketCloseStatus.ProtocolError);
                return false;
            }

            int count;
            if (length == 126) {
                if (reader.Remaining < 2)
                    return false;
                count = BitConverter.ToUInt16(reader.ReadBytes(2).EnsureEndian());
            }
            else if (length == 127) {
                if (reader.Remaining < 8)
                    return false;
                count = (int)BitConverter.ToUInt64(reader.ReadBytes(8).EnsureEndian());
            }
            else count = length;

            count = masked ? count + 4 : count;

            if (reader.Remaining < count)
                return false;

            byte[] payload;

            if (masked) {
                byte[] mask = reader.ReadBytes(4);
                payload = reader.ReadBytes(count - 4);
                for (int i = 0; i < payload.Length; i++)
                    payload[i] ^= mask[i % 4];
            }
            else payload = reader.ReadBytes(count);

            switch (opcode) {
                case WebSocketOpCode.Text: {

                    string tmp = Encoding.UTF8.GetString(payload);
                    Match match = Regex.Match(tmp, "[a-z]+?:", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    if (match.Success) {
                        //is ib0t message
                        Isib0tSocket = true;
                        MessageType = WebSocketMessageType.Text;
                        Handleib0tMessage(tmp, payload.Length);
                        return true;
                    }
                    else {
                        int id = tmp.ExtractId();
                        if (id > -1) {
                            MessageType = WebSocketMessageType.Text;
                            OnTextPacketReceived((byte)id, tmp, payload.Length);
                            return true;
                        }
                    }

                    //this is a message type we don't recongnize.
                    Disconnect(WebSocketCloseStatus.InvalidPayloadData);
                    return false;
                }
                case WebSocketOpCode.Ping:
                    var ping = new PingPongPacket(payload) { IsPing = true };
                    OnBinaryPacketReceived(ping, payload.Length);
                    SendAsync(ping);
                    return true;
                case WebSocketOpCode.Pong:
                    return true;
                case WebSocketOpCode.Binary:
                    MessageType = WebSocketMessageType.Binary;
                    OnBinaryPacketReceived(payload[0], payload, 1, payload.Length - 1);
                    return true;
                default://don't handle any of the other opcodes right now
                    Disconnect(WebSocketCloseStatus.InvalidMessageType);
                    return false;
            }
        }

        protected virtual bool ReadHttpRequest(PacketReader reader)
        {
            var sb = new StringBuilder();

            string header = string.Empty;
            bool haveHeader = false;

            while (reader.Remaining >= 0) {
                sb.Append(reader.ReadChars(Math.Min(4, (int)reader.Remaining)));
                header = sb.ToString();
                if (header.EndsWith("\r\n\r\n")) {
                    haveHeader = true;
                    break;
                }
            }

            if (!haveHeader) return false;

            int contentLen = 0;
            string[] lines = header.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Match match = Regex.Match(lines[0], "(\\S+)\\s+/(\\S*)\\s+(\\S*)");

            if (match.Success) {
                var token = new HttpRequestState {
                    Method = match.Groups[1].Value,
                    RequestUri = match.Groups[2].Value,
                    Protocol = match.Groups[3].Value
                };
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
                if (contentLen > 0) {
                    if (reader.Remaining < contentLen)
                        return false;
                    token.Content = reader.ReadString(contentLen);
                }
                OnHttpRequestReceived(token);
                return true;
            }
            else {
                Disconnect();
                return false;
            }
        }

        private void RepositionStream(PacketReader reader, long position)
        {
            reader.Position = position;
            byte[] tmp = reader.ReadBytes((int)reader.Remaining);
            readStream.SetLength(tmp.Length);
            readStream.Position = 0;
            readStream.Write(tmp);
        }

        /*
        protected virtual void ReceiveCompleted(object sender, IOTaskCompleteEventArgs<SocketReceiveTask> e)
        {
            if (e.Task.Exception == null) {

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
            else OnException(e.Task.Exception);

            if (!IsConnected) e.Task.Completed -= recvHandler;
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
                /*\/
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
                    e.Task.Count = length == 17735 ? 16 : 17;//need to receive rest of request from stream
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
                e.Task.UserToken = token;
                e.Task.Count = Socket.Available == 0 ? 1 : Socket.Available;
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

                        var ping = new PingPongPacket(buffer) { IsPing = true };
                        
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
        */

        [Obsolete("The ib0t protocol is currently deprecated and will be removed in the future.")]
        private void Handleib0tMessage(string message, int transferred) 
        {
            OnTextPacketReceived(0, message, transferred, true);
        }

        protected virtual void OnException(Exception ex)
        {
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
                RequestReceived?.Invoke(this, new HttpRequestEventArgs(state));
            }
            catch(Exception ex) {
                OnException(ex);
            }
        }

        #endregion

        #region " SSL / TLS "

        public void AuthenticateAsClient(string host)
        {
            if (tlsStreams != null)
                throw new InvalidOperationException("TLS has already been activated on this Socket.");
            try {
                Socket.Blocking = true;

                tlsStreams = new SocketTlsStreams(Socket);
                tlsStreams.SslStream.AuthenticateAsClient(host);

                SendQueue();
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        public void AuthenticateAsServer(string certificate, string password)
        {
            using var cert = new X509Certificate2(certificate, password);
            AuthenticateAsServer(cert);
        }

        public void AuthenticateAsServer(string certificate, SecureString password)
        {
            using var cert = new X509Certificate2(certificate, password);
            AuthenticateAsServer(cert);
        }

        public void AuthenticateAsServer(X509Certificate certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");
            if (tlsStreams != null)
                throw new InvalidOperationException("TLS has already been activated on this Socket.");
            try {
                Socket.Blocking = true;

                tlsStreams = new SocketTlsStreams(Socket);
                tlsStreams.SslStream.AuthenticateAsServer(certificate);

                SendQueue();
            }
            catch (Exception ex) {
                OnException(ex);
            }
        }

        #endregion

        public virtual void Close()
        {
            sending = false;
            receiving = false;

            Monitor.Stop();

            readStream?.Dispose();
            tlsStreams?.Dispose();
            Socket.Destroy();
        }

        public virtual void Dispose()
        {
            Close();
            Socket = null;
            Formatter = null;
            sendHandler = null;
            recvHandler = null;
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
        public event EventHandler<HttpRequestEventArgs>    RequestReceived;
    }
}

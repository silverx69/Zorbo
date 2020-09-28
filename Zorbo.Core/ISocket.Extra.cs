using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.WebSockets;

namespace Zorbo.Core
{
    public enum SocketProtocol 
    {
        TCP,
        UDP,
    }

    public enum SocketFamily 
    {
        IPv4,
        IPv6,
    }

    public enum ReceiveStatus
    {
        Header,
        Request_Head,
        Request_Body,
        Decode_Length,
        Payload
    }

    public enum WebSocketOpCode : byte
    {
        Continuation,
        Text = 0x1,
        Binary =0x2,
        Close = 0x8,
        Ping = 0x9,
        Pong = 0xA
    }

    public class HttpRequestState
    {
        public int Total { get; set; }

        public string Method { get; set; }

        public string RequestUri { get; set; }

        public string Protocol { get; set; }

        public string Content { get; set; }

        public List<byte> Buffer { 
            get;
            private set; 
        }

        public Dictionary<string, string> Headers {
            get;
            private set;
        }

        public HttpRequestState()
        {
            Buffer = new List<byte>();
            Headers = new Dictionary<string, string>();
        }
    }

    public class WebSocketReceiveState 
    {
        public WebSocketOpCode OpCode { get; set; }

        public bool IsMasking { get; set; }
    }

    public class AcceptEventArgs : EventArgs
    {
        public ISocket Socket { get; set; }

        public AcceptEventArgs(ISocket socket) {
            Socket = socket;
        }
    }

    public class ConnectEventArgs : EventArgs
    {
        public Object UserToken { get; set; }

        public ConnectEventArgs() { }

        public ConnectEventArgs(object userToken)
        {
            UserToken = userToken;
        }
    }

    public class DisconnectEventArgs : EventArgs
    {
        public Object UserToken { get; set; }

        public DisconnectEventArgs() { }

        public DisconnectEventArgs(object userToken)
        {
            UserToken = userToken;
        }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public IPEndPoint RemoteEndPoint { get; set; }

        public ExceptionEventArgs() { }

        public ExceptionEventArgs(Exception ex, IPAddress address) {
            Exception = ex;
            RemoteEndPoint = new IPEndPoint(address, 0);
        }

        public ExceptionEventArgs(Exception ex, IPEndPoint remoteEp) {
            Exception = ex;
            RemoteEndPoint = remoteEp;
        }
    }
    
    public class PacketEventArgs : EventArgs
    {
        public IPacket Packet { get; private set; }

        public WebSocketMessageType Type { get; private set; }

        public int Transferred { get; private set; }

        public IPEndPoint RemoteEndPoint { get; set; }

        
        public PacketEventArgs(IPacket packet, WebSocketMessageType type, int transferred) {
            Packet = packet;
            Type = type;
            Transferred = transferred;
        }

        public PacketEventArgs(IPacket packet, WebSocketMessageType type, int transferred, IPEndPoint ep) {
            Packet = packet;
            Type = type;
            Transferred = transferred;
            RemoteEndPoint = ep;
        }
    }

    public class RequestEventArgs : EventArgs
    {
        readonly HttpRequestState state;

        public string Method { get { return state.Method; } }

        public string RequestUri{ get { return state.RequestUri; } }

        public string Protocol { get { return state.Protocol; } }

        public Dictionary<string, string> Headers { get { return state.Headers; } }

        public string Content { get { return state.Content; } }

        public int Transferred { get { return state.Total; } }

        public RequestEventArgs(HttpRequestState state)
        {
            this.state = state;
        }
    }
}

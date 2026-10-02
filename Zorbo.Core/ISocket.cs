using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Zorbo.Core
{
    public interface ISocket : IDisposable
    {
        /// <summary>
        /// True if the socket has an established connection, otherwise false
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// True if the socket is using the WebSockets protocol, otherwise false
        /// </summary>
        bool IsWebSocket { get; }

        /// <summary>
        /// True if the WebSocket is using the (deprecated) ib0t protocol, otherwise false
        /// </summary>
        bool Isib0tSocket { get; }

        /// <summary>
        /// True if SSL/TLS is enabled on this ISocket, otherwise false
        /// </summary>
        bool IsTLSEnabled { get; }

        /// <summary>
        /// Gets a value that represents an object used to track the IO of the socket
        /// </summary>
        IMonitor Monitor { get; }

        /// <summary>
        /// Gets or sets a value that represents the message formatter currently in use by the socket
        /// </summary>
        IPacketFormatter Formatter { get; set; }

        /// <summary>
        /// Gets the local endpoint currently associated with the socket
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the remote endpoint currently associated with the socket
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        void Connect(string host, int port);
        void Connect(IPAddress ip, int port);
        void Connect(IPEndPoint ep);

        void Disconnect();
        void Disconnect(object usertoken);

        void Bind(IPEndPoint ep);

        void Listen();
        void Listen(int backlog);

        void SendAsync(IPacket packet);
        void SendAsync(IPacket packet, IPEndPoint remoteEp);

        void ReceiveAsync();

        event EventHandler<AcceptEventArgs> Accepted;
        event EventHandler<ExceptionEventArgs> Exception;
        event EventHandler<PacketEventArgs> PacketSent;
        event EventHandler<PacketEventArgs> PacketReceived;
        event EventHandler<HttpRequestEventArgs> RequestReceived;
        event EventHandler<DisconnectEventArgs> Disconnected;
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Zorbo.Core.Data;
using Zorbo.Core.Interfaces;

namespace Zorbo.Ares.Sockets
{
    public sealed class AresUdpSocket : ISocket, IDisposable
    {
        static readonly IPEndPoint receiveEp;


        public Socket Socket { 
            get;
            private set;
        }

        public bool IsConnected { 
            get { return false; } 
        }

        public bool IsWebSocket {
            get { return false; }
        }

        public bool Isib0tSocket {
            get { return false; }
        }

        public IOMonitor Monitor { get; }

        IMonitor ISocket.Monitor {
            get { return Monitor; }
        }

        public IPacketFormatter Formatter {
            get;
            set;
        }


        public IPEndPoint LocalEndPoint {
            get {
                if (Socket != null)
                    return (IPEndPoint)Socket.LocalEndPoint;

                return null;
            }
        }

        public IPEndPoint RemoteEndPoint {
            get { return null; }
        }

        bool ISocket.IsTLSEnabled {
            get { return false; }
        }


        static AresUdpSocket() 
        {
            receiveEp = new IPEndPoint(IPAddress.Any, 0);
        }

        public AresUdpSocket(IPacketFormatter formatter)
        {
            Formatter = formatter;
            Socket = SocketManager.CreateUdp();
            Monitor = new IOMonitor();
        }


        public void Bind(IPEndPoint ep)
        {
            if (Socket == null)
                Socket = SocketManager.CreateUdp();

            Monitor.Start();
            Socket.Bind(ep);
        }

        public void SendAsync(IPacket packet, IPEndPoint remoteEp)
        {
            byte[] tmp = Formatter.Format(packet);

            var task = new SocketSendTask(tmp) {
                UserToken = packet,
                RemoteEndPoint = remoteEp
            };

            task.Completed += SendComplete;
            if (Socket != null) Socket.QueueSend(task);
        }

        private void SendComplete(object sender, IOTaskCompleteEventArgs<SocketSendTask> e)
        {
            e.Task.Completed -= SendComplete;

            if (e.Task.Exception == null) {
                
                Monitor.AddOutput(e.Task.Transferred);

                try {
                    var msg = (IPacket)e.Task.UserToken;
                    PacketSent?.Invoke(this, new PacketEventArgs(msg, WebSocketMessageType.Binary, e.Task.Transferred, e.Task.RemoteEndPoint));
                }
                catch (Exception ex) {
                    OnException(ex, e.Task.RemoteEndPoint);
                }
            }
            else OnException(e.Task.Exception, e.Task.RemoteEndPoint);
        }

        public void ReceiveAsync()
        {
            SocketReceiveTask task = new SocketReceiveTask(8192);

            task.Completed += ReceiveComplete;
            task.RemoteEndPoint = receiveEp;

            if (Socket != null) Socket.QueueReceive(task);
        }

        private void ReceiveComplete(object sender, IOTaskCompleteEventArgs<SocketReceiveTask> e)
        {
            if (e.Task.Exception == null) {

                if (e.Task.Transferred > 0) {

                    Monitor.AddInput(e.Task.Transferred);
                    OnPacketReceived(
                        e.Buffer.Buffer[e.Buffer.Offset], //id
                        e.Buffer.Buffer, e.Buffer.Offset + 1, 
                        e.Task.Transferred - 1, 
                        e.Task.RemoteEndPoint);

                    e.Task.RemoteEndPoint = receiveEp;
                    if (Socket != null) Socket.QueueReceive(e.Task);
                }
            }
            else OnException(e.Task.Exception, e.Task.RemoteEndPoint);
        }


        private void OnException(Exception ex, IPEndPoint remoteEp)
        {
            Exception?.Invoke(this, new ExceptionEventArgs(ex, remoteEp));
        }

        private void OnPacketReceived(byte id, byte[] payload, int offset, int count, IPEndPoint remoteEp)
        {
            if (Formatter != null) {
                try {
                    IPacket msg = Formatter.Unformat(id, payload, offset, count);
                    PacketReceived?.Invoke(this, new PacketEventArgs(msg, WebSocketMessageType.Binary, count + 1, remoteEp));
                }
                catch (Exception ex) {
                    OnException(ex, remoteEp);
                }
            }
        }



        public void Close()
        {
            Socket.Destroy();
            Socket = null;

            Monitor.Reset();

            Exception = null;
            PacketSent = null;
            PacketReceived = null;
        }

        public void Dispose()
        {
            Close();
            Formatter = null;
        }


        public event EventHandler<ExceptionEventArgs> Exception;
        public event EventHandler<PacketEventArgs> PacketSent;
        public event EventHandler<PacketEventArgs> PacketReceived;

        //Not used -- hidden
        void ISocket.Connect(string host, int port)
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        void ISocket.Connect(IPAddress ip, int port)
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        void ISocket.Connect(IPEndPoint ep)
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        void ISocket.Disconnect()
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        void ISocket.Disconnect(object usertoken)
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        void ISocket.Listen()
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        void ISocket.Listen(int backlog)
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        void ISocket.SendAsync(IPacket packet)
        {
            throw new NotImplementedException("Not used for Udp operations");
        }

        event EventHandler<AcceptEventArgs> ISocket.Accepted { add { } remove { } }
        event EventHandler<RequestEventArgs> ISocket.RequestReceived { add { } remove { } }
        event EventHandler<DisconnectEventArgs> ISocket.Disconnected { add { } remove { } }
    }
}

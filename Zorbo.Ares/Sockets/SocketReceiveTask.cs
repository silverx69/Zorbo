using System;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Zorbo.Core;
using Zorbo.Core.Data;

namespace Zorbo.Ares.Sockets
{
    public sealed class SocketReceiveTask : IOTask<SocketReceiveTask>
    {
        int count = 0;

        EndPoint remoteEp;
        MemoryStream stream;
        readonly AsyncCallback receiveCallback;

        public Int32 Count {
            get { return count; }
            internal set { count = value; }
        }

        public Int32 Transferred {
            get;
            internal set;
        }


        public Socket Socket {
            get;
            internal set;
        }

        public SslStream SslStream {
            get;
            internal set;
        }

        public Object UserToken {
            get;
            set;
        }

        public Exception Exception {
            get;
            internal set;
        }

        public IPEndPoint RemoteEndPoint {
            get { return (IPEndPoint)remoteEp; }
            internal set {
                if (value == null)
                    remoteEp = null;
                else
                    remoteEp = new IPEndPoint(value.Address, value.Port);
            }
        }


        public SocketReceiveTask(int count) {
            Count = count;
            receiveCallback = new AsyncCallback(ExecuteComplete);
        }


        public void Execute(IOBuffer buffer) {

            if (stream == null)
                stream = new MemoryStream();

            if (SslStream == null)
                ExecuteReceive(buffer);
            else
                ExecuteTLSReceive(buffer);
        }

        private void ExecuteReceive(IOBuffer buffer) 
        {
            int count = Math.Min(Count - Transferred, SocketManager.BufferSize);
            try {
                if (Socket.Poll(0, SelectMode.SelectRead)) {

                    if (Socket.ProtocolType == ProtocolType.Tcp) {
                        Socket.BeginReceive(buffer.Buffer, buffer.Offset, count, SocketFlags.None, receiveCallback, buffer);
                    }
                    else {
                        Socket.BeginReceiveFrom(buffer.Buffer, buffer.Offset, count, SocketFlags.None, ref remoteEp, receiveCallback, buffer);
                    }
                }
                else OnContinue(buffer);
            }
            catch (ObjectDisposedException) {
                OnCompleted(buffer);
            }
            catch (Exception ex) {
                Exception = ex;
                OnCompleted(buffer);
            }
        }

        private async void ExecuteTLSReceive(IOBuffer buffer)
        {
            int count = Math.Min(Count - Transferred, SocketManager.BufferSize);
            try {
                if (Socket.Poll(0, SelectMode.SelectRead)) {
                    do {
                        int recvd = await SslStream.ReadAsync(buffer.Buffer, buffer.Offset, count);

                        if (recvd == 0)
                            throw new SocketException((int)SocketError.NotConnected);

                        Transferred += recvd;
                    }
                    while (Transferred < count);
                }
                else OnContinue(buffer);
            }
            catch (Exception ex) {
                Exception = ex;
                OnCompleted(buffer);
            }
        }

        private void ExecuteComplete(IAsyncResult ar) {
            IOBuffer args = (IOBuffer)ar.AsyncState;
            try {
                Int32 count = 0;
                SocketError err = SocketError.Success;

                if (Socket.ProtocolType == ProtocolType.Tcp)
                    count = Socket.EndReceive(ar, out err);
                else
                    count = Socket.EndReceiveFrom(ar, ref remoteEp);

                if (count == 0) {
                    Logging.Warning(
                        "SocketReceiveTask",
                        "Received nothing from Socket with Error: {0}", err);
                    Exception = new SocketException((int)SocketError.NotConnected);
                }
                else if (err != SocketError.Success) {
                    Logging.Warning(
                        "SocketReceiveTask",
                        "Receive operation failed with Error: {0}", err);
                    Exception = new SocketException((int)err);
                }
                else {
                    Transferred += count;
                    stream.Write(args.Buffer, args.Offset, count);

                    //can't continue Udp packets, whole message is received at once
                    if (Transferred < Count && Socket.ProtocolType == ProtocolType.Tcp) {
                        OnContinue(args);
                        return;
                    }
                }
            }
            catch (Exception ex) {
                Exception = ex;
            }

            OnCompleted(args);
        }

        private void OnContinue(IOBuffer buffer) {
            buffer.Release();
            Socket.QueueReceive(this);
        }

        private void OnCompleted(IOBuffer buffer) {
            try {
                stream.Position = 0;
                stream.Read(buffer.Buffer, buffer.Offset, Transferred);
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            catch { }

            try {
                Completed?.Invoke(Socket, new IOTaskCompleteEventArgs<SocketReceiveTask>(this, buffer));
            }
            catch { }

            Transferred = 0;
            buffer.Release();
        }

        public event EventHandler<IOTaskCompleteEventArgs<SocketReceiveTask>> Completed;
    }
}

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Zorbo.Core;
using Zorbo.Core.Data;

namespace Zorbo.Ares.Sockets
{
    public sealed class SocketTlsStreams : IDisposable
    {
        public NetworkStream Stream { get; private set; }

        public SslStream SslStream { get; private set; }

        public SocketTlsStreams(Socket socket, bool leaveOpen = false)
        {
            Stream = new NetworkStream(socket, !leaveOpen);
            SslStream = new SslStream(Stream, leaveOpen);
            SslStream.ReadTimeout = 1000;
        }

        public void Dispose()
        {
            SslStream?.Dispose();
            Stream?.Dispose();
        }
    }

    public sealed class SocketReceiveTask : IOTask<SocketReceiveTask>
    {
        EndPoint remoteEp;
        volatile bool finished;
        readonly AsyncCallback receiveCallback;

        public Int32 Count {
            get;
            internal set;
        }

        public Int32 Transferred {
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

        public Socket Socket {
            get;
            internal set;
        }

        public SocketTlsStreams TlsStreams {
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
        
        public SocketReceiveTask()
        {
            receiveCallback = new AsyncCallback(ExecuteComplete);
        }

        public SocketReceiveTask(int count)
            : this() {
            Count = count;
            receiveCallback = new AsyncCallback(ExecuteComplete);
        }

         
        public void Execute(IOBuffer buffer)
        {
            finished = false;
            if (TlsStreams == null) {
                ExecuteReceive(buffer);
            }
            else
                ExecuteTLSReceive(buffer);
        }

        private void ExecuteReceive(IOBuffer buffer)
        {
            int count = Math.Min(Count - Transferred, SocketManager.BufferSize);
            try {
                if (Socket.Poll(0, SelectMode.SelectRead)) {
                    if (Socket.ProtocolType == ProtocolType.Tcp)
                        Socket.BeginReceive(buffer.Buffer, buffer.Offset + Transferred, count, SocketFlags.None, receiveCallback, buffer);
                    else
                        Socket.BeginReceiveFrom(buffer.Buffer, buffer.Offset + Transferred, count, SocketFlags.None, ref remoteEp, receiveCallback, buffer);
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
            int count;
            try {
                count = Math.Min(Count - Transferred, SocketManager.BufferSize);
                int ret = await TlsStreams.SslStream.ReadAsync(buffer.Buffer, buffer.Offset + Transferred, count);

                if (ret == 0)
                    Exception = new SocketException((int)SocketError.NotConnected);
                else
                    Transferred += ret;

                OnCompleted(buffer);
            }
            catch (ObjectDisposedException) {
                OnCompleted(buffer);
            }
            catch (Exception ex) {
                Exception = ex;
                OnCompleted(buffer);
            }
        }

        private void ExecuteComplete(IAsyncResult ar)
        {
            IOBuffer args = (IOBuffer)ar.AsyncState;
            try {
                Int32 count = 0;
                SocketError err = SocketError.Success;

                if (Socket.ProtocolType == ProtocolType.Tcp)
                    count = Socket.EndReceive(ar, out err);
                else
                    count = Socket.EndReceiveFrom(ar, ref remoteEp);

                if (count == 0)
                    Exception = new SocketException((int)SocketError.NotConnected);

                else if (err != SocketError.Success)
                    Exception = new SocketException((int)err);
                else {
                    Transferred += count;
                }
            }
            catch (Exception ex) {
                Exception = ex;
            }

            OnCompleted(args);
        }

        private void OnContinue(IOBuffer buffer)
        {
            if (finished) return;
            finished = true;
            buffer.Release();
            Socket.QueueReceive(this);
        }

        private void OnCompleted(IOBuffer buffer)
        {
            if (finished) return;
            finished = true;
            OnFinished(buffer);
        }

        private void OnFinished(IOBuffer buffer)
        {
            try { Completed?.Invoke(Socket, new IOTaskCompleteEventArgs<SocketReceiveTask>(this, buffer)); }
            catch { }
            Transferred = 0;
            buffer.Release();
        }

        public event EventHandler<IOTaskCompleteEventArgs<SocketReceiveTask>> Completed;
    }
}
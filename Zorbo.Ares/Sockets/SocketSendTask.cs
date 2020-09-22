using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

using Zorbo.Core.Data;

namespace Zorbo.Ares.Sockets
{
    public sealed class SocketSendTask : IOTask<SocketSendTask>
    {
        IPEndPoint remoteEp;
        readonly AsyncCallback sendCallback;

        public byte[] Data { get; set; }

        public Int32 Count { get; set; }

        public Int32 Offset {
            get;
            internal set;
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
            get { return remoteEp; }
            internal set {
                if (value == null)
                    remoteEp = null;
                else
                    remoteEp = new IPEndPoint(value.Address, value.Port);
            }
        }


        public SocketSendTask() {
            sendCallback = new AsyncCallback(ExecuteComplete);
        }

        public SocketSendTask(byte[] buffer)
            : this() {
            Data = buffer;
            Count = Data.Length;
        }

        public SocketSendTask(byte[] buffer, int offset, int count)
            : this() {
            Data = buffer;
            Offset = offset;
            Count = count;
        }


        public void Execute(IOBuffer buffer) {
            if (SslStream == null)
                ExecuteSend(buffer);
            else
                ExecuteTLSSend(buffer);
        }


        private void ExecuteSend(IOBuffer buffer) 
        {
            int count = Math.Min(Count - Transferred - Offset, SocketManager.BufferSize);
            Array.Copy(Data, Offset + Transferred, buffer.Buffer, buffer.Offset, count);
            try {
                if (Socket.Poll(0, SelectMode.SelectWrite)) {

                    if (Socket.ProtocolType == ProtocolType.Tcp)
                        Socket.BeginSend(buffer.Buffer, buffer.Offset, count, SocketFlags.None, sendCallback, buffer);
                    else
                        Socket.BeginSendTo(buffer.Buffer, buffer.Offset, count, SocketFlags.None, RemoteEndPoint, sendCallback, buffer);
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

        private async void ExecuteTLSSend(IOBuffer buffer) 
        {
            int count = Math.Min(Count - Transferred - Offset, SocketManager.BufferSize);
            Array.Copy(Data, Offset + Transferred, buffer.Buffer, buffer.Offset, count);
            try {
                if (Socket.Poll(0, SelectMode.SelectWrite)) {

                    await SslStream.WriteAsync(buffer.Buffer, buffer.Offset, count);
                    OnCompleted(buffer);
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

        private void ExecuteComplete(IAsyncResult ar) {
            IOBuffer args = (IOBuffer)ar.AsyncState;

            try {
                Int32 count = 0;
                SocketError err = SocketError.Success;

                if (Socket.ProtocolType == ProtocolType.Tcp)
                    count = Socket.EndSend(ar, out err);
                else
                    count = Socket.EndSendTo(ar);

                if (count == 0)
                    Exception = new SocketException((int)SocketError.NotConnected);

                else if (err != SocketError.Success)
                    Exception = new SocketException((int)err);

                else {
                    Transferred += count;

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
            Socket.QueueSend(this);
        }

        private void OnCompleted(IOBuffer buffer) {
            try {
                Completed?.Invoke(Socket, new IOTaskCompleteEventArgs<SocketSendTask>(this, buffer));
            }
            catch { }

            Transferred = 0;
            buffer.Release();
        }


        public event EventHandler<IOTaskCompleteEventArgs<SocketSendTask>> Completed;
    }
}

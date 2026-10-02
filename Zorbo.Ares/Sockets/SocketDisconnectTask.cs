using System;
using System.Net.Sockets;

using Zorbo.Core.Data;

namespace Zorbo.Ares.Sockets
{
    public sealed class SocketDisconnectTask : IOTask<SocketDisconnectTask>
    {
        readonly AsyncCallback disconnectComplete;


        public Socket Socket {
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

        public SocketDisconnectTask() {
            disconnectComplete = new AsyncCallback(ExecuteComplete);
        }

        public void Execute(IOBuffer buffer) {
            try {
                Socket.BeginDisconnect(false, disconnectComplete, buffer);
            }
            catch (Exception e) {
                Exception = e;
                OnCompleted(buffer);
            }
        }

        public void ExecuteComplete(IAsyncResult ar) {
            IOBuffer buff = (IOBuffer)ar.AsyncState;

            try {
                Socket.EndDisconnect(ar);
            }
            catch (Exception ex) {
                Exception = ex;
            }

            OnCompleted(buff);
        }


        private void OnCompleted(IOBuffer buffer) {
            try {
                Completed?.Invoke(Socket, new IOTaskCompleteEventArgs<SocketDisconnectTask>(this, buffer));
            }
            catch { }

            buffer.Release();
        }

        public event EventHandler<IOTaskCompleteEventArgs<SocketDisconnectTask>> Completed;
    }
}

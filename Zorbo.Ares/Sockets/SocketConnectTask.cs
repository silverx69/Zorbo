using System;
using System.Net;
using System.Net.Sockets;

using Zorbo.Core.Data;

namespace Zorbo.Ares.Sockets
{
    public sealed class SocketConnectTask : IOTask<SocketConnectTask>
    {
        readonly AsyncCallback connectComplete;


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

        public string Address {
            get;
            private set;
        }

        public int Port {
            get;
            private set;
        }

        public SocketConnectTask(string host, int port)
        {
            Address = host;
            Port = port;
        }

        public SocketConnectTask(IPAddress ip, int port)
            : this(ip.ToString(), port) { }

        public SocketConnectTask(IPEndPoint remoteEp) {
            Address = remoteEp.Address.ToString();
            Port = remoteEp.Port;
            connectComplete = new AsyncCallback(ExecuteComplete);
        }

        public void Execute(IOBuffer buffer) {
            try {
                Socket.BeginConnect(Address, Port, connectComplete, buffer);
            }
            catch (Exception ex) {
                Exception = ex;
                OnCompleted(buffer);
            }
        }

        private void ExecuteComplete(IAsyncResult ar) {
            IOBuffer buff = (IOBuffer)ar.AsyncState;

            try {
                Socket.EndConnect(ar);
            }
            catch (Exception ex) {
                Exception = ex;
            }

            OnCompleted(buff);
        }

        private void OnCompleted(IOBuffer buffer) {
            try {
                Completed?.Invoke(Socket, new IOTaskCompleteEventArgs<SocketConnectTask>(this, buffer));
            }
            catch { }
            buffer.Release();
        }

        public event EventHandler<IOTaskCompleteEventArgs<SocketConnectTask>> Completed;
    }
}

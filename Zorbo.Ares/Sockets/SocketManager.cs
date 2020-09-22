using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Zorbo.Core;
using Zorbo.Core.Data;

namespace Zorbo.Ares.Sockets
{
    public sealed class SocketManager : IOManager<IOTask>
    {
        readonly int maxOutPackets = 0;

        readonly EventHandler acceptComplete = null;
        readonly EventHandler connectComplete = null;
        readonly EventHandler disconnectComplete = null;

        readonly Stack<IOBuffer> connPool = null;
        readonly Queue<SocketConnectTask> connQueue = null;

        readonly Stack<IOBuffer> discPool = null;
        readonly Queue<SocketDisconnectTask> discQueue = null;

        readonly Stack<IOBuffer> acceptPool = null;
        readonly Queue<SocketAcceptTask> acceptQueue = null;

        public const int BufferSize = 8 * 1024;
        public const int MaxListeningSockets = 20;

        public SocketManager(int maxOutgoingPackets, int stackSize = 60)
            : base(stackSize, BufferSize)
        {

            maxOutPackets = maxOutgoingPackets;

            connPool = new Stack<IOBuffer>();
            discPool = new Stack<IOBuffer>();
            acceptPool = new Stack<IOBuffer>();

            connQueue = new Queue<SocketConnectTask>();
            discQueue = new Queue<SocketDisconnectTask>();
            acceptQueue = new Queue<SocketAcceptTask>();

            acceptComplete = new EventHandler(ExecuteAcceptComplete);
            connectComplete = new EventHandler(ExecuteConnectComplete);
            disconnectComplete = new EventHandler(ExecuteDisconnectComplete);
        }

        public void QueueAccept(SocketAcceptTask task)
        {
            lock (acceptQueue) acceptQueue.Enqueue(task);
        }

        public void QueueConnect(SocketConnectTask task)
        {
            lock (connQueue) connQueue.Enqueue(task);
        }

        public override void QueueWrite(IOTask task)
        {
            if (writeQueue.Count < maxOutPackets)
                base.QueueWrite(task);
        }

        public void QueueDisconnect(SocketDisconnectTask task)
        {
            lock (discQueue) discQueue.Enqueue(task);
        }


        protected override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < 5; i++) {
                var buff = new IOBuffer();
                buff.Completed += acceptComplete;

                acceptPool.Push(buff);
            }

            for (int i = 0; i < MaxListeningSockets; i++) {
                var buff = new IOBuffer();
                buff.Completed += connectComplete;

                connPool.Push(buff);
            }

            for (int i = 0; i < 5; i++) {
                var buff = new IOBuffer();
                buff.Completed += disconnectComplete;

                discPool.Push(buff);
            }
        }

        protected override void Dispose()
        {
            base.Dispose();
            acceptPool.Clear();
            acceptQueue.Clear();
            connPool.Clear();
            connQueue.Clear();
            discPool.Clear();
            discQueue.Clear();
        }

        volatile int exec = 0;
        protected override void WorkerLoop()
        {
            while (Alive) {
                ExecuteAccept();
                ExecuteConnect();
                ExecuteRead();
                ExecuteWrite();
                ExecuteDisconnect();
                if (++exec >= 10) {
                    exec = 0;
                    Thread.Sleep(3);
                }
            }
        }


        private void ExecuteAccept()
        {
            lock (acceptPool)
                lock (acceptQueue) {

                    if (acceptPool.Count == 0 || acceptQueue.Count == 0)
                        return;

                    var args = acceptPool.Pop();
                    var task = acceptQueue.Dequeue();

                    task.Execute(args);
                }
        }

        private void ExecuteAcceptComplete(object sender, EventArgs e)
        {
            lock (acceptPool) acceptPool.Push((IOBuffer)sender);
        }


        private void ExecuteConnect()
        {
            lock (connPool)
                lock (acceptQueue) {

                    if (connPool.Count == 0 || connQueue.Count == 0)
                        return;

                    var args = connPool.Pop();
                    var task = connQueue.Dequeue();

                    task.Execute(args);
                }
        }

        private void ExecuteConnectComplete(object sender, EventArgs e)
        {
            lock (connPool) connPool.Push((IOBuffer)sender);
        }


        private void ExecuteDisconnect()
        {
            lock (discPool)
                lock (discQueue) {

                    if (discPool.Count == 0 || discQueue.Count == 0)
                        return;

                    var args = discPool.Pop();
                    var task = discQueue.Dequeue();

                    task.Execute(args);
                }
        }

        private void ExecuteDisconnectComplete(object sender, EventArgs e)
        {
            lock (discPool) discPool.Push((IOBuffer)sender);
        }


        public static Socket CreateTcp()
        {
            return CreateTcp(AddressFamily.InterNetwork);
        }

        public static Socket CreateTcp(AddressFamily family)
        {
            Socket socket = null;
            try {
                socket = new Socket(
                    family,
                    SocketType.Stream,
                    ProtocolType.Tcp) {
                    SendBufferSize = BufferSize,
                    ReceiveBufferSize = BufferSize
                };
                socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            }
            catch (Exception ex) {
                Logging.Error("SocketManager", ex);
            }
            return socket;
        }


        public static Socket CreateUdp()
        {
            return CreateUdp(AddressFamily.InterNetwork);
        }

        public static Socket CreateUdp(AddressFamily family)
        {
            Socket socket = null;
            try {
                socket = new Socket(
                    family,
                    SocketType.Dgram,
                    ProtocolType.Udp) {
                    SendBufferSize = BufferSize,
                    ReceiveBufferSize = BufferSize
                };
                socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            }
            catch (Exception ex) {
                Logging.Error("SocketManager", ex);
            }
            return socket;
        }
    }
}
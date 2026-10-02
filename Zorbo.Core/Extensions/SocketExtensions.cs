using System.Net.Sockets;

namespace Zorbo
{
    public static partial class SocketExtensions
    {
        const int SIO_UDP_CONNRESET = -1744830452;

        public static Socket CreateTcp() {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            // disable nagle algorithm
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            return socket;
        }

        public static Socket CreateUdp() {
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            // suppress exception when closing udp sockets
            socket.IOControl(SIO_UDP_CONNRESET, [0, 0, 0, 0], null);
            return socket;
        }

        public static void Destroy(this Socket socket) {
            try {
                socket?.Close(100);
#if !MONO
                socket?.Dispose();
#endif
            }
            catch { }
        }

        public static async Task DestroyAsync(this Socket socket) => Destroy(socket);
    }
}

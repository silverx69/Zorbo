using System.Net;
using System.Net.NetworkInformation;

namespace Zorbo.Net
{
    public static class Network
    {
        public static List<IPAddress> GetLocalAddresses() {
            var ips = new List<IPAddress>();
            foreach (var n in NetworkInterface.GetAllNetworkInterfaces()) {
                if (n.OperationalStatus != OperationalStatus.Up)
                    continue;
                foreach (var uaddr in n.GetIPProperties().UnicastAddresses)
                    ips.Add(uaddr.Address);
            }
            return ips;
        }
    }
}

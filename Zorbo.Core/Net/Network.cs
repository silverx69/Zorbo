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
                foreach (var u in n.GetIPProperties().UnicastAddresses)
                    ips.Add(u.Address);
            }
            return ips;
        }

        public static async Task<List<IPAddress>> GetLocalAddressesAsync() {
            var ips = new List<IPAddress>();
            foreach (var n in NetworkInterface.GetAllNetworkInterfaces()) {
                if (n.OperationalStatus != OperationalStatus.Up)
                    continue;
                foreach (var u in n.GetIPProperties().UnicastAddresses)
                    ips.Add(u.Address);
            }
            return ips;
        }
    }
}

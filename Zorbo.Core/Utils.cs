using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Zorbo.Core
{
    public static class Utils
    {
        static MD5 md5;
        static SHA1 sha1;

        public static MD5 MD5 {
            get { return md5 ??= MD5.Create(); }
        }
        public static SHA1 SHA1 {
            get { return sha1 ??= SHA1.Create(); }
        }


        public static Random Random = new Random();


        public static short EnsureEndian(this short value)
        {
            if (BitConverter.IsLittleEndian) {
                byte[] tmp = BitConverter.GetBytes(value);
                Array.Reverse(tmp);
                return BitConverter.ToInt16(tmp, 0);
            }
            return value;
        }

        public static ushort EnsureEndian(this ushort value)
        {
            if (BitConverter.IsLittleEndian) {
                byte[] tmp = BitConverter.GetBytes(value);
                Array.Reverse(tmp);
                return BitConverter.ToUInt16(tmp, 0);
            }
            return value;
        }

        public static int EnsureEndian(this int value)
        {
            if (BitConverter.IsLittleEndian) {
                byte[] tmp = BitConverter.GetBytes(value);
                Array.Reverse(tmp);
                return BitConverter.ToInt32(tmp, 0);
            }
            return value;
        }

        public static uint EnsureEndian(this uint value)
        {
            if (BitConverter.IsLittleEndian) {
                byte[] tmp = BitConverter.GetBytes(value);
                Array.Reverse(tmp);
                return BitConverter.ToUInt32(tmp, 0);
            }
            return value;
        }

        public static long EnsureEndian(this long value)
        {
            if (BitConverter.IsLittleEndian) {
                byte[] tmp = BitConverter.GetBytes(value);
                Array.Reverse(tmp);
                return BitConverter.ToInt64(tmp, 0);
            }
            return value;
        }

        public static ulong EnsureEndian(this ulong value)
        {
            if (BitConverter.IsLittleEndian) {
                byte[] tmp = BitConverter.GetBytes(value);
                Array.Reverse(tmp);
                return BitConverter.ToUInt64(tmp, 0);
            }
            return value;
        }

        public static byte[] EnsureEndian(this byte[] input) {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(input);
            return input;
        }


        public static IEnumerable<IPAddress> GetLocalAddresses()
        {
            var ips = new List<IPAddress>();

            NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(s => s.OperationalStatus == OperationalStatus.Up)
                .Select(s => s.GetIPProperties().UnicastAddresses.Select(s => s.Address))
                .ForEach(s => ips.AddRange(s));

            return ips;
        }
    }
}

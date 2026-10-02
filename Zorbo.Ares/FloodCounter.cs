using Newtonsoft.Json;
using System;
using System.Net;
using Zorbo.Core;
using Zorbo.Core.Models;

namespace Zorbo.Ares
{
    public class FloodCounter : ModelBase, IFloodCounter
    {
        [JsonProperty("count", Required = Required.Always)]
        public int Count { 
            get; 
            set; 
        }

        [JsonProperty("last", Required = Required.Always)]
        public DateTime Last {
            get;
            set; 
        }

        public FloodCounter() { }

        public FloodCounter(int count, DateTime last) {
            Last = last;
            Count = count;
        }
    }

    public class UdpFloodCounter : FloodCounter, IEquatable<UdpFloodCounter>
    {
        [JsonProperty("id", Required = Required.Always)]
        public byte Id { get; set; }

        [JsonProperty("address", Required = Required.Always)]
        public IPAddress Address { get; set; }

        public UdpFloodCounter() { }

        public UdpFloodCounter(byte id, IPAddress ip, int count, DateTime last)
            : base(count, last) {
            Id = id;
            Address = ip;
        }

        public bool Equals(UdpFloodCounter other)
        {
            if (other == null)
                return false;
            return Id == other.Id && IPAddress.Equals(Address, other.Address);
        }

        public override bool Equals(object obj)
        {
            if (obj is IPAddress ip)
                return Equals(ip);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Address);
        }
    }
}

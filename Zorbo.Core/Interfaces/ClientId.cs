using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Core.Interfaces
{
    [JsonObject]
    public class ClientId :
        IEquatable<ClientId>,
        IEquatable<Record>,
        IEquatable<IClient>
    {
        [JsonProperty("guid", DefaultValueHandling = DefaultValueHandling.Include)]
        public Guid Guid { get; set; }

        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Include)]
        public IPAddress ExternalIp { get; set; }

        public ClientId() { }

        public ClientId(IClient client)
            : this(client.ClientId.Guid, client.ClientId.ExternalIp) { }

        public ClientId(Guid guid, IPAddress address)
        {
            this.Guid = guid;
            this.ExternalIp = address;
        }

        public override bool Equals(object obj)
        {
#pragma warning disable IDE0038 // Use pattern matching
            if (obj is ClientId)
                return Equals((ClientId)obj);
            else if (obj is IClient)
                return Equals((IClient)obj);
            else if (obj is Record)
                return Equals((Record)obj);
#pragma warning restore IDE0038 // Use pattern matching
            return false;
        }

        public bool Equals(Record other)
        {
            return other != null && (Guid.Equals(other.ClientId.Guid) || ExternalIp.Equals(other.ClientId.ExternalIp));
        }

        public bool Equals(ClientId other)
        {
            return other != null && (Guid.Equals(other.Guid) || ExternalIp.Equals(other.ExternalIp));
        }

        public bool Equals(IClient other)
        {
            return Equals(other?.ClientId);
        }

        public override int GetHashCode()
        {
            return (Guid.GetHashCode() + ExternalIp.GetHashCode()) & int.MaxValue;
        }
    }
}

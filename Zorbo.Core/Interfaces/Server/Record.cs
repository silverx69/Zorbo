using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Transactions;
using Zorbo.Core;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Core.Interfaces.Server
{
    [JsonObject]
    public class Record : 
        IEquatable<Record>,
        IEquatable<IClient>
    {
        [JsonProperty("id", Required = Required.Always)]
        public ClientId ClientId { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("muzzled", Required = Required.Always)]
        public bool Muzzled { get; set; }

        [JsonProperty("trusted", Required = Required.Always)]
        public bool Trusted { get; set; }

        [JsonProperty("last_seen", Required = Required.Always)]
        public DateTime LastSeen { get; set; }

        public bool Equals(Record other)
        {
            return ClientId.Equals(other?.ClientId);
        }

        public bool Equals(IClient other)
        {
            return ClientId.Equals(other?.ClientId);
        }

        public override int GetHashCode()
        {
            return ClientId.GetHashCode();
        }
    }
}

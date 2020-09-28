using Newtonsoft.Json;
using System;
using Zorbo.Core;

namespace Zorbo.Ares.Packets
{
    /// <summary>
    /// Used as the base class for all packets used in the <see cref="Zorbo.Ares.Packets.Channels"/> namespace.
    /// </summary>
    [JsonObject]
    [Serializable]
    public abstract class AresUdpPacket : IPacket
    {
        [JsonProperty("id", Required = Required.Always)]
        public abstract AresUdpId Id { get; }

        byte IPacket.Id { get { return (byte)Id; } }
    }
}

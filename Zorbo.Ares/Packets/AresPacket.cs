using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Zorbo.Ares.Packets.Formatters;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Packets
{
    /// <summary>
    /// Used as the base class for all packets used in the <see cref="Zorbo.Ares.Packets.Chatroom"/> namespace.
    /// </summary>
    [JsonObject]
    [Serializable]
    public abstract class AresPacket : ModelBase, IPacket
    {
        [JsonProperty("id", Required = Required.Always)]
        public abstract AresId Id { get; }

        byte IPacket.Id { get { return (byte)Id; } }
    }
}

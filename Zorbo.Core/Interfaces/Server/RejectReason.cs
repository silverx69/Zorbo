using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Interfaces.Server
{
    public enum RejectReason : byte
    {
        /// <summary>
        /// Client's login is invalid or malicious
        /// </summary>
        InvalidLogin,
        /// <summary>
        /// Username is already being used
        /// </summary>
        NameTaken,
        /// <summary>
        /// User sent a login packet while logged in
        /// </summary>
        Flooded,
        /// <summary>
        /// User is banned from the room
        /// </summary>
        Banned,
        /// <summary>
        /// User's Dns HostName is banned
        /// </summary>
        DnsBanned,
        /// <summary>
        /// User's IP Range is banned
        /// </summary>
        RangeBanned,
        /// <summary>
        /// Too many clients from the same IP
        /// </summary>
        TooManyBots,
        /// <summary>
        /// A plugin has rejected entry to the room
        /// </summary>
        Plugins,
    }
}

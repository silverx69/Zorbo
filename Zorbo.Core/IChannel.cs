using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Zorbo.Core
{
    public interface IChannel : 
        IServerRecord, 
        IHashlink
    {
        string Name { get; set; }
        string Topic { get; set; }
        string Version { get; set; }
        
        ushort Users { get; }

        bool WebSockets { get; }
        bool SupportJson { get; }

        Language Language { get; }
    }
}

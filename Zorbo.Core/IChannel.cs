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

        ushort Users { get; set; }

        IPAddress LocalIp { get; set; }
    }
}

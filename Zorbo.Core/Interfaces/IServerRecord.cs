using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Zorbo.Core.Interfaces
{
    public interface IServerRecord : INotifyPropertyChanged
    {
        ushort Port { get; set; }
        
        uint AckCount { get; set; }
        uint TryCount { get; set; }
        
        DateTime LastAcked { get; set; }
        DateTime LastSendIPs { get; set; }
        DateTime LastAskedFirewall { get; set; }

        IPAddress ExternalIp { get; set; }
    }
}

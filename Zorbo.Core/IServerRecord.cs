using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Zorbo.Core
{
    public interface IServerRecord : INotifyPropertyChanged
    {
        string Domain { get; set; }
        ushort TlsPort { get; set; }

        IPAddress ExternalIp { get; set; }
        IPAddress InternalIp { get; set; }

        ushort Port { get; set; }
    }
}

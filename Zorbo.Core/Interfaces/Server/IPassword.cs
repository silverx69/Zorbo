using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.ComponentModel;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IPassword : INotifyPropertyChanged
    {
        ClientId ClientId { get; set; }
        string Sha1Text { get; set; }
        AdminLevel Level { get; set; }
    }
}

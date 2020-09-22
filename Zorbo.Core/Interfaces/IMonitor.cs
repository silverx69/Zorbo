using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Interfaces
{
    public interface IMonitor : INotifyPropertyChanged
    {
        long SpeedIn { get; }
        long SpeedOut { get; }
        long LastBytesIn { get; }
        long LastBytesOut { get; }
        long TotalBytesIn { get; }
        long TotalBytesOut { get; }
    }
}

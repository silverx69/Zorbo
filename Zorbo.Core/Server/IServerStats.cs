using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Server
{
    public interface IServerStats : 
        IMonitor, 
        INotifyPropertyChanged
    {
        int PeakUsers { get; }

        int Joined { get; }
        int Parted { get; }
        int Rejected { get; }

        int Banned { get; }
        int CaptchaBanned { get; }

        int InvalidLogins { get; }
        int FloodsTriggered { get; }

        int PacketsSent { get; }
        int PacketsReceived { get; }

        DateTime StartTime { get; }
    }
}

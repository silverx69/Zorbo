using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Zorbo.Core
{
    public interface IChannelList : INotifyPropertyChanged
    {
        IMonitor Monitor { get; }

        uint AddIpHits { get; }
        uint AckIpHits { get; }
        uint AckInfoHits { get; }
        uint SendInfoHits { get; }
        uint CheckFirewallHits { get; }

        bool Listing { get; set; }
        bool FirewallOpen { get; }
        bool FinishedTestingFirewall { get; }

        IObservableCollection<IChannel> Channels { get; }
    }
}

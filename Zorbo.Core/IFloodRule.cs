using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Zorbo.Core
{
    public interface IFloodRule : 
        IEquatable<IFloodRule>, 
        INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the flood rule
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The packet id to monitor for flooding
        /// </summary>
        byte Id { get; }
        
        /// <summary>
        /// The number of packets to accept before considering it a flood
        /// </summary>
        double Count { get; set; }

        /// <summary>
        /// Gets or sets the amount of time, in milliseconds, after the last packet before the counter is reset
        /// </summary>
        double Timeout { get; set; }

    }
}

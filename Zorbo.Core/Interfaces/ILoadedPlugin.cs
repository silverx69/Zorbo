using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Interfaces
{
    public interface ILoadedPlugin<THost, TPlugin> : INotifyPropertyChanged where TPlugin : IPlugin<THost>
    {
        string Name { get; }
        
        TPlugin Plugin { get; }

        bool Enabled { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core.Interfaces;
using Zorbo.Core.Models;

namespace Zorbo.Core.Plugins
{
    public class LoadedPlugin<THost, TPlugin> : ModelBase, ILoadedPlugin<THost, TPlugin> where TPlugin : IPlugin<THost>
    {
#pragma warning disable IDE0044 // Add readonly modifier
        TPlugin plugin = default;
        bool enabled = false;
        string name = string.Empty;
#pragma warning restore IDE0044 // Add readonly modifier

        public string Name {
            get { return name; }
            set { OnPropertyChanged(() => name, value); }
        }

        public TPlugin Plugin {
            get { return plugin; }
            set { OnPropertyChanged(() => plugin, value); }
        }

        public Boolean Enabled {
            get { return enabled; }
            set { OnPropertyChanged(() => enabled, value); }
        }

        public LoadedPlugin(string name, TPlugin plugin) {
            Name = name;
            Plugin = plugin;
        }
    }
}

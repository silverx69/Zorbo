using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Ares.Server;

namespace Zorbo.Server.WPF
{
    /// <summary>
    /// Dirivative of AresServerConfig. Used to implement Windows specific properties or settings.
    /// </summary>
    public class Configuration : AresServerConfig
    {
#pragma warning disable IDE0044 // Add readonly modifier
        bool autoload = false;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("autoload")]
        public bool LoadWithWindows {
            get { return autoload; }
            set { OnPropertyChanged(() => autoload, value); }
        }
    }
}

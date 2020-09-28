using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zorbo.Core;
using Zorbo.Core.Models;

namespace Zorbo.Server.WPF
{
    public class Directories : ModelBase, IDirectories
    {
#pragma warning disable IDE0044 // Add readonly modifier
        string appData;
        string appLogs;
        //string appCache;
        string appCerts;
        string appPlugins;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("data")]
        public string AppData {
            get { return appData; }
            set { OnPropertyChanged(() => appData, value); }
        }

        [JsonProperty("logs")]
        public string Logs {
            get { return appLogs; }
            set { OnPropertyChanged(() => appLogs, value); }
        }

        [JsonProperty("plugins")]
        public string Plugins {
            get { return appPlugins; }
            set { OnPropertyChanged(() => appPlugins, value); }
        }

        [JsonProperty("certs")]
        public string Certificates {
            get { return appCerts; }
            set { OnPropertyChanged(() => appCerts, value); }
        }

        public static string BaseDirectory {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public Directories()
        {
            appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zorbo");
            appLogs = Path.Combine(appData, "Logs");
            appPlugins = Path.Combine(appData, "Plugins");
            appCerts = Path.Combine(appData, "Certs");
        }

        public void EnsureExists() {
            if (!Directory.Exists(AppData)) Directory.CreateDirectory(AppData);
            if (!Directory.Exists(Logs)) Directory.CreateDirectory(Logs);
            if (!Directory.Exists(Plugins)) Directory.CreateDirectory(Plugins);
            if (!Directory.Exists(Certificates)) Directory.CreateDirectory(Certificates);
        }
    }
}

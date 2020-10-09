using Newtonsoft.Json;
using System;
using System.IO;
using Zorbo.Core.Models;

namespace Zorbo.Core
{
    public class Directories : ModelBase, IDirectories
    {
#pragma warning disable IDE0044 // Add readonly modifier
        string appData;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("data")]
        public string AppData {
            get { return appData; }
            set {
                if (appData != value) {
                    appData = value;
                    OnPropertyChanged(nameof(AppData));
                    OnPropertyChanged(nameof(Logs));
                    OnPropertyChanged(nameof(Plugins));
                    OnPropertyChanged(nameof(Certificates));
                }
            }
        }

        [JsonProperty("logs")]
        public string Logs {
            get { return Path.Combine(AppData, "Logs"); }
        }

        [JsonProperty("plugins")]
        public string Plugins {
            get { return Path.Combine(AppData, "Plugins"); }
        }

        [JsonProperty("certs")]
        public string Certificates {
            get { return Path.Combine(AppData, "Certs"); }
        }

        public static string BaseDirectory {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public Directories()
        {
            AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zorbo");
        }

        public Directories(string appDataDirectory)
        {
            AppData = appDataDirectory;
        }

        public void EnsureExists() {
            if (!Directory.Exists(AppData)) Directory.CreateDirectory(AppData);
            if (!Directory.Exists(Logs)) Directory.CreateDirectory(Logs);
            if (!Directory.Exists(Plugins)) Directory.CreateDirectory(Plugins);
            if (!Directory.Exists(Certificates)) Directory.CreateDirectory(Certificates);
        }
    }
}

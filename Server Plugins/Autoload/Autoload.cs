using System;
using System.Collections.ObjectModel;
using System.IO;
using Zorbo.Core;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;
using Zorbo.Core.Plugins.Server;

namespace Autoload
{
    public class Autoload : ServerPlugin
    {
        public ModelList<string> Plugins {
            get;
            private set;
        }

        public Autoload() {
            this.Plugins = new ModelList<string>();
        }

        public override void OnPluginLoaded() {
            LoadFile();
            Server.PluginHost.KillPlugin("Autoload");
        }

        internal void LoadFile()
        {
            Plugins.Clear();

            Stream stream = null;
            StreamReader reader = null;

            try {
                string name = Path.Combine(Directory, "Autoload.txt");

                if (!File.Exists(name))
                    stream = File.Create(name);
                else
                    stream = File.Open(name, FileMode.Open, FileAccess.Read);

                reader = new StreamReader(stream);

                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();

                    if (!String.IsNullOrEmpty(line))
                        Plugins.Add(line);
                }
            }
            catch { }
            finally {
                if (reader != null) {
                    reader.Close();
                    reader.Dispose();
                }

                if (stream != null)
                    stream.Dispose();
            }

            Plugins.ForEach((s) => Server.PluginHost.LoadPlugin(s));
        }

        internal void SaveFile()
        {
            Stream stream = null;
            StreamWriter writer = null;

            try {
                stream = File.Open(Path.Combine(Directory, "Autoload.txt"), FileMode.Create, FileAccess.Write);
                writer = new StreamWriter(stream);

                Plugins.ForEach((s) => writer.WriteLine(s));

                writer.Flush();
            }
            finally {
                if (writer != null) {
                    writer.Close();
                    writer.Dispose();
                }

                if (stream != null)
                    stream.Dispose();
            }
        }
    }
}

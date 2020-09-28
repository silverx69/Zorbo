using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zorbo.Core.Models;

namespace Zorbo.Core.Plugins
{
    public abstract class PluginHost<THost, TPlugin> :
        ModelList<LoadedPlugin<THost, TPlugin>>,
        IEnumerable<LoadedPlugin<THost, TPlugin>>,
        IPluginHost<THost, TPlugin>,
        IEnumerable where TPlugin : IPlugin<THost>
    {
        public override int Count {
            get { return this.Count(s => s.Enabled); }
        }

        public string BaseDirectory {
            get;
            private set;
        }

        public PluginHost(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
        }

        public bool LoadPlugin(string name)
        {
            return LoadPlugin(name, true);
        }

        internal bool LoadPlugin(string name, bool enable)
        {
            string lowname = name.ToLower();

            if (lowname.EndsWith(".dll")) {
                name = name[0..^4];
                lowname = lowname[0..^4];
            }

            lock (SyncRoot) {
                int index = this.FindIndex(s => s.Name.ToLower() == lowname);

                if (index > -1) {
                    EnablePlugin(this[index]);
                    return true;
                }
            }

            string dir = Path.Combine(BaseDirectory, name);
            string file = Path.Combine(dir, name + ".dll");

            if (!Directory.Exists(dir) || !File.Exists(file))
                return false;

            try {
                var assembly = Assembly.LoadFrom(file);

                var pluginType = typeof(TPlugin);
                var impl = assembly.GetExportedTypes().Find(s => pluginType.IsAssignableFrom(s));

                if (impl == null)
                    throw new PluginLoadException("Specified plugin does not contain a valid IPlugin implementation.");

                var plugin = new LoadedPlugin<THost, TPlugin>(name, (TPlugin)Activator.CreateInstance(impl));

                lock (SyncRoot) this.Add(plugin);
                if (enable) EnablePlugin(plugin);

                return true;
            }
            catch (PluginLoadException plex) {
                OnError(GetType().Name, nameof(LoadPlugin), plex);
            }
            catch (Exception ex) {
                OnError(GetType().Name, nameof(LoadPlugin), new PluginLoadException("The plugin failed to load (See inner exception for details).", ex));
            }
            return false;
        }

        private void EnablePlugin(LoadedPlugin<THost, TPlugin> plugin)
        {
            if (!plugin.Enabled) {
                plugin.Enabled = true;
                RaisePropertyChanged(nameof(Count));
                OnPluginLoaded(plugin);
            }
        }

        private void DisablePlugin(LoadedPlugin<THost, TPlugin> plugin)
        {
            if (plugin.Enabled) {
                plugin.Enabled = false;
                RaisePropertyChanged(nameof(Count));
                OnPluginKilled(plugin);
            }
        }

        public bool ImportPlugin(string path)
        {
            if (File.Exists(path)) {
                try {
                    FileInfo info = new FileInfo(path);

                    if (info.Extension != ".dll")
                        throw new FileLoadException("Invalid plugin file specified. Plugin must be a Class Library (.dll).");

                    string plugin_path = Path.Combine(BaseDirectory, info.Name);

                    if (!Directory.Exists(plugin_path))
                        Directory.CreateDirectory(plugin_path);

                    info.CopyTo(plugin_path, true);
                    return LoadPlugin(info.Name);
                }
                catch (Exception ex) {
                    OnError(GetType().Name, nameof(ImportPlugin), ex);
                }
            }

            return false;
        }

        public void KillPlugin(string name)
        {
            lock (SyncRoot) {
                string lowname = name.ToLower();
                int index = this.FindIndex(s => s.Name.ToLower() == lowname);

                if (index == -1) return;
                DisablePlugin(this[index]);
            }
        }


        protected abstract void OnPluginLoaded(LoadedPlugin<THost, TPlugin> plugin);

        protected abstract void OnPluginKilled(LoadedPlugin<THost, TPlugin> plugin);


        protected virtual void OnError(LoadedPlugin<THost, TPlugin> plugin, string method, Exception ex)
        {
            OnError(plugin.Name, method, ex);
        }

        protected virtual void OnError(string name, string method, Exception ex)
        {
            var error = new PluginErrorInfo(name, method, ex);

            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnError(error);
                }
                catch {
                    /* do not route back into the plugins.. possible stack overflow */
                    /* ToDo: report to log? */
                }
            }
        }


        protected void RaisePluginLoaded(LoadedPlugin<THost, TPlugin> plugin)
        {
            Loaded?.Invoke(this, plugin);
        }

        protected void RaisePluginKilled(LoadedPlugin<THost, TPlugin> plugin)
        {
            Killed?.Invoke(this, plugin);
        }

        public event PluginEventHandler<THost, TPlugin> Loaded;
        public event PluginEventHandler<THost, TPlugin> Killed;
    }
}
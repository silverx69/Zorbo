using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zorbo.Core.Models;

namespace Zorbo.Core.Plugins
{
    public abstract class PluginHost<TPlugin> :
        ModelList<LoadedPlugin<TPlugin>>,
        IEnumerable<LoadedPlugin<TPlugin>>,
        IPluginHost<TPlugin>,
        IEnumerable where TPlugin : IPlugin
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

        protected virtual bool LoadPlugin(string name, bool enable)
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

            try {
                var context = GetPluginContext(name);
                if (context == null) return false;

                var assembly = context.LoadFromAssemblyPath(context.FilePath);

                Type impl = null;
                Type pluginType = typeof(TPlugin);

                foreach (var type in assembly.GetExportedTypes()) {
                    if (pluginType.IsAssignableFrom(type))
                        impl = type;
                }

                if (impl == null)
                    throw new PluginLoadException("Specified plugin does not contain a valid IPlugin implementation.");

                var plugin = new LoadedPlugin<TPlugin>(name, (TPlugin)Activator.CreateInstance(impl));

                lock (SyncRoot) this.Add(plugin);
                if (enable) EnablePlugin(plugin);

                return true;
            }
            catch (PluginLoadException plex) {
                OnError(GetType().Name, nameof(LoadPlugin), plex);
            }
            catch (Exception ex) {
                OnError(GetType().Name, nameof(LoadPlugin), ex);
            }
            return false;
        }

        protected void EnablePlugin(LoadedPlugin<TPlugin> plugin)
        {
            if (plugin.Enabled) 
                DisablePlugin(plugin);
            plugin.Enabled = true;
            RaisePropertyChanged(nameof(Count));
            OnPluginLoaded(plugin);
        }

        protected void DisablePlugin(LoadedPlugin<TPlugin> plugin)
        {
            if (plugin.Enabled) {
                plugin.Enabled = false;
                RaisePropertyChanged(nameof(Count));
                OnPluginKilled(plugin);
            }
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


        protected virtual PluginContext GetPluginContext(string dllname)
        {
            return new PluginContext(Path.Combine(BaseDirectory, dllname, dllname + ".dll"));
        }


        protected abstract void OnPluginLoaded(LoadedPlugin<TPlugin> plugin);

        protected abstract void OnPluginKilled(LoadedPlugin<TPlugin> plugin);


        protected virtual void OnError(LoadedPlugin<TPlugin> plugin, string method, Exception ex)
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


        protected void RaisePluginLoaded(LoadedPlugin<TPlugin> plugin)
        {
            Loaded?.Invoke(this, plugin);
        }

        protected void RaisePluginKilled(LoadedPlugin<TPlugin> plugin)
        {
            Killed?.Invoke(this, plugin);
        }

        public event PluginEventHandler<TPlugin> Loaded;
        public event PluginEventHandler<TPlugin> Killed;
    }
}
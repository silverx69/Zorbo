using System.Collections.Specialized;
using System.ComponentModel;

namespace Zorbo.Core.Plugins
{
    public interface IPluginHost<TPlugin> :
        IObservableCollection<LoadedPlugin<TPlugin>>,
        INotifyPropertyChanged,
        INotifyCollectionChanged where TPlugin : IPlugin
    {
        bool LoadPlugin(string name);
        void KillPlugin(string name);

        event PluginEventHandler<TPlugin> Loaded;
        event PluginEventHandler<TPlugin> Killed;
    }

    public delegate void PluginEventHandler<TPlugin>(object sender, LoadedPlugin<TPlugin> plugin) where TPlugin : IPlugin;
}

using System.Collections.Specialized;
using System.ComponentModel;

namespace Zorbo.Core.Plugins
{
    public interface IPluginHost<THost, TPlugin> :
        IObservableCollection<LoadedPlugin<THost, TPlugin>>,
        INotifyPropertyChanged,
        INotifyCollectionChanged where TPlugin : IPlugin<THost>
    {
        bool LoadPlugin(string name);
        bool ImportPlugin(string path);
        void KillPlugin(string name);

        event PluginEventHandler<THost, TPlugin> Loaded;
        event PluginEventHandler<THost, TPlugin> Killed;
    }

    public delegate void PluginEventHandler<THost, TPlugin>(object sender, LoadedPlugin<THost, TPlugin> plugin) where TPlugin : IPlugin<THost>;
}

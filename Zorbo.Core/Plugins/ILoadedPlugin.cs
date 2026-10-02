using System.ComponentModel;

namespace Zorbo.Core.Plugins
{
    public interface ILoadedPlugin<TPlugin> : INotifyPropertyChanged where TPlugin : IPlugin
    {
        string Name { get; }
        
        bool Enabled { get; set; }

        TPlugin Plugin { get; }
    }
}

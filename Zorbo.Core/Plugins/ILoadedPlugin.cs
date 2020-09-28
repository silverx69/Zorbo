using System.ComponentModel;

namespace Zorbo.Core.Plugins
{
    public interface ILoadedPlugin<THost, TPlugin> : INotifyPropertyChanged where TPlugin : IPlugin<THost>
    {
        string Name { get; }
        
        TPlugin Plugin { get; }

        bool Enabled { get; set; }
    }
}

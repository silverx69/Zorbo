using System.Windows.Controls;
using Zorbo.Core.Plugins.Server;

namespace Zorbo.Server.WPF.Plugins
{
    /// <summary>
    /// Provides a WPF-based interface for implementing a Zorbo plugin that has a UI
    /// </summary>
    public interface IWPFPlugin : IServerPlugin
    {
        Control GUI { get; }
    }
}

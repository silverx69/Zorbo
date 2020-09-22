using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Server.WPF.Plugins
{
    /// <summary>
    /// Provides a WindowsForms-based interface for implementing a Zorbo plugin that has a UI
    /// </summary>
    public interface IWinFormsPlugin : IServerPlugin
    {
        Control GUI { get; }
    }
}

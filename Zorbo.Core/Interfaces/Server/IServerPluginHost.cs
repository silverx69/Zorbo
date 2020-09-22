using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Plugins.Server;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IServerPluginHost : IPluginHost<IServer, ServerPlugin>
    {
    }
}

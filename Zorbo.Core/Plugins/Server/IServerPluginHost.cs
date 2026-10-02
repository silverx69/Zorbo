using Zorbo.Core.Server;

namespace Zorbo.Core.Plugins.Server
{
    public interface IServerPluginHost : IPluginHost<ServerPlugin>
    {
        IServer Server { get; set; }
    }
}

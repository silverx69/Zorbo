using System.IO;
using Zorbo.Core.Server;

namespace Zorbo.Core.Plugins.Server
{
    public class ServerPluginHostCore : ServerPluginHost
    {
        public ServerPluginHostCore(IServer server)
            : base(server) { }

        protected override PluginContext GetPluginContext(string dllname)
        {
            return new PluginContextCore(Path.Combine(BaseDirectory, dllname, dllname + ".dll"));
        }
    }
}

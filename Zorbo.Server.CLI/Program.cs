using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using Zorbo.Ares.Server;
using Zorbo.Core;
using Zorbo.Core.Models;


namespace Zorbo.Server.CLI
{
    class Program
    {
        static AresServer server;
        static ManualResetEvent shutdown = new ManualResetEvent(false);


        static int Main(string[] args)
        {
            Console.Title = "Zorbo";

            var config = Persistence.LoadModel<AresServerConfig>(Path.Combine(Directories.AppData, "config.json"));
            server = new AresServer(config);

            server.PluginHost.LoadPlugin("Autoload");

            if (server.Config.AutoStartServer)
                server.Start();

            AssemblyLoadContext.Default.Unloading += Default_Unloading;

            shutdown.WaitOne();

            return 0;
        }

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            if (server.Running)
            {
                server.Stop();
            }

            shutdown.Set();
        }
    }
}

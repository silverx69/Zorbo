using System;
using System.IO;
using Zorbo.Core.Client;

namespace Zorbo.Core.Plugins.Client
{
    public class ChatClientPluginHost : 
        PluginHost<IChatClient, ChatClientPlugin>,
        IChatClientPluginHost
    {
        public IChatClient Client {
            get;
            private set;
        }

        public ChatClientPluginHost(IChatClient client)
            : base(client.Directories.AppData)
        {
            Client = client;
        }

        protected override void OnPluginLoaded(LoadedPlugin<IChatClient, ChatClientPlugin> plugin)
        {
            try {
                plugin.Plugin.Directory = Path.Combine(BaseDirectory, plugin.Name);
                plugin.Plugin.OnPluginLoaded(Client);
            }
            catch (Exception ex) {
                OnError(plugin, nameof(OnPluginLoaded), ex);
            }

            try {
                RaisePluginLoaded(plugin);
            }
            catch (Exception ex) {
                OnError(plugin, "Loaded::EventHandler", ex);
            }
        }

        protected override void OnPluginKilled(LoadedPlugin<IChatClient, ChatClientPlugin> plugin)
        {
            try {
                plugin.Plugin.OnPluginKilled();
            }
            catch (Exception ex) {
                OnError(plugin, nameof(OnPluginKilled), ex);
            }

            try {
                RaisePluginKilled(plugin);
            }
            catch (Exception ex) {
                OnError(plugin, "Killed::EventHandler", ex);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Client;

namespace Zorbo.Core.Plugins.Client
{
    public abstract class ChatClientPlugin : IChatClientPlugin
    {
        /// <summary>
        /// Gets or sets the IServer instance hosting this plugin.
        /// </summary>
        public IChatClient ChatClient {
            get;
            protected set;
        }
        /// <summary>
        /// Gets / sets the full path to the directory the plugin was loaded from. 
        /// Set by the PluginHost so the IPlugin knows where it was loaded from, has no effect if modified.
        /// </summary>
        public string Directory { get; set; }
        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        public void OnPluginLoaded(IChatClient client)
        {
            ChatClient = client;
            OnPluginLoaded();
        }
        /// <summary>
        /// Called when the plugin is loaded. Not used by Host application, this is for simplicity purposes.
        /// </summary>
        public virtual void OnPluginLoaded() { }
        /// <summary>
        /// Called when the plugin is killed
        /// </summary>
        public virtual void OnPluginKilled() { }
        /// <summary>
        /// Occurs when an unhandled exception occurs in any plugin (all plugins receive notification [for debugging?])
        /// </summary>
        public virtual void OnError(IErrorInfo error) { }
    }
}

namespace Zorbo.Core.Plugins
{
    public interface IPlugin<T>
    {
        /// <summary>
        /// Sets the full path to the directory the plugin was loaded from. 
        /// Set by the PluginHost so the IPlugin knows where it was loaded from, has no effect if modified.
        /// </summary>
        string Directory { set; }
        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        void OnPluginLoaded(T host);
        /// <summary>
        /// Called when the plugin is killed
        /// </summary>
        void OnPluginKilled();
        /// <summary>
        /// Occurs when an unhandled exception occurs in any plugin (all plugins receive notification [for debugging?])
        /// </summary>
        void OnError(IErrorInfo error);
    }
}

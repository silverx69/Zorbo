using System;
using System.IO;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Core.Plugins.Server
{
    public class ServerPluginHost : 
        PluginHost<IServer, ServerPlugin>,
        IServerPluginHost
    {
        public IServer Server {
            get;
            private set;
        }

        public ServerPluginHost(IServer server) 
            : base() {
            Server = server;
        }

        protected override void OnPluginLoaded(LoadedPlugin<IServer, ServerPlugin> plugin)
        {
            try {
                plugin.Plugin.Directory = Path.Combine(Directories.Plugins, plugin.Name);
                plugin.Plugin.OnPluginLoaded(Server);
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

        protected override void OnPluginKilled(LoadedPlugin<IServer, ServerPlugin> plugin)
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


        public void OnCaptcha(IClient client, CaptchaEvent @event)
        {
            foreach (var plugin in this) {
                try {
                    plugin.Plugin.OnCaptcha(client, @event);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnCaptcha), ex);
                }
            }
        }


        public void OnJoinRejected(IClient client, RejectReason reason)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnJoinRejected(client, reason);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnJoinRejected), ex);
                }
            }
        }

        public SupportFlags OnSendFeatures(IClient client, SupportFlags features)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        features |= plugin.Plugin.OnSendFeatures(client, features);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnSendFeatures), ex);
                }
            }
            return features;
        }

        public void OnSendJoin(IClient client)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnSendJoin(client);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnSendJoin), ex);
                }
            }
        }

        public bool OnJoinCheck(IClient client)
        {
            bool ret = true;

            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled && !plugin.Plugin.OnJoinCheck(client))
                        ret = false;
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnJoinCheck), ex);
                }
            }

            return ret;
        }

        public void OnJoin(IClient client)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnJoin(client);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnJoin), ex);
                }
            }
        }

        public void OnPart(IClient client, Object state)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnPart(client, state);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnPart), ex);
                }
            }
        }


        public bool OnVroomJoinCheck(IClient client, UInt16 vroom)
        {
            bool ret = true;

            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled && !plugin.Plugin.OnVroomJoinCheck(client, vroom))
                        ret = false;
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnVroomJoinCheck), ex);
                }
            }

            return ret;
        }

        public void OnVroomJoin(IClient client)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnVroomJoin(client);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnVroomJoin), ex);
                }
            }
        }

        public void OnVroomPart(IClient client)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnVroomPart(client);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnVroomPart), ex);
                }
            }
        }

        public void OnHelp(IClient client)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnHelp(client);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnHelp), ex);
                }
            }
        }

        public void OnLogin(IClient client, IPassword password)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnLogin(client, password);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnLogin), ex);
                }
            }
        }

        public bool OnRegister(IClient client, IPassword password)
        {
            bool ret = true;

            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled && !plugin.Plugin.OnRegister(client, password))
                        ret = false;
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnRegister), ex);
                }
            }

            return ret;
        }

        public bool OnFileReceived(IClient client, ISharedFile file)
        {
            bool ret = true;

            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled && !plugin.Plugin.OnFileReceived(client, file))
                        ret = false;
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnFileReceived), ex);
                }
            }

            return ret;
        }


        public bool OnBeforePacket(IClient client, IPacket packet)
        {
            bool ret = true;

            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled && !plugin.Plugin.OnBeforePacket(client, packet))
                        ret = false;
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnBeforePacket), ex);
                }
            }

            return ret;
        }

        public void OnAfterPacket(IClient client, IPacket packet)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnAfterPacket(client, packet);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnAfterPacket), ex);
                }
            }
        }

        public void OnPacketSent(IClient client, IPacket packet)
        {
            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled)
                        plugin.Plugin.OnPacketSent(client, packet);
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnAfterPacket), ex);
                }
            }
        }

        public bool OnFlood(IClient client, IFloodRule rule, IPacket packet)
        {
            bool ret = true;

            foreach (var plugin in this) {
                try {
                    if (plugin.Enabled && !plugin.Plugin.OnFlood(client, rule, packet))
                        ret = false;
                }
                catch (Exception ex) {
                    OnError(plugin, nameof(OnFlood), ex);
                }
            }

            return ret;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Server;

namespace Zorbo.Core.Plugins.Server
{
    public abstract class ServerPlugin : IServerPlugin
    {
        /// <summary>
        /// Gets or sets the IServer instance hosting this plugin.
        /// </summary>
        public IServer Server { get; set; }
        /// <summary>
        /// Gets or sets a value specifying custom command triggers
        /// </summary>
        public string[] CustomTriggers { get; protected set; }
        /// <summary>
        /// Gets / sets the full path to the directory the plugin was loaded from. 
        /// Set by the PluginHost so the IPlugin knows where it was loaded from, has no effect if modified.
        /// </summary>
        public string Directory { get; set; }
        /// <summary>
        /// Called when the plugin is loaded. Not used by Host application, this is for simplicity purposes.
        /// </summary>
        public virtual void OnPluginLoaded() { }
        /// <summary>
        /// Called when the plugin is killed
        /// </summary>
        public virtual void OnPluginKilled() { }
        /// <summary>
        /// Occurs when a user enters, exits, answers, or gets banned by the captcha
        /// </summary>
        public virtual void OnCaptcha(IClient client, CaptchaEvent @event) { }
        /// <summary>
        /// Occurs when a user joins the room for the first time, allows adding or overriding server features
        /// </summary>
        public virtual SupportFlags OnSendFeatures(IClient client, SupportFlags features) { return features; }
        /// <summary>
        /// Occurs when the user joins the room or any vroom (like userlist, avatars, etc)
        /// </summary>
        public virtual void OnSendJoin(IClient client) { }
        /// <summary>
        /// Checks if a user is allowed to join the room before sending the join message to the room
        /// </summary>
        public virtual bool OnJoinCheck(IClient client) { return true; }
        /// <summary>
        /// Occurs when a user is denied logging into the server
        /// </summary>
        public virtual void OnJoinRejected(IClient client, RejectReason reason) { }
        /// <summary>
        /// Occurs after a user is allowed to join the room
        /// </summary>
        public virtual void OnJoin(IClient client) { }
        /// <summary>
        /// Occurs when a user disconnects from the room (with state, if any, supplied at IClient.Disconnect(state))
        /// </summary>
        public virtual void OnPart(IClient client, Object state) { }
        /// <summary>
        /// Occurs before a user moves from one vroom to another, if function returns false, the user is denied
        /// </summary>
        public virtual bool OnVroomJoinCheck(IClient client, UInt16 vroom) { return true; }
        /// <summary>
        /// Occurs after a user is allowed to move from one vroom to another
        /// </summary>
        public virtual void OnVroomJoin(IClient client) { }
        /// <summary>
        /// Occurs when a user leaves from any vroom
        /// </summary>
        public virtual void OnVroomPart(IClient client) { }
        /// <summary>
        /// Occurs when a user performs the help command
        /// </summary>
        /// <param name="client"></param>
        public virtual void OnHelp(IClient client) { }
        /// <summary>
        /// Occurs when a user logs into the server using a password
        /// </summary>
        public virtual void OnLogin(IClient client, IPassword password) { }
        /// <summary>
        /// Occurs when a user registers a password with the server, if function returns false, the password is rejected
        /// </summary>
        public virtual bool OnRegister(IClient client, IPassword password) { return true; }
        /// <summary>
        /// Occurs when a user sends a shared file item, if function returns false, the file is rejected
        /// </summary>
        public virtual bool OnFileReceived(IClient client, ISharedFile file) { return true; }
        /// <summary>
        /// Occurs during processing of public text messages. These behave like built-in commands and cannot be overriden.
        /// </summary>
        public virtual bool OnTextCommand(IClient client, string cmd, string args) { return true; }
        /// <summary>
        /// Occurs just before any packet (minus join, login, regiser), some packets (public, emote, personal, private, etc are
        /// overridable by returning false.
        /// </summary>
        public virtual bool OnBeforePacket(IClient client, IPacket packet) { return true; }
        /// <summary>
        /// Occurs after a packet is received or not overriden by a plugin
        /// </summary>
        public virtual void OnAfterPacket(IClient client, IPacket packet) { }
        /// <summary>
        /// Occurs after a packet is successfully send to a client
        /// </summary>
        public virtual void OnPacketSent(IClient client, IPacket packet) { }
        /// <summary>
        /// Occurs when an anonymous http(s) request occurs on the socket (A normal http(s) request).
        /// </summary>
        /// <returns></returns>
        public virtual bool OnHttpRequest(ISocket socket, RequestEventArgs args) { return true; }
        /// <summary>
        /// Occurs when a http(s) request occurs on the socket from an IClient already connected (This is not normal behaviour, but technically possible).
        /// </summary>
        /// <returns></returns>
        public virtual bool OnHttpRequest(IClient socket, RequestEventArgs args) { return true; }
        /// <summary>
        /// Occurs when a user floods the server with packets, returning false will prevent Zorbo from handling the packet. 
        /// It's up to the Plugin to determine any extra action taken when a flood rule is broken.
        /// </summary>
        /// <param name="client">The client that triggered the flood rule.</param>
        /// <param name="rule">The flood rule that was violated.</param>
        /// <param name="packet">The packet that was sent that triggered the flood rule.</param>
        /// <returns></returns>
        public virtual bool OnFlood(IClient client, IFloodRule rule, IPacket packet) { return true; }
        /// <summary>
        /// Occurs when an unhandled exception occurs in any plugin (all plugins receive notification [for debugging?])
        /// </summary>
        public virtual void OnError(IErrorInfo error) { }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Zorbo.Core.Server;

namespace Zorbo.Core.Plugins.Server
{
    public interface IServerPlugin : IPlugin
    {
        /// <summary>
        /// Gets or sets the IServer instance hosting this plugin.
        /// </summary>
        IServer Server { get; set; }
        /// <summary>
        /// Gets or sets a value specifying custom command triggers
        /// </summary>
        string[] CustomTriggers { get; }
        /// <summary>
        /// Occurs when a user enters, exits, answers, or gets banned by the captcha
        /// </summary>
        void OnCaptcha(IClient client, CaptchaEvent @event);
        /// <summary>
        /// Occurs when a user joins the room for the first time, allows adding or overriding server features
        /// </summary>
        /// <returns></returns>
        SupportFlags OnSendFeatures(IClient client, SupportFlags features);
        /// <summary>
        /// Occurs when the user joins the room or any vroom (like userlist, avatars, etc)
        /// </summary>
        void OnSendJoin(IClient client);
        /// <summary>
        /// Checks if a user is allowed to join the room before sending the join message to the room
        /// </summary>
        bool OnJoinCheck(IClient client);
        /// <summary>
        /// Occurs when a user is denied logging into the server
        /// </summary>
        void OnJoinRejected(IClient client, RejectReason reason);
        /// <summary>
        /// Occurs after a user is allowed to join the room
        /// </summary>
        void OnJoin(IClient client);
        /// <summary>
        /// Occurs when a user disconnects from the room (with state, if any, supplied at IClient.Disconnect(state))
        /// </summary>
        void OnPart(IClient client, Object state);
        /// <summary>
        /// Occurs before a user moves from one vroom to another, if function returns false, the user is denied
        /// </summary>
        bool OnVroomJoinCheck(IClient client, UInt16 vroom);
        /// <summary>
        /// Occurs after a user is allowed to move from one vroom to another
        /// </summary>
        void OnVroomJoin(IClient client);
        /// <summary>
        /// Occurs when a user leaves from any vroom
        /// </summary>
        void OnVroomPart(IClient client);
        /// <summary>
        /// Occurs when a user performs the help command
        /// </summary>
        /// <param name="client"></param>
        void OnHelp(IClient client);
        /// <summary>
        /// Occurs when a user logs into the server using a password
        /// </summary>
        void OnLogin(IClient client, IPassword password);
        /// <summary>
        /// Occurs when a user logs out of the server (sets their admin to 'User')
        /// </summary>
        void OnLogout(IClient client);
        /// <summary>
        /// Occurs when a user registers a password with the server, if function returns false, the password is rejected
        /// </summary>
        bool OnRegister(IClient client, IPassword password);
        /// <summary>
        /// Occurs when a user sends a shared file item, if function returns false, the file is rejected
        /// </summary>
        bool OnFileReceived(IClient client, ISharedFile file);
        /// <summary>
        /// Occurs during processing of public text messages. These behave like built-in commands and cannot be overriden.
        /// </summary>
        bool OnTextCommand(IClient client, string cmd, string args);
        /// <summary>
        /// Occurs just before any packet (minus join, login, regiser), some packets (public, emote, personal, private, etc are
        /// overridable by returning false.
        /// </summary>
        bool OnBeforePacket(IClient client, IPacket packet);
        /// <summary>
        /// Occurs after a packet is received or not overriden by a plugin
        /// </summary>
        void OnAfterPacket(IClient client, IPacket packet);
        /// <summary>
        /// Occurs after a packet is successfully send to a client
        /// </summary>
        void OnPacketSent(IClient client, IPacket packet);
        /// <summary>
        /// Occurs when an anonymous http(s) request occurs on the socket (A normal http(s) request).
        /// </summary>
        /// <returns></returns>
        bool OnHttpRequest(ISocket socket, HttpRequestEventArgs args);
        /// <summary>
        /// Occurs when a http(s) request occurs on the socket from an IClient already connected (This is not normal behaviour, but technically possible).
        /// </summary>
        /// <returns></returns>
        bool OnHttpRequest(IClient socket, HttpRequestEventArgs args);
        /// <summary>
        /// Occurs when a user floods the server with packets
        /// </summary>
        /// <param name="client">The client that triggered the flood rule.</param>
        /// <param name="rule">The flood rule that was violated.</param>
        /// <param name="packet">The packet that was sent that triggered the flood rule.</param>
        /// <returns></returns>
        bool OnFlood(IClient client, IFloodRule rule, IPacket packet);
    }
}

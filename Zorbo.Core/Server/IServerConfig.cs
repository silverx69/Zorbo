using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Security;

namespace Zorbo.Core.Server
{
    public interface IServerConfig : IConfig, INotifyPropertyChanged
    {
        /// <summary>
        /// True to show the bot user in the userlist, otherwise false
        /// </summary>
        bool ShowBot { get; set; }
        /// <summary>
        /// The name of the server bot in the userlist
        /// </summary>
        string BotName { get; set; }
        /// <summary>
        /// The server's topic
        /// </summary>
        string Topic { get; set; }
        /// <summary>
        /// The server's original topic
        /// </summary>
        string OrgTopic { get; }
        /// <summary>
        /// The port the server will listen for connections on
        /// </summary>
        ushort Port { get; }
        /// <summary>
        /// The ip address of the adapter that the socket will bind to, this is generally left as 0.0.0.0
        /// </summary>
        IPAddress LocalIp { get; set; }
        /// <summary>
        /// Default avatar for the server
        /// </summary>
        byte[] OrgAvatar { get; }
        /// <summary>
        /// The website link of the server
        /// </summary>
        Website Website { get; set; }
        /// <summary>
        /// The number of clients allowed per ipaddress
        /// </summary>
        ushort MaxClones { get; set; }
        /// <summary>
        /// The maximum number of clients allowed to be in the room at once
        /// </summary>
        ushort MaxClients { get; }
        /// <summary>
        /// The common (often preferred) language of the server
        /// </summary>
        Language Language { get; set; }
        /// <summary>
        /// True to hide IP addresses from other users, otherwise false
        /// </summary>
        bool HideIPs { get; set; }
        /// <summary>
        /// True if all connections on Local Area Network are host, otherwise false
        /// </summary>
        bool LocalAreaIsHost { get; set; }
        /// <summary>
        /// True if users who are muzzled can send pm's, otherwise false
        /// </summary>
        bool MuzzledPMs { get; set; }
        /// <summary>
        /// True if the server can use UDP sockets, otherwise false.
        /// </summary>
        bool UseUdpSockets { get; set; }
        /// <summary>
        /// True if the server can use TCP sockets, otherwise false.
        /// </summary>
        bool UseTcpSockets { get; set; }
        /// <summary>
        /// True if the server should listen for TLS connections (Port + 1), otherwise false.
        /// </summary>
        bool UseTlsSockets { get; set; }
        /// <summary>
        /// The domain name associated with the certificate. Often referred to as the Common Name, or CN.
        /// </summary>
        string Domain { get; set; }
        /// <summary>
        /// The port the server will listen for TLS connections on.
        /// </summary>
        ushort TlsPort { get; set; }
        /// <summary>
        /// Gets or sets the file location of the SSL certificate to use for TLS.
        /// </summary>
        string Certificate { get; set; }
        /// <summary>
        /// Gets or sets the password that was used the secure the supplied SSL certificate.
        /// </summary>
        SecureString CertificatePassword { get; set; }
        /// <summary>
        /// True if the server can use WebSockets, otherwise false.
        /// </summary>
        bool UseWebSockets { get; set; }
        /// <summary>
        /// True to show the channel on the channel list, otherwise false.
        /// </summary>
        bool ShowChannel { get; set; }
        /// <summary>
        /// True to use captcha-like protection on the server for automated clients, otherwise false
        /// </summary>
        bool BotProtection { get; set; }
        /// <summary>
        /// The number of days a ban of any kind is effective for (0 for unlimited)
        /// </summary>
        uint BanLength { get; set; }
        /// <summary>
        /// The length of time to keep a password if it doesn't get used, a TimeSpan == 0 for infinite
        /// </summary>
        TimeSpan ExpireOldPasswords { get; set; }
        /// <summary>
        /// Returns the ServerFeatures flags that represent the values in this IServerConfig instance
        /// </summary>
        SupportFlags GetFeatures();
    }
}

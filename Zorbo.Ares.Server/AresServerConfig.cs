using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using Zorbo.Ares.Resources;
using Zorbo.Core;
using Zorbo.Core.Models;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Server
{
    public class AresServerConfig : ModelBase, IServerConfig
    {
#pragma warning disable IDE0044 // Add readonly modifier
        IDirectories directories = null;

        ushort port = 34567;
        ushort tlsport = 8080;

        IPAddress localIp = IPAddress.Any;

        string name = Strings.DefaultName;
        string botname = Strings.DefaultBotName;
        string topic = Strings.DefaultTopic;
        string orgTopic = string.Empty;
        string domain = string.Empty;
        string certificate = string.Empty;
        SecureString certpassword = null;

        byte[] avatar = null;
        byte[] orgAvatar = null;

        Website website = null;

        uint banlength = 0;

        ushort maxClones = 4;
        ushort maxClients = 500;

        bool autostart = false;

        bool allowPrivate = true;
        bool allowCompression = true;
        bool allowEncryption = false;
        bool allowVoice = true;

        bool lanhost = true;//treat lan connections as LocalHost
        bool hideIps = true;
        bool muzzledPms = true;
        bool botProtection = true;
        bool useTcpSockets = true;
        bool useTlsSockets = true;
        bool useWebSockets = true;
        bool useUdpSockets = true;
        bool showOnChannelList = true;

        Language language = Language.English;
        TimeSpan expirepass = TimeSpan.FromDays(90);
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("name")]
        public string Name {
            get { return name; }
            set { OnPropertyChanged(() => name, value); }
        }

        [JsonProperty("botname")]
        public string BotName {
            get { return botname; }
            set { OnPropertyChanged(() => botname, value); }
        }

        [JsonProperty("topic")]
        public string Topic {
            get { return topic; }
            set {
                if (topic != value) {
                    topic = value;

                    if (string.IsNullOrEmpty(OrgTopic))
                        OrgTopic = topic;

                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public string OrgTopic {
            get { return orgTopic; }
            private set { OnPropertyChanged(() => orgTopic, value); }
        }

        [JsonProperty("port")]
        public ushort Port {
            get { return port; }
            set { OnPropertyChanged(() => port, value); }
        }

        [JsonProperty("local_ip")]
        public IPAddress LocalIp {
            get { return localIp; }
            set { OnPropertyChanged(() => localIp, value); }
        }

        [JsonIgnore]
        public byte[] Avatar {
            get { return avatar; }
            set {
                if (avatar != value) {
                    avatar = value;
                    OnPropertyChanged();
                    if (avatar != null && orgAvatar == null) 
                        orgAvatar = avatar;
                }
            }
        }

        [JsonProperty("avatar", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public byte[] OrgAvatar {
            get { return orgAvatar; }
            set {
                if (orgAvatar != null) 
                    return;
                orgAvatar = value;
                OnPropertyChanged();
                if (orgAvatar != null && avatar == null)
                    avatar = orgAvatar;
            }
        }

        [JsonProperty("website", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Website Website {
            get { return website; }
            set { OnPropertyChanged(() => website, value); }
        }

        [JsonIgnore]
        public IDirectories Directories {
            get {
                if (directories == null) {
                    directories = new Directories();
                    directories.EnsureExists();
                }
                return directories;
            }
            set { OnPropertyChanged(() => directories, value); }
        }

        [JsonProperty("banlength")]
        public uint BanLength {
            get { return banlength; }
            set { OnPropertyChanged(() => banlength, value); }
        }

        [JsonProperty("maxclones")]
        public ushort MaxClones {
            get { return maxClones; }
            set { OnPropertyChanged(() => maxClones, value); }
        }

        [JsonProperty("maxclients")]
        public ushort MaxClients {
            get { return maxClients; }
            set { OnPropertyChanged(() => maxClients, value); }
        }

        [JsonProperty("lang")]
        public Language Language {
            get { return language; }
            set { OnPropertyChanged(() => language, value); }
        }

        [JsonProperty("private")]
        public bool AllowPrivate {
            get { return allowPrivate; }
            set { OnPropertyChanged(() => allowPrivate, value); }
        }

        [JsonProperty("compress")]
        public bool AllowCompression {
            get { return allowCompression; }
            set { OnPropertyChanged(() => allowCompression, value); }
        }

        [JsonProperty("encrypt")]
        public bool AllowEncryption {
            get { return allowEncryption; }
            set { OnPropertyChanged(() => allowEncryption, value); }
        }

        [JsonProperty("voice")]
        public bool AllowVoice {
            get { return allowVoice; }
            set { OnPropertyChanged(() => allowVoice, value); }
        }

        [JsonProperty("lanhost")]
        public bool LocalAreaIsHost {
            get { return lanhost; }
            set { OnPropertyChanged(() => lanhost, value); }
        }

        [JsonProperty("hideips")]
        public Boolean HideIPs {
            get { return hideIps; }
            set { OnPropertyChanged(() => hideIps, value); }
        }

        [JsonProperty("muzzlepm")]
        public Boolean MuzzledPMs {
            get { return muzzledPms; }
            set { OnPropertyChanged(() => muzzledPms, value); }
        }

        [JsonProperty("showroom")]
        public Boolean ShowChannel {
            get { return showOnChannelList; }
            set { OnPropertyChanged(() => showOnChannelList, value); }
        }

        [JsonProperty("botprotect")]
        public Boolean BotProtection {
            get { return botProtection; }
            set { OnPropertyChanged(() => botProtection, value); }
        }

        [JsonProperty("autostart")]
        public Boolean AutoStartServer {
            get { return autostart; }
            set { OnPropertyChanged(() => autostart, value); }
        }

        [JsonProperty("passexpire")]
        public Int64 ExpireOldPasswords {
            get { return expirepass.Ticks; }
            set {
                if (expirepass.Ticks != value) {
                    expirepass = TimeSpan.FromTicks(value);
                    OnPropertyChanged();
                }
            }
        }

        TimeSpan IServerConfig.ExpireOldPasswords {
            get { return expirepass; }
            set {
                if (!expirepass.Equals(value)) {
                    expirepass = value;
                    RaisePropertyChanged(nameof(ExpireOldPasswords));
                }
            }
        }

        [JsonProperty("usetcpsockets")]
        public bool UseTcpSockets {
            get { return useTcpSockets; }
            set { OnPropertyChanged(() => useTcpSockets, value); }
        }

        [JsonProperty("usetlssockets")]
        public bool UseTlsSockets {
            get { return useTlsSockets; }
            set { OnPropertyChanged(() => useTlsSockets, value); }
        }

        [JsonProperty("tlsdomain")]
        public string DomainName {
            get { return domain; }
            set { OnPropertyChanged(() => domain, value); }
        }

        [JsonProperty("tlsport")]
        public ushort TlsPort {
            get { return tlsport; }
            set { OnPropertyChanged(() => tlsport, value); }
        }

        [JsonProperty("tlscert")]
        public string Certificate {
            get { return certificate; }
            set { OnPropertyChanged(() => certificate, value); }
        }

        [JsonProperty("tlscert_password")]
        public SecureString CertificatePassword {
            get { return certpassword; }
            set {
                if (certpassword != null && value != null && 
                    certpassword.ToNativeString() == value.ToNativeString()) 
                    return;
                certpassword = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("usewebsockets")]
        public bool UseWebSockets {
            get { return useWebSockets; }
            set { OnPropertyChanged(() => useWebSockets, value); }
        }

        [JsonProperty("useudpsockets")]
        public bool UseUdpSockets {
            get { return useUdpSockets; }
            set { OnPropertyChanged(() => useUdpSockets, value); }
        }

        public static IEnumerable<Language> LanguageValues {
            get { return Enum.GetValues(typeof(Language)).Cast<Language>(); }
        }

        public AresServerConfig() {
            Website = new Website();
        }

        public SupportFlags GetFeatures()
        {
            SupportFlags ret = SupportFlags.SHARING;

            if (AllowPrivate) ret |= SupportFlags.PRIVATE;
            if (AllowCompression) ret |= SupportFlags.COMPRESSION;
            if (AllowVoice) {
                ret |= SupportFlags.VOICE;
                ret |= SupportFlags.OPUS_VOICE;
            }
            return ret;
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Zorbo.Ares.Resources;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server
{
    public class AresServerConfig : ModelBase, IServerConfig
    {
#pragma warning disable IDE0044 // Add readonly modifier
        ushort port = 34567;
        IPAddress localIp = IPAddress.Any;

        AresAvatar avatar;
        AresAvatar orgAvatar;

        string name = Strings.DefaultName;
        string botname = Strings.DefaultBotName;
        string topic = Strings.DefaultTopic;
        string orgTopic = string.Empty;

        uint banlength = 0;

        ushort maxClones = 4;
        ushort maxClients = 500;

        bool autostart = false;

        bool allowPrivate = true;
        bool allowCompression = false;
        bool allowEncryption = false;
        bool allowVoice = true;
        bool allowOpusVoice = true;

        bool lanhost = true;//treat lan connections as LocalHost
        bool hideIps = true;
        bool muzzledPms = true;
        bool botProtection = true;
        bool useTcpSockets = true;
        bool useUdpSockets = true;
        bool useWebSockets = true;
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

        [JsonProperty("banlength")]
        public uint BanLength {
            get { return banlength; }
            set { OnPropertyChanged(() => banlength, value); }
        }

        [JsonIgnore]
        public AresAvatar Avatar {
            get { return avatar; }
            set {
                if (avatar == null || !avatar.Equals(value)) {
                    avatar = value;
                    OnPropertyChanged();
                    if (avatar != null) OrgAvatar = avatar;
                }
            }
        }

        IAvatar IConfig.Avatar {
            get { return Avatar; }
            set { Avatar = value as AresAvatar; }
        }

        [JsonProperty("avatar", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public AresAvatar OrgAvatar {
            get { return orgAvatar; }
            set {
                if (orgAvatar != null) return;
                orgAvatar = value;
                OnPropertyChanged();
                if (avatar == null) Avatar = orgAvatar;
            }
        }

        IAvatar IServerConfig.OrgAvatar {
            get { return OrgAvatar; }
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

        [JsonProperty("opus")]
        public bool AllowOpusVoice {
            get { return allowOpusVoice; }
            set { OnPropertyChanged(() => allowOpusVoice, value); }
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

        [JsonProperty("useudpsockets")]
        public bool UseUdpSockets {
            get { return useUdpSockets; }
            set { OnPropertyChanged(() => useUdpSockets, value); }
        }

        [JsonProperty("usetcpsockets")]
        public bool UseTcpSockets {
            get { return useTcpSockets; }
            set { OnPropertyChanged(() => useTcpSockets, value); }
        }

        [JsonProperty("usewebsockets")]
        public bool UseWebSockets {
            get { return useWebSockets; }
            set { OnPropertyChanged(() => useWebSockets, value); }
        }

        [JsonProperty("usetlssockets")]
        public bool UseTlsSockets {
            get { return useWebSockets; }
            set { OnPropertyChanged(() => useWebSockets, value); }
        }

        public static IEnumerable<Language> LanguageValues {
            get { return Enum.GetValues(typeof(Language)).Cast<Language>(); }
        }

        public AresServerConfig() { }

        public SupportFlags GetFeatures()
        {
            SupportFlags ret = SupportFlags.NONE;

            if (AllowPrivate) ret |= SupportFlags.PRIVATE;
            if (AllowCompression) ret |= SupportFlags.COMPRESSION;
            if (AllowVoice) ret |= SupportFlags.VOICE;
            if (AllowOpusVoice) ret |= SupportFlags.OPUS_VOICE;

            return ret;
        }
    }
}
using Jurassic.Library;
using System;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Config : ScriptObject
    {
        Avatar avatar = null;
        Avatar orgAvatar = null;

        readonly JScript script = null;
        readonly IServerConfig config = null;

        public Config(JScript script, IServerConfig config)
            : base(script.Engine) {

            this.config = config;
            this.script = script;

            this.PopulateFunctions();
        }

        [JSProperty(Name = "name", IsEnumerable = true)]
        public string Name {
            get { return config.Name; }
        }

        [JSProperty(Name = "botName", IsEnumerable = true)]
        public string BotName {
            get { return config.BotName; }
        }

        [JSProperty(Name = "topic", IsEnumerable = true)]
        public string Topic {
            get { return config.Topic; }
            set { config.Topic = value; }
        }

        [JSProperty(Name = "orgTopic", IsEnumerable = true)]
        public string OrgTopic {
            get { return config.OrgTopic; }
        }

        [JSProperty(Name = "port", IsEnumerable = true)]
        public int Port {
            get { return (int)config.Port; }
        }

        [JSProperty(Name = "language", IsEnumerable = true)]
        public int Language {
            get { return (int)config.Language; }
            set { config.Language = (Language)value; }
        }

        [JSProperty(Name = "avatar", IsEnumerable = true)]
        public Avatar Avatar {
            get {
                if (avatar == null)//IAvatar might not be a JS object
                    avatar = new Avatar(script, config.Avatar);

                return avatar;
            }
            set {
                avatar = value;
                config.Avatar = value; 
            }
        }

        [JSProperty(Name = "orgAvatar", IsEnumerable = true)]
        public Avatar OrgAvatar {
            get {
                if (orgAvatar == null)
                    orgAvatar = new Avatar(script, config.OrgAvatar);

                return orgAvatar;
            }
        }

        [JSProperty(Name = "botProtection", IsEnumerable = true)]
        public bool BotProtection {
            get { return config.BotProtection; }
            set { config.BotProtection = value; }
        }

        [JSProperty(Name = "allowPrivate", IsEnumerable = true)]
        public bool AllowPrivate {
            get { return config.AllowPrivate; }
            set { config.AllowPrivate = value; }
        }

        [JSProperty(Name = "allowCompression", IsEnumerable = true)]
        public bool AllowCompression {
            get { return config.AllowCompression; }
            set { config.AllowCompression = value; }
        }

        [JSProperty(Name = "allowEncryption", IsEnumerable = true)]
        public bool AllowEncryption {
            get { return config.AllowEncryption; }
            set { config.AllowEncryption = value; }
        }

        [JSProperty(Name = "allowVoice", IsEnumerable = true)]
        public bool AllowVoice {
            get { return config.AllowVoice; }
            set { config.AllowVoice = value; }
        }

        [JSProperty(Name = "allowOpusVoice", IsEnumerable = true)]
        public bool AllowOpusVoice {
            get { return config.AllowOpusVoice; }
            set { config.AllowOpusVoice = value; }
        }

        [JSProperty(Name = "muzzledPMs", IsEnumerable = true)]
        public bool MuzzledPMs {
            get { return config.MuzzledPMs; }
            set { config.MuzzledPMs = value; }
        }

        [JSProperty(Name = "maxClones", IsEnumerable = true)]
        public int MaxClones {
            get { return config.MaxClones; }
            set { config.MaxClones = (ushort)value; }
        }

        [JSProperty(Name = "maxClients", IsEnumerable = true)]
        public int MaxClients {
            get { return config.MaxClients; }
        }

        [JSProperty(Name = "expireOldPasswords", IsEnumerable = true)]
        public double ExpireOldPasswords {
            get { return config.ExpireOldPasswords.TotalDays; }
            set { config.ExpireOldPasswords = TimeSpan.FromDays(value); }
        }

        [JSProperty(Name = "showChannel", IsEnumerable = true)]
        public bool ShowChannel {
            get { return config.ShowChannel; }
            set { config.ShowChannel = value; }
        }
    }
}

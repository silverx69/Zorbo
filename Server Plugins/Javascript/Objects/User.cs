using Jurassic;
using Jurassic.Library;
using System;
using System.Text.RegularExpressions;
using Zorbo.Core;
using Zorbo.Core.Server;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class User : ScriptObject
    {
        readonly JScript script = null;

        public IClient Client { get; } = null;

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            public Constructor(JScript script)
                : base(script.Engine.Function.InstancePrototype, "User", new User(script)) {
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public User Call() {
                return null;
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public User Construct() {
                return null;
            }
        }

        #endregion

        private User(JScript script)
            : base(script.Engine) {

            this.script = script;
            this.Client = new UserInvalid();

            this.PopulateFunctions();
        }

        public User(JScript script, IClient client)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["User"]).InstancePrototype) {

            this.script = script;
            this.Client = client;
            this.Monitor = new Monitor(script, client.Monitor);

            this.PopulateFunctions();
        }

        [JSProperty(Name = "id", IsEnumerable = true)]
        public int Id {
            get { return Client.Id; }
        }

        [JSProperty(Name = "guid", IsEnumerable = true)]
        public string Guid {
            get { return Client.Guid.ToString(); }
        }
        
        [JSProperty(Name = "admin", IsEnumerable = true)]
        public int Admin {
            get { return (int)Client.Admin; }
            set { Client.Admin = (AdminLevel)value; }
        }

        [JSProperty(Name = "name", IsEnumerable = true)]
        public string Name {
            get { return Client.Name; }
            set { Client.Name = value; }
        }

        [JSProperty(Name = "orgName", IsEnumerable = true)]
        public string OrgName {
            get { return Client.OrgName; }
        }

        [JSProperty(Name = "loggedIn", IsEnumerable = true)]
        public bool LoggedIn {
            get { return Client.LoggedIn; }
        }

        [JSProperty(Name = "connected", IsEnumerable = true)]
        public bool Connected {
            get { return Client.Connected; }
        }

        [JSProperty(Name = "localHost", IsEnumerable = true)]
        public bool LocalHost {
            get { return Client.LocalHost; }
        }

        [JSProperty(Name = "fastPing", IsEnumerable = true)]
        public bool FastPing {
            get { return Client.FastPing; }
        }

        [JSProperty(Name = "isCaptcha", IsEnumerable = true)]
        public bool IsCaptcha {
            get { return Client.IsCaptcha; }
            set { Client.IsCaptcha = value; }
        }

        [JSProperty(Name = "muzzled", IsEnumerable = true)]
        public bool Muzzled {
            get { return Client.Muzzled; }
            set { Client.Muzzled = value; }
        }

        [JSProperty(Name = "cloaked", IsEnumerable = true)]
        public bool Cloaked {
            get { return Client.Cloaked; }
            set { Client.Cloaked = value; }
        }

        [JSProperty(Name = "browsable", IsEnumerable = true)]
        public bool Browsable {
            get { return Client.Browsable; }
        }

        [JSProperty(Name = "encryption", IsEnumerable = true)]
        public bool Encryption {
            get { return Client.Encryption; }
        }

        [JSProperty(Name = "compression", IsEnumerable = true)]
        public bool Compression {
            get { return Client.Compression; }
        }

        [JSProperty(Name = "html", IsEnumerable = true)]
        public bool SupportHtml {
            get { return (Client.Features & ClientFlags.HTML) == ClientFlags.HTML; }
        }

        [JSProperty(Name = "voice", IsEnumerable = true)]
        public bool SupportVoice {
            get { return (Client.Features & ClientFlags.VOICE) == ClientFlags.VOICE; }
        }

        [JSProperty(Name = "privateVoice", IsEnumerable = true)]
        public bool SupportPrivateVoice {
            get { return (Client.Features & ClientFlags.PRIVATE_VOICE) == ClientFlags.PRIVATE_VOICE; }
        }

        [JSProperty(Name = "token", IsEnumerable = true)]
        public int Token {
            get { return (int)Client.Token; }
        }

        [JSProperty(Name = "age", IsEnumerable = true)]
        public int Age {
            get { return Client.Age; }
        }

        [JSProperty(Name = "gender", IsEnumerable = true)]
        public int Sex {
            get { return (int)Client.Gender; }
        }

        [JSProperty(Name = "country", IsEnumerable = true)]
        public int Country {
            get { return (int)Client.Country; }
        }

        [JSProperty(Name = "vroom", IsEnumerable = true)]
        public int Vroom {
            get { return Client.Vroom; }
            set { Client.Vroom = (ushort)Convert.ToUInt16(value); }
        }

        [JSProperty(Name = "fileCount", IsEnumerable = true)]
        public int FileCount {
            get { return Client.FileCount; }
        }

        [JSProperty(Name = "nodePort", IsEnumerable = true)]
        public int NodePort {
            get { return Client.NodePort; }
        }

        [JSProperty(Name = "listenPort", IsEnumerable = true)]
        public int ListenPort {
            get { return Client.ListenPort; }
        }

        [JSProperty(Name = "nodeIp", IsEnumerable = true)]
        public string NodeIp {
            get { return Client.NodeIp.ToString(); }
        }

        [JSProperty(Name = "localIp", IsEnumerable = true)]
        public string LocalIp {
            get { return Client.LocalIp.ToString(); }
        }

        [JSProperty(Name = "externalIp", IsEnumerable = true)]
        public string ExternalIp {
            get { return Client.ExternalIp.ToString(); }
        }

        [JSProperty(Name = "features", IsEnumerable = true)]
        public int Features {
            get { return (int)Client.Features; }
        }

        [JSProperty(Name = "avatar", IsEnumerable = true)]
        public ArrayInstance Avatar {
            get { return Client.Avatar.ToJSArray(Engine); }
            set { Client.Avatar = value.ToArray<byte>(); }
        }

        [JSProperty(Name = "orgAvatar", IsEnumerable = true)]
        public ArrayInstance OrgAvatar {
            get { return Client.OrgAvatar.ToJSArray(Engine); }
        }

        [JSProperty(Name = "version", IsEnumerable = true)]
        public string Version {
            get { return Client.Version; }
        }

        [JSProperty(Name = "region", IsEnumerable = true)]
        public string Region {
            get { return Client.Region; }
        }

        [JSProperty(Name = "message", IsEnumerable = true)]
        public string Message {
            get { return Client.Message; }
            set { Client.Message = value; }
        }

        [JSProperty(Name = "monitor", IsEnumerable = true)]
        public Monitor Monitor { get; } = null;

        [JSFunction(Name = "sendPacket", IsEnumerable = true, IsWritable = false)]
        public void SendPacket(object a)
        {
            if (++script.Counters["json"] > 100)
                throw new JavaScriptException(Engine.Error.Construct("Send packet limit reached"), 0, null);

            string tmp = string.Empty;

            if (a is ObjectInstance)
                tmp = JSONObject.Stringify(Engine, a);

            else if (a is string || a is ConcatenatedString)
                tmp = a.ToString();

            if (!string.IsNullOrEmpty(tmp)) {
                Match match = Regex.Match(tmp, "(['\"]*)id\\1:\\s*(['\"]*)(\\d+)\\2", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success) {
                    if (byte.TryParse(match.Groups[3].Value, out byte id)) {
                        IPacket p = Room.Formatter.Unformat(id, tmp);
                        if (p != null) Client.SendPacket(p);
                    }
                }
            }
        }

        [JSFunction(Name = "ban", IsEnumerable = true, IsWritable = false)]
        public void Ban(object a) {
            object state = null;

            if (!(a is Undefined) && !(a is Null))
                state = a;
            
            Client.Ban(state);
        }

        [JSFunction(Name = "disconnect", IsEnumerable = true, IsWritable = false)]
        public void Disconnect(object a) {
            object state = null;

            if (!(a is Undefined) && !(a is Null))
                state = a;
            
            Client.Disconnect(state);
        }
    }
}

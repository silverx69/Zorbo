using cb0tProtocol;
using Javascript.Objects;
using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zorbo.Ares.Packets;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Javascript
{
    public class Script
    {
        static readonly StringBuilder default_script;


        public string Name {
            get;
            private set;
        }

        public string Directory {
            get {
                return Path.Combine(JurassicPlugin.Self.Directory, "Scripts", Name);
            }
        }

        public ScriptEngine Engine { get; private set; }


        internal Room Room { get; private set; }

        internal Dictionary<string, int> Counters { get; }


        static Script() {
            default_script = new StringBuilder();
            default_script.AppendLine("function onCaptcha(userobj, event) { }");
            default_script.AppendLine("function onSendLogin(userobj) { }");
            default_script.AppendLine("function onJoinCheck(userobj) { return true;}");
            default_script.AppendLine("function onJoinRejected(userobj, reason) { }");
            default_script.AppendLine("function onJoin(userobj) { }");
            default_script.AppendLine("function onPart(userobj, state) { }");
            default_script.AppendLine("function onVroomJoinCheck(userobj) { return true;}");
            default_script.AppendLine("function onVroomJoin(userobj) { }");
            default_script.AppendLine("function onVroomPart(userobj) { }");
            default_script.AppendLine("function onHelp(userobj) { }");
            default_script.AppendLine("function onLogin(userobj, passobj) { }");
            default_script.AppendLine("function onRegister(userobj, passobj) { return true;}");
            default_script.AppendLine("function onFileReceived(userobj, file) { return true;}");
            default_script.AppendLine("function onBeforePacket(userobj, jsonpacket) { return true;}");
            default_script.AppendLine("function onAfterPacket(userobj, jsonpacket) { }");
            default_script.AppendLine("function onPacketSent(userobj, jsonpacket) { }");
            default_script.AppendLine("function onFlood(userobj, rule, jsonpacket) { return true;}");
            default_script.AppendLine("function onError(error) { }");
        }

        public Script(String name) {
            this.Name = name;
            this.Counters = new Dictionary<string, int>();

            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);

            Counters.Add("print", 0);
            Counters.Add("text", 0);
            Counters.Add("emote", 0);
            Counters.Add("private", 0);
            Counters.Add("website", 0);
            Counters.Add("html", 0);
            Counters.Add("json", 0);

            this.Engine = new ScriptEngine();

            //ENUMERATIONS - ENUMERATED
            Engine.Global.DefineProperty("AresId",
                new PropertyDescriptor(new Objects.Enum(this, typeof(AresId)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("AdvancedId",
                new PropertyDescriptor(new Objects.Enum(this, typeof(AdvancedId)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("Language",
                new PropertyDescriptor(new Objects.Enum(this, typeof(Language)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("Country",
                new PropertyDescriptor(new Objects.Enum(this, typeof(Country)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("Gender",
                new PropertyDescriptor(new Objects.Enum(this, typeof(Gender)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("AdminLevel",
                new PropertyDescriptor(new Objects.Enum(this, typeof(AdminLevel)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("CaptchaEvent",
                new PropertyDescriptor(new Objects.Enum(this, typeof(CaptchaEvent)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("RejectReason",
                new PropertyDescriptor(new Objects.Enum(this, typeof(RejectReason)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("ClientFeatures",
                new PropertyDescriptor(new Objects.Enum(this, typeof(ClientFlags)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("ServerFeatures",
                new PropertyDescriptor(new Objects.Enum(this, typeof(SupportFlags)), PropertyAttributes.Enumerable | PropertyAttributes.Sealed), true);

            // INSTANCE CLASSES - NOT ENUMERATED 
            Engine.Global.DefineProperty("Error",
                new PropertyDescriptor(new Objects.Error.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("Collection",
                new PropertyDescriptor(new Objects.Collection.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("List",
                new PropertyDescriptor(new Objects.List.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("ReadOnlyList",
                new PropertyDescriptor(new Objects.ReadOnlyList.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("TimeSpan",
                new PropertyDescriptor(new Objects.TimeSpanInstance.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("Monitor",
                new PropertyDescriptor(new Objects.Monitor.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("RoomStats",
                new PropertyDescriptor(new Objects.RoomStats.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("HttpRequest",
                new PropertyDescriptor(new Objects.HttpRequest.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("Hashlink",
                new PropertyDescriptor(new Objects.Hashlink.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("FloodRule",
                new PropertyDescriptor(new Objects.FloodRule.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("EncodingInstance",
                new PropertyDescriptor(new Objects.EncodingInstance.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("UserId",
                new PropertyDescriptor(new Objects.UserId.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("User",
                new PropertyDescriptor(new Objects.User.Constructor(this), PropertyAttributes.Sealed), true);

            Engine.Global.DefineProperty("UserRecord",
                new PropertyDescriptor(new Objects.UserRecord.Constructor(this), PropertyAttributes.Sealed), true);

            // GLOBAL (STATIC) CLASSES
            this.Room = new Room(this, JurassicPlugin.Self.Server);
            // ease of use functions
            Engine.SetGlobalFunction("user", new Func<Object, User>(Room.FindUser));
            Engine.SetGlobalFunction("print", new Action<Object, Object>(Room.SendAnnounce));

            Engine.Global.DefineProperty("user",
                new PropertyDescriptor(Engine.Global["user"], PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("print",
                new PropertyDescriptor(Engine.Global["print"], PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("Room",
                new PropertyDescriptor(Room, PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("Channels",
                new PropertyDescriptor(new Objects.Channels(this, JurassicPlugin.Self.Server.Channels), PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("File",
                new PropertyDescriptor(new Objects.File(this), PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("Script",
                new PropertyDescriptor(new Objects.Script(this), PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("Hashlinks",
                new PropertyDescriptor(new Objects.Hashlinks(this), PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("Encoding",
                new PropertyDescriptor(new Objects.Encoding(this, true), PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("Base64",
                new PropertyDescriptor(new Objects.Base64(this), PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            Engine.Global.DefineProperty("BitConverter",
                new PropertyDescriptor(new Objects.BitConverter(this), PropertyAttributes.Sealed | PropertyAttributes.Enumerable), true);

            foreach (var embedded in JurassicPlugin.Embedded) {
                Engine.Global.DefineProperty(
                    embedded.Key,
                    new PropertyDescriptor(embedded.Value.Create(this), embedded.Value.Attributes), true);
            }

            Eval(default_script.ToString());
        }

        public object Eval(string code) {
            return this.Engine.Evaluate(code);
        }

        public T Eval<T>(string code) {
            return this.Engine.Evaluate<T>(code);
        }

        public void Unload() {
            this.Room = null;
            this.Engine = null;
        }

        internal void ResetCounters() {
            var keys = Counters.Select((s) => s.Key);
            foreach (var key in keys.ToArray()) Counters[key] = 0;
        }
    }
}

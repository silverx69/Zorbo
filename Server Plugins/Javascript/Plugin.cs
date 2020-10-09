using Javascript.Objects;
using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.Chatroom.ib0t;
using Zorbo.Core;
using Zorbo.Core.Models;
using Zorbo.Core.Plugins.Server;
using Zorbo.Core.Server;

namespace Javascript
{
    public class JurassicPlugin : ServerPlugin
    {
        internal static JurassicPlugin Self {
            get;
            private set;
        }

        public static ModelList<Script> Scripts { 
            get; 
            private set;
        }

        internal static Dictionary<string, EmbeddedType> Embedded { 
            get;
            private set;
        }


        internal sealed class EmbeddedType
        {
            public Type Type { get; set; }
            
            public PropertyAttributes Attributes { get; set; }

            public System.Reflection.ConstructorInfo Ctor { get; set; }

            public EmbeddedType() { }

            public EmbeddedType(Type type, System.Reflection.ConstructorInfo ctor, PropertyAttributes attrs) {
                Type = type;
                Ctor = ctor;
                Attributes = attrs;
            }

            public object Create(Script script) {
                var ptype = Ctor.GetParameters()[0].ParameterType;

                if (ptype == typeof(Script))
                    return Ctor.Invoke(new[] { script });
                else
                    return Ctor.Invoke(new[] { script.Engine });
            }
        }

        static JurassicPlugin() {
            Scripts = new ModelList<Script>();
            Embedded = new Dictionary<string, EmbeddedType>();
        }

        public JurassicPlugin() { Self = this; }


        public bool EmbedObject(string name, Type prototype, PropertyAttributes attrs) {

            if (!typeof(ObjectInstance).IsAssignableFrom(prototype))
                throw new ArgumentException("Prototype type must inherit from ObjectInstance", "prototype");

            var ctors = prototype.GetConstructors();

            foreach(var ctor in ctors) {

                var @params = ctor.GetParameters();
                if (@params.Length != 1) continue;

                var ptype = @params[0].ParameterType;

                if (ptype != typeof(Script) &&
                    ptype != typeof(ScriptEngine)) continue;

                EmbedRunningScripts(name, new EmbeddedType(prototype, ctor, attrs));
                return true;
            }

            return false;
        }

        private void EmbedRunningScripts(string name, EmbeddedType e) {
            Embedded[name] = e;

            foreach (var s in Scripts) {
                s.Engine.Global.DefineProperty(
                    name,
                    new PropertyDescriptor(e.Create(s), e.Attributes), true);
            }
        }

        public override void OnPluginLoaded() {
            Server.SendAnnounce("Jurassic plugin has been loaded!!");
            if (JSScript.Load("autoload"))
                JSScript.Kill("autoload");
        }

        public override void OnPluginKilled() {
            Server.SendAnnounce("Jurassic plugin has been unloaded!!");
        }


        public override void OnCaptcha(IClient client, CaptchaEvent @event) {

            lock (Scripts) {
                foreach (var s in Scripts) {

                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        s.Engine.CallGlobalFunction("onCaptcha", user, (int)@event);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override void OnSendJoin(IClient client) {

            lock (Scripts) {
                foreach (var s in Scripts) {

                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        s.Engine.CallGlobalFunction("onSendLogin", user);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override bool OnJoinCheck(IClient client) {
            bool ret = true;

            lock (Scripts) {
                foreach (var s in Scripts) {
                    User user = new User(s, client);

                    s.Room.Users.Items.Add(user);

                    try {
                        bool b = s.Engine.CallGlobalFunction<bool>("onJoinCheck", user);
                        if (!b) ret = false;
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }

            return ret;
        }

        public override void OnJoin(IClient client) {

            lock (Scripts) {
                foreach (var s in Scripts) {

                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        s.Engine.CallGlobalFunction("onJoin", user);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override void OnJoinRejected(IClient client, RejectReason reason) {

            lock (Scripts) {
                foreach (var s in Scripts) {

                    int index = s.Room.Users.Items.FindIndex((x) => ((User)x).Client == client);
                    if (index < 0) continue;

                    try {
                        s.Engine.CallGlobalFunction("onJoinRejected", s.Room.Users[index], (int)reason);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override void OnPart(IClient client, object state) {

            lock (Scripts) {
                foreach (var s in Scripts) {

                    int index = s.Room.Users.Items.FindIndex((x) => ((User)x).Client == client);
                    if (index < 0) continue;

                    try {
                        s.Engine.CallGlobalFunction("onPart", s.Room.Users.Items[index], state);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }

                    s.Room.Users.Items.RemoveAt(index);
                }
            }
        }

        public override bool OnVroomJoinCheck(IClient client, ushort vroom) {
            bool ret = true;

            lock (Scripts) {
                foreach (var s in Scripts) {

                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        bool b = s.Engine.CallGlobalFunction<bool>("onVroomJoinCheck", user, vroom);
                        if (!b) ret = false;
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }

            return ret;
        }

        public override void OnVroomJoin(IClient client) {

            lock (Scripts) {
                foreach (var s in Scripts) {

                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        s.Engine.CallGlobalFunction("onVroomJoin", user);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override void OnVroomPart(IClient client) {
            lock (Scripts) {
                foreach (var s in Scripts) {
                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        s.Engine.CallGlobalFunction("onVroomPart", user);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override void OnHelp(IClient client) {
            lock (Scripts) {
                foreach (var s in Scripts) {
                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        s.Engine.CallGlobalFunction("onHelp", user);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override void OnLogin(IClient client, IPassword password) {
            lock (Scripts) {
                foreach (var s in Scripts) {
                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        s.Engine.CallGlobalFunction<bool>("onLogin", user, new Objects.Password(s, password));
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override bool OnRegister(IClient client, IPassword password) {
            bool ret = true;
            lock (Scripts) {
                foreach (var s in Scripts) {
                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        bool b = s.Engine.CallGlobalFunction<bool>("onRegister", user, new Objects.Password(s, password));
                        if (!b) ret = false;
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
            return ret;
        }

        public override bool OnTextCommand(IClient client, string cmd, string args)
        {
            if (client.Admin >= AdminLevel.Admin) {
                switch (cmd) {
                    case "loadscript":
                        JSScript.Load(args);
                        break;
                    case "killscript":
                        JSScript.Kill(args);
                        break;
                }
            }
            return true;
        }

        public override bool OnBeforePacket(IClient client, IPacket packet) {
            bool ret = true;

            lock (Scripts) {
                string strpacket = Json.Serialize(packet);
                foreach (var s in Scripts) {

                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;

                    try {
                        bool b = s.Engine.CallGlobalFunction<bool>("onBeforePacket", user, strpacket);
                        if (!b) ret = false;
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }

            return ret;
        }

        public override void OnAfterPacket(IClient client, IPacket packet) {
            lock (Scripts) {
                string strpacket = Zorbo.Core.Json.Serialize(packet);
                foreach (var s in Scripts) {
                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;
                    try {
                        s.Engine.CallGlobalFunction("onAfterPacket", user, strpacket);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override void OnPacketSent(IClient client, IPacket packet)
        {
            lock (Scripts) {
                string strpacket = Json.Serialize(packet);
                foreach (var s in Scripts) {
                    User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                    if (user == null) continue;
                    try {
                        s.Engine.CallGlobalFunction("onPacketSent", user, strpacket);
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
        }

        public override bool OnFlood(IClient client, IFloodRule rule, IPacket packet) {
            bool ret = true;
            lock (Scripts) {
                foreach (var s in Scripts) {
                    try {
                        User user = (User)s.Room.Users.Items.Find((x) => ((User)x).Client == client);
                        if (user == null) continue;

                        var floodRule = rule as Objects.FloodRule ?? new Objects.FloodRule(s, rule);

                        if (!s.Engine.CallGlobalFunction<bool>("onFlood", user, floodRule, new Packet(s, packet)))
                            ret = false;
                    }
                    catch (JavaScriptException jex) {
                        OnError(jex);
                    }
                    finally { s.ResetCounters(); }
                }
            }
            return ret;
        }

        public override void OnError(IErrorInfo error) {

            if (error.Exception is JavaScriptException exception) 
                OnError(exception);
        }


        internal void OnError(JavaScriptException jex) {
            lock (Scripts) {
                foreach (var s in Scripts) {
                    try {
                        s.Engine.CallGlobalFunction("onError", new Objects.Error(s, jex));
                    }
                    catch (JavaScriptException) { }
                }
            }
        }

        internal void SendRoomScribble(string name, RoomScribble scribble)
        {
            byte[] buffer;
            if (scribble.Size <= 4000) {
                Server.SendAnnounce(string.Format("\x000314--- From {0}", name));
                Server.SendPacket(new ClientCustom(Server.Config.BotName, "cb0t_scribble_once", scribble.RawImage()));
            }
            else {
                scribble.Index = 0;
                buffer = scribble.Read();

                Server.SendAnnounce(string.Format("\x000314--- From {0}", name));
                Server.SendPacket(new ClientCustom(Server.Config.BotName, "cb0t_scribble_first", buffer));

                while (scribble.Remaining > 0) {
                    buffer = scribble.Read();

                    if (scribble.Remaining > 0)
                        Server.SendPacket(new ClientCustom(Server.Config.BotName, "cb0t_scribble_chunk", buffer));
                    else
                        Server.SendPacket(new ClientCustom(Server.Config.BotName, "cb0t_scribble_last", buffer));
                }
            }

            var ib0ts = Server.Users.Where(s => s.Socket.Isib0tSocket);
            if (ib0ts.Count() > 0) {
                buffer = Zlib.Decompress(scribble.RawImage());

                int height = RoomScribble.GetHeight(buffer);
                string base64 = Convert.ToBase64String(buffer);

                string[] chunks = new string[(int)Math.Round((double)(base64.Length / 1024), MidpointRounding.AwayFromZero)];

                for (int i = 0; i < chunks.Length; i++)
                    chunks[i] = base64.Substring(i * 1024, 1024);

                ib0ts.ForEach(s => s.SendPacket(new ScribbleHead(name, height, chunks.Length)));

                foreach (string chunk in chunks)
                    ib0ts.ForEach(s => s.SendPacket(new ScribbleBlock(chunk)));
            }
        }

        internal void SendRoomScribble(Predicate<IClient> pred, string name, RoomScribble scribble)
        {
            byte[] buffer;
            if (scribble.Size <= 4000) {
                Server.SendAnnounce(pred, string.Format("\x000314--- From {0}", name));
                Server.SendPacket(pred, new ClientCustom(Server.Config.BotName, "cb0t_scribble_once", scribble.RawImage()));
            }
            else {
                scribble.Index = 0;

                int length = Math.Min((int)scribble.Received, 4000);
                buffer = scribble.Read();

                Server.SendAnnounce(pred, string.Format("\x000314--- From {0}", name));
                Server.SendPacket(pred, new ClientCustom(Server.Config.BotName, "cb0t_scribble_first", buffer));

                while (scribble.Remaining > 0) {
                    buffer = scribble.Read();

                    if (scribble.Remaining > 0)
                        Server.SendPacket(pred, new ClientCustom(Server.Config.BotName, "cb0t_scribble_chunk", buffer));
                    else
                        Server.SendPacket(pred, new ClientCustom(Server.Config.BotName, "cb0t_scribble_last", buffer));
                }
            }

            var ib0ts = Server.Users.Where(s => s.Socket.Isib0tSocket && pred(s));

            if (ib0ts.Count() > 0) {
                buffer = Zlib.Decompress(scribble.RawImage());

                int height = RoomScribble.GetHeight(buffer);
                string base64 = Convert.ToBase64String(buffer);

                string[] chunks = new string[(int)Math.Round((double)(base64.Length / 1024), MidpointRounding.AwayFromZero)];

                for (int i = 0; i < chunks.Length; i++)
                    chunks[i] = base64.Substring(i * 1024, 1024);

                ib0ts.ForEach(s => s.SendPacket(new ScribbleHead(name, height, chunks.Length)));

                foreach (string chunk in chunks)
                    ib0ts.ForEach(s => s.SendPacket(new ScribbleBlock(chunk)));
            }
        }
    }
}

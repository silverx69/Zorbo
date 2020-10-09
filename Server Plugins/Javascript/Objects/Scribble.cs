using Jurassic;
using Jurassic.Library;
using System;
using System.IO;
using Zorbo.Ares.Packets.Chatroom;

namespace Javascript.Objects
{
    public class Scribble : ObjectInstance
    {
        readonly Script script = null;
        string source = string.Empty;

        RoomScribble scribble = null;

        [JSProperty(Name = "source")]
        public string Source {
            get { return source; }
            set {
                if (source != value) {
                    source = value;
                    if (scribble != null) {
                        scribble.Reset();
                        scribble = null;
                    }
                }
            }
        }

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly Script script = null;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, "Scribble", new Scribble(script)) {

                this.script = script;
            }

            [JSCallFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public Scribble Call(object a) {
                return Construct(a);
            }

            [JSConstructorFunction(Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
            public Scribble Construct(object a) {
                if (a != null && !(a is Undefined)) {
                    if (a is String || a is ConcatenatedString)
                        return new Scribble(script, InstancePrototype, a.ToString());
                }
                return new Scribble(script, InstancePrototype);
            }
        }

        #endregion

        private Scribble(Script script)
            : base(script.Engine) {

            this.script = script;
            this.PopulateFunctions();
        }

        private Scribble(Script script, ObjectInstance proto)
            : base(script.Engine, proto) {

            this.script = script;
            this.PopulateFunctions();
        }

        private Scribble(Script script, ObjectInstance proto, string source)
            : base(script.Engine, proto) {

            this.script = script;
            this.Source = source;
            this.PopulateFunctions();
        }

        [JSFunction(Name = "load", IsEnumerable = true)]
        public bool Load(object state) {

            if (scribble != null) {
                LoadCallback(state);
                return true;
            }

            try {
                string path = Path.Combine(script.Directory, Source);

                if (Uri.TryCreate(Source, UriKind.Absolute, out Uri toGet) ||
                    Uri.TryCreate(path, UriKind.Absolute, out toGet)) {

                    if (toGet.IsFile) {
                        FileInfo file = new FileInfo(toGet.AbsolutePath);
                        if (file.Exists && file.Directory.FullName != script.Directory)
                            throw new UnauthorizedAccessException("You are not allowed to access this file.");
                    }
                }
                else throw new UriFormatException(Source);

                scribble = new RoomScribble();
                scribble.Download(toGet, LoadCallback, state);
            }
            catch(Exception ex) {
                OnError(ex);
                return false;
            }

            return true;
        }

        private void LoadCallback(object state) {
            if (state is Exception exception) {
                OnError(exception);
            }
            else {
                this.CallMemberFunction("onLoad", state);
            }
        }

        [JSFunction(Name = "onLoad", IsEnumerable = true, IsConfigurable = true, IsWritable = true)]
        public virtual void OnLoad(object state) {
            Send(state ?? Undefined.Value);
        }

        [JSFunction(Name = "send", IsEnumerable = true)]
        public void Send(object a) {
            var plugin = JurassicPlugin.Self;

            if (a is Undefined) {
                plugin.SendRoomScribble(plugin.Server.Config.BotName, scribble);
            }
            else if (a is int || a is double) {
                int x  = (int)a;
                plugin.SendRoomScribble((s) => s.Vroom == x, plugin.Server.Config.BotName, scribble);
            }
            else if (a is User user) {
                plugin.SendRoomScribble((s) => s == user.Client, plugin.Server.Config.BotName, scribble);
            }
        }

        private void OnError(Exception ex) {
            this.CallMemberFunction(
                    "onError",
                    new Javascript.Objects.Error(
                        script, 
                        new JavaScriptException(Engine.Error.Construct(ex.Message), 0, null)));
        }

        [JSFunction(Name = "onError", IsEnumerable = true, IsConfigurable = true, IsWritable = true)]
        public virtual void OnError(object state) { }
    }
}

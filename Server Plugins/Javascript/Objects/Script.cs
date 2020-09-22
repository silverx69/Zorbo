using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Core;

using Jurassic;
using Jurassic.Library;

using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Script : ScriptObject
    {
        public Script(JScript script)
            : base(script.Engine) {
            
            this.PopulateFunctions();
        }

        [JSFunction(Name = "load", IsEnumerable = true, IsWritable = false)]
        public static bool Load(string name) {

            name = name.ToLower();
            
            var script = JurassicPlugin.Scripts.Find((s) => s.Name.ToLower() == name);
            if (script != null) Kill(name);
            
            string path = Path.Combine(JurassicPlugin.Self.Directory, "Scripts", name, name + ".js");

            try {
                script = new JScript(name);

                JurassicPlugin.Self.Server.Users.ForEach((s) => script.Room.Users.Items.Add(new User(script, s)));
                JurassicPlugin.Scripts.Add(script);

                script.Eval(System.IO.File.ReadAllText(path));
                script.ResetCounters();

                return true;
            }
            catch (JavaScriptException jex) {
                JurassicPlugin.Self.OnError(jex);
                Kill(name);
            }
            return false;
        }

        [JSFunction(Name = "create", IsEnumerable = true, IsWritable = false)]
        public static bool Create(string name) {

            name = name.ToLower();

            var script = JurassicPlugin.Scripts.Find((s) => s.Name.ToLower() == name);
            if (script != null) Kill(name);

            try {
                script = new JScript(name);

                JurassicPlugin.Self.Server.Users.ForEach((s) => script.Room.Users.Items.Add(new User(script, s)));
                JurassicPlugin.Scripts.Add(script);

                script.ResetCounters();

                return true;
            }
            catch (JavaScriptException jex) {
                JurassicPlugin.Self.OnError(jex);
                Kill(name);
            }

            return false;
        }

        [JSFunction(Name = "eval", IsEnumerable = true, IsWritable = false)]
        public static object Eval(string name, string code) {
            object ret = null;
            name = name.ToLower();
            
            var script = JurassicPlugin.Scripts.Find((s) => s.Name.ToLower() == name);
            if (script == null) return string.Empty;

            try {
                ret = script.Eval(code);
            }
            catch (JavaScriptException jex) {
                JurassicPlugin.Self.OnError(jex);
            }
            finally { script.ResetCounters(); }

            return ret;
        }

        [JSFunction(Name = "kill", IsEnumerable = true, IsWritable = false)]
        public static void Kill(string name) {

            name = name.ToLower();

            int index = JurassicPlugin.Scripts.FindIndex((s) => s.Name.ToLower() == name);
            if (index < 0) return;

            JurassicPlugin.Scripts[index].Unload();
            JurassicPlugin.Scripts.RemoveAt(index);
        }
    }
}

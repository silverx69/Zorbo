using Jurassic;
using Jurassic.Library;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Zorbo.Ares.Packets.Formatters;
using Zorbo.Core;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using JScript = Javascript.Script;

namespace Javascript.Objects
{
    public class Room : ScriptObject
    {
#pragma warning disable IDE0044 // Add readonly modifier
        Config config = null;

        JScript script = null;
        IServer server = null;
        List users = null;
#pragma warning restore IDE0044 // Add readonly modifier

        public Room(JScript script, IServer server)
            : base(script.Engine) {

            this.script = script;
            this.server = server;

            this.users = new List(script);
            this.Users = new ReadOnlyList(script, users);

            this.Stats = new RoomStats(script, server.Stats);
            this.Records = new Records(script, server.History);
            this.Admins = new Admins(script, server.History.Admin);
            this.Bans = new Banned(script, server.History);
            this.RangeBans = new RangeBanned(script, server.History);
            this.FloodRules = new FloodRules(script, server.FloodRules);

            this.PopulateFields();
            this.PopulateFunctions();
        }

        [JSProperty(Name = "topic", IsEnumerable = true)]
        public string Topic {
            get { return this.server.Config.Topic; }
            set { this.server.Config.Topic = value; }
        }

        [JSProperty(Name = "users", IsEnumerable = true)]
        public ReadOnlyList Users { get; } = null;

        [JSProperty(Name = "running", IsEnumerable = true)]
        public bool Running {
            get { return this.server.Running; }
        }

        [JSProperty(Name = "localIp", IsEnumerable = true)]
        public string LocalIp {
            get { return this.server.InternalIp?.ToString() ?? "0.0.0.0"; }
        }

        [JSProperty(Name = "externalIp", IsEnumerable = true)]
        public string ExternalIp {
            get { return this.server.ExternalIp?.ToString() ?? "0.0.0.0"; }
        }

        [JSProperty(Name = "config", IsEnumerable = true)]
        public Config Config {
            get {
                if (config == null)
                    config = new Config(script, server.Config);

                return config;
            }
        }

        [JSProperty(Name = "stats", IsEnumerable = true)]
        public RoomStats Stats { get; }

        [JSProperty(Name = "records", IsEnumerable = true)]
        public Records Records { get; }

        [JSProperty(Name = "admin", IsEnumerable = true)]
        public Admins Admins { get; }

        [JSProperty(Name = "bans", IsEnumerable = true)]
        public Banned Bans { get; }

        [JSProperty(Name = "rangeBans", IsEnumerable = true)]
        public RangeBanned RangeBans { get; }

        [JSProperty(Name = "floodRules", IsEnumerable = true)]
        public FloodRules FloodRules { get; }

        public User FindUser(object search) {

            if (search is Null)
                return null;

            if (search is int id) {
                return (User)this.script.Room.Users.Items.Find((s) => ((User)s).Id == id);
            }
            else if (search is NumberInstance instance) {
                id = (int)instance.Value;
                return (User)this.script.Room.Users.Items.Find((s) => ((User)s).Id == id);
            }
            else if (search is String || search is ConcatenatedString) {
                string str = search.ToString();
                return (User)script.Room.Users.Items.Find((s) => ((User)s).Name.StartsWith(str));
            }
            else if (search is FunctionInstance match) {

                for (int i = 0; i < this.script.Room.Users.Items.Count; i++) {
                    object ret = null;
                    User user = (User)script.Room.Users.Items[i];

                    try {
                        ret = match.Call(this, user);

                        if (TypeConverter.ConvertTo<bool>(Engine, ret))
                            return user;
                    }
                    catch (JavaScriptException jex) {
                        JurassicPlugin.Self.OnError(jex);
                    }
                }
            }

            return null;
        }

        [JSFunction(Name = "sendAnnounce", IsEnumerable = true, IsWritable = false)]
        public void SendAnnounce(object a, object b) {

            if (++script.Counters["print"] > 100)
                throw new JavaScriptException(Engine.Error.Construct("Send announce limit reached"), 0, null);

            if (b is Undefined)
                server.SendAnnounce(a.ToString());

            else if (a is User user)
                server.SendAnnounce(user.Client, b.ToString());

            else if (a is int id)
                server.SendAnnounce((s) => s.Vroom == id, b.ToString());
            else if (a is NumberInstance inst) {
                id = (int)inst.Value;
                server.SendAnnounce((s) => s.Vroom == id, b.ToString());
            }
            else if (a is FunctionInstance match) {

                for (int i = 0; i < this.script.Room.Users.Items.Count; i++) {
                    user = (User)this.script.Room.Users.Items[i];
                    object ret = null;

                    try {
                        ret = match.Call(this, user);

                        if (TypeConverter.ConvertTo<bool>(Engine, ret))
                            server.SendAnnounce(user.Client, b.ToString());
                    }
                    catch (JavaScriptException jex) {
                        JurassicPlugin.Self.OnError(jex);
                    }
                }
            }
        }

        [JSFunction(Name = "sendText", IsEnumerable = true, IsWritable = false)]
        public void SendText(object a, object b, object c) {

            if (++script.Counters["text"] > 100)
                throw new JavaScriptException(Engine.Error.Construct("Send text limit reached"), 0, null);

            if (c is Undefined) {
                if (a is String || a is ConcatenatedString)
                    server.SendText(a.ToString(), b.ToString());

                else if (a is User user)
                    server.SendText(user.Client, b.ToString());
            }
            else if (a is String || a is ConcatenatedString)
                server.SendText(a.ToString(), b.ToString(), c.ToString());

            else if (a is User user) {
                if (b is String || b is ConcatenatedString)
                    server.SendText(user.Client, b.ToString(), c.ToString());

                else if (b is User u2)
                    server.SendText(user.Client, u2.Client, c.ToString());
            }
            else if (a is int id) {
                if (b is String || b is ConcatenatedString)
                    server.SendText((s) => s.Vroom == id, b.ToString(), c.ToString());

                else if (b is User u2)
                    server.SendText((s) => s.Vroom == id, u2.Client, c.ToString());
            }
            else if (a is FunctionInstance match) {

                for (int i = 0; i < this.script.Room.Users.Items.Count; i++) {
                    user = (User)this.script.Room.Users.Items[i];
                    object ret = null;

                    try {
                        ret = match.Call(this, user);

                        if (TypeConverter.ConvertTo<bool>(Engine, ret)) {
                            if (b is String || b is ConcatenatedString)
                                server.SendText(user.Client, b.ToString(), c.ToString());

                            else if (b is User u2)
                                server.SendText(user.Client, u2.Client, c.ToString());
                        }
                    }
                    catch (JavaScriptException jex) {
                        JurassicPlugin.Self.OnError(jex);
                    }
                }
            }
        }

        [JSFunction(Name = "sendEmote", IsEnumerable = true, IsWritable = false)]
        public void SendEmote(object a, object b, object c) {

            if (++script.Counters["emote"] > 100)
                throw new JavaScriptException(Engine.Error.Construct("Send emote limit reached"), 0, null);

            if (c is Undefined) {
                if (a is String || a is ConcatenatedString)
                    server.SendEmote(a.ToString(), b.ToString());

                else if (a is User user)
                    server.SendEmote(user.Client, b.ToString());
            }
            else if (a is String || a is ConcatenatedString)
                server.SendEmote(a.ToString(), b.ToString(), c.ToString());

            else if (a is User user) {
                if (b is String || b is ConcatenatedString)
                    server.SendEmote(user.Client, b.ToString(), c.ToString());

                else if (b is User user1)
                    server.SendEmote(user.Client, user1.Client, c.ToString());
            }
            else if (a is int id) {
                if (b is String || b is ConcatenatedString)
                    server.SendEmote((s) => s.Vroom == id, b.ToString(), c.ToString());

                else if (b is User user1)
                    server.SendEmote((s) => s.Vroom == id, user1.Client, c.ToString());
            }
            else if (a is FunctionInstance match) {

                for (int i = 0; i < this.script.Room.Users.Items.Count; i++) {
                    user = (User)this.script.Room.Users.Items[i];
                    object ret = null;

                    try {
                        ret = match.Call(this, user);

                        if (TypeConverter.ConvertTo<bool>(Engine, ret)) {
                            if (b is String || b is ConcatenatedString)
                                server.SendEmote(user.Client, b.ToString(), c.ToString());

                            else if (b is User user1)
                                server.SendEmote(user.Client, user1.Client, c.ToString());
                        }
                    }
                    catch (JavaScriptException jex) {
                        JurassicPlugin.Self.OnError(jex);
                    }
                }
            }
        }

        [JSFunction(Name = "sendPrivate", IsEnumerable = true, IsWritable = false)]
        public void SendPrivate(object a, object b, object c) {

            if (++script.Counters["private"] > 150)
                throw new JavaScriptException(Engine.Error.Construct("Send PM limit reached"), 0, null);

            if (c is Undefined) 
                return;

            if (a is Undefined || b is Undefined)
                return;

            if (a is String || a is ConcatenatedString)
                server.SendPrivate(a.ToString(), b.ToString(), c.ToString());

            else if (a is User user) {
                if (b is String || b is ConcatenatedString)
                    server.SendPrivate(user.Client, b.ToString(), c.ToString());
                
                else if (b is User user1)
                    server.SendPrivate(user.Client, user1.Client, c.ToString());
            }
            else if (a is FunctionInstance match) {
                
                for (int i = 0; i < this.script.Room.Users.Items.Count; i++) {
                    user = (User)this.script.Room.Users.Items[i];
                    try {
                        object ret = match.Call(this, user);
                        if (TypeConverter.ConvertTo<bool>(Engine, ret)) {
                            if (b is String || b is ConcatenatedString)
                                server.SendPrivate(user.Client, b.ToString(), c.ToString());

                            else if (b is User user1)
                                server.SendPrivate(user.Client, user1.Client, c.ToString());
                        }
                    }
                    catch (JavaScriptException jex) {
                        JurassicPlugin.Self.OnError(jex);
                    }
                }
            }
        }

        [JSFunction(Name = "sendWebsite", IsEnumerable = true, IsWritable = false)]
        public void SendWebSite(object a, object b, object c) {

            if (++script.Counters["website"] > 100)
                throw new JavaScriptException(Engine.Error.Construct("Send website limit reached"), 0, null);

            if (c is Undefined)
                server.SendWebsite(a.ToString(), b.ToString());

            else if (a is String || a is ConcatenatedString)
                server.SendWebsite(a.ToString(), b.ToString(), c.ToString());

            else if (a is User user)
                server.SendWebsite(user.Client, b.ToString(), c.ToString());

            else if (a is FunctionInstance match) {

                for (int i = 0; i < this.script.Room.Users.Items.Count; i++) {
                    user = (User)this.script.Room.Users.Items[i];
                    try {
                        object ret = match.Call(this, user);
                        if (TypeConverter.ConvertTo<bool>(Engine, ret))
                            server.SendWebsite(user.Client, b.ToString(), c.ToString());
                    }
                    catch (JavaScriptException jex) {
                        JurassicPlugin.Self.OnError(jex);
                    }
                }
            }
        }

        [JSFunction(Name = "sendPacket", IsEnumerable = true, IsWritable = false)]
        public void SendPacket(object a, object b)
        {
            if (++script.Counters["json"] > 100)
                throw new JavaScriptException(Engine.Error.Construct("Send packet limit reached"), 0, null);

            if (b is Undefined) {
                var formatter = new ServerFormatter();
                string tmp = a.ToString();
                Match match = Regex.Match(tmp, "(['\"]*)id\\1:\\s*(['\"]*)(\\d+)\\2", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success) {
                    if (byte.TryParse(match.Groups[3].Value, out byte id)) {
                        IPacket p = formatter.Unformat(id, tmp);
                        if (p != null) server.SendPacket(p);
                        return;
                    }
                }
            }
            else if (a is User user) {
                var formatter = new ServerFormatter();
                string tmp = b.ToString();
                Match match = Regex.Match(tmp, "(['\"]*)id\\1:\\s*(['\"]*)(\\d+)\\2", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success) {
                    if (byte.TryParse(match.Groups[3].Value, out byte id)) {
                        IPacket p = formatter.Unformat(id, tmp);
                        if (p != null) server.SendPacket(user.Client, p);
                        return;
                    }
                }
            }
            else if (a is FunctionInstance match) {
                var formatter = new ServerFormatter();
                string tmp = b.ToString();
                Match regm = Regex.Match(tmp, "(['\"]*)id\\1:\\s*(['\"]*)(\\d+)\\2", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (regm.Success) {
                    if (byte.TryParse(regm.Groups[3].Value, out byte id)) {
                        IPacket p = formatter.Unformat(id, tmp);
                        if (p != null) {
                            for (int i = 0; i < this.script.Room.Users.Items.Count; i++) {
                                user = (User)this.script.Room.Users.Items[i];
                                try {
                                    object ret = match.Call(this, user);
                                    if (TypeConverter.ConvertTo<bool>(Engine, ret)) {
                                        server.SendPacket(user.Client, p);
                                    }
                                }
                                catch (JavaScriptException jex) {
                                    JurassicPlugin.Self.OnError(jex);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

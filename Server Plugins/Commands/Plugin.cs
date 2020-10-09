using Commands.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Zorbo.Ares;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Server;
using Zorbo.Core;
using Zorbo.Core.Plugins.Server;
using Zorbo.Core.Server;

namespace Commands
{
    public class Commands : ServerPlugin
    {
        static Dictionary<AdminLevel, List<string>> help;

        public Commands()
        {
            CustomTriggers = new[] { ".", "!" };
        }

        public override void OnPluginLoaded() {
            LoadHelp();
            Server.SendAnnounce("Zorbo Commands Plugin Loaded");
        }

        public override void OnHelp(IClient client)
        {
            foreach (var pair in help) {
                if (client.Admin >= pair.Key)
                    foreach (var line in pair.Value)
                        Server.SendAnnounce(client, line);
            }
        }

        public override void OnVroomJoin(IClient client)
        {
            Server.SendAnnounce(client, string.Format(Strings.VroomNotice, client.Vroom));
        }

        public override bool OnTextCommand(IClient client, string cmd, string args)
        {
            switch (cmd) {
                case "id":
                    Server.SendAnnounce(client, String.Format(Strings.UserId, client.Id));
                    break;
                case "vroom":
                    ushort vroom = 0;

                    if (UInt16.TryParse(args, out vroom))
                        client.Vroom = vroom;

                    break;
                case "admins":
                case "listadmins": {
                    if (client.Admin > AdminLevel.User) {

                        var tmp = Server.History.Admin.ToList();
                        tmp.Sort((a, b) => ((int)a.Admin) - ((int)b.Admin));

                        Server.SendAnnounce(client, String.Format("-- Admins: {0} --", tmp.Count));

                        foreach (var user in tmp)
                            Server.SendAnnounce(client, String.Format("{0}: {1}", user.Admin, user.Name));
                    }
                }
                break;
                case "topic":
                    if (client.Admin > AdminLevel.Moderator)
                        Server.Config.Topic = args;

                    break;
                case "info":
                case "whois": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null) {
                            Server.SendAnnounce(client, String.Format("-- Whois: {0} --", target.Name));
                            Server.SendAnnounce(client, String.Format("Id: {0}", target.Id));
                            Server.SendAnnounce(client, String.Format("Level: {0}", target.Admin));
                            Server.SendAnnounce(client, String.Format("Vroom: {0}", target.Vroom));
                            Server.SendAnnounce(client, String.Format("Muzzled: {0}", target.Muzzled));
                            Server.SendAnnounce(client, String.Format("IsCaptcha: {0}", target.IsCaptcha));
                            Server.SendAnnounce(client, String.Format("Address: {0}", target.ExternalIp));
                            Server.SendAnnounce(client, String.Format("Client: {0}", target.Version));
                        }
                        else {
                            Server.SendAnnounce(client, "-- Userlist Info --");

                            foreach (var user in Server.Users)
                                Server.SendAnnounce(client, String.Format("[{0}] - {1}", user.Id, user.Name));
                        }
                    }
                }
                break;
                case "whowas": {
                    if (client.Admin > AdminLevel.User) {
                        var targets = AresCommands.ParseHistoryCommand(Server, args, out args);

                        if (targets.Count > 0) {
                            for (int i = 0; i < targets.Count; i++) {
                                var target = targets[i];

                                Server.SendAnnounce(client, String.Format("-- Whowas: {0} --", target.Name));
                                Server.SendAnnounce(client, String.Format("Muzzled: {0}", target.Muzzled));
                                Server.SendAnnounce(client, String.Format("Trusted: {0}", target.Trusted));
                                Server.SendAnnounce(client, String.Format("Address: {0}", target.ClientId.ExternalIp));
                                Server.SendAnnounce(client, String.Format("LastSeen: {0} (Server Time)", target.LastSeen));

                                if (i < (targets.Count - 1)) Server.SendAnnounce(client, " ");
                            }
                        }
                        else Server.SendAnnounce(client, Strings.HistoryNotFound);
                    }
                }
                break;
                case "muzzle": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null && client.Admin > target.Admin)
                            target.Muzzled = true;
                    }
                }
                break;
                case "unmuzzle": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null && client.Admin > target.Admin)
                            target.Muzzled = false;
                    }
                }
                break;
                case "kill":
                case "kick":
                case "disconnect": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null && client.Admin > target.Admin) {
                            Server.SendAnnounce(String.Format(Strings.Kicked, target.Name));
                            target.Disconnect();
                        }
                    }
                }
                break;
                case "redirect": {
                    if (client.Admin > AdminLevel.Moderator) {

                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);
                        AresChannel hash = Hashlinks.FromHashlinkString<AresChannel>(args);

                        if (hash != null) {

                            if (target != null && client.Admin > target.Admin)
                                Server.SendPacket(target, new Redirect(hash, String.Format(Strings.Redirected, hash.Name)));
                        }
                    }
                }
                break;
                case "ban": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null && client.Admin > target.Admin) {
                            Server.SendAnnounce(String.Format(Strings.Banned, target.Name));
                            target.Ban();
                        }
                    }
                }
                break;
                case "bans":
                case "listbans": {
                    if (client.Admin > AdminLevel.User) {

                        string format = "[{0}] - {1}";
                        Server.SendAnnounce(client, "-- Banned Users --");

                        for (int i = 0; i < Server.History.Bans.Count; i++) {

                            var banned = Server.History.Bans[i];
                            var record = Server.History.Records.Find((s) => banned.Equals(s));

                            if (record != null)
                                Server.SendAnnounce(client, String.Format(format, i, record.Name));
                        }
                    }
                }
                break;
                case "unban": {
                    if (client.Admin > AdminLevel.User) {
                        var targets = AresCommands.ParseHistoryCommand(Server, args, out args);

                        if (targets.Count > 0) {
                            targets.ForEach((s) => {
                                Server.History.Bans.Remove(s.ClientId);
                                Server.SendAnnounce(String.Format(Strings.Unbanned, s.Name));
                            });
                        }
                        else Server.SendAnnounce(client, Strings.HistoryNotFound);
                    }
                }
                break;
                case "cbans":
                case "clearbans": {
                    if (client.Admin > AdminLevel.Moderator) {
                        Server.History.Bans.Clear();
                        Server.SendAnnounce(String.Format(Strings.BansCleared, client.Name));
                    }
                }
                break;
                case "banrange":
                    if (client.Admin > AdminLevel.Moderator) {
                        try {
                            Regex reg = new Regex(args, RegexOptions.IgnoreCase);
                            Server.History.RangeBans.Add(reg);

                            Server.SendAnnounce(String.Format(Strings.RangeBanAdd, reg.ToString()));
                        }
                        catch (ArgumentException ex) {
                            Server.SendAnnounce(String.Format(Strings.InvalidPattern, ex.Message));
                        }
                    }
                    break;
                case "rbans":
                case "listrangebans": {
                    if (client.Admin > AdminLevel.Moderator) {

                        string format = "[{0}] - {1}";
                        Server.SendAnnounce(client, "-- Banned IP Addresses --");

                        for (int i = 0; i < Server.History.RangeBans.Count; i++) {

                            var regex = Server.History.RangeBans[i];
                            Server.SendAnnounce(client, String.Format(format, i, regex.ToString()));
                        }
                    }
                }
                break;
                case "unbanrange":
                    if (client.Admin > AdminLevel.Moderator) {

                        if (int.TryParse(args, out int index)) {

                            if (index < 0) index = 0;

                            if (index >= Server.History.RangeBans.Count)
                                Server.SendAnnounce(client, Strings.InvalidIndex);
                            else {
                                Regex regex = Server.History.RangeBans[index];
                                Server.History.RangeBans.RemoveAt(index);

                                Server.SendAnnounce(String.Format(Strings.RangeBanRemove, regex.ToString()));
                            }
                        }
                        else {
                            try {
                                Regex regex = new Regex(args);
                                Server.History.RangeBans.Remove(regex);

                                Server.SendAnnounce(String.Format(Strings.RangeBanRemove, regex.ToString()));
                            }
                            catch (Exception ex) {
                                Server.SendAnnounce(client, String.Format(Strings.InvalidPattern, ex.Message));
                            }
                        }
                    }
                    break;
                case "crangebans":
                case "clearrangebans": {
                    if (client.Admin > AdminLevel.Moderator) {
                        Server.History.RangeBans.Clear();
                        Server.SendAnnounce(String.Format(Strings.RangeBansCleared, client.Name));
                    }
                }
                break;
                case "grant": {
                    if (client.Admin >= AdminLevel.Host) {
                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null && Enum.TryParse(args, out AdminLevel tmp)) {

                            if (client.LocalHost || client.Admin >= target.Admin)
                                target.Admin = tmp;

                            else target.Admin = (AdminLevel)Math.Min((int)client.Admin, (int)tmp);

                            var password = Server.History.Admin.Passwords.Find((s) => s.ClientId.Equals(target));

                            if (password != null) {
                                password.ClientId = target.ClientId;
                                password.Level = tmp;
                            }
                        }
                    }
                }
                break;
                case "captcha": {
                    if (client.Admin >= AdminLevel.Host) {

                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null)
                            target.IsCaptcha = true;
                    }
                }
                break;
                case "skipcaptcha": {
                    if (client.Admin >= AdminLevel.Host) {

                        IClient target = AresCommands.ParseUserCommand(Server, args, out args);

                        if (target != null)
                            target.IsCaptcha = false;
                    }
                }
                break;
                case "loadplugin":
                    if (client.Admin >= AdminLevel.Host)
                        Server.PluginHost.LoadPlugin(args);

                    break;
                case "killplugin":
                    if (client.Admin >= AdminLevel.Host)
                        Server.PluginHost.KillPlugin(args);

                    break;
                case "plugins":
                case "listplugins":
                    if (client.Admin > AdminLevel.User) {
                        Server.SendAnnounce(client, "-- Active plugins --");

                        foreach (var plugin in Server.PluginHost)
                            if (plugin.Enabled)
                                Server.SendAnnounce(client, plugin.Name);
                    }
                    break;
            }

            return true;
        }

        internal static void LoadHelp() 
        {
            if (help != null) return;
            help = new Dictionary<AdminLevel, List<string>>();

            Stream stream = null;
            StreamReader reader = null;

            try {
                Regex regex = new Regex("^\\[(\\w+?)\\]$");
                Assembly asm = Assembly.GetExecutingAssembly();

                stream = asm.GetManifestResourceStream("Commands.Help.txt");
                reader = new StreamReader(stream);

                AdminLevel level = AdminLevel.User;
                help[level] = new List<string>();

                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    Match match = regex.Match(line);

                    if (match.Success) {
                        string str = match.Groups[1].Value;

                        if (str != "General") {
                            level = (AdminLevel)Enum.Parse(typeof(AdminLevel), str);
                            help[level] = new List<string>();
                        }
                    }

                    help[level].Add(line);
                }
            }
            catch { }
            finally {
                if (reader != null) {
                    reader.Close();
                    reader.Dispose();
                }

                if (stream != null)
                    stream.Dispose();
            }
        }
    }
}

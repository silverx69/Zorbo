using Commands.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Zorbo.Ares;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Core;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;

namespace Commands
{
    class Processor
    {
        static Dictionary<AdminLevel, List<String>> help;

        internal static void LoadHelp() {
            help = new Dictionary<AdminLevel, List<String>>();

            Stream stream = null;
            StreamReader reader = null;

            try {
                Regex regex = new Regex("^\\[(\\w+?)\\]$");
                Assembly asm = Assembly.GetExecutingAssembly();

                stream = asm.GetManifestResourceStream("Commands.Help.txt");
                reader = new StreamReader(stream);

                AdminLevel level = AdminLevel.User;
                help[level] = new List<String>();

                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    Match match = regex.Match(line);

                    if (match.Success) {
                        string str = match.Groups[1].Value;

                        if (str != "General") {
                            level = (AdminLevel)Enum.Parse(typeof(AdminLevel), str);
                            help[level] = new List<String>();
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

        internal static void SendHelp(IServer server, IClient client) {
            foreach (var pair in help) {
                if (client.Admin >= pair.Key)
                    foreach (var line in pair.Value)
                        server.SendAnnounce(client, line);
            }
        }


        internal static bool HandleCommand(IServer server, IClient client, string text)
        {
            string cmd = String.Empty;
            string args = String.Empty;

            int sep = text.IndexOf('\x20');

            if (sep == -1)
                cmd = text;
            else {
                cmd = text.Substring(0, sep);

                if (text.Length > (sep + 1))
                    args = text.Substring(sep + 1);
            }

            switch (cmd) {
                case "id":
                    server.SendAnnounce(client, String.Format(Strings.UserId, client.Id));
                    break;
                case "vroom":
                    ushort vroom = 0;

                    if (UInt16.TryParse(args, out vroom))
                        client.Vroom = vroom;

                    break;
                case "admins":
                case "listadmins": {
                    if (client.Admin > AdminLevel.User) {

                        var tmp = server.History.Admin.ToList();
                        tmp.Sort((a, b) => ((int)a.Admin) - ((int)b.Admin));

                        server.SendAnnounce(client, String.Format("-- Admins: {0} --", tmp.Count));

                        foreach (var user in tmp) {
                            server.SendAnnounce(client, String.Format("{0}: {1}", user.Admin, user.Name));
                        }
                    }
                }
                break;
                case "topic":
                    if (client.Admin > AdminLevel.Moderator)
                        server.Config.Topic = args;

                    break;
                case "info":
                case "whois": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null) {
                            server.SendAnnounce(client, String.Format("-- Whois: {0} --", target.Name));
                            server.SendAnnounce(client, String.Format("Id: {0}", target.Id));
                            server.SendAnnounce(client, String.Format("Level: {0}", target.Admin));
                            server.SendAnnounce(client, String.Format("Vroom: {0}", target.Vroom));
                            server.SendAnnounce(client, String.Format("Muzzled: {0}", target.Muzzled));
                            server.SendAnnounce(client, String.Format("IsCaptcha: {0}", target.IsCaptcha));
                            server.SendAnnounce(client, String.Format("Address: {0}", target.ExternalIp));
                            server.SendAnnounce(client, String.Format("Client: {0}", target.Version));
                        }
                        else {
                            server.SendAnnounce(client, "-- Userlist Info --");

                            foreach (var user in server.Users)
                                server.SendAnnounce(client, String.Format("[{0}] - {1}", user.Id, user.Name));
                        }
                    }
                }
                break;
                case "whowas": {
                    if (client.Admin > AdminLevel.User) {
                        var targets = ParseHistoryCommand(server, args, out args);

                        if (targets.Count > 0) {
                            for (int i = 0; i < targets.Count; i++) {
                                var target = targets[i];

                                server.SendAnnounce(client, String.Format("-- Whowas: {0} --", target.Name));
                                server.SendAnnounce(client, String.Format("Muzzled: {0}", target.Muzzled));
                                server.SendAnnounce(client, String.Format("Trusted: {0}", target.Trusted));
                                server.SendAnnounce(client, String.Format("Address: {0}", target.ClientId.ExternalIp));
                                server.SendAnnounce(client, String.Format("LastSeen: {0} (Server Time)", target.LastSeen));

                                if (i < (targets.Count - 1)) server.SendAnnounce(client, " ");
                            }
                        }
                        else server.SendAnnounce(client, Strings.HistoryNotFound);
                    }
                }
                break;
                case "muzzle": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null && client.Admin > target.Admin)
                            target.Muzzled = true;
                    }
                }
                break;
                case "unmuzzle": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null && client.Admin > target.Admin)
                            target.Muzzled = false;
                    }
                }
                break;
                case "kill":
                case "kick":
                case "disconnect": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null && client.Admin > target.Admin) {
                            server.SendAnnounce(String.Format(Strings.Kicked, target.Name));
                            target.Disconnect();
                        }
                    }
                }
                break;
                case "redirect": {
                    if (client.Admin > AdminLevel.Moderator) {

                        IClient target = ParseUserCommand(server, args, out args);
                        AresChannel hash = Hashlinks.FromHashlinkString<AresChannel>(args);

                        if (hash != null) {

                            if (target != null && client.Admin > target.Admin)
                                server.SendPacket(target, new Redirect(hash, String.Format(Strings.Redirected, hash.Name)));
                        }
                    }
                }
                break;
                case "ban": {
                    if (client.Admin > AdminLevel.User) {
                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null && client.Admin > target.Admin) {
                            server.SendAnnounce(String.Format(Strings.Banned, target.Name));
                            target.Ban();
                        }
                    }
                }
                break;
                case "listbans": {
                    if (client.Admin > AdminLevel.User) {

                        string format = "[{0}] - {1}";
                        server.SendAnnounce(client, "-- Banned Users --");

                        for (int i = 0; i < server.History.Bans.Count; i++) {

                            var banned = server.History.Bans[i];
                            var record = server.History.Records.Find((s) => banned.Equals(s));

                            if (record != null)
                                server.SendAnnounce(client, String.Format(format, i, record.Name));
                        }
                    }
                }
                break;
                case "unban": {
                    if (client.Admin > AdminLevel.User) {
                        var targets = ParseHistoryCommand(server, args, out args);

                        if (targets.Count > 0) {
                            targets.ForEach((s) => {
                                server.History.Bans.Remove(s.ClientId);
                                server.SendAnnounce(String.Format(Strings.Unbanned, s.Name));
                            });
                        }
                        else server.SendAnnounce(client, Strings.HistoryNotFound);
                    }
                }
                break;
                case "cbans":
                case "clearbans": {
                    if (client.Admin > AdminLevel.Moderator) {
                        server.History.Bans.Clear();
                        server.SendAnnounce(String.Format(Strings.BansCleared, client.Name));
                    }
                }
                break;
                case "banrange":
                    if (client.Admin > AdminLevel.Moderator) {
                        try {
                            Regex reg = new Regex(args, RegexOptions.IgnoreCase);
                            server.History.RangeBans.Add(reg);

                            server.SendAnnounce(String.Format(Strings.RangeBanAdd, reg.ToString()));
                        }
                        catch (ArgumentException ex) {
                            server.SendAnnounce(String.Format(Strings.InvalidPattern, ex.Message));
                        }
                    }
                    break;
                case "listrangebans": {
                    if (client.Admin > AdminLevel.Moderator) {

                        string format = "[{0}] - {1}";
                        server.SendAnnounce(client, "-- Banned IP Addresses --");

                        for (int i = 0; i < server.History.RangeBans.Count; i++) {

                            var regex = server.History.RangeBans[i];
                            server.SendAnnounce(client, String.Format(format, i, regex.ToString()));
                        }
                    }
                }
                break;
                case "unbanrange":
                    if (client.Admin > AdminLevel.Moderator) {

                        if (int.TryParse(args, out int index)) {

                            if (index < 0) index = 0;

                            if (index >= server.History.RangeBans.Count)
                                server.SendAnnounce(client, Strings.InvalidIndex);
                            else {
                                Regex regex = server.History.RangeBans[index];
                                server.History.RangeBans.RemoveAt(index);

                                server.SendAnnounce(String.Format(Strings.RangeBanRemove, regex.ToString()));
                            }
                        }
                        else {
                            try {
                                Regex regex = new Regex(args);
                                server.History.RangeBans.Remove(regex);

                                server.SendAnnounce(String.Format(Strings.RangeBanRemove, regex.ToString()));
                            }
                            catch (Exception ex) {
                                server.SendAnnounce(client, String.Format(Strings.InvalidPattern, ex.Message));
                            }
                        }
                    }
                    break;
                case "crangebans":
                case "clearrangebans": {
                    if (client.Admin > AdminLevel.Moderator) {
                        server.History.RangeBans.Clear();
                        server.SendAnnounce(String.Format(Strings.RangeBansCleared, client.Name));
                    }
                }
                break;
                case "grant": {
                    if (client.Admin >= AdminLevel.Host) {
                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null && Enum.TryParse(args, out AdminLevel tmp)) {

                            if (client.LocalHost || client.Admin >= target.Admin)
                                target.Admin = tmp;

                            else target.Admin = (AdminLevel)Math.Min((int)client.Admin, (int)tmp);

                            var password = server.History.Admin.Passwords.Find((s) => s.ClientId.Equals(target));

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

                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null)
                            target.IsCaptcha = true;
                    }
                }
                break;
                case "skipcaptcha": {
                    if (client.Admin >= AdminLevel.Host) {

                        IClient target = ParseUserCommand(server, args, out args);

                        if (target != null)
                            target.IsCaptcha = false;
                    }
                }
                break;
                case "loadplugin":
                    if (client.Admin >= AdminLevel.Host)
                        server.PluginHost.LoadPlugin(args);

                    break;
                case "killplugin":
                    if (client.Admin >= AdminLevel.Host)
                        server.PluginHost.KillPlugin(args);

                    break;
                case "listplugins":
                    if (client.Admin > AdminLevel.User) {
                        server.SendAnnounce(client, "-- Active plugins --");

                        foreach (var plugin in server.PluginHost)
                            if (plugin.Enabled)
                                server.SendAnnounce(client, plugin.Name);
                    }
                    break;
            }

            return true;
        }


        public static IClient ParseUserCommand(IServer server, string input, out string args) {
            ushort uid = 0;
            IClient target = null;

            args = String.Empty;
            String user = String.Empty;
            
            StringBuilder sb = new StringBuilder();

            //exact match
            sb.Append("^\"(?<uid>.+?)\" (?<args>.+)$|");
            sb.Append("^\"(?<uid>.+?)\"$|");
            sb.Append("^'(?<uid>.+?)' (?<args>.+)$|");
            sb.Append("^'(?<uid>.+?)'$|");
            sb.Append("^`(?<uid>.+?)` (?<args>.+)$|");
            sb.Append("^`(?<uid>.+?)`$");

            Regex regex = new Regex(sb.ToString());
            Match match = regex.Match(input);

            if (match.Success) {
                user = match.Groups[1].Value;

                if (match.Groups[2].Success)
                    args = match.Groups[2].Value;

                if (UInt16.TryParse(user, out uid))
                    target = server.FindUser((s) => s.Id == uid);
                else
                    target = server.FindUser((s) =>
                        s.Name == user ||
                        s.ExternalIp.ToString() == user);
            }
            else {
                sb.Clear();
                //search match
                sb.Append("^(?<uid>.+?) (?<args>.+)$|");
                sb.Append("^(?<uid>[^\x20]+?)$");

                regex = new Regex(sb.ToString());
                match = regex.Match(input);

                if (match.Success) {
                    user = match.Groups[1].Value;

                    if (match.Groups[2].Success)
                        args = match.Groups[2].Value;

                    if (UInt16.TryParse(user, out uid))
                        target = server.FindUser((s) => s.Id == uid);
                    else
                        target = server.FindUser((s) => 
                            s.Name.StartsWith(user) ||
                            s.ExternalIp.ToString().StartsWith(user));
                }
            }

            return target;
        }

        public static List<Record> ParseHistoryCommand(IServer server, string input, out string args) {
            var targets = new List<Record>();

            args = String.Empty;
            String user = String.Empty;

            StringBuilder sb = new StringBuilder();

            //exact match
            sb.Append("^\"(?<uid>.+?)\" (?<args>.+)$|");
            sb.Append("^\"(?<uid>.+?)\"$|");
            sb.Append("^'(?<uid>.+?)' (?<args>.+)$|");
            sb.Append("^'(?<uid>.+?)'$|");
            sb.Append("^`(?<uid>.+?)` (?<args>.+)$|");
            sb.Append("^`(?<uid>.+?)`$");

            Regex regex = new Regex(sb.ToString());
            Match match = regex.Match(input);

            if (match.Success) {
                user = match.Groups[1].Value;

                targets = server.History.Records.FindAll((s) =>
                    s.Name == user ||
                    s.ClientId.ExternalIp.ToString() == user).ToList();
            }
            else {
                sb.Clear();
                //search match
                sb.Append("^(?<uid>.+?) (?<args>.+)$|");
                sb.Append("^(?<uid>[^\x20]+?)$");

                regex = new Regex(sb.ToString());
                match = regex.Match(input);

                if (match.Success) {
                    user = match.Groups[1].Value;

                    targets = server.History.Records.FindAll((s) =>
                        s.Name.StartsWith(user) ||
                        s.ClientId.ExternalIp.ToString().StartsWith(user)).ToList();
                }
            }

            return targets;
        }
    }
}

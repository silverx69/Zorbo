using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Zorbo.Ares.Resources;
using Zorbo.Core;
using Zorbo.Core.Plugins.Server;
using Zorbo.Core.Server;

namespace Zorbo.Ares.Server
{
    public static class AresCommands
    {
        internal static bool HandleCommand(IServer server, IClient client, string text)
        {
            string cmd = ParseCommand(text, out string args);

            switch (cmd) {
                case "help":
                    SendHelp(server, client);
                    break;
                case "login":
                    HandleLogin(server, client, args);
                    return true;
                case "register":
                    HandleRegister(server, client, args);
                    return true;
            }

            if (!((ServerPluginHost)server.PluginHost).OnTextCommand(client, cmd, args))
                return true;

            return false;
        }

        internal static bool HandlePreCommand(IServer server, IClient client, string text)
        {
            string trigger = ((ServerPluginHost)server.PluginHost).GetTrigger(text);

            if (string.IsNullOrWhiteSpace(trigger) && text.StartsWith('#')) 
                trigger = "#";

            if (!string.IsNullOrWhiteSpace(trigger))
                return HandleCommand(server, client, text.Substring(trigger.Length));

            return false;
        }

        private static void SendHelp(IServer server, IClient client)
        {
            server.SendAnnounce(client, "[Zorbo Help Display]");
            server.SendAnnounce(client, "#help - initiates the help display");
            server.SendAnnounce(client, "#login <password> - attempts to login as an administrator");
            server.SendAnnounce(client, "#register <password> - registers a password with the server");

            ((ServerPluginHost)server.PluginHost).OnHelp(client);
        }

        internal static void HandleLogin(IServer server, IClient client, string password) 
        {
            password = Password.CreateSha1Text(password);
            var pass = server.History.Admin.Passwords.Find((s) => s.Sha1Text == password);

            if (pass == null) {
                ((AresServer)server).Stats.InvalidLogins++;
                server.SendAnnounce(client, Strings.InvalidLogin);
            }
            else {
                pass.ClientId = new ClientId(client.Guid, client.ExternalIp);

                server.SendAnnounce(client, String.Format(Strings.LoggedIn, pass.Level));
                client.Admin = pass.Level;

                ((ServerPluginHost)server.PluginHost).OnLogin(client, pass);
            }
        }

        internal static void HandleAutoLogin(IServer server, IClient client, byte[] password) 
        {
            var pass = server.History.Admin.Passwords.CheckSha1(client, password);

            if (pass == null) {
                ((AresServer)server).Stats.InvalidLogins++;
                server.SendAnnounce(client, Strings.InvalidLogin);
            }
            else {
                pass.ClientId = new ClientId(client.Guid, client.ExternalIp);

                server.SendAnnounce(client, String.Format(Strings.LoggedIn, pass.Level));
                client.Admin = pass.Level;

                ((ServerPluginHost)server.PluginHost).OnLogin(client, pass);
            }
        }

        internal static void HandleRegister(IServer server, IClient client, string text)
        {
            var passes = server.History.Admin.Passwords;
            var pass = new Password(client, text);

            if (!passes.Contains(pass)) {
                passes.Add(pass);
                server.SendAnnounce(client, Strings.Registered);
                ((ServerPluginHost)server.PluginHost).OnRegister(client, pass);
            }
        }

        public static string ParseCommand(string input, out string args)
        {
            args = string.Empty;
            int sep = input.IndexOf('\x20');
            string cmd;
            if (sep == -1)
                cmd = input;
            else {
                cmd = input.Substring(0, sep);
                if (input.Length > (sep + 1))
                    args = input.Substring(sep + 1);
            }
            return cmd;
        }

        public static IClient ParseUserCommand(IServer server, string input, out string args)
        {
            ushort uid = 0;
            IClient target = null;

            args = string.Empty;
            string user = string.Empty;

            var sb = new StringBuilder();

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

        public static List<Record> ParseHistoryCommand(IServer server, string input, out string args)
        {
            var targets = new List<Record>();

            args = string.Empty;
            string user = string.Empty;

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

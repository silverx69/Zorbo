using System;
using System.IO;
using System.Text.RegularExpressions;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Core;
using Zorbo.Core.Plugins.Server;
using Zorbo.Core.Server;

namespace Motd
{
    public class Motd : ServerPlugin
    {
        string[] motdlines;

        private void LoadMotd() {
            Stream stream = null;
            StreamReader reader = null;

            try {
                string file = Path.Combine(Directory, "motd.txt");

                if (File.Exists(file)) {
                    stream = File.Open(file, FileMode.Open, FileAccess.Read);
                    reader = new StreamReader(stream);
                    if (stream.Length > 0) {
                        Server.SendAnnounce(string.Format("Motd loaded [{0} bytes]", stream.Length));
                        motdlines = reader
                            .ReadToEnd()
                            .Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    }
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

        private void ShowMotd(IClient client) {

            if (motdlines != null && motdlines.Length > 0) {

                for (int i = 0; i < motdlines.Length; i++) {
                    string line = motdlines[i].Trim();
                    Server.SendAnnounce(client, ReplaceVars(client, line));
                }
            }
        }

        private string ReplaceVars(IClient client, string input) {
            Regex regex = new Regex("(?<tag>\\+n|\\+ip|\\+time|\\+dns|\\+vroom)", RegexOptions.IgnoreCase);
            Match match = regex.Match(input);

            while (match.Success) {
                var group = match.Groups[1];
                string replace = string.Empty;

                switch (group.Value.ToLower()) {
                    case "+n":
                        replace = client.Name;
                        break;
                    case "+id":
                        replace = client.Id.ToString();
                        break;
                    case "+ip":
                        replace = client.ExternalIp.ToString();
                        break;
                    case "+time":
                        replace = DateTime.Now.ToShortTimeString();
                        break;
                    case "+date":
                        replace = DateTime.Now.ToShortDateString();
                        break;
                    case "+vroom":
                        replace = client.Vroom.ToString();
                        break;
                }

                input = input.Remove(match.Groups[1].Index, match.Groups[1].Length);
                input = input.Insert(group.Index, replace);

                match = regex.Match(input, group.Index + replace.Length);
            }

            return input;
        }

        public override void OnPluginLoaded() {
            Server.SendAnnounce("Motd plugin has been loaded!");
            LoadMotd();
        }

        public override void OnPluginKilled() {
            Server.SendAnnounce("Motd plugin has been unloaded!");
        }

        public override void OnSendJoin(IClient client)
        {
            if (client.Vroom == 0 && !client.Extended.ContainsKey("HasSeenMotd")) {
                ShowMotd(client);
                client.Extended["HasSeenMotd"] = true;
            }
        }

        public override bool OnTextCommand(IClient client, string cmd, string args)
        {
            if (client.Admin >= AdminLevel.Admin) {
                switch (cmd) {
                    case "loadmotd":
                        LoadMotd();
                        break;
                    case "viewmotd":
                        ShowMotd(client);
                        break;
                }
            }
            return true;
        }
    }
}

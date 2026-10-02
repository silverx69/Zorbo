using System;
using System.Collections.Generic;
using System.Linq;
using Zorbo.Ares.Formatters;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Ares.Packets.Chatroom.ib0t;
using Zorbo.Core;
using Zorbo.Core.Plugins.Server;
using Zorbo.Core.Server;

namespace cb0tProtocol
{
#pragma warning disable IDE1006 // Naming Styles
    public class cb0tProtocol : ServerPlugin
#pragma warning restore IDE1006 // Naming Styles
    {
        public override void OnPluginLoaded()
        {
            foreach (var user in Server.Users) {
                user.Extended["CustomFont"] = null;
                user.Extended["SupportEmote"] = false;
                user.Extended["VoiceIgnore"] = new List<String>();
                user.Extended["CustomEmote"] = new List<ClientEmoteItem>();
            }

            foreach (var user in Server.Users)
                OnSendJoin(user);

            Server.SendAnnounce("cb0tProtocol plugin has been loaded!!");
        }

        public override void OnPluginKilled()
        {
            // Send packets to inform cb0t we've stopped supporting the advanced features
            var voice_support = new Advanced(new ServerVoiceSupport() {
                Enabled = false,
                HighQuality = false
            });

            foreach (var user in Server.Users) {

                if (user.LoggedIn) {

                    if (Server.Config.AllowVoice)
                        user.SendPacket(voice_support);

                    bool pubvoice = (user.Features & ClientFlags.VOICE) == ClientFlags.VOICE;
                    bool privoice = (user.Features & ClientFlags.PRIVATE_VOICE) == ClientFlags.PRIVATE_VOICE;
                    
                    if (pubvoice || privoice) {

                        Server.SendPacket(new Advanced(new ServerVoiceSupportUser() {
                            Username = user.Name,
                            Public = false,
                            Private = false,
                        }));
                    }

                    if ((bool)user.Extended["SupportEmote"]) {

                        Server.SendPacket(new Advanced(new ServerEmoteSupport(0)));

                        foreach (var emote in (List<ClientEmoteItem>)user.Extended["CustomEmote"])
                            Server.SendPacket(new Advanced(new ServerEmoteDelete() {
                                Username = user.Name,
                                Shortcut = emote.Shortcut,
                            }));
                    }
                }
                user.Extended.Remove("CustomFont");
                user.Extended.Remove("SupportEmote");
                user.Extended.Remove("VoiceIgnore");
                user.Extended.Remove("CustomEmote");
            }
            
            Server.SendAnnounce("cb0tProtocol plugin has been unloaded!!");
        }

        public override SupportFlags OnSendFeatures(IClient client, SupportFlags features)
        {
            if ((features & SupportFlags.ROOM_SCRIBBLES) != SupportFlags.ROOM_SCRIBBLES) {
                features |= SupportFlags.ROOM_SCRIBBLES;
            }

            if ((features & SupportFlags.PRIVATE_SCRIBBLES) != SupportFlags.PRIVATE_SCRIBBLES) {
                features |= SupportFlags.PRIVATE_SCRIBBLES;
            }
            
            return features;
        }

        public override void OnSendJoin(IClient client)
        {
            if (Server.Config.AllowVoice) {
                client.SendPacket(new Advanced(new ServerVoiceSupport() {
                    Enabled = true,
                    HighQuality = true,
                }));
            }

            client.SendPacket(new Advanced(new ServerEmoteSupport(16)));

            for (int i = 0; i < Server.Users.Count; i++) {
                IClient target = Server.Users[i];

                if (target != client &&
                    target.LoggedIn &&
                    target.Vroom == client.Vroom) {

                    var font = (ServerFont)target.Extended["CustomFont"];
                    if (font != null) client.SendPacket(font);

                    bool pubvoice = (target.Features & ClientFlags.VOICE) == ClientFlags.VOICE;
                    bool privoice = (target.Features & ClientFlags.PRIVATE_VOICE) == ClientFlags.PRIVATE_VOICE;

                    if (pubvoice || privoice) {
                        client.SendPacket(new Advanced(new ServerVoiceSupportUser() {
                            Username = target.Name,
                            Public = pubvoice,
                            Private = privoice,
                        }));
                    }
                }
            }
        }

        public override bool OnJoinCheck(IClient client)
        {
            client.Extended["CustomFont"] = null;
            client.Extended["SupportEmote"] = false;
            client.Extended["VoiceIgnore"] = new List<String>();
            client.Extended["CustomEmote"] = new List<ClientEmoteItem>();

            return true;
        }

        public override bool OnTextCommand(IClient client, string cmd, string args)
        {
            switch (cmd) {
                case "scribble":
                    var scribble = RoomScribble.GetScribble(client);
                    if (Uri.TryCreate(args, UriKind.Absolute, out Uri uri)) {
                        if (uri.IsFile)
                            Server.SendAnnounce((s) => s.Vroom == client.Vroom, "File uri is not suppored by the scribble command.");

                        else if (uri.IsWellFormedOriginalString()) {

                            scribble.Download(uri, (s) => {
                                SendRoomScribble((c) => c.Vroom == client.Vroom, client.Name, scribble);
                            },
                            uri);
                        }
                    }
                    return false;
            }
            return true;
        }

        public override void OnAfterPacket(IClient client, IPacket packet)
        {
            if (packet.Id == (byte)AresId.MSG_CHAT_ADVANCED_FEATURES_PROTOCOL) {

                if (!(((Advanced)packet).Payload is AdvancedPacket advanced)) 
                    return;// could be "Unknown" type

                switch ((AdvancedId)advanced.Id) {
                    case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_ADD_TAGS:
                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_REM_TAGS:
                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_FONT:
                        ClientFont font = (ClientFont)advanced;
                        ServerFont userfont = (ServerFont)client.Extended["CustomFont"];

                        if (userfont == null) {
                            userfont = new ServerFont();
                            client.Extended["CustomFont"] = userfont;
                        }

                        userfont.Username = client.Name;
                        userfont.Size = font.Size;
                        userfont.Name = font.Name;
                        userfont.NameColor = font.NameColor;
                        userfont.TextColor = font.TextColor;
                        userfont.NameColor2 = font.NameColor2;
                        userfont.TextColor2 = font.TextColor2;

                        if (string.IsNullOrEmpty(userfont.NameColor2))
                            userfont.NameColor2 = userfont.NameColor.ToHtmlColor();

                        if (string.IsNullOrEmpty(userfont.TextColor2))
                            userfont.TextColor2 = userfont.TextColor.ToHtmlColor();

                        Server.SendPacket((s) => s.Vroom == client.Vroom, new Advanced(userfont));

                        if (String.IsNullOrWhiteSpace(font.Name))
                            client.Extended["CustomFont"] = null;

                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_VC_SUPPORTED:
                        ClientVoiceSupport vcs = (ClientVoiceSupport)advanced;

                        if (vcs.Public)
                            client.Features |= ClientFlags.VOICE;

                        if (vcs.Private)
                            client.Features |= ClientFlags.PRIVATE_VOICE;

                        Server.SendPacket((s) => s.Vroom == client.Vroom,
                            new Advanced(new ServerVoiceSupportUser() {
                                Username = client.Name,
                                Public = vcs.Public,
                                Private = vcs.Private,
                            }));
                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_VC_FIRST:
                        ClientVoiceFirst vcf = (ClientVoiceFirst)advanced;

                        Server.SendPacket((s) =>
                            (s.Vroom == client.Vroom) &&
                            (s.Features & ClientFlags.VOICE) == ClientFlags.VOICE &&
                            !((List<String>)s.Extended["VoiceIgnore"]).Contains(client.Name),

                            new Advanced(new ServerVoiceFirst() {
                                Username = client.Name,
                                Data = vcf.Data
                            }));

                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_VC_FIRST_TO: {

                        ClientVoiceFirstTo vcf2 = (ClientVoiceFirstTo)advanced;

                        IClient target = Server.FindUser((s) =>
                            s.Name == vcf2.Username &&
                            s.Vroom == client.Vroom);

                        if (target == null) return;

                        if ((client.Features & ClientFlags.PRIVATE_VOICE) != ClientFlags.PRIVATE_VOICE)
                            client.SendPacket(new Advanced(new ServerVoiceNoPrivate() {
                                Username = target.Name
                            }));

                        else if (((List<String>)target.Extended["VoiceIgnore"]).Contains(client.Name))
                            client.SendPacket(new Advanced(new ServerVoiceIgnore() {
                                Username = target.Name
                            }));

                        else target.SendPacket(new Advanced(new ServerVoiceFirstFrom() {
                            Username = client.Name,
                            Data = vcf2.Data
                        }));
                    }
                    break;
                    case AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK:
                        ClientVoiceChunk vcc = (ClientVoiceChunk)advanced;

                        Server.SendPacket((s) =>
                            (s.Vroom == client.Vroom) &&
                            (s.Features & ClientFlags.VOICE) == ClientFlags.VOICE &&
                            !((List<String>)s.Extended["VoiceIgnore"]).Contains(client.Name),

                            new Advanced(new ServerVoiceChunk() {
                                Username = client.Name,
                                Data = vcc.Data
                            }));

                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_VC_CHUNK_TO: {

                        ClientVoiceChunkTo vcc2 = (ClientVoiceChunkTo)advanced;

                        IClient target = Server.FindUser((s) =>
                            s.Name == vcc2.Username &&
                            s.Vroom == client.Vroom);

                        if (target == null) return;

                        if ((client.Features & ClientFlags.PRIVATE_VOICE) == ClientFlags.PRIVATE_VOICE)
                            client.SendPacket(new Advanced(new ServerVoiceNoPrivate() {
                                Username = target.Name
                            }));

                        else if (((List<String>)target.Extended["VoiceIgnore"]).Contains(client.Name))
                            client.SendPacket(new Advanced(new ServerVoiceIgnore() {
                                Username = target.Name
                            }));

                        else target.SendPacket(new Advanced(new ServerVoiceFirstFrom() {
                            Username = client.Name,
                            Data = vcc2.Data
                        }));
                    }
                    break;
                    case AdvancedId.MSG_CHAT_CLIENT_VC_IGNORE:
                        ClientVoiceIgnore vci = (ClientVoiceIgnore)advanced;
                        List<String> ignores = (List<String>)client.Extended["VoiceIgnore"];

                        if (ignores.Contains(vci.Username)) {

                            ignores.RemoveAll((s) => s == vci.Username);
                            Server.SendAnnounce(client, String.Format("You are now allowing voice chat from {0}", vci.Username));
                        }
                        else {
                            ignores.Add(vci.Username);
                            Server.SendAnnounce(client, String.Format("You are now ignoring voice chat from {0}", vci.Username));
                        }
                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_SUPPORTS_CUSTOM_EMOTES:
                        client.Extended["SupportEmote"] = true;

                        foreach (var user in Server.Users) {
                            if (user.Vroom == client.Vroom) {

                                var emotes = (List<ClientEmoteItem>)client.Extended["CustomEmote"];

                                if (emotes.Count > 0) {
                                    foreach (var emote in emotes)
                                        client.SendPacket(new Advanced(new ServerEmoteItem() {
                                            Username = user.Name,
                                            Shortcut = emote.Shortcut,
                                            Size = emote.Size,
                                            Image = emote.Image,
                                        }));
                                }
                            }
                        }
                        break;
                    case AdvancedId.MSG_CHAT_SERVER_CUSTOM_EMOTES_ITEM: {
                        client.Extended["SupportEmote"] = true;
                        ClientEmoteItem item = (ClientEmoteItem)advanced;

                        ((List<ClientEmoteItem>)client.Extended["CustomEmote"]).Add(item);

                        if (client.Cloaked) {
                            Server.SendPacket((s) =>
                                    s.Admin >= client.Admin &&
                                    s.Vroom == client.Vroom &&
                                    (bool)s.Extended["SupportEmote"],
                                    new Advanced(new ServerEmoteItem() {
                                        Username = client.Name,
                                        Shortcut = item.Shortcut,
                                        Size = item.Size,
                                        Image = item.Image,
                                    }));
                        }
                        else {
                            Server.SendPacket((s) => 
                                    s.Vroom == client.Vroom && 
                                    (bool)s.Extended["SupportEmote"],
                                    new Advanced(new ServerEmoteItem() {
                                        Username = client.Name,
                                        Shortcut = item.Shortcut,
                                        Size = item.Size,
                                        Image = item.Image,
                                    }));
                        }
                    }
                    break;
                    case AdvancedId.MSG_CHAT_CLIENT_CUSTOM_EMOTE_DELETE: {
                        ClientEmoteDelete item = (ClientEmoteDelete)advanced;

                        var emotes = ((List<ClientEmoteItem>)client.Extended["CustomEmote"]);
                        int index = emotes.FindIndex(s => s.Shortcut == item.Shortcut);

                        if (index > -1) {
                            emotes.RemoveAt(index);

                            Server.SendPacket((s) => s.Vroom == client.Vroom && (bool)s.Extended["SupportEmote"],
                                new Advanced(new ServerEmoteDelete() {
                                    Username = client.Name,
                                    Shortcut = item.Shortcut,
                                }));
                        }
                    }
                    break;
                    case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_FIRST:
                        OnScribbleFirst(client, (ClientScribbleFirst)advanced);
                        break;
                    case AdvancedId.MSG_CHAT_CLIENT_ROOM_SCRIBBLE_CHUNK:
                        OnScribbleChunk(client, (ClientScribbleChunk)advanced);
                        break;
                }
            }
        }

        private void OnScribbleFirst(IClient client, ClientScribbleFirst first)
        {
            var scribble = RoomScribble.GetScribble(client);

            scribble.Reset();
            scribble.Size = first.Size;
            scribble.Chunks = (ushort)(first.Chunks + 1);//scribble object counts first chunk
            scribble.Write(first.Data);

            if (scribble.IsComplete) SendRoomScribble(client, scribble);
        }

        private void OnScribbleChunk(IClient client, ClientScribbleChunk chunk)
        {
            var scribble = RoomScribble.GetScribble(client);

            scribble.Write(chunk.Data);

            if (scribble.IsComplete) SendRoomScribble(client, scribble);
        }

        //Used for Sending scribble objects from another client
        //
        private void SendRoomScribble(IClient client, RoomScribble scribble)
        {
            SendRoomScribble(
                (s) => s != client && s.Vroom == client.Vroom,
                client.Name,
                scribble);
        }

        internal void SendRoomScribble(Predicate<IClient> pred, string name, RoomScribble scribble)
        {
            byte[] buffer;
            if (scribble.Size <= 4000) {
                Server.SendAnnounce(pred, string.Format("\x000314--- From {0}", name));
                Server.SendPacket(pred, new ClientCustom(string.Empty, "cb0t_scribble_once", scribble.RawImage()));
            }
            else {
                scribble.Index = 0;

                int length = Math.Min((int)scribble.Received, 4000);

                buffer = scribble.Read();

                Server.SendAnnounce(pred, string.Format("\x000314--- From {0}", name));
                Server.SendPacket(pred, new ClientCustom(string.Empty, "cb0t_scribble_first", buffer));

                while (scribble.Remaining > 0) {
                    buffer = scribble.Read();

                    if (scribble.Remaining > 0)
                        Server.SendPacket(pred, new ClientCustom(string.Empty, "cb0t_scribble_chunk", buffer));
                    else
                        Server.SendPacket(pred, new ClientCustom(string.Empty, "cb0t_scribble_last", buffer));
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

        public override void OnError(IErrorInfo error)
        {
            Server.SendAnnounce(error.Name + " has caused: " + error.Exception.Message);
        }
    }
}
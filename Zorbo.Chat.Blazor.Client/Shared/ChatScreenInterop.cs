using cb0tProtocol;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Zorbo.Ares.Packets;
using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Chat.Blazor.Client.Classes;
using Zorbo.Chat.Blazor.Shared;
using Zorbo.Core;

namespace Zorbo.Chat.Blazor.Client.Shared
{
    public class ChatScreenHelper
    {
        readonly ChatScreenInterop Interop;
        readonly ChatClient ChatClient = ChatClient.Self;

        public ChatScreenHelper(ChatScreenInterop interop) {
            Interop = interop;
        }

        [JSInvokable]
        public bool OpenChannel(string jchannel)
        {
            ServerRecord channel = Json.Deserialize<ServerRecord>(jchannel);
            if (channel == null) 
                return false;

            if (ChatClient.ActiveChat.Channel.Equals(channel))
                return true;

            ChatClient.JoiningChannel = null;
            ChatClient.JoiningChannel = channel;

            var instance = ChatClient.OpenChannel(ChatClient.JoiningChannel, new Uri(Interop.Navigation.BaseUri));
            if (instance != null) {
                Interop.Navigation.NavigateTo(string.Format("/chatrooms/{0}", instance.Channel.HashlinkUri));
                return true;
            }

            return false;
        }

        [JSInvokable]
        public string DecodeHashlink(string hashlink)
        {
            try {
                return Json.Serialize(Hashlinks.FromHashlinkString<ServerRecord>(hashlink));
            }
            catch { }
            return null;
        }
    }

    public class ChatScreenInterop : IDisposable
    {
        string prev_ts;

        volatile bool isInvoking;
        ConcurrentQueue<ChatMessage> pendingInvokes;

        public readonly IJSRuntime JSRuntime;
        public readonly NavigationManager Navigation;
        public readonly ElementReference ChatElement;

        private DotNetObjectReference<ChatScreenHelper> ObjRef;

        public ChatScreenInterop(IJSRuntime runtime, ElementReference chatElement, NavigationManager navigation)
        {
            JSRuntime = runtime;
            Navigation = navigation;
            ChatElement = chatElement;
            pendingInvokes = new ConcurrentQueue<ChatMessage>();
            ObjRef = DotNetObjectReference.Create(new ChatScreenHelper(this));
        }

        public void Clear()
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            jip.InvokeVoid("ZorboApp.clearScreen", ChatElement);
        }

        public void InitResize() 
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            jip.InvokeVoid("ZorboApp.initResize", ChatElement);
        }

        public void InvokeMessage(ChatMessage msg)
        {
            if (!isInvoking) {
                isInvoking = true;
                pendingInvokes.Enqueue(msg);
                InvokeMessageQueue();
            }
            else pendingInvokes.Enqueue(msg);
        }

        private async void InvokeMessageQueue()
        {
            while (pendingInvokes.TryDequeue(out ChatMessage msg))
                await InvokeMessageAsync(msg);

            isInvoking = false;
        }

        private async Task InvokeMessageAsync(ChatMessage message)
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            var packet = message.Message;

            string msg;
            string ts = message.Timestamp.ToString("M/dd h:mm tt");

            if (ts != prev_ts) {
                prev_ts = ts;
                msg = string.Format(" ~~~~ {0} ~~~~ ", ts);
                await jip.InvokeVoidAsync("ZorboApp.addTime", ObjRef, ChatElement, msg, "gray");
            }

            switch (packet.Id) {
                case (byte)AresId.MSG_CHAT_SERVER_MYFEATURES:
                    var f = (Features)packet;
                    msg = string.Format("Server: {0}", f.Version);
                    await jip.InvokeVoidAsync("ZorboApp.addAnnounce", ObjRef, ChatElement, msg, "navy");
                    if (f.Language != Language.Unknown) {
                        msg = string.Format("Language: {0}", f.Language);
                        await jip.InvokeVoidAsync("ZorboApp.addAnnounce", ObjRef, ChatElement, msg, "navy");
                    }
                    break;
                case (byte)AresId.MSG_CHAT_SERVER_ERROR:
                    await jip.InvokeVoidAsync("ZorboApp.addAnnounce", ObjRef, ChatElement, ((Error)packet).Message, "navy");
                    break;
                case (byte)AresId.MSG_CHAT_SERVER_NOSUCH:
                    await jip.InvokeVoidAsync("ZorboApp.addAnnounce", ObjRef, ChatElement, ((Announce)packet).Message, "red");
                    break;
                case (byte)AresId.MSG_CHAT_SERVER_TOPIC:
                    msg = string.Format("Topic changed: {0}", ((TopicBase)packet).Message);
                    await jip.InvokeVoidAsync("ZorboApp.addAnnounce", ObjRef, ChatElement, msg, "navy");
                    break;
                case (byte)AresId.MSG_CHAT_SERVER_PUBLIC:
                    var pub = (ServerPublic)packet;
                    await jip.InvokeVoidAsync("ZorboApp.addPublic", ObjRef, ChatElement, pub.Username, pub.Message, "black", "blue");
                    break;
                case (byte)AresId.MSG_CHAT_SERVER_EMOTE:
                    var emo = (ServerEmote)packet;
                    await jip.InvokeVoidAsync("ZorboApp.addEmote", ObjRef, ChatElement, emo.Username, emo.Message, "purple");
                    break;
                case (byte)AresId.MSG_CHAT_SERVER_JOIN:
                    var join = (ChatJoin)packet;
                    msg = string.Format("{0} has joined the chat.", join.Username);
                    await jip.InvokeVoidAsync("ZorboApp.addAnnounce", ObjRef, ChatElement, msg, "green");
                    break;
                case (byte)AresId.MSG_CHAT_SERVER_PART:
                    var part = (Parted)packet;
                    msg = string.Format("{0} has left the chat.", part.Username);
                    await jip.InvokeVoidAsync("ZorboApp.addAnnounce", ObjRef, ChatElement, msg, "orange");
                    break;
                case (byte)AdvancedId.MSG_CHAT_SERVER_ROOM_SCRIBBLE:
                    var scribble = (ChatScribble)packet;
                    await jip.InvokeVoidAsync("ZorboApp.addScribble", ChatElement, scribble.Base64Scribble);
                    break;
            }
        }

        public void Dispose()
        {
            ObjRef?.Dispose();
        }
    }
}

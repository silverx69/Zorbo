using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using Zorbo.Chat.Blazor.Client.Classes;
using Zorbo.Chat.Blazor.Shared;
using Zorbo.Core;

namespace Zorbo.Chat.Blazor.Client.Pages
{
    public class ChannelsHelper
    {
        readonly ChannelsInterop interop;
        private ChatClient ChatClient = ChatClient.Self;

        public ChannelsHelper(ChannelsInterop interop)
        {
            this.interop = interop;
        }

        [JSInvokable]
        public void OpenChannel(string jchannel)
        {
            ServerRecord channel = Json.Deserialize<ServerRecord>(jchannel);
            if (channel == null) return;

            ChatClient.JoiningChannel = null;
            ChatClient.JoiningChannel = channel;

            if (ChatClient.Username == "Zorbo User")
                interop.ShowNameDialog();
            else
                interop.JoinChannel();
        }
    }

    public class ChannelsInterop : IDisposable
    {
        public readonly IJSRuntime JSRuntime;
        public readonly NavigationManager Navigation;

        private readonly ChatClient ChatClient = ChatClient.Self;
        private readonly DotNetObjectReference<ChannelsHelper> ObjRef;

        public ChannelsInterop(IJSRuntime runtime, NavigationManager navigation)
        {
            JSRuntime = runtime;
            Navigation = navigation;
            ObjRef = DotNetObjectReference.Create(new ChannelsHelper(this));
        }

        public void ShowNameDialog()
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            jip.InvokeVoid("ZorboApp.showNameDialog");
        }

        public void HideNameDialog()
        {
            var jip = (IJSInProcessRuntime)JSRuntime;

            jip.InvokeVoid("ZorboApp.hideNameDialog");
            jip.InvokeVoid("ZorboApp.writeCookie", "ZorboApp.Username", ChatClient.Username, 14);

            JoinChannel();
        }

        public void ShowHashlinkDialog()
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            jip.InvokeVoid("ZorboApp.showHashlinkDialog");
        }

        public void HideHashlinkDialog(bool cancelled = false)
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            jip.InvokeVoid("ZorboApp.hideHashlinkDialog");
            
            if (!cancelled && !string.IsNullOrEmpty(ChatClient.HashlinkText)) {

                var channel = Hashlinks.FromHashlinkString<ServerRecord>(ChatClient.HashlinkText);
                if (channel != null) {
                    ChatClient.JoiningChannel = null;
                    ChatClient.JoiningChannel = channel;

                    if (ChatClient.Username == "Zorbo User")
                        ShowNameDialog();
                    else
                        JoinChannel();
                }
            }
        }

        public void SetChannelList()
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            jip.InvokeVoid("ZorboApp.setChannelList", ObjRef);
        }

        public void AddChannels(ElementReference table, string jchannels)
        {
            var jip = (IJSInProcessRuntime)JSRuntime;
            jip.InvokeVoid("ZorboApp.addChannels", table, jchannels);
        }

        public void JoinChannel()
        {
            var instance = ChatClient.OpenChannel(ChatClient.JoiningChannel, new Uri(Navigation.BaseUri));
            if (instance != null) Navigation.NavigateTo(string.Format("/chatrooms/{0}", instance.Channel.HashlinkUri));
        }

        public void Dispose()
        {
            ObjRef?.Dispose();
        }
    }
}

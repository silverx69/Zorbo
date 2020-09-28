using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Zorbo.Chat.Blazor.Shared;
using Zorbo.Core.Models;

namespace Zorbo.Chat.Blazor.Client.Classes
{
    public class ChatClient : ModelBase, IDisposable
    {
        bool showingMenu = true;
        string username = "";
        
        IPAddress externalIp = null;

        string menuText = "Zorbo Menu";
        string headerText = "Welcome to Zorbo!";
        string hashlinkText = string.Empty;
        string channelSearch = string.Empty;

        Timer updateTimer = null;
        ChatInstance prevChat = null;
        ChatInstance activeChat = null;

        ServerRecord joiningChannel = null;
        List<ServerRecord> channels = null;
        List<ChatInstance> openChannels = null;

        public bool IsShowingMenu {
            get { return showingMenu; }
            set { OnPropertyChanged(() => showingMenu, value); }
        }

        public string Username {
            get { return username; }
            set { OnPropertyChanged(() => username, value); }
        }

        public string MenuText {
            get { return menuText; }
            set { OnPropertyChanged(() => menuText, value); }
        }

        public string HeaderText {
            get { return headerText; }
            set { OnPropertyChanged(() => headerText, value); }
        }

        public string HashlinkText {
            get { return hashlinkText; }
            set { OnPropertyChanged(() => hashlinkText, value); }
        }

        public string ChannelSearch {
            get { return channelSearch; }
            set { OnPropertyChanged(() => channelSearch, value); }
        }

        public IPAddress ExternalIp {
            get { return externalIp; }
            set { OnPropertyChanged(() => externalIp, value); }
        }

        public ChatInstance ActiveChat {
            get { return activeChat; }
            private set { OnPropertyChanged(() => activeChat, value); }
        }

        public ChatInstance PreviousChat {
            get { return prevChat; }
            set { OnPropertyChanged(() => prevChat, value); }
        }

        public ServerRecord JoiningChannel {
            get { return joiningChannel; }
            set { OnPropertyChanged(() => joiningChannel, value); }
        }

        public List<ServerRecord> Channels {
            get { return channels; }
            set { OnPropertyChanged(() => channels, value); }
        }

        public List<ChatInstance> OpenChannels {
            get { return openChannels; }
            set { OnPropertyChanged(() => openChannels, value); }
        }

        public ChatClient() {
            Self = this;
            channels = new List<ServerRecord>();
            openChannels = new List<ChatInstance>();
            updateTimer = new Timer(OnTimerElapsed, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
        }

        public void Dispose()
        {
            updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            updateTimer.Dispose();
            openChannels.ForEach(s => s.Dispose());
            openChannels.Clear();
            channels.Clear();
        }

        public async Task<IPAddress> GetAddress(Uri baseUri) {

            if (externalIp != null) 
                return externalIp;

            using var client = new HttpClient();
            string ip = await client.GetStringAsync(new Uri(baseUri, "api/Chat/address"));

            IPAddress tmp = IPAddress.Parse(ip);

            //websocket doesn't like ipv6 loopbacks apparently
            if (IPAddress.IsLoopback(tmp) && tmp.AddressFamily == AddressFamily.InterNetworkV6)
                tmp = IPAddress.Loopback;

            return ExternalIp = tmp;
        }

        private async void OnTimerElapsed(object state)
        {
            DateTime now = DateTime.Now;
            foreach(var chat in OpenChannels) {
                if (chat.IsConnected) {
                    await chat.CheckUpdate(now);
                }
                else if (now.Subtract(chat.LastConnectAttempt).TotalSeconds >= 30d) {
                    chat.LastConnectAttempt = now;
                    await chat.ReconnectAsync();
                }
            }
        }

        public ChatInstance OpenChannel(ServerRecord channel, Uri baseUri)
        {
            ChatInstance i;
            lock (OpenChannels) {
                i = OpenChannels.Find(s => s.Channel.Equals(channel));

                if (i == null) {
                    i = new ChatInstance(channel, baseUri);
                    OpenChannels.Add(i);
                }
            }
            if (i != ActiveChat) PreviousChat = ActiveChat;

            return ActiveChat = i;
        }

        public async Task CloseChannel(ChatInstance chat)
        {
            await chat.DisconnectAsync(true);

            if (chat == ActiveChat)
                ActiveChat = PreviousChat;

            lock (OpenChannels)
                OpenChannels.Remove(chat);

            chat.Dispose();
        }

        public async Task<List<ServerRecord>> GetChannels(string baseUri)
        {
            using var client = new HttpClient();
            return Channels = await client.GetFromJsonAsync<List<ServerRecord>>(new Uri(new Uri(baseUri), "api/Chat/channels"));
        }

        public static ChatClient Self { get; private set; }
    }
}

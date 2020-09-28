using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IServer : INotifyPropertyChanged
    {
        ISocket Socket { get; }//plugins have to be careful accessing this property
        Boolean Running { get; }

        IPAddress ExternalIp { get; }
        IPAddress InternalIp { get; }

        IHistory History { get; }
        IServerStats Stats { get; }
        IServerConfig Config { get; }
        IChannelList Channels { get; }
        IServerPluginHost PluginHost { get; }
        IObservableCollection<IClient> Users { get; }
        IList<IFloodRule> FloodRules { get; }

        void Start();
        void Stop();

        IClient FindUser(Predicate<IClient> client);

        void SendPacket(IPacket packet);
        void SendPacket(IClient user, IPacket packet);
        void SendPacket(Predicate<IClient> match, IPacket packet);

        void SendText(string sender, string text);
        void SendText(IClient sender, string text);
        void SendText(string target, string sender, string text);
        void SendText(IClient target, string sender, string text);
        void SendText(IClient target, IClient sender, string text);
        void SendText(Predicate<IClient> match, string sender, string text);
        void SendText(Predicate<IClient> match, IClient sender, string text);

        void SendEmote(string sender, string text);
        void SendEmote(IClient sender, string text);
        void SendEmote(string target, string sender, string text);
        void SendEmote(IClient target, string sender, string text);
        void SendEmote(IClient target, IClient sender, string text);
        void SendEmote(Predicate<IClient> match, string sender, string text);
        void SendEmote(Predicate<IClient> match, IClient sender, string text);

        void SendPrivate(string target, string sender, string text);
        void SendPrivate(IClient target, string sender, string text);
        void SendPrivate(IClient target, IClient sender, string text);
        void SendPrivate(Predicate<IClient> match, string sender, string text);
        void SendPrivate(Predicate<IClient> match, IClient sender, string text);

        void SendAnnounce(string text);
        void SendAnnounce(IClient target, string text);
        void SendAnnounce(Predicate<IClient> target, string text);
        //sends configured website
        void SendWebsite();
        //sends custom website
        void SendWebsite(string address, string caption);
        void SendWebsite(string target, string address, string caption);
        void SendWebsite(IClient target, string address, string caption);
        void SendWebsite(Predicate<IClient> match, string address, string caption);
    }
}

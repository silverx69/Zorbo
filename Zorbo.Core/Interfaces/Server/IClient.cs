using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IClient : INotifyPropertyChanged, IDisposable
    {
        IServer Server { get; }
        ISocket Socket { get; }//plugins have to be careful accessing this property
        IMonitor Monitor { get; }

        ushort Id { get; }
        Guid Guid { get; }
        uint Token { get; }

        ClientId ClientId { get; }
        AdminLevel Admin { get; set; }

        bool LoggedIn { get; }
        bool Connected { get; }
        bool LocalHost { get; set; }
        bool Browsable { get; }
        bool Muzzled { get; set; }
        bool Cloaked { get; set; }
        bool IsCaptcha { get; set; }
        bool FastPing { get; }
        bool Encryption { get; }
        bool Compression { get; }

        byte Age { get; }
        Gender Gender { get; }
        Country Country { get; }

        IAvatar Avatar { get; set; }
        IAvatar OrgAvatar { get; }

        string Name { get; set; }
        string OrgName { get; }
        string Version { get; }
        string Region { get; }
        string Message { get; set; }

        ushort Vroom { get; set; }
        ushort FileCount { get; }
        ushort NodePort { get; }
        ushort ListenPort { get; }

        IPAddress NodeIp { get; }
        IPAddress LocalIp { get; }
        IPAddress ExternalIp { get; }

        DateTime JoinTime { get; }
        DateTime CaptchaTime { get; }
        DateTime LastUpdate { get; set; }

        ClientFlags Features { get; set; }

        IObservableCollection<string> Ignored { get; }
        IDictionary<byte, IFloodCounter> Counters { get; }
        IDictionary<string, object> Extended { get; }

        void SendPacket(IPacket packet);
        void SendPacket(Predicate<IClient> match, IPacket packet);

        void Ban();
        void Ban(object state);

        void Disconnect();
        void Disconnect(object state);
    }
}

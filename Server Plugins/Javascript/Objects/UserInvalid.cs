using System;
using System.Collections.Generic;
using System.ComponentModel;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Javascript.Objects
{
    public class UserInvalid : IClient
    {
        public ushort Id {
            get { return ushort.MaxValue; }
        }

        public Guid Guid {
            get { return Guid.Empty; }
        }

        public IServer Server {
            get { return null; }
        }

        public ISocket Socket {
            get { return null; }
        }

        public IMonitor Monitor {
            get { return null; }
        }

        public ClientId ClientId {
            get { return null; }
        }

        public AdminLevel Admin {
            get { return AdminLevel.User; }
            set { }
        }

        public uint Token {
            get { return (uint)base.GetHashCode(); }
        }

        public bool LoggedIn {
            get { return false; }
        }

        public bool Connected {
            get { return false; }
        }

        public bool LocalHost {
            get { return true; }
            set { }
        }

        public bool Browsable {
            get { return false; }
        }

        public bool Muzzled {
            get { return false; }
            set { }
        }

        public bool Cloaked {
            get { return false; }
            set { }
        }

        public bool IsCaptcha {
            get { return false; }
            set { }
        }

        public bool AllowHtml {
            get { return false; }
        }

        public bool FastPing {
            get { return false; }
        }

        public bool Compression {
            get { return false; }
        }

        public bool Encryption {
            get { return false; }
        }

        public byte Age {
            get { return 0; }
        }

        public Gender Gender {
            get { return Gender.Unknown; }
        }

        public Country Country {
            get { return Country.Unknown; }
        }

        public byte[] Avatar {
            get { return null; }
            set { }
        }

        public byte[] OrgAvatar {
            get { return null; }
        }

        public string Name {
            get { return null; }
            set { }
        }

        public string OrgName {
            get { return null; }
        }

        public string Version {
            get { return null; }
        }

        public string Region {
            get { return null; }
        }

        public string Message {
            get { return null; }
            set { }
        }

        public ushort Vroom {
            get { return 0; }
            set { }
        }

        public ushort FileCount {
            get { return 0; }
        }

        public ushort NodePort {
            get { return 0; }
        }

        public ushort ListenPort {
            get { return 0; }
        }

        public System.Net.IPAddress NodeIp {
            get { return null; }
        }

        public System.Net.IPAddress LocalIp {
            get { return null; }
        }

        public System.Net.IPAddress ExternalIp {
            get { return null; }
        }

        public System.Net.IPHostEntry DnsEntry {
            get { return null; }
        }

        public ClientFlags Features {
            get { return ClientFlags.NONE; }
            set { }
        }

        public DateTime JoinTime {
            get { return DateTime.MinValue; }
        }

        public DateTime CaptchaTime {
            get { return DateTime.MinValue; }
        }

        public DateTime LastUpdate {
            get { return DateTime.MinValue; }
            set { }
        }

        public IObservableCollection<string> Ignored {
            get { return null; }
        }

        public IObservableCollection<ISharedFile> Files {
            get { return null; }
        }

        public IDictionary<string, object> Extended {
            get { return null; }
        }

        public IDictionary<byte, IFloodCounter> Counters {
            get { return null; }
        }

        public void SendPacket(IPacket packet) { }

        public void SendPacket(Predicate<IClient> match, IPacket packet) { }

        public void Ban() { }

        public void Ban(object state) { }

        public void Disconnect() { }

        public void Disconnect(object state) { }

        public void Dispose() { }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add { }
            remove { }
        }
    }
}

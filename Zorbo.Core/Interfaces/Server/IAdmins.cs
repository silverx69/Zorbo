using System;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IAdmins<TPassword> : IObservableCollection<IClient> where TPassword : IPassword
    {
        IPasswords<TPassword> Passwords { get; }
    }
}
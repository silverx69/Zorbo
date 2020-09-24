using System;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IAdmins : IObservableCollection<IClient>
    {
        IPasswords Passwords { get; }
    }
}
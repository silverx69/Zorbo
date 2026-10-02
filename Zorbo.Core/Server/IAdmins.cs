using System;

namespace Zorbo.Core.Server
{
    public interface IAdmins : IObservableCollection<IClient>
    {
        IPasswords Passwords { get; }
    }
}
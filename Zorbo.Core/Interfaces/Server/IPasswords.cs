using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IPasswords<TPassword> : 
        IObservableCollection<TPassword>,
        IList<TPassword>,
        ICollection<TPassword>,
        IEnumerable<TPassword> where TPassword : IPassword
    {
        bool Add(IClient client, string password);
        bool Remove(string password);

        IPassword CheckSha1(IClient client, byte[] password);
    }
}

using System.ComponentModel;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IHistory<TPassword> : INotifyPropertyChanged where TPassword : IPassword
    {
        IAdmins<TPassword> Admin { get; }

        IBanned Bans { get; }
        IRangeBanned RangeBans { get; }
        IObservableCollection<Record> Records { get; }

        Record Add(IClient client);
    }
}

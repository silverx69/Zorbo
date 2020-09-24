using System.ComponentModel;

namespace Zorbo.Core.Interfaces.Server
{
    public interface IHistory : INotifyPropertyChanged
    {
        IAdmins Admin { get; }
        IBanned Bans { get; }
        IRangeBanned RangeBans { get; }
        IObservableCollection<Record> Records { get; }

        Record Add(IClient client);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Zorbo.Core.Interfaces
{
    public interface IObservableCollection<T> :
        IList<T>,
        ICollection<T>,
        IEnumerable<T>,
        IObservableCollection
    {
        bool Remove(Predicate<T> search);
        int RemoveAll(Predicate<T> search);

        void Sort(Comparison<T> comparison);
    }

    public interface IObservableCollection : 
        IEnumerable,
        INotifyPropertyChanged,
        INotifyCollectionChanged {
        object SyncRoot { get; }
    }
}

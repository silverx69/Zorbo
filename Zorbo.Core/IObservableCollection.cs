using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Zorbo.Core
{
    public interface IObservableCollection<T> :
        IEnumerable<T>,
        IObservableCollection
    {
        new T this[int index] { get;set; }

        bool Remove(T item);
        bool Remove(Predicate<T> search);

        int RemoveAll(Predicate<T> search);

        void Sort(Comparison<T> comparison);
    }

    public interface IObservableCollection : 
        IList,
        ICollection,
        IEnumerable,
        INotifyPropertyChanged,
        INotifyCollectionChanged {
    }
}

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Zorbo
{
    public interface IObservable : INotifyPropertyChanged
    {
    }

    public interface IObservableCollection : IObservable, ICollection, INotifyCollectionChanged
    {
    }

    public interface IObservableCollection<T> : ICollection<T>, IObservableCollection
    {
    }

    public interface IObservableList<T> : IList<T>, IObservableCollection<T>, IList
    {
        bool Remove(Predicate<T> search);

        int RemoveAll(Predicate<T> search);

        void Sort(Comparison<T> comparison);
    }

    public interface IReadOnlyObservableCollection<T> : IReadOnlyCollection<T>, IObservableCollection
    {

    }

    public interface IReadOnlyObservableList<T> : IReadOnlyList<T>, IReadOnlyObservableCollection<T>
    {
        void Sort(Comparison<T> comparison);
    }
}
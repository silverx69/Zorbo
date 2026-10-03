using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Zorbo.Collections
{
    public class ReadOnlyObservableList<T> : Observable, IReadOnlyObservableList<T>
    {
        public int Count => InnerList.Count;

        public T this[int index] => InnerList[index];

        public Lock SyncRoot => InnerList.SyncRoot;

        object ICollection.SyncRoot => ((ICollection)InnerList).SyncRoot;

        bool ICollection.IsSynchronized => ((ICollection)InnerList).IsSynchronized;


        protected ObservableList<T> InnerList {
            get;
            private set;
        }

        protected ReadOnlyObservableList() {
            InnerList = [];
        }

        public ReadOnlyObservableList(IEnumerable<T> collection) {
            InnerList = [.. collection];
            InnerList.PropertyChanged += InnerList_PropertyChanged;
        }

        public ReadOnlyObservableList(ObservableList<T> innerList) {
            InnerList = innerList ?? throw new ArgumentNullException(nameof(innerList));
            InnerList.PropertyChanged += InnerList_PropertyChanged;
        }

        private void InnerList_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e.PropertyName);
        }

        public void CopyTo(Array array, int index) {
            InnerList.CopyTo(array, index);
        }

        public void Sort() {
            InnerList.Sort();
        }

        public void Sort(Comparison<T> comparison) {
            InnerList.Sort(comparison);
        }

        public IEnumerator<T> GetEnumerator() {
            return InnerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return InnerList.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged {
            add { InnerList.CollectionChanged += value; }
            remove { InnerList.CollectionChanged -= value; }
        }
    }
}

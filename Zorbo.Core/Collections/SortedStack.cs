using System.Collections;

namespace Zorbo.Collections
{
    public sealed class SortedStack<T> : IEnumerable<T>, ICollection
    {
        Comparison<T> comparison;
        readonly ObservableList<T> list;
        
        public int Count {
            get { return list.Count; }
        }

        bool ICollection.IsSynchronized { get { return true; } }

#pragma warning disable CS9216 // A value of type 'System.Threading.Lock' converted to a different type will use likely unintended monitor-based locking in 'lock' statement.
        object ICollection.SyncRoot { get { return list.SyncRoot; } }
#pragma warning restore CS9216 // A value of type 'System.Threading.Lock' converted to a different type will use likely unintended monitor-based locking in 'lock' statement.

        public SortedStack() {
            list = [];
        }

        public T Pop() {
            lock (list.SyncRoot) {
                T ret = list[0];
                list.RemoveAt(0);

                return ret;
            }
        }

        public void Push(T item) {
            lock (list.SyncRoot) {
                list.Add(item);

                if (comparison == null)
                    list.Sort();
                else
                    list.Sort(comparison);
            }
        }


        public void Clear() {
            lock (list.SyncRoot) list.Clear();
        }


        public void SetSort(Comparison<T> comparison) {
            this.comparison = comparison;
            lock (list.SyncRoot) list.Sort(comparison);
        }


        void ICollection.CopyTo(Array array, int index) {
            ((ICollection)list).CopyTo(array, index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return list.GetEnumerator();
        }
    }
}

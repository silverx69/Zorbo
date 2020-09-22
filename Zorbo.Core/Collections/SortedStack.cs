using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core.Collections
{
    public sealed class SortedStack<T> : IEnumerable<T>, ICollection
    {
        readonly List<T> list;
        Comparison<T> comparison;


        public int Count {
            get { return list.Count; }
        }

        bool ICollection.IsSynchronized {
            get { return true; }
        }

        object ICollection.SyncRoot {
            get { return ((ICollection)list).SyncRoot; }
        }

        public SortedStack() {
            list = new List<T>();
        }

        public T Pop() {
            lock (list) {
                T ret = list[0];
                list.RemoveAt(0);

                return ret;
            }
        }

        public void Push(T item) {
            lock (list) {
                list.Add(item);

                if (comparison == null)
                    list.Sort();
                else
                    list.Sort(comparison);
            }
        }


        public void Clear() {
            lock (list) list.Clear();
        }


        public void SetSort(Comparison<T> comparison) {

            this.comparison = comparison;
            lock (list) list.Sort(comparison);
        }


        void ICollection.CopyTo(Array array, int index) {
            ((ICollection)list).CopyTo(array, index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new SafeEnumerator<T>(list);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new SafeEnumerator<T>(list);
        }
    }
}

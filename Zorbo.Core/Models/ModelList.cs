using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zorbo.Core.Collections;
using Zorbo.Core.Interfaces;

namespace Zorbo.Core.Models
{
    [JsonArray]
    public class ModelList<T> : 
        ModelBase, 
        IObservableCollection<T>
    {
        readonly List<T> m_list;

        public T this[int index] {
            get { return m_list[index]; }
            set {
                T item = m_list[index];
                m_list[index] = value;
                OnCollectionChanged(value, item, index);
            }
        }

        public virtual int Count { get => m_list.Count; }

        public virtual bool IsReadOnly { get => false; }

        protected internal object SyncRoot { get; } = new object();


        bool IList.IsFixedSize { get => false; }

        bool ICollection.IsSynchronized { get => false; }

        object ICollection.SyncRoot { get => SyncRoot; }

        object IList.this[int index] {
            get => this[index];
            set { if (value is T v) this[index] = v; }
        }


        public ModelList() {
            m_list = new List<T>();
        }

        public ModelList(int capacity) { 
            m_list = new List<T>(capacity);
        }

        public ModelList(IEnumerable<T> collection) {
            m_list = new List<T>(collection);
        }


        int IList.IndexOf(object value)
        {
            if (value is T v)
                return IndexOf(v);
            return -1;
        }

        public virtual int IndexOf(T item) { lock(SyncRoot) return m_list.IndexOf(item); }


        bool IList.Contains(object value)
        {
            if (value is T v)
                return Contains(v);
            return false;
        }

        public virtual bool Contains(T item) { lock(SyncRoot) return m_list.Contains(item); }


        int IList.Add(object value)
        {
            if (value is T v) 
                Add(v);
            return Count;
        }

        public virtual void Add(T item) {
            lock(SyncRoot) m_list.Add(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        public virtual bool AddIfUnique(T item) {
            lock (SyncRoot) {
                if (m_list.Where(s => s.Equals(item)).Count() > 0)
                    return false;
                m_list.Add(item);
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
            return true;
        }

        public virtual void AddRange(IEnumerable<T> collection) {
            lock (SyncRoot) m_list.AddRange(collection);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, collection);
        }

        void IList.Insert(int index, object value)
        {
            if (value is T v) Insert(index, v);
        }

        public virtual void Insert(int index, T item) {
            lock (SyncRoot) m_list.Insert(index, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        public virtual void InsertRange(int index, IEnumerable<T> collection) {
            lock (SyncRoot) m_list.InsertRange(index, collection);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, collection, index);
        }

        void IList.Remove(object value)
        {
            if (value is T v) Remove(v);
        }

        public virtual bool Remove(T item) {
            int i = IndexOf(item);
            if (i > -1) {
                RemoveAt(i);
                return true;
            }
            return false;
        }

        public virtual bool Remove(Predicate<T> match)
        {
            T item;
            lock (SyncRoot) {
                for (int i = (m_list.Count - 1); i >= 0; i--) {
                    item = m_list[i];
                    if (match(item)) {
                        m_list.RemoveAt(i);
                        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, i);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void RemoveAt(int index) {
            T item;
            lock (SyncRoot) {
                item = m_list[index];
                m_list.RemoveAt(index);
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        public virtual void RemoveRange(int index, int count) {
            T[] tmp = new T[count];
            lock (SyncRoot) {
                CopyToNoLock(index, tmp, 0, count);
                m_list.RemoveRange(index, count);
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, tmp, index);
        }

        public virtual int RemoveAll(Predicate<T> match) {
            List<T> tmp = new List<T>();
            lock (SyncRoot) {
                for (int i = (m_list.Count - 1); i >= 0; i--) {
                    if (match(m_list[i])) {
                        tmp.Add(m_list[i]);
                        m_list.RemoveAt(i);
                    }
                }
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, tmp);
            return tmp.Count;
        }

        public virtual void Clear() {
            lock (SyncRoot) m_list.Clear();
            OnCollectionCleared();
        }

        public virtual void CopyTo(T[] array, int arrayIndex) {
            lock (SyncRoot) m_list.CopyTo(array, arrayIndex);
        }

        public virtual void CopyTo(int index, T[] array, int arrayIndex, int count) {
            lock (SyncRoot) m_list.CopyTo(index, array, arrayIndex, count);
        }

        public virtual void CopyTo(Array array, int index)
        {
            Array.Copy(this.ToArray(), 0, array, index, this.Count);
        }

        protected virtual void CopyToNoLock(int index, T[] array, int arrayIndex, int count) {
            m_list.CopyTo(index, array, arrayIndex, count);
        }

        public void Sort(Comparison<T> comparison)
        {
            lock (SyncRoot) m_list.Sort(comparison);
        }

        public virtual IEnumerator<T> GetEnumerator() {
            return new SafeEnumerator<T>(m_list);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new SafeEnumerator<T>(m_list);
        }

        protected virtual void OnCollectionCleared() {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void OnCollectionChanged(T newitem, T olditem, int index) {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newitem, olditem, index));
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, T item, int index = -1) {
            if (index < 0)
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
            else
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<T> collection, int index = -1) {
            if (index < 0)
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, collection.ToList()));
            else
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, collection.ToList(), index));
            RaisePropertyChanged(nameof(Count));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}

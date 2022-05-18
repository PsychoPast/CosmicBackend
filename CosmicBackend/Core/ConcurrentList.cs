using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CosmicBackend.Core
{
    internal class ConcurrentList<T> : IList<T>, IList
    {
        private readonly object _syncObj;

        private readonly List<T> _innerList;

        internal ConcurrentList()
        {
            _innerList = new List<T>();
            _syncObj = new();
        }

        internal ConcurrentList(IEnumerable<T> collection)
        {
            _innerList = collection != null ? new(collection) : throw new ArgumentNullException(nameof(collection));
            _syncObj = new();
        }

        internal ConcurrentList(int capacity)
        {
            _innerList = new(capacity);
            _syncObj = new();
        }

        public int Count
        {
            get
            {
                lock (_syncObj)
                {
                    return _innerList.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_syncObj)
                {
                    return _innerList[index];
                }
            }
            set
            {
                lock (_syncObj)
                {
                    if (index < 0 || index >= _innerList.Count)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(index),
                            index,
                            $"Value must be between 0 and {_innerList.Count - 1}");

                    }

                    SetItem(index, value);
                }
            }
        }

        public void Add(T item)
        {
            lock (_syncObj)
            {
                int index = _innerList.Count;
                InsertItem(index, item);
            }
        }

        public void Clear()
        {
            lock (_syncObj)
            {
                ClearItems();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (_syncObj)
            {
                _innerList.CopyTo(array, index);
            }
        }

        public bool Contains(T item)
        {
            lock (_syncObj)
            {
                return _innerList.Contains(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_syncObj)
            {
                return _innerList.GetEnumerator();
            }
        }

        public int IndexOf(T item)
        {
            lock (_syncObj)
            {
                return InternalIndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_syncObj)
            {
                if (index < 0 || index > _innerList.Count)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        $"Value must be between 0 and {_innerList.Count}");

                }

                InsertItem(index, item);
            }
        }

        private int InternalIndexOf(T item)
        {
            int count = _innerList.Count;

            for (int i = 0; i < count; i++)
            {
                if (Equals(_innerList[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Remove(T item)
        {
            lock (_syncObj)
            {
                int index = InternalIndexOf(item);
                if (index < 0)
                {
                    return false;
                }

                RemoveItem(index);
                return true;
            }
        }

        public void RemoveAt(int index)
        {
            lock (_syncObj)
            {
                if (index < 0 || index >= _innerList.Count)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        $"Value must be between 0 and {_innerList.Count - 1}");

                }

                RemoveItem(index);
            }
        }

        protected virtual void ClearItems() => _innerList.Clear();

        protected virtual void InsertItem(int index, T item) => _innerList.Insert(index, item);

        protected virtual void RemoveItem(int index) => _innerList.RemoveAt(index);

        protected virtual void SetItem(int index, T item) => _innerList[index] = item;

        bool ICollection<T>.IsReadOnly => false;

        IEnumerator IEnumerable.GetEnumerator() => ((IList)_innerList).GetEnumerator();

        bool ICollection.IsSynchronized => true;

        public object SyncRoot => _syncObj;

        void ICollection.CopyTo(Array array, int index)
        {
            lock (_syncObj)
            {
                ((IList)_innerList).CopyTo(array, index);
            }
        }

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                VerifyValueType(value);
                this[index] = (T)value;
            }
        }

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        int IList.Add(object value)
        {
            VerifyValueType(value);

            lock (_syncObj)
            {
                Add((T)value);
                return Count - 1;
            }
        }

        bool IList.Contains(object value)
        {
            VerifyValueType(value);
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            VerifyValueType(value);
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            VerifyValueType(value);
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            VerifyValueType(value);
            Remove((T)value);
        }

        private static void VerifyValueType(object value)
        {
            if (value == null)
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                {
                    throw new ArgumentException("Type can't be null");
                }
            }
            else if (!(value is T))
            {
                throw new ArgumentException($"Wrong type, {value.GetType().FullName}");
            }
        }
    }
}
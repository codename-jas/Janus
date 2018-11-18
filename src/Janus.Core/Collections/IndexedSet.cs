using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace Janus.Core.Collections
{
    public class IndexedSet<T> : IList<T>, IDisposable
    {

        #region [ Constructor/Destructor/IDisposable ]

        public IndexedSet()
        {
            this._entityLock = new ReaderWriterLockSlim();
            ResizeDataStructures();
            this._count = 0;
        }

        public IndexedSet(IEnumerable<T> initialItems)
        {
            if (initialItems is null)
                throw new ArgumentNullException(nameof(initialItems));
            this._entityLock = new ReaderWriterLockSlim();
            var itemsToAdd = initialItems.Union(new T[] { }).ToArray();
            ResizeDataStructures(itemsToAdd.Length);
            itemsToAdd.Select(x => x.GetHashCode()).ToArray().CopyTo(this._keys, 0);
            itemsToAdd.CopyTo(this._slots, 0);
            this._count = itemsToAdd.Length;
        }

        ~IndexedSet()
        {
            this.Dispose(false);
        }

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed)
                return;
            if (disposing)
                DisposeExplicit();
            DisposeImplicit();

            this.Disposed = true;
        }

        protected virtual void DisposeExplicit() 
        {
            this._keys = null;
            this._slots = null;
        }
        protected virtual void DisposeImplicit()
        {
            if (this._entityLock != null)
                this._entityLock.Dispose();
        }

        #endregion

        #region [ Fields ]

        protected int[] _keys;
        protected T[] _slots;
        protected ReaderWriterLockSlim _entityLock;
        protected int _count;
        public bool Disposed { get; protected set; }

        #endregion

        #region [ Indexers/Properties ]

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new IndexOutOfRangeException("Index cannot be less than Zero!");
                T result;
                this._entityLock.EnterReadLock();
                try
                {
                    result = index < this._count ? this._slots[index] : (default);
                }
                finally
                {
                    this._entityLock.ExitReadLock();
                }
                return result;
            }
            set => Insert(index, value);
        }

        public int Count => this._count;

        public bool IsReadOnly => false;

        #endregion

        #region [ Private Methods ]

        private bool ResizeRequired => !(this._count < this._keys.Length);

        private void ResizeDataStructures(int currentSize = 0)
        {
            var nextSize = Helpers.NumericHelper.GetNextPrime(currentSize);
            if (nextSize > this.Count)
            {
                var tempKeys = new int[nextSize];
                var tempSlots = new T[nextSize];
                if (this.Count > 0)
                {
                    this._keys.CopyTo(tempKeys, 0);
                    this._slots.CopyTo(tempSlots, 0);
                }
                this._keys = tempKeys;
                this._slots = tempSlots;
            }
            else
                throw new OverflowException("Reached maximum capacity, can't add more!");
        }

        private void AddNewItem(T item)
        {
            this._keys[this._count] = item.GetHashCode();
            this._slots[this._count] = item;
            this._count++;
        }

        #endregion

        #region [ IList ]

        public void Add(T item)
        {
            if (!this.Contains(item))
            {
                this._entityLock.EnterWriteLock();
                try
                {
                    if (this.ResizeRequired)
                        ResizeDataStructures(this.Count);
                    this.AddNewItem(item);
                }
                finally
                {
                    this._entityLock.ExitWriteLock();
                }
            }
        }

        public void Clear()
        {
            this._entityLock.EnterWriteLock();
            try
            {
                var minSize = Helpers.NumericHelper.GetNextPrime(0);
                this._keys = new int[minSize];
                this._slots = new T[minSize];
            }
            finally
            {
                this._entityLock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            bool contains;
            this._entityLock.EnterReadLock();
            try
            {
                contains = this._keys.Contains(item.GetHashCode());
            }
            finally
            {
                this._entityLock.ExitReadLock();
            }
            return contains;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._entityLock.EnterWriteLock();
            try
            {
                this._slots.CopyTo(array, arrayIndex);
            }
            finally
            {
                this._entityLock.ExitWriteLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            this._entityLock.EnterReadLock();
            try
            {
                if (this.Count > 0)
                {
                    var counter = 0;
                    while (counter < this.Count)
                        yield return this._slots[counter++];
                }
            }
            finally
            {
                this._entityLock.ExitReadLock();
            }
        }

        public int IndexOf(T item)
        {
            var itemIndex = -1;
            var hashCode = item.GetHashCode();
            this._entityLock.EnterReadLock();
            try
            {
                for (var i = 0; i < this._keys.Length; i++)
                {
                    if (this._keys[i] == hashCode)
                    {
                        itemIndex = i;
                        break;
                    }
                }
            }
            finally
            {
                this._entityLock.ExitReadLock();
            }
            if (itemIndex < 0)
                throw new KeyNotFoundException();
            return itemIndex;
        }

        public void Insert(int index, T item)
        {
            if (index > this.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Connot insert into non-contagious location!");
            if (!this.Contains(item))
            {
                this._entityLock.EnterWriteLock();
                try
                {
                    if (this.ResizeRequired)
                        ResizeDataStructures(this.Count);
                    //where are we trying to add - end or beginning or middle
                    if (index == this._count)
                        this.AddNewItem(item);
                    else
                    {
                        var keysBelow = new Span<int>(new int[this.Count - index]);
                        var slotsBelow = new Span<T>(new T[this.Count - index]);
                        var windowKeysBelow = new Span<int>(this._keys, index, this.Count - index);
                        var windowSlotsBelow = new Span<T>(this._slots, index, this.Count - index);
                        windowKeysBelow.CopyTo(keysBelow);
                        windowSlotsBelow.CopyTo(slotsBelow);
                        this._keys[index] = item.GetHashCode();
                        this._slots[index] = item;
                        keysBelow.ToArray().CopyTo(this._keys, index + 1);
                        slotsBelow.ToArray().CopyTo(this._slots, index + 1);
                    }
                    this._count++;
                }
                finally
                {
                    this._entityLock.ExitWriteLock();
                }
            }
        }

        public bool Remove(T item)
        {
            if (this.Contains(item))
            {
                var index = this.IndexOf(item);
                this.RemoveAt(index);
                return true;
            }
            else
                return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Cannot be less than zero!");
            this._entityLock.EnterUpgradeableReadLock();
            try
            {
                if (index < this.Count)
                {
                    this._entityLock.EnterWriteLock();
                    try
                    {
                        var numItemsToMove = (this.Count - 1) - index;
                        if (numItemsToMove > 0)
                        {
                            var keysBelow = new Span<int>(new int[numItemsToMove]);
                            var slotsBelow = new Span<T>(new T[numItemsToMove]);
                            var windowKeysBelow = new Span<int>(this._keys, index + 1, numItemsToMove);
                            var windowSlotsBelow = new Span<T>(this._slots, index + 1, numItemsToMove);
                            windowKeysBelow.CopyTo(keysBelow);
                            windowSlotsBelow.CopyTo(slotsBelow);
                            keysBelow.ToArray().CopyTo(this._keys, index);
                            slotsBelow.ToArray().CopyTo(this._slots, index);
                        }
                        this._count--;
                        this._keys[this.Count] = 0;
                        this._slots[this.Count] = default;
                    }
                    finally
                    {
                        this._entityLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this._entityLock.ExitUpgradeableReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #endregion
    }
}
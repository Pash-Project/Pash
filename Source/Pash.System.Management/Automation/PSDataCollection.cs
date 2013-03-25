// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Management.Automation
{
    public class PSDataCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable, IDisposable
    {
        private bool isopen;
        private bool isenumerated;
        private IList<T> data;


        public event EventHandler<DataAddedEventArgs> DataAdded
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                throw new NotImplementedException();
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                throw new NotImplementedException();
            }
        }
        public event EventHandler Completed
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                throw new NotImplementedException();
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                throw new NotImplementedException();
            }
        }
        public bool IsOpen
        {
            get
            {
                return this.isopen;
            }
        }
        internal bool ReleaseOnEnumeration
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        internal bool IsEnumerated
        {
            get
            {
                return isenumerated;
            }
            set
            {
                isenumerated = value;
            }
        }
        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                int cnt = 0;

                if (data != null)
                    cnt = data.Count;

                return cnt;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        bool IList.IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        object IList.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        object ICollection.SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public PSDataCollection()
            : this(new List<T>())
        {

        }
        public PSDataCollection(IEnumerable<T> items)
            : this(new List<T>(items))
        {

        }
        public PSDataCollection(int capacity)
            : this(new List<T>(capacity))
        {

        }

        /// <summary>
        /// TODO: .
        /// </summary>
        internal PSDataCollection(IList<T> listToUse)
        {
            isopen = false;
            isenumerated = false;
            data = listToUse;
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PSDataCollectionEnumerator<T>(this);
        }

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PSDataCollectionEnumerator<T>(this);
        }

        public Collection<T> ReadAll()
        {
            throw new NotImplementedException();
        }

        protected virtual void InsertItem(Guid psInstanceId, int index, T item)
        {
            throw new NotImplementedException();
        }

        protected virtual void RemoveItem(int index)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }

    }
}

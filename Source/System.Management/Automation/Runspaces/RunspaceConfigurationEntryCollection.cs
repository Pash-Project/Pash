// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Management.Automation.Runspaces
{
    public sealed class RunspaceConfigurationEntryCollection<T> : IEnumerable<T> where T : RunspaceConfigurationEntry
    {
        private Collection<T> collection = new Collection<T>();

        public RunspaceConfigurationEntryCollection()
        {
            // TODO: implement RunspaceConfigurationEntryCollection
        }

        public RunspaceConfigurationEntryCollection(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                T result;
                try
                {
                    result = this.collection[index];
                }
                finally
                {
                    /* Out of bounds is ignored. */
                }
                return result;
            }
        }

        public void Append(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        public void Append(T item)
        {
            collection.Add(item);
        }

        public void Prepend(IEnumerable<T> items) { throw new NotImplementedException(); }
        public void Prepend(T item) { throw new NotImplementedException(); }

        public void RemoveItem(int index)
        {
            try
            {
                collection.RemoveAt(index);
            }
            catch
            {
                Console.WriteLine("OutofRange - {0}", index);
            }
        }

        public void RemoveItem(int index, int count)
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    collection.RemoveAt(index);
                }
            }
            catch
            {
            }
        }

        public void Reset()
        {
            for(int i =0; i < collection.Count; i++)
                collection.RemoveAt(0);
        }

        public void Update() { throw new NotImplementedException(); }


        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        #endregion
    }
}

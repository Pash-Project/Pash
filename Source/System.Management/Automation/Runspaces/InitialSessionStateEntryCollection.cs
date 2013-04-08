// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
    public sealed class InitialSessionStateEntryCollection<T> : IEnumerable<T>, IEnumerable where T : InitialSessionStateEntry
    {
        private Collection<T> collection;

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
                return collection[index];
            }
        }

        public Collection<T> this[string name]
        {
            get
            {
                Collection<T> collection = new Collection<T>();

                foreach (T current in this.collection)
                {
                    if (current.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        collection.Add(current);
                    }
                }

                return collection;
            }
        }

        public InitialSessionStateEntryCollection()
        {
            collection = new Collection<T>();
        }

        public InitialSessionStateEntryCollection(IEnumerable<T> items)
        {
            this.collection = new Collection<T>();
            foreach (T current in items)
            {
                this.collection.Add(current);
            }

        }

        public InitialSessionStateEntryCollection<T> Clone()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            collection.Clear();
        }

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

        public void Clear()
        {
            collection.Clear();
        }

        public void Remove(string name, object type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Add(T item)
        {
            this.collection.Add(item);
        }

        /// <summary>
        /// .
        /// </summary>
        public void Add(IEnumerable<T> items)
        {
            foreach (T current in items)
            {
                this.collection.Add(current);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }
    }
}

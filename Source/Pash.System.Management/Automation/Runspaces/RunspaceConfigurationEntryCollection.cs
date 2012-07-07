using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Management.Automation.Runspaces
{
    public sealed class RunspaceConfigurationEntryCollection<T> : IEnumerable<T> where T : RunspaceConfigurationEntry
    {
        public RunspaceConfigurationEntryCollection()
        {
            // TODO: implement RunspaceConfigurationEntryCollection
        }
        public RunspaceConfigurationEntryCollection(IEnumerable<T> items) { throw new NotImplementedException(); }

        public int Count { get { throw new NotImplementedException(); } }

        public T this[int index] { get { throw new NotImplementedException(); } }

        public void Append(IEnumerable<T> items) { throw new NotImplementedException(); }
        public void Append(T item) { throw new NotImplementedException(); }
        public void Prepend(IEnumerable<T> items) { throw new NotImplementedException(); }
        public void Prepend(T item) { throw new NotImplementedException(); }
        public void RemoveItem(int index) { throw new NotImplementedException(); }
        public void RemoveItem(int index, int count) { throw new NotImplementedException(); }
        public void Reset() { throw new NotImplementedException(); }
        public void Update() { throw new NotImplementedException(); }

        // internals
        //internal void AddBuiltInItem(System.Collections.Generic.IEnumerable<T> items);
        //internal void AddBuiltInItem(T item);
        //internal void RemovePSSnapIn(string PSSnapinName);
        //internal void Update(bool force);
        //internal System.Collections.ObjectModel.Collection<T> UpdateList { get; }
        //internal event System.Management.Automation.Runspaces.RunspaceConfigurationEntryUpdateEventHandler OnUpdate;

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

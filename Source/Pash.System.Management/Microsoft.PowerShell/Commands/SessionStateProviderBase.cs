using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Provider;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class SessionStateProviderBase : ContainerCmdletProvider, IContentCmdletProvider
    {
        protected SessionStateProviderBase()
        {
        }

        protected override void ClearItem(string path) { throw new NotImplementedException(); }
        protected override void CopyItem(string path, string copyPath, bool recurse) { throw new NotImplementedException(); }

        protected override void GetChildItems(string path, bool recurse)
        {
            if (path == "\\")
                path = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                IDictionary sessionStateTable = GetSessionStateTable();

                foreach (DictionaryEntry entry in sessionStateTable)
                {
                    WriteItemObject(entry.Value, (string)entry.Key, false);
                }
            }
            else
            {
                object item = GetSessionStateItem(path);

                if (item != null)
                {
                    WriteItemObject(item, path, false);
                }
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers) { throw new NotImplementedException(); }
        protected override void GetItem(string name) { throw new NotImplementedException(); }
        protected override bool HasChildItems(string path) { throw new NotImplementedException(); }
        protected override bool IsValidPath(string path) { throw new NotImplementedException(); }

        protected override bool ItemExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            return null != GetSessionStateItem(path);
        }

        protected override void NewItem(string path, string type, object newItem) { throw new NotImplementedException(); }
        protected override void RemoveItem(string path, bool recurse) { throw new NotImplementedException(); }
        protected override void RenameItem(string name, string newName) { throw new NotImplementedException(); }
        protected override void SetItem(string name, object value) { throw new NotImplementedException(); }

        // internals
        internal virtual bool CanRenameItem(object item)
        {
            return true;
        }

        internal abstract object GetSessionStateItem(string name);
        internal abstract IDictionary GetSessionStateTable();
        internal virtual object GetValueOfItem(object item)
        {
            if (item is DictionaryEntry)
            {
                return ((DictionaryEntry) item).Value;
            }
            return item;
        }
        internal abstract void RemoveSessionStateItem(string name);
        internal abstract void SetSessionStateItem(string name, object value, bool writeItem);

        #region IContentCmdletProvider Members

        public void ClearContent(string path)
        {
            throw new NotImplementedException();
        }

        public object ClearContentDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        public IContentReader GetContentReader(string path)
        {
            throw new NotImplementedException();
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        public IContentWriter GetContentWriter(string path)
        {
            throw new NotImplementedException();
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

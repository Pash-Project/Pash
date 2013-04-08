// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Provider;
using System.Management.Automation;
using System.Management;

namespace Microsoft.PowerShell.Commands
{
    public abstract class SessionStateProviderBase : ContainerCmdletProvider, IContentCmdletProvider
    {
        protected SessionStateProviderBase()
        {
        }

        protected override void ClearItem(Path path) { throw new NotImplementedException(); }
        protected override void CopyItem(Path path, Path copyPath, bool recurse) { throw new NotImplementedException(); }

        protected override void GetChildItems(Path path, bool recurse)
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

        protected override void GetChildNames(Path path, ReturnContainers returnContainers) { throw new NotImplementedException(); }
        protected override void GetItem(Path name) { throw new NotImplementedException(); }
        protected override bool HasChildItems(Path path) { throw new NotImplementedException(); }
        protected override bool IsValidPath(Path path) { throw new NotImplementedException(); }

        protected override bool ItemExists(Path path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            return null != GetSessionStateItem(path);
        }

        protected override void NewItem(Path path, string type, object newItem) { throw new NotImplementedException(); }
        protected override void RemoveItem(Path path, bool recurse) { throw new NotImplementedException(); }
        protected override void RenameItem(Path name, Path newName) { throw new NotImplementedException(); }
        protected override void SetItem(Path name, object value) { throw new NotImplementedException(); }

        // internals
        internal virtual bool CanRenameItem(object item)
        {
            return true;
        }

        internal abstract object GetSessionStateItem(Path name);
        internal abstract IDictionary GetSessionStateTable();
        internal virtual object GetValueOfItem(object item)
        {
            if (item is DictionaryEntry)
            {
                return ((DictionaryEntry)item).Value;
            }
            return item;
        }
        internal abstract void RemoveSessionStateItem(Path name);
        internal abstract void SetSessionStateItem(Path name, object value, bool writeItem);

        #region IContentCmdletProvider Members

        public void ClearContent(Path path)
        {
            throw new NotImplementedException();
        }

        public object ClearContentDynamicParameters(Path path)
        {
            throw new NotImplementedException();
        }

        public IContentReader GetContentReader(Path path)
        {
            throw new NotImplementedException();
        }

        public object GetContentReaderDynamicParameters(Path path)
        {
            throw new NotImplementedException();
        }

        public IContentWriter GetContentWriter(Path path)
        {
            throw new NotImplementedException();
        }

        public object GetContentWriterDynamicParameters(Path path)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

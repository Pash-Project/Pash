// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
    /// <summary>
    /// Exposes functionality to cmdlets from providers.
    /// </summary>
    public sealed class ContentCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal ContentCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public void Clear(string path)
        {
            IContentCmdletProvider provider = GetContentCmdletProvider(path);
            provider.ClearContent(path);
        }

        private IContentCmdletProvider GetContentCmdletProvider(string path)
        {
            PSDriveInfo drive;
            var provider = _cmdlet.State.SessionStateGlobal.GetProviderByPath(path, out drive) as IContentCmdletProvider;
            if (provider != null)
            {
                return provider;
            }

            throw new PSInvalidOperationException(String.Format("The provider for path '{0}' is not a IContentCmdletProvider", path));
        }

        public Collection<IContentReader> GetReader(string path)
        {
            IContentCmdletProvider provider = GetContentCmdletProvider(path);
            var readers = new Collection<IContentReader>();
            readers.Add(provider.GetContentReader(path));
            return readers;
       }

        public Collection<IContentWriter> GetWriter(string path) { throw new NotImplementedException(); }
    }
}

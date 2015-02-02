// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using Pash.Implementation;

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
            string providerPath = GetProviderPath(path);
            provider.ClearContent(providerPath);
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

        private string GetProviderPath(string path)
        {
            if (path.IndexOf("::") == -1)
            {
                return path;
            }

            PSDriveInfo drive;
            ProviderInfo providerInfo;
            var globber = new PathGlobber(_cmdlet.ExecutionContext.SessionState);
            return globber.GetProviderSpecificPath(path, out providerInfo, out drive);
        }

        public Collection<IContentReader> GetReader(string path)
        {
            IContentCmdletProvider provider = GetContentCmdletProvider(path);
            var readers = new Collection<IContentReader>();
            string providerPath = GetProviderPath(path);
            readers.Add(provider.GetContentReader(providerPath));
            return readers;
       }

        public Collection<IContentWriter> GetWriter(string path) { throw new NotImplementedException(); }
    }
}

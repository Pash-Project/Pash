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
    public sealed class ContentCmdletProviderIntrinsics : CmdletProviderIntrinsicsBase
    {

        internal ContentCmdletProviderIntrinsics(Cmdlet cmdlet) : base (cmdlet)
        {
        }

        internal ContentCmdletProviderIntrinsics(SessionState sessionState) : base (sessionState)
        {
        }

        #region public API

        public void Clear(string path)
        {
            Clear(new [] { path }, false, false);
        }

        public void Clear(string[] path, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            Clear(path, runtime);
        }

        public Collection<IContentReader> GetReader(string path)
        {
            return GetReader(new [] { path }, false, false);
        }

        public Collection<IContentReader> GetReader(string[] path, bool force, bool literalPath)
        {
            var readers = new Collection<IContentReader>();
            foreach (var p in path)
            {
                IContentCmdletProvider provider = GetContentCmdletProvider(p);
                string providerPath = GetProviderPath(p);
                readers.Add(provider.GetContentReader(providerPath));
            }
            return readers;
       }

        public Collection<IContentWriter> GetWriter(string path)
        {
            return GetWriter(new [] { path }, false, false);
        }

        public Collection<IContentWriter> GetWriter(string[] path, bool force, bool literalPath)
        {
            var writers = new Collection<IContentWriter>();
            foreach (var p in path)
            {
                IContentCmdletProvider provider = GetContentCmdletProvider(p);
                string providerPath = GetProviderPath(p);
                writers.Add(provider.GetContentWriter(providerPath));
            }
            return writers;
        }

        #endregion

        #region internal API

        internal void Clear(string[] path, ProviderRuntime runtime)
        {
            GlobAndInvoke<IContentCmdletProvider>(path, runtime,
                (curPath, provider) => provider.ClearContent(curPath)
            );
        }


        #endregion

        #region private helpers

        private IContentCmdletProvider GetContentCmdletProvider(string path)
        {
            ProviderInfo providerInfo;
            var globber = new PathGlobber(SessionState);
            globber.GetProviderSpecificPath(path, new ProviderRuntime(SessionState), out providerInfo);
            var provider = SessionState.Provider.GetInstance(providerInfo) as IContentCmdletProvider;
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

            ProviderInfo providerInfo;
            var globber = new PathGlobber(SessionState);
            return globber.GetProviderSpecificPath(path, new ProviderRuntime(SessionState), out providerInfo);
        }

        #endregion
    }
}

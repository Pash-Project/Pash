// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using Pash.Implementation;
using System.Collections.Generic;

namespace System.Management.Automation
{
    /// <summary>
    /// Exposes functionality to cmdlets from providers.
    /// </summary>
    public sealed class ContentCmdletProviderIntrinsics : CmdletProviderIntrinsicsBase
    {
        private ItemCmdletProviderIntrinsics Item
        {
            get
            {
                return new ItemCmdletProviderIntrinsics(InvokingCmdlet);
            }
        }

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
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            return GetReader(path, runtime);
       }

        public Collection<IContentWriter> GetWriter(string path)
        {
            return GetWriter(new [] { path }, false, false);
        }

        public Collection<IContentWriter> GetWriter(string[] path, bool force, bool literalPath)
        {
            var runtime = new ProviderRuntime(SessionState, force, literalPath);
            return GetWriter(path, runtime);
        }

        #endregion

        #region internal API

        internal void Clear(string[] path, ProviderRuntime runtime)
        {
            foreach (var curPath in path)
            {
                CmdletProvider provider;
                var globbedPaths = Globber.GetGlobbedProviderPaths(curPath, runtime, out provider);
                var contentProvider = CmdletProvider.As<IContentCmdletProvider>(provider);
                provider.ProviderRuntime = runtime; // make sure the runtime is set!
                foreach (var p in globbedPaths)
                {
                    try
                    {
                        if (Item.Exists(p, runtime))
                        {
                            contentProvider.ClearContent(p);
                        }
                    }
                    catch (Exception e)
                    {
                        HandleCmdletProviderInvocationException(e);
                    }
                }
            }
        }

        internal Collection<IContentReader> GetReader(string[] path, ProviderRuntime runtime)
        {
            return GlobAndCollect<IContentReader>(path, runtime,
                (curPath, provider) => provider.GetContentReader(curPath)
            );
        }

        internal Collection<IContentWriter> GetWriter(string[] path, ProviderRuntime runtime)
        {
            return GlobAndCollect<IContentWriter>(path, runtime,
                (curPath, provider) => provider.GetContentWriter(curPath)
            );
        }

        #endregion

        #region private helpers

        private Collection<T> GlobAndCollect<T>(IList<string> paths, ProviderRuntime runtime,
            Func<string, IContentCmdletProvider, T> method)
        {
            var returnCollection = new Collection<T>();
            foreach (var curPath in paths)
            {
                CmdletProvider provider;
                var globbedPaths = Globber.GetGlobbedProviderPaths(curPath, runtime, out provider);
                var contentProvider = CmdletProvider.As<IContentCmdletProvider>(provider);
                provider.ProviderRuntime = runtime; // make sure the runtime is set
                foreach (var p in globbedPaths)
                {
                    try
                    {
                        returnCollection.Add(method(p, contentProvider));
                    }
                    catch (Exception e)
                    {
                        HandleCmdletProviderInvocationException(e);
                    }
                }
            }
            return returnCollection;
        }

        #endregion
    }
}

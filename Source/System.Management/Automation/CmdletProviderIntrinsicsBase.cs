using System;
using System.Management.Automation.Internal;
using Pash.Implementation;
using System.Management.Automation.Provider;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public class CmdletProviderIntrinsicsBase
    {
        internal Cmdlet InvokingCmdlet { get; private set; }
        internal SessionState SessionState { get; private set; }
        internal PathIntrinsics Path { get; private set; }
        internal PathGlobber Globber { get; private set; }

        internal CmdletProviderIntrinsicsBase(SessionState sessionState)
        {
            SessionState = sessionState;
            Path = new PathIntrinsics(sessionState);
            Globber = new PathGlobber(sessionState);
        }

        internal CmdletProviderIntrinsicsBase(Cmdlet cmdlet) : this(cmdlet.ExecutionContext.SessionState)
        {
            InvokingCmdlet = cmdlet;
        }

        internal void GlobAndInvoke<T>(IList<string> paths, ProviderRuntime runtime, Action<string, T> method) where T : CmdletProvider
        {
            foreach (var curPath in paths)
            {
                CmdletProvider provider;
                var globbedPaths = Globber.GetGlobbedProviderPaths(curPath, runtime, out provider);
                var itemProvider = CmdletProvider.As<T>(provider);
                foreach (var p in globbedPaths)
                {
                    try
                    {
                        method(p, itemProvider);
                    }
                    catch (Exception e)
                    {
                        HandleCmdletProviderInvocationException(e);
                    }
                }
            }
        }

        internal void HandleCmdletProviderInvocationException(Exception e)
        {
            // Whenever we call some function of a provider, we should check for exceptions
            // and handle them here in a unique way.
            // For now: Only check if we already deal with a CmdletProviderInvocationException or create one
            if ((e is CmdletProviderInvocationException) || (e is CmdletInvocationException))
            {
                throw e;
            }
            throw new CmdletProviderInvocationException(e.Message, e);
        }
    }
}


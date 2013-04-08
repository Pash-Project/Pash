// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Host;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
    public sealed class RunspacePool : IDisposable
    {
        public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public Guid InstanceId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public bool IsDisposed
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public RunspacePoolStateInfo RunspacePoolStateInfo
        {
            get
            {
                throw new NotImplementedException(); ;
            }
        }
        public InitialSessionState InitialSessionState
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public PSThreadOptions ThreadOptions
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public ApartmentState ApartmentState
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal RunspacePool(int minRunspaces, int maxRunspaces, RunspaceConfiguration runspaceConfiguration, PSHost host)
        {
            throw new NotImplementedException();
        }

        internal RunspacePool(int minRunspaces, int maxRunspaces, InitialSessionState initialSessionState, PSHost host)
        {
            throw new NotImplementedException();
        }

        internal RunspacePool(int minRunspaces, int maxRunspaces, TypeTable typeTable, PSHost host, PSPrimitiveDictionary applicationArguments, RunspaceConnectionInfo connectionInfo)
        {
            throw new NotImplementedException();
        }

        public bool SetMaxRunspaces(int maxRunspaces)
        {
            throw new NotImplementedException();
        }

        public int GetMaxRunspaces()
        {
            throw new NotImplementedException();
        }

        public bool SetMinRunspaces(int minRunspaces)
        {
            throw new NotImplementedException();
        }
        public int GetMinRunspaces()
        {
            throw new NotImplementedException();
        }

        public int GetAvailableRunspaces()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }
        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndOpen(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndClose(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public PSPrimitiveDictionary GetApplicationPrivateData()
        {
            throw new NotImplementedException();
        }
    }
}

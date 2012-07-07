using System;
using System.Management.Automation.Host;

namespace System.Management.Automation
{
    public class EngineIntrinsics
    {
        public PSHost Host { get { throw new NotImplementedException(); } }
        public CommandInvocationIntrinsics InvokeCommand { get { throw new NotImplementedException(); } }
        public ProviderIntrinsics InvokeProvider { get { throw new NotImplementedException(); } }
        public SessionState SessionState { get { throw new NotImplementedException(); } }

        // internals
        //internal EngineIntrinsics(ExecutionContext context) { throw new NotImplementedException(); }
    }
}

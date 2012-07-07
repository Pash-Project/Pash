using System;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class SessionState
    {
        internal SessionStateGlobal SessionStateGlobal { get; private set; }

        public DriveManagementIntrinsics Drive { get; private set; }
        public PathIntrinsics Path { get; private set; }
        public CmdletProviderManagementIntrinsics Provider { get; private set; }
        public PSVariableIntrinsics PSVariable { get; private set; }

        // internal
        internal SessionState(SessionStateGlobal sessionState)
        {
            SessionStateGlobal = sessionState;
            Drive = new DriveManagementIntrinsics(sessionState);
            Path = new PathIntrinsics(sessionState);
            Provider = new CmdletProviderManagementIntrinsics(sessionState);
            PSVariable = new PSVariableIntrinsics(sessionState);
        }

        internal SessionState(SessionState sessionState)
        {
            SessionStateGlobal = sessionState.SessionStateGlobal;
            Drive = sessionState.Drive;
            Path = sessionState.Path;
            Provider = sessionState.Provider;
            PSVariable = sessionState.PSVariable;
        }

        //internal SessionStateInternal Internal { get; };
    }
}

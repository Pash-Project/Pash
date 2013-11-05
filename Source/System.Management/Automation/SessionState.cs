// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;

namespace System.Management.Automation
{
    public sealed class SessionState
    {
        internal SessionStateGlobal SessionStateGlobal { get; private set; }
        internal SessionStateScope SessionStateScope { get; private set; }

        public DriveManagementIntrinsics Drive { get; private set; }
        public PathIntrinsics Path { get; private set; }
        public CmdletProviderManagementIntrinsics Provider { get; private set; }
        public PSVariableIntrinsics PSVariable { get; private set; }

        // creates a session state with a new (glovbal) scope
        internal SessionState(SessionStateGlobal sessionStateGlobal)
               : this(sessionStateGlobal, null)
        {
            defaultInit();
        }

        // creates a new scope and sets the parent session state's scope as predecessor        
        internal SessionState(SessionState parentSession)
               : this(parentSession.SessionStateGlobal, parentSession.SessionStateScope)
        {
        }

        //actual constructor work, but hidden to not be used accidently in a stupid way
        private SessionState(SessionStateGlobal sessionStateGlobal, SessionStateScope parent) {
            SessionStateGlobal = sessionStateGlobal;
            SessionStateScope = (parent == null) ? new SessionStateScope(sessionStateGlobal) :
                                                   new SessionStateScope(parent);
            Drive = new DriveManagementIntrinsics(SessionStateScope);
            Path = new PathIntrinsics(SessionStateGlobal);
            Provider = new CmdletProviderManagementIntrinsics(SessionStateGlobal);
            PSVariable = new PSVariableIntrinsics(SessionStateScope);
        }


        private void defaultInit()
        {
            PSVariable.Set("true", true);
            PSVariable.Set("false", false);
            PSVariable.Set("null", null);
        }
    }
}

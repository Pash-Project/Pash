// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public sealed class SessionState
    {
        private SessionStateScope<AliasInfo> _aliasScope;
        private SessionStateScope<FunctionInfo> _functionScope;
        private SessionStateScope<PSVariable> _variableScope;
        private SessionStateScope<PSDriveInfo> _driveScope;
        private bool _isScriptScope;

        internal SessionStateGlobal SessionStateGlobal { get; private set; }
        internal bool IsScriptScope
        {
            get { return _isScriptScope; }
            set
            {
                _isScriptScope = value;
                _aliasScope.IsScriptScope = value;
                _functionScope.IsScriptScope = value;
                _variableScope.IsScriptScope = value;
                _driveScope.IsScriptScope = value;
            }
        }

        internal AliasIntrinsics Alias { get; private set; }
        internal FunctionIntrinsics Function { get; private set; }


        public DriveManagementIntrinsics Drive { get; private set; }
        public PathIntrinsics Path { get; private set; }
        public CmdletProviderManagementIntrinsics Provider { get; private set; }
        public PSVariableIntrinsics PSVariable { get; private set; }

        // creates a session state with a new (glovbal) scope
        internal SessionState(SessionStateGlobal sessionStateGlobal)
               : this(sessionStateGlobal, null, null, null, null)
        {
            defaultInit();
        }

        // creates a new scope and sets the parent session state's scope as predecessor        
        internal SessionState(SessionState parentSession)
               : this(parentSession.SessionStateGlobal, parentSession._functionScope,
                      parentSession._variableScope, parentSession._driveScope, parentSession._aliasScope)
        {
        }

        //actual constructor work, but hidden to not be used accidently in a stupid way
        private SessionState(SessionStateGlobal sessionStateGlobal, SessionStateScope<FunctionInfo> functions,
                             SessionStateScope<PSVariable> variables, SessionStateScope<PSDriveInfo> drives,
                             SessionStateScope<AliasInfo> aliases) {
            SessionStateGlobal = sessionStateGlobal;

            _aliasScope = new SessionStateScope<AliasInfo>(aliases, SessionStateCategory.Alias);
            _functionScope = new SessionStateScope<FunctionInfo>(functions, SessionStateCategory.Function);
            _variableScope = new SessionStateScope<PSVariable>(variables, SessionStateCategory.Variable);
            _driveScope = new SessionStateScope<PSDriveInfo>(drives, SessionStateCategory.Drive);

            IsScriptScope = false;
            Function = new FunctionIntrinsics(this, _functionScope);
            Alias = new AliasIntrinsics(this, _aliasScope);

            Drive = new DriveManagementIntrinsics(this, _driveScope);
            Path = new PathIntrinsics(SessionStateGlobal);
            Provider = new CmdletProviderManagementIntrinsics(SessionStateGlobal);
            PSVariable = new PSVariableIntrinsics(this, _variableScope);
        }


        private void defaultInit()
        {
            PSVariable.Set(new PSVariable("true", true, ScopedItemOptions.Constant));
            PSVariable.Set(new PSVariable("false", false, ScopedItemOptions.Constant));
            PSVariable.Set(new PSVariable("null", null, ScopedItemOptions.Constant));
            PSVariable.Set(new PSVariable("Error", new Collection<ErrorRecord>(), ScopedItemOptions.Constant));
        }
    }
}

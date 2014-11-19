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
        internal ModuleIntrinsics LoadedModules { get; private set; }
        // CmdletIntrinsics Cmdlet

        public PSModuleInfo Module { get; private set; }

        public DriveManagementIntrinsics Drive { get; private set; }
        public PathIntrinsics Path { get; private set; }
        public CmdletProviderManagementIntrinsics Provider { get; private set; }
        public PSVariableIntrinsics PSVariable { get; private set; }

        // creates a session state with a new (global) scope
        internal SessionState(SessionStateGlobal sessionStateGlobal)
               : this(sessionStateGlobal, null)
        {
        }

        // creates a new scope and sets the parent session state's scope as predecessor        
        internal SessionState(SessionState parentSession)
               : this(parentSession.SessionStateGlobal, parentSession)
        {
        }

        //actual constructor work, but hidden to not be used accidently in a stupid way
        private SessionState(SessionStateGlobal sessionStateGlobal, SessionState parent) {
            SessionStateGlobal = sessionStateGlobal;

            var parentAliasScope = parent == null ? null : parent._aliasScope;
            var parentFunctionScope = parent == null ? null : parent._functionScope;
            var parentVariableScope = parent == null ? null : parent._variableScope;
            var parentDriveScope = parent == null ? null : parent._driveScope;

            _aliasScope = new SessionStateScope<AliasInfo>(parentAliasScope, SessionStateCategory.Alias);
            _functionScope = new SessionStateScope<FunctionInfo>(parentFunctionScope, SessionStateCategory.Function);
            _variableScope = new SessionStateScope<PSVariable>(parentVariableScope, SessionStateCategory.Variable);
            _driveScope = new SessionStateScope<PSDriveInfo>(parentDriveScope, SessionStateCategory.Drive);

            IsScriptScope = false;
            Function = new FunctionIntrinsics(_functionScope);
            Alias = new AliasIntrinsics(_aliasScope);

            Drive = new DriveManagementIntrinsics(this, _driveScope);
            Path = new PathIntrinsics(SessionStateGlobal);
            Provider = new CmdletProviderManagementIntrinsics(SessionStateGlobal);
            PSVariable = new PSVariableIntrinsics(_variableScope);
            LoadedModules = new ModuleIntrinsics(this);
        }

        internal void SetModule(PSModuleInfo module)
        {
            Module = module;
        }
    }
}

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
        private SessionStateScope<PSModuleInfo> _moduleScope;
        private SessionStateScope<CmdletInfo> _cmdletScope;

        internal SessionStateGlobal SessionStateGlobal { get; private set; }
        internal bool IsScriptScope { get; set; }

        internal AliasIntrinsics Alias { get; private set; }
        internal FunctionIntrinsics Function { get; private set; }
        internal ModuleIntrinsics LoadedModules { get; private set; }
        internal CmdletIntrinsics Cmdlet { get; private set; }

        public PSModuleInfo Module { get; private set; }

        public DriveManagementIntrinsics Drive { get; private set; }
        public PathIntrinsics Path { get; private set; }
        public PSVariableIntrinsics PSVariable { get; private set; }
        public CmdletProviderManagementIntrinsics Provider
        {
            get
            {
                return SessionStateGlobal.Provider;
            }
        }


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
            var parentModuleScope = parent == null ? null : parent._moduleScope;
            var parentCmdletScope = parent == null ? null : parent._cmdletScope;

            _aliasScope = new SessionStateScope<AliasInfo>(this, parentAliasScope, SessionStateCategory.Alias);
            _functionScope = new SessionStateScope<FunctionInfo>(this, parentFunctionScope, SessionStateCategory.Function);
            _variableScope = new SessionStateScope<PSVariable>(this, parentVariableScope, SessionStateCategory.Variable);
            _driveScope = new SessionStateScope<PSDriveInfo>(this, parentDriveScope, SessionStateCategory.Drive);
            _moduleScope = new SessionStateScope<PSModuleInfo>(this, parentModuleScope, SessionStateCategory.Module);
            _cmdletScope = new SessionStateScope<CmdletInfo>(this, parentCmdletScope, SessionStateCategory.Cmdlet);

            IsScriptScope = false;
            Function = new FunctionIntrinsics(_functionScope);
            Alias = new AliasIntrinsics(_aliasScope);

            Drive = new DriveManagementIntrinsics(_driveScope);
            Path = new PathIntrinsics(this);
            PSVariable = new PSVariableIntrinsics(_variableScope);
            LoadedModules = new ModuleIntrinsics(_moduleScope);
            Cmdlet = new CmdletIntrinsics(_cmdletScope);
        }

        internal void SetModule(PSModuleInfo module)
        {
            Module = module;
        }
    }
}

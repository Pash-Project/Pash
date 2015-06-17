// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Host;
using Pash.Implementation;

namespace System.Management.Automation
{
    public class EngineIntrinsics
    {
        private readonly ExecutionContext _executionContext;

        public PSHost Host
        {
            get
            {
                return _executionContext.LocalHost;
            }
        }

        public CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                // maybe we need a cmdlet reference or create a new one?
                throw new NotImplementedException();
            }
        }

        public ProviderIntrinsics InvokeProvider
        {
            get
            {
                // I guess we need a cmdlet reference
                throw new NotImplementedException();
            }
        }

        public SessionState SessionState
        {
            get
            {
                return _executionContext.SessionState;
            }
        }

        internal EngineIntrinsics(ExecutionContext context)
        {
            _executionContext = context;
        }
    }
}

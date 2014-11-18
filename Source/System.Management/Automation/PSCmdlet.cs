// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Host;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public class PSCmdlet : Cmdlet
    {
        protected PSCmdlet() { }

        public string ParameterSetName { get; internal set; }

        public PSHost Host
        {
            get
            {
                return PSHostInternal;
            }
        }

        private CommandInvocationIntrinsics _invokeCommand;

        public CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                if (_invokeCommand == null)
                {
                    _invokeCommand = new CommandInvocationIntrinsics(ExecutionContext, (PipelineCommandRuntime)CommandRuntime);
                }
                return _invokeCommand;
            }
        }

        private ProviderIntrinsics _invokeProvider;
        public ProviderIntrinsics InvokeProvider
        {
            get
            {
                if (_invokeProvider == null)
                {
                    _invokeProvider = new ProviderIntrinsics(this);
                }
                return _invokeProvider;

            }
        }

        public InvocationInfo MyInvocation
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SessionState SessionState
        {
            get
            {
                return State;
            }
        }

        public PathInfo CurrentProviderLocation(string providerId)
        {
            if (string.IsNullOrEmpty(providerId))
                throw new NullReferenceException("ProviderID can't be null");

            return SessionState.Path.CurrentProviderLocation(providerId);
        }

        public Collection<string> GetResolvedProviderPathFromPSPath(string path, out ProviderInfo provider)
        {
            return SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);
        }

        public string GetUnresolvedProviderPathFromPSPath(string path)
        {
            return SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
        }

        public object GetVariableValue(string name)
        {
            return SessionState.PSVariable.GetValue(name);
        }

        public object GetVariableValue(string name, object defaultValue)
        {
            return SessionState.PSVariable.GetValue(name, defaultValue);
        }

        // internal bool HasDynamicParameters { get; private set; }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections.ObjectModel;
using System.Management.Automation;
using Pash.Implementation;

namespace Pash.Implementation
{
    internal class ProviderRuntime
    {
        internal Collection<string> Include { get; set; }
        internal Collection<string> Exclude { get; set; }
        internal string Filter { get; set; }
        internal SwitchParameter Force { get; set; }
        internal ExecutionContext ExecutionContext { get; private set; }
        internal PSCredential Credential { get; set; }
        internal SwitchParameter AvoidWildcardExpansion { get; set; }
        internal PSDriveInfo PSDriveInfo { get; set; }

        internal bool PassThru { get; set; }

        private Cmdlet _cmdlet;
        private Collection<PSObject> _outputData;
        private Collection<ErrorRecord> _errorData;

        private ProviderRuntime()
        {
            _outputData = new Collection<PSObject>();
            _errorData = new Collection<ErrorRecord>();
            PassThru = true;
        }

        internal ProviderRuntime(Cmdlet cmdlet)
            : this(cmdlet.ExecutionContext)
        {
            _cmdlet = cmdlet;
        }

        internal ProviderRuntime(ExecutionContext executionContext)
            : this(executionContext, false, false)
        {
        }

        internal ProviderRuntime(ExecutionContext executionContext, bool force, bool avoidWildcardExpansion)
            : this()
        {
            ExecutionContext = executionContext;
            AvoidWildcardExpansion = avoidWildcardExpansion;
            Force = force;
        }

        internal Collection<PSObject> RetreiveAllProviderData()
        {
            Collection<PSObject> output = _outputData;
            _outputData = new Collection<PSObject>();
            return output;
        }

        internal void WriteObject(object obj)
        {
            if (_cmdlet != null && PassThru)
            {
                _cmdlet.WriteObject(obj);
            }
            else
            {
                _outputData.Add(PSObject.AsPSObject(obj));
            }
        }

        internal void WriteError(ErrorRecord errorRecord)
        {
            // Don't check PassThru here. Cmdlets that don't want to see output are likely to see the errors anyway.
            // Introduce another flag if something similar is needed at som time
            if (_cmdlet != null)
            {
                _cmdlet.WriteError(errorRecord);
            }
            else
            {
                _errorData.Add(errorRecord);
            }
        }

        public void ThrowFirstErrorOrContinue()
        {
            if (_errorData.Count > 0)
            {
                throw new ProviderInvocationException(_errorData[0]);
            }
        }
    }
}

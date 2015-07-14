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
        internal SessionState SessionState { get; private set; }
        internal PSCredential Credential { get; set; }
        internal SwitchParameter AvoidGlobbing { get; set; }
        internal PSDriveInfo PSDriveInfo { get; set; }
        internal bool IgnoreFiltersForGlobbing { get; set; }

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
            : this(cmdlet.ExecutionContext.SessionState)
        {
            _cmdlet = cmdlet;
        }

        internal ProviderRuntime(SessionState sessionState)
            : this(sessionState, false, false)
        {
        }

        internal ProviderRuntime(SessionState sessionState, bool force, bool avoidWildcardExpansion)
            : this()
        {
            SessionState = sessionState;
            AvoidGlobbing = avoidWildcardExpansion;
            Force = force;
            Credential = PSCredential.Empty;
        }

        public ProviderRuntime(ProviderRuntime runtime)
            : this(runtime.SessionState, runtime.Force, runtime.AvoidGlobbing)
        {
            _cmdlet = runtime._cmdlet;
            PassThru = runtime.PassThru;
            PSDriveInfo = runtime.PSDriveInfo;
            Include = new Collection<string>(runtime.Include);
            Exclude = new Collection<string>(runtime.Exclude);
            Filter = runtime.Filter;
            AvoidGlobbing = runtime.AvoidGlobbing;
            IgnoreFiltersForGlobbing = runtime.IgnoreFiltersForGlobbing;
            Credential = new PSCredential(runtime.Credential);
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

        internal void ThrowFirstErrorOrContinue()
        {
            if (_errorData.Count > 0)
            {
                throw new ProviderInvocationException(_errorData[0]);
            }
        }

        internal Collection<PSObject> ThrowFirstErrorOrReturnResults()
        {
            ThrowFirstErrorOrContinue();
            return RetreiveAllProviderData();
        }

        internal bool HasFilters()
        {
            return (Include != null && Include.Count > 0) || (Exclude != null && Exclude.Count > 0);
        }
    }
}

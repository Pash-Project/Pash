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

        private Cmdlet _cmdlet;
        private Collection<PSObject> _outputData;
        private Collection<ErrorRecord> _errorData;

        private ProviderRuntime()
        {
            _outputData = new Collection<PSObject>();
            _errorData = new Collection<ErrorRecord>();
        }

        internal ProviderRuntime(ExecutionContext executionContext)
            : this()
        {
            ExecutionContext = executionContext;
        }

        internal ProviderRuntime(Cmdlet cmdlet)
            : this()
        {
            _cmdlet = cmdlet;
            ExecutionContext = cmdlet.ExecutionContext;
        }

        internal Collection<PSObject> RetreiveAllProviderData()
        {
            Collection<PSObject> output = _outputData;
            _outputData = new Collection<PSObject>();
            return output;
        }

        internal void WriteObject(object obj)
        {
            if (_cmdlet != null)
            {
                _cmdlet.WriteObject(obj);
            }
            else
            {
                _outputData.Add(PSObject.AsPSObject(obj));
            }
        }
    }
}
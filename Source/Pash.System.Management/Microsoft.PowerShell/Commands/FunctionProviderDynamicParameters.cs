using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class FunctionProviderDynamicParameters
    {
        private ScopedItemOptions _options;
        private bool _optionsSet;

        public FunctionProviderDynamicParameters()
        {
            
        }

        [Parameter]
        public ScopedItemOptions Options
        {
            get
            {
                return _options;
            }
            set
            {
                _optionsSet = true;
                _options = value;
            }
        }

        internal bool OptionsSet { get { return _optionsSet; } }
    }
}
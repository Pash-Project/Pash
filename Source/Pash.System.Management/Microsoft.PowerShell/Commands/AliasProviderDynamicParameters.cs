// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class AliasProviderDynamicParameters
    {
        private ScopedItemOptions _options;
        private bool _optionsSet;

        public AliasProviderDynamicParameters()
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

using System;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.Commands
{
    public class CollectForEndProcessingCommandBase : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected Collection<PSObject> InputCollection { get; set; }

        protected CollectForEndProcessingCommandBase()
        {
            InputCollection = new Collection<PSObject>();
        }

        protected override void ProcessRecord()
        {
            InputCollection.Add(InputObject);
        }
    }
}


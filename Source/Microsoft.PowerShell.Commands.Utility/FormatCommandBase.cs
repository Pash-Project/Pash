using System;
using System.Management.Automation;
using System.Configuration;
using System.Xml;
using Extensions.Reflection;

namespace Microsoft.PowerShell.Commands.Utility
{
    public class FormatCommandBase : PSCmdlet
    {
        internal FormatManager FormatManager { get; private set; }

        [ParameterAttribute(ValueFromPipeline=true)] 
        public PSObject InputObject { get; set; }

        internal FormatCommandBase(FormatShape shape)
        {
            FormatManager = new FormatManager(shape, ExecutionContext);
        }

        protected override void BeginProcessing()
        {
            // just make sure to have the up to data execution context
            FormatManager.SetExecutionContext(ExecutionContext);
        }

        protected override void ProcessRecord()
        {
            foreach (var res in FormatManager.Process(InputObject))
            {
                WriteObject(res);
            }
        }

        protected override void EndProcessing()
        {
            foreach (var res in FormatManager.End())
            {
                WriteObject(res);
            }
        }

    }
}


using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Out", "Default")]
    public class OutDefaultCommand : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            // TODO: output the data vid OutHostCommand

            // TODO: should we print Null?
            if (InputObject.ImmediateBaseObject == null)
                return;

            Console.WriteLine(InputObject.ToString());
        }
    }
}
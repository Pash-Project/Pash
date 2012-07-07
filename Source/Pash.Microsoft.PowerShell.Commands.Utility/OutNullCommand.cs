using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Out", "Null", SupportsShouldProcess = false)]
    public class OutNullCommand : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord() { }
    }
}
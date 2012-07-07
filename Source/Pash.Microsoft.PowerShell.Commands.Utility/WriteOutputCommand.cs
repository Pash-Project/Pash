using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Write", "Output")]
    public sealed class WriteOutputCommand : PSCmdlet
    {
        [AllowEmptyCollection, Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true), AllowNull]
        public PSObject[] InputObject { get; set; }

        public WriteOutputCommand()
        {
        }

        protected override void ProcessRecord()
        {
            if (InputObject != null)
            {
                foreach (PSObject psObject in InputObject)
                {
                    WriteObject(psObject, true);
                }
            }
        }
    }
}
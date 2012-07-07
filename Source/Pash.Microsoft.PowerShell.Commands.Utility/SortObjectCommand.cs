using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Sort", "Object")]
    public sealed class SortObjectCommand : OrderObjectBase
    {
        [Parameter]
        public SwitchParameter Descending { get; set; }

        [Parameter]
        public SwitchParameter Unique { get; set; }

        public SortObjectCommand()
        {
        }

        protected override void EndProcessing()
        {
            // TODO: do the sorting
            foreach (PSObject obj in InputObjects)
            {
                WriteObject(obj);
            }
        }
    }
}
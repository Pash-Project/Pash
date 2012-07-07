using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class ObjectCmdletBase : PSCmdlet
    {
        [Parameter]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter]
        public string Culture { get; set; }

        public ObjectCmdletBase()
        {
        }
    }
}
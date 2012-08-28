using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Extensions.String;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet("Write", "Host")]
    public sealed class WriteHostCommand : ConsoleColorCmdlet
    {
        [Parameter(Position = 0, ValueFromRemainingArguments = true, ValueFromPipeline = true)]
        public PSObject Object { get; set; }

        [Parameter]
        public SwitchParameter NoNewline { get; set; }

        [Parameter]
        public Object Separator { get; set; }

        protected override void ProcessRecord()
        {
            if (Object == null) return;

            Action<ConsoleColor, ConsoleColor, string> writeAction;

            if (NoNewline) writeAction = Host.UI.Write;
            else writeAction = Host.UI.WriteLine;

            if (Object is Enumerable)
            {
                throw new NotImplementedException();
            }
            else
            {
                writeAction(ForegroundColor, BackgroundColor, Object.ToString());
            }
        }
    }
}

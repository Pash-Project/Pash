using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Extensions.String;
using System.Collections;

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
            Action<ConsoleColor, ConsoleColor, string> writeAction;

            if (NoNewline) writeAction = Host.UI.Write;
            else writeAction = Host.UI.WriteLine;

            if (Object == null)
            {
                writeAction(ForegroundColor, BackgroundColor, "");
            }
            else
            {
                writeAction(ForegroundColor, BackgroundColor, Object.ToString());
            }
        }
    }
}

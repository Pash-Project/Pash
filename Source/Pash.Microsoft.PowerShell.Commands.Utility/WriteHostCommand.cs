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
            Action<ConsoleColor, ConsoleColor, string> writeAction;

            if (NoNewline) writeAction = Host.UI.Write;
            else writeAction = Host.UI.WriteLine;

            if (Object == null)
            {
                writeAction(ForegroundColor, BackgroundColor, "");
            }

			//TODO look for a mono-only pre-compiler #if flag and wrap this
#if __MonoCS__
			else if (Object.BaseObject.GetType() == typeof(Enumerable))
#else
            else if (Object.BaseObject is Enumerable)
#endif
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

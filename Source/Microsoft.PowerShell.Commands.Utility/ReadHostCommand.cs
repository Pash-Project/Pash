using System;
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Read, "Host"
            /*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113371" */)]
    public class ReadHostCommand : PSCmdlet
    {
        [Parameter(Position=0, ValueFromRemainingArguments=true)]
        [AllowNull]
        public Object Prompt { get; set; }

        [Parameter]
        public SwitchParameter AsSecureString { get; set; }

        protected override void EndProcessing()
        {
            var ui = Host.UI;
            if (Prompt != null)
            {
                ui.Write(Prompt.ToString() + ": ");
            }

            object input;
            if (AsSecureString.IsPresent)
            {
                input = ui.ReadLineAsSecureString();
            }
            else
            {
                input = ui.ReadLine();
            }
            WriteObject(input);
        }
    }
}


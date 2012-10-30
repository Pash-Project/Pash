using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    // http://msdn.microsoft.com/en-us/library/microsoft.powershell.commands.setaliascommand.aspx
    [Cmdlet("Set", "Alias", SupportsShouldProcess = true/*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113390"*/)]
    //[OutputTypeAttribute(System.Compiler.TypeNode[])] 
    // Set-Alias [-Name] <string> [-Value] <string> [-Description <string>] [-Force] [-Option {None |
    // ReadOnly | Constant | Private | AllScope}] [-PassThru] [-Scope <string>] [-Confirm] [-WhatIf] [
    // <CommonParameters>]
    public sealed class SetAliasCommand : /*WriteAliasCommandBase*/PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Value { get; set; }

        [Parameter]
        public string Description { get; set; }

        // TODO:
        [Parameter]
        public string Option { get; set; }

        // TODO:
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        // TODO:
        [Parameter]
        public string Scope { get; set; }

        // TODO:
        [Parameter]
        public string Confirm { get; set; }

        // TODO:
        [Parameter]
        public string WhatIf { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                SessionState.SessionStateGlobal.SetAlias(this.Name, this.Value);
            }
            catch// (Exception ex)
            {
                //TODO:
                //WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, alias));
                return;
            }
        }
    }
}

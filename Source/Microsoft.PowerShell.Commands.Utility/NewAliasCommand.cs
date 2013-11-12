// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    // http://msdn.microsoft.com/en-us/library/microsoft.powershell.commands.setaliascommand.aspx
    [Cmdlet("New", "Alias", SupportsShouldProcess = true/*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113390"*/)]
    //[OutputTypeAttribute(System.Compiler.TypeNode[])] 
    // New-Alias [-Name] <string> [-Value] <string> [-Description <string>] [-Force] [-Option {None |
    // ReadOnly | Constant | Private | AllScope}] [-PassThru] [-Scope <string>] [-Confirm] [-WhatIf] [
    // <CommonParameters>]
    public sealed class NewAliasCommand : /*WriteAliasCommandBase*/PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Value { get; set; }

        [Parameter]
        public string Description { get; set; }

        // TODO:
        [Parameter]
        public ScopedItemOptions Option { get; set; }

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
            AliasInfo info = new AliasInfo(Name, Value, SessionState.SessionStateGlobal.CommandManager, Option);
            SessionState.Alias.New(info, Scope);
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Collections;
using Extensions.Enumerable;

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

        delegate void WriteAction(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);

        protected override void ProcessRecord()
        {
            WriteAction writeAction;

            if (NoNewline) writeAction = Host.UI.Write;
            else writeAction = Host.UI.WriteLine;

            writeAction(this.ForegroundColor, this.BackgroundColor, Compose());
        }

        string Compose()
        {
            if (Object == null)
            {
                return "";
            }
            else if (Object.BaseObject is Array)
            {
                return ((object[])Object.BaseObject).JoinString(" ");
            }
            else if (Object.BaseObject is IList)
            {
                return ((IEnumerable<object>)Object.BaseObject).JoinString(" ");
            }
            else
            {
                return Object.ToString();
            }
        }
    }
}

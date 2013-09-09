// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace TestHost.TestHelpers
{
    [Cmdlet("Test", "InvokeScript")]
    public class TestInvokeScriptCommand : PSCmdlet
    {
        [Parameter]
        public string Script { get; set; }

        protected override void ProcessRecord()
        {
            InvokeCommand.InvokeScript(Script);
        }
    }
}

// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace TestHost.TestHelpers
{
    [Cmdlet(VerbsDiagnostic.Test, "VerboseOutput")]
    public sealed class TestVerboseOutputCommand : PSCmdlet
    {
        public TestVerboseOutputCommand()
        {
            Message = "test";
        }

        [Parameter]
        public string Message { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("WriteVerbose: " + Message);
            WriteWarning("WriteWarning: " + Message);
        }
    }
}

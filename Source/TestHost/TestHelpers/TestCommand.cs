// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System.Management.Automation;

namespace TestHost.TestHelpers
{
    [Cmdlet(VerbsLifecycle.Invoke, "Test")]
    public class TestCommand : PSCmdlet
    {
        [Parameter]
        public string Parameter { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(string.Format("Parameter='{0}'", Parameter));
        }
    }
}

// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace TestHost.TestHelpers
{
    [Cmdlet("Test", "CreateError")]
    public class TestCreateErrorCommand : PSCmdlet
    {
        public enum Phases
        {
            Process,
            Begin,
            End
        };

        [Parameter]
        public Phases Phase { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public string Message { get; set; }

        [Parameter]
        public SwitchParameter Terminating { get; set; }

        [Parameter]
        public SwitchParameter NoError { get; set; }

        private void DoError()
        {
            var err = new TestException(Message ?? "testerror").ErrorRecord;
            if (NoError.IsPresent)
            {
                if (Message != null)
                {
                    Host.UI.WriteLine(Message);
                }
            }
            else if (Terminating.IsPresent)
            {
                ThrowTerminatingError(err);
            }
            else
            {
                WriteError(err);
            }
        }

        protected override void BeginProcessing()
        {
            if (Phase.Equals(Phases.Begin))
            {
                DoError();
            }
        }

        protected override void ProcessRecord()
        {
            if (Phase.Equals(Phases.Process))
            {
                DoError();
            }
            // write input object to pipeline for further processing
            if (Message != null)
            {
                WriteObject(Message);
            }
        }

        protected override void EndProcessing()
        {
            if (Phase.Equals(Phases.End))
            {
                DoError();
            }
        }
    }
}

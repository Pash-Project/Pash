// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace TestHost.TestHelpers
{
    [Cmdlet("Test", "CreateError")]
    public class TestCreateError : PSCmdlet
    {
        public enum Phases
        {
            Process,
            Begin,
            End
        };

        [Parameter]
        public Phases Phase { get; set; }

        [Parameter]
        public string Message { get; set; }

        [Parameter]
        public SwitchParameter Terminating { get; set; }

        [Parameter]
        public SwitchParameter NoError { get; set; }

        private void DoWork()
        {
            var err = new TestException(Message ?? "testerror").ErrorRecord;
            if (NoError.IsPresent)
            {
                if (Message != null)
                {
                    WriteObject(Message);
                }
                else
                {
                    WriteObject(err);
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
                DoWork();
            }
        }

        protected override void ProcessRecord()
        {
            if (Phase.Equals(Phases.Process))
            {
                DoWork();
            }
        }

        protected override void EndProcessing()
        {
            if (Phase.Equals(Phases.End))
            {
                DoWork();
            }
        }
    }
}

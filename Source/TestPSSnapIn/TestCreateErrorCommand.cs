// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace TestHost.TestHelpers
{
    public class TestException : Exception
    {
        public TestException(string message)
            : base(message)
        {
        }

        public ErrorRecord ErrorRecord
        {
            get
            {
                return new ErrorRecord(this, "Test", ErrorCategory.NotSpecified, null);
            }
        }
    }

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
        public SwitchParameter Exception { get; set; }

        [Parameter]
        public SwitchParameter NoError { get; set; }

        private void DoError()
        {
            if (NoError.IsPresent)
            {
                return;
            }

            var ex = new TestException(Message ?? "testerror");
            var err = ex.ErrorRecord;
            if (Terminating.IsPresent)
            {
                ThrowTerminatingError(err);
            }
            else if (Exception.IsPresent)
            {
                throw ex;
            }
            else
            {
                WriteError(err);
            }
        }

        protected override void BeginProcessing()
        {
            if (!NoError.IsPresent)
            {
                WriteObject("begin");
            }
            if (Phase.Equals(Phases.Begin))
            {
                DoError();
            }
        }

        protected override void ProcessRecord()
        {
            if (!NoError.IsPresent)
            {
                WriteObject("process");
            }
            if (Phase.Equals(Phases.Process))
            {
                DoError();
            }
            if (Message != null)
            {
                WriteObject(Message);
            }
        }

        protected override void EndProcessing()
        {
            if (!NoError.IsPresent)
            {
                WriteObject("end");
            }
            if (Phase.Equals(Phases.End))
            {
                DoError();
            }
        }
    }
}

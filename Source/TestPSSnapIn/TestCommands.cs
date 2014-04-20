using System;
using System.Management.Automation;

namespace TestPSSnapIn
{
    [Cmdlet(VerbsDiagnostic.Test, "PSSnapin")]
    public class TestCommand : PSCmdlet
    {
        public static string OutputString = "works";


        protected override void ProcessRecord()
        {
            WriteObject(OutputString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoMandatories", DefaultParameterSetName = "Reversed")]
    public class TestNoMandatoriesCommand : PSCmdlet
    {
        [Parameter(Position = 0, ParameterSetName = "Correct")]
        [Parameter(Position = 2, ParameterSetName = "Reversed")]
        public string One { get; set; }

        [Parameter(Position = 2, ParameterSetName = "Correct")]
        [Parameter(Position = 0, ParameterSetName = "Reversed")]
        public string Two { get; set; }

        [Parameter(ValueFromPipeline = true, ParameterSetName = "Correct")]
        public string RandomString;

        [Parameter(ValueFromPipeline = true, ParameterSetName = "Reversed")]
        public int RandomInt;

        protected override void ProcessRecord()
        {
            string setName = RandomString != null ? "Correct" : "Reversed";
            WriteObject(String.Format("{0}: {1} {2}", setName, One, Two));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "MandatoryInOneSet", DefaultParameterSetName = "Default")]
    public class TestMandatoryInOneSetCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Default", Position = 1, Mandatory = true)]
        [Parameter(ParameterSetName = "Alternative", Position = 1, Mandatory = false)]
        [Parameter(ParameterSetName = "Third", Position = 1, Mandatory = true)]
        public string TestParam { get; set; }

        [Parameter(ParameterSetName = "Third", Position = 2, Mandatory = true)]
        public string TestParam2 { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void ProcessRecord()
        {
            WriteObject(RandomString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoDefaultSet")]
    public class TestNoDefaultSetCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Default", Position = 1, Mandatory = false)]
        [Parameter(ParameterSetName = "Alternative", Position = 1, Mandatory = false)]
        public string TestParam { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void ProcessRecord()
        {
            WriteObject(RandomString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "CmdletPhases")]
    public class TestCmdletPhasesCommand : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void BeginProcessing()
        {
            WriteObject("Begin");
        }

        protected override void ProcessRecord()
        {
            WriteObject("Process");
        }

        protected override void EndProcessing()
        {
            WriteObject("End");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "ThrowError")]
    public class TestThrowErrorCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            ThrowTerminatingError(new ErrorRecord(new Exception("testerror"), "TestError",
                                                  ErrorCategory.InvalidOperation, null));
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "Dummy")]
    public class TestDummyCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoParameters")]
    public class TestNoParametersCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject("works");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "SwitchAndPositional")]
    public class TestSwitchAndPositionalCommand : PSCmdlet
    {
        [Parameter]
        public SwitchParameter Switch { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string RandomString;

        protected override void ProcessRecord()
        {
            WriteObject(RandomString);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "SameAliases")]
    public class TestSameAliasesCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "One")]
        [Alias("arg")]
        public string Arg1 { get; set; }

        [Parameter(ParameterSetName = "Two")]
        [Alias("ARG")]
        public string Arg2 { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("works");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "DefaultParameterSetDoesntExist", DefaultParameterSetName = "DoesntExist")]
    public class TestDefaultParameterSetDoesntExistCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "One", Mandatory = true)]
        public string Arg1 { get; set; }

        [Parameter(ParameterSetName = "Two", Mandatory = true)]
        public string Arg2 { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("works");
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NoDefaultSetAndTwoPositionals")]
    public sealed class TestNoDefaultSetAndTwoPositionalsCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "First", Mandatory = true, Position = 0)]
        public string Arg1 { get; set; }

        [Parameter(ParameterSetName = "Second", Mandatory = true, Position = 0)]
        public string Arg2 { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject("1:" + Arg1 + ", 2:" + Arg2);
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "DefaultSetIsAllParameterSetAndTwoParameterSets",
        DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
    public sealed class TestDefaultSetIsAllParameterSetAndTwoParameterSetsCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string First { get; set; }

        protected override void ProcessRecord()
        {
            if (First == null) {
                WriteObject("First: null");
            } else {
                WriteObject("First:" + First);
            }
        }
        
        [Parameter(Position = 1, ParameterSetName = "Set1")]
        [Parameter(Position = 1, ParameterSetName = "Set2")]
        [ValidateNotNullOrEmpty]
        public string Second { get; set; }

        [Parameter(ParameterSetName = "Set1")]
        public SwitchParameter ListAvailable { get; set; }
    }

    [Cmdlet(VerbsDiagnostic.Test, "TwoAmbiguousParameterSets")]
    public sealed class TestTwoAmbiguousParameterSetsCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string First { get; set; }

        protected override void ProcessRecord()
        {
            if (First == null) {
                WriteObject("First: null");
            } else {
                WriteObject("First:" + First);
            }
        }
        
        [Parameter(Position = 1, ParameterSetName = "Set1")]
        [Parameter(Position = 1, ParameterSetName = "Set2")]
        [ValidateNotNullOrEmpty]
        public string Second { get; set; }

        [Parameter(ParameterSetName = "Set1")]
        public SwitchParameter ListAvailable { get; set; }
    }
}

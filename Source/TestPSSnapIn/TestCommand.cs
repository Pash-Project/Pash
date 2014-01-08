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
}


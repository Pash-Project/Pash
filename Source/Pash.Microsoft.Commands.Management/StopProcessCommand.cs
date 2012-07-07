using System;
using System.Management.Automation;
using System.Diagnostics;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Stop", "Process", DefaultParameterSetName = "Id", SupportsShouldProcess = true)]
    public sealed class StopProcessCommand : ProcessBaseCommand
    {
        [Parameter(Position = 0, ParameterSetName = "Id", Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true, ValueFromPipelineByPropertyName = true), Alias(new string[] { "ProcessName" })]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        public StopProcessCommand()
        {
        }

        protected override void ProcessRecord()
        {
            // TODO: deal with ShouldProcess

            if (((Name == null) || (Name.Length == 0)) &&
                ((Id == null) || (Id.Length == 0)) &&
                ((InputObject == null) || (InputObject.Length == 0)))
                throw new Exception("No parameters specified for stop-process command");

            foreach (Process process in FindProcesses())
            {
                bool stopped = false;
                try
                {
                    stopped = process.HasExited;
                }
                catch (Exception ex)
                {
                    // TODO: WriteError
                    continue;
                }
                if (!stopped)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        // TODO: WriteError
                    }
                }
                if (PassThru)
                {
                    WriteObject(process);
                }
            }
        }
    }
}
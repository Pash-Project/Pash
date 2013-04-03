// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Start", "Service", DefaultParameterSetName = "Default", SupportsShouldProcess = true)]
    public sealed class StartServiceCommand : Cmdlet
    {
        protected override void ProcessRecord()
        {
            ServiceController[] _services = ServiceController.GetServices();
            foreach (String _name in Name)
                foreach (ServiceController _service in _services)
                {
                    if (_service.ServiceName == _name)
                    {
                        try
                        {
                            _service.Start();
                        }

                        // Mimic PowerShell's lack of reporting error when process is already started.
                        catch (Exception)
                        {
                        }

                        _service.WaitForStatus(ServiceControllerStatus.Running);
                        if (PassThru.ToBool()) WriteObject(_service);
                    }
                }
        }

        [Alias(new string[] { "ServiceName" }),
         Parameter(Position = 0,
             Mandatory = true,
             ParameterSetName = "Default",
             ValueFromPipelineByPropertyName = true,
             ValueFromPipeline = true)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

    }
}

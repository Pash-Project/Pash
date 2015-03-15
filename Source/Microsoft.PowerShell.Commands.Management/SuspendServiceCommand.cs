// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Suspend", "Service", DefaultParameterSetName = "Default", SupportsShouldProcess = true)]
    [OutputType(typeof(ServiceController))]
    public sealed class SuspendServiceCommand : Cmdlet
    {
        protected override void ProcessRecord()
        {
            ServiceController[] _services = ServiceController.GetServices();
            foreach (String _name in Name)
                foreach (ServiceController _service in _services)
                {
                    if ((_service.ServiceName == _name) && (_service.CanPauseAndContinue))
                    {
                        if (!Force.ToBool())
                            if (_service.DependentServices.Length != 0)
                                WriteError(new ErrorRecord(new InvalidOperationException("Cannot suspend service \"" + _service.ServiceName + "\" because other services are dependent on it. Use -Force to override."), "ServiceHasDependentServices", ErrorCategory.InvalidOperation, _service));
                            else _service.Pause();
                        else _service.Pause();
                        _service.WaitForStatus(ServiceControllerStatus.Paused);
                        if (PassThru.ToBool()) WriteObject(_service);
                    }
                }
        }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Alias(new string[] { "ServiceName" }),
         Parameter(
             ParameterSetName = "Default",
             Position = 0,
             Mandatory = true,
             ValueFromPipelineByPropertyName = true,
             ValueFromPipeline = true)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }
    }

}

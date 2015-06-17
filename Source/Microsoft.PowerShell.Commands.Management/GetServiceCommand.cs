// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Get-Service
    /// 
    /// DESCRIPTION
    ///   Displays system services available on the system.
    /// 
    /// RELATED PASH COMMANDS
    ///   Stop-Service
    ///   Restart-Service
    ///   Suspend-Service
    ///   Resume-Service
    /// </summary>
    [Cmdlet("Get", "Service", DefaultParameterSetName = "Default")]
    [OutputType(typeof(ServiceController))]
    public sealed class GetServiceCommand : Cmdlet
    {
        protected override void ProcessRecord()
        {
            if (Name != null)
            {
                ServiceController[] _services = ServiceController.GetServices();
                foreach (String _name in Name)
                    foreach (ServiceController _service in _services)
                    {
                        if (_service.ServiceName.ToLower() == _name.ToLower())
                            WriteObject(_service);
                    }
            }

            else WriteObject(ServiceController.GetServices(), true);
        }

        /// <summary>
        /// Gets the service by it's name.
        /// </summary>
        [Alias(new string[] { "ServiceName" }),
         Parameter(
             ParameterSetName = "Default",
             Position = 0,
             ValueFromPipelineByPropertyName = true,
             ValueFromPipeline = true)]
        public string[] Name { get; set; }
    }
}

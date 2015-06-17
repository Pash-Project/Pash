// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections.Generic;
using System.Management.Automation;
using System.Management;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Location", DefaultParameterSetName = "Location")]
    [OutputType(typeof(PathInfo), typeof(PathInfoStack))]
    public class GetLocationCommand : DriveMatchingCoreCommandBase
    {
        [Parameter(ParameterSetName = "Location", ValueFromPipelineByPropertyName = true)]
        public string[] PSDrive { get; set; }

        [Parameter(ParameterSetName = "Location", ValueFromPipelineByPropertyName = true)]
        public string[] PSProvider { get; set; }

        [Parameter(ParameterSetName = "Stack")]
        public SwitchParameter Stack { get; set; }

        [Parameter(ParameterSetName = "Stack", ValueFromPipelineByPropertyName = true)]
        public string[] StackName { get; set; }

        public GetLocationCommand()
        {
            PSProvider = new string[0];
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Stack")
            {
                // TODO: implement the location stack manipulations
                return;
            }
            else
            {
                if ((PSDrive != null) && (PSDrive.Length > 0))
                {
                    // If location is requested for a specific drive
                    List<PSDriveInfo> list = GetDrives(PSDrive, null, PSProvider, "local");
                    foreach (PSDriveInfo pdi in list)
                    {
                        WriteObject(new PathInfo(pdi, new Path(pdi.CurrentLocation).MakePath(pdi.Name), SessionState));
                    }
                }
                else if ((PSProvider != null) && (PSProvider.Length > 0))
                {
                    // If location was requested for a specific provider
                    foreach (string proverName in PSProvider)
                    {
                        foreach (ProviderInfo pi in SessionState.Provider.GetAll())
                        {
                            if (pi.IsNameMatch(proverName))
                            {
                                WriteObject(SessionState.Path.CurrentProviderLocation(pi.FullName));
                            }
                        }
                    }
                }
                else
                {
                    // If nothing specific was requested - return the current location
                    WriteObject(SessionState.Path.CurrentLocation);
                }
            }
        }
    }
}

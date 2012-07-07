using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Location", DefaultParameterSetName = "Location")]
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
                    foreach (string str in PSDrive)
                    {
                        List<PSDriveInfo> list = GetDrivesByName(str, PSProvider);

                        foreach (PSDriveInfo pdi in list)
                        {
                            WriteObject(new PathInfo(pdi, pdi.Provider, PathIntrinsics.MakePath(pdi.CurrentLocation, pdi), SessionState));
                        }
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
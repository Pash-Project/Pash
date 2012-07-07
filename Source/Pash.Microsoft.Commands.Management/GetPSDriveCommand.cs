using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "PSDrive", DefaultParameterSetName = "Name")]
    public class GetPSDriveCommand : DriveMatchingCoreCommandBase
    {
        [Parameter(Position = 0, ParameterSetName = "LiteralName", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public string[] LiteralName { get; set; }

        [Parameter(Position = 0, ParameterSetName = "Name", ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string[] PSProvider { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string Scope { get; set; }

        public GetPSDriveCommand()
        {
            PSProvider = new string[0];
        }

        protected override void ProcessRecord()
        {
            if ((PSProvider == null) || ((PSProvider != null) && (PSProvider.Length == 0)))
            {
                // TODO: as soon as we'll have formatters use the next line
                // WriteObject(SessionState.Provider.GetAll(), true);

                foreach (ProviderInfo providerInfo in SessionState.Provider.GetAll())
                {
                    WriteObject(providerInfo.Drives, true);
                }
            }
            else
            {
                foreach (string str in PSProvider)
                {
                    // TODO: deal with Wildcards
                    try
                    {
                        Collection<ProviderInfo> sendToPipeline = SessionState.Provider.Get(str);
                        WriteObject(sendToPipeline, true);
                    }
                    catch (ProviderNotFoundException exception)
                    {
                        WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                    }
                }
            }

        }
    }

 

}
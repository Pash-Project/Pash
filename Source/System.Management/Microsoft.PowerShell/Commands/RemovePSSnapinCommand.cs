// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "PSSnapin", SupportsShouldProcess = true)]
    public sealed class RemovePSSnapinCommand : PSSnapInCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        public RemovePSSnapinCommand()
        {

        }

        protected override void ProcessRecord()
        {
            foreach (string curName in Name)
            {
                Collection<PSSnapInInfo> snapins;
                try
                {
                    snapins = GetSnapIns(curName);
                }
                catch (PSArgumentException ex)
                {
                    WriteError(ex.ErrorRecord);
                    continue;
                }
                //Remove all matching snapins
                foreach (var curSnapin in snapins)
                {
                    if (ShouldProcess(curSnapin.ToString()))
                    {
                        //TODO: remove the snapins from the RunspaceConfiguration, and therefore from all its
                        //RunspaceConfigurationEntry collection which then call registered Update-delegates s.t.
                        //the ExecutionContexts associated with that collection is updated
                        //For now we use simply the SessionStateGlobal
                        try
                        {
                            var snapin = ExecutionContext.SessionStateGlobal.RemovePSSnapIn(curSnapin.Name, ExecutionContext);
                            if (PassThru.IsPresent)
                            {
                                WriteObject(snapin);
                            }
                        }
                        catch (PSArgumentException e)
                        {
                            WriteError(e.ErrorRecord);
                        }
                    }
                }
            }
        }
    }
}




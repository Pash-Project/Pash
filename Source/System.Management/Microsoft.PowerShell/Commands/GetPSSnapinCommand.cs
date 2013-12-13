// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "PSSnapin")]
    public sealed class GetPSSnapinCommand : PSSnapInCommandBase
    {
        [Parameter(Position = 0, Mandatory = false)]
        public string[] Name { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Registered { get; set; }

        public GetPSSnapinCommand()
        {

        }

        protected override void BeginProcessing()
        {
            if (Name == null || Name.Length < 1)
            {
                Name = new string[] { "*" }; //Will simply get all of the desired type
            }

            foreach (var curName in Name)
            {
                Collection<PSSnapInInfo> snapins;
                try
                {
                    snapins = Registered ? GetRegisteredSnapIns(curName) : GetSnapIns(curName);
                }
                catch (PSArgumentException ex)
                {
                    WriteError(ex.ErrorRecord);
                    continue;
                }
                foreach (PSSnapInInfo info in snapins)
                {
                    WriteObject(info);
                }
            }
        }
    }
}

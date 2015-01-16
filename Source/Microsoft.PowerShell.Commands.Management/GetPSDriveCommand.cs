// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System;

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

        protected override void ProcessRecord()
        {
            var drives = SessionState.Drive.GetAllAtScope(Scope);
            WriteObject(FilterDriveByName(FilterDriveByProvider(drives)), true);
        }

        private IEnumerable<PSDriveInfo> FilterDriveByProvider(IEnumerable<PSDriveInfo> drives)
        {
            if (PSProvider == null)
            {
                return drives;
            }
            return from d in drives where d.Provider.IsAnyNameMatch(PSProvider) select d;
        }

        private IEnumerable<PSDriveInfo> FilterDriveByName(IEnumerable<PSDriveInfo> drives)
        {
            if (LiteralName != null)
            {
                return from d in drives
                       where LiteralName.Contains(d.Name, StringComparer.InvariantCultureIgnoreCase)
                       select d;
            }
            if (Name == null)
            {
                return drives;
            }
            var wildcards = (from n in Name select new WildcardPattern(n, WildcardOptions.IgnoreCase)).ToArray();
            return from d in drives
                   where WildcardPattern.IsAnyMatch(wildcards, d.Name)
                   select d;
        }
    }
}

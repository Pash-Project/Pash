// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "PSProvider")]
    public class GetPSProviderCommand : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        public string[] PSProvider { get; set; }

        protected override void ProcessRecord()
        {
            var providers = PSProvider == null ? new [] { "*" } : PSProvider;
            var patterns = (from p in providers select new WildcardPattern(p, WildcardOptions.IgnoreCase)).ToArray();
            foreach (ProviderInfo info in SessionState.Provider.GetAll())
            {
                if (WildcardPattern.IsAnyMatch(patterns, info.Name))
                {
                    WriteObject(info);
                }
            }
        }
    }
}

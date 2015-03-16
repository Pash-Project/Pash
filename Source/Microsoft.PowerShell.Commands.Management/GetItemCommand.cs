// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Item", DefaultParameterSetName="Path"
            /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113319" */)]
    [OutputType(typeof(Boolean), typeof(String), typeof(FileInfo), typeof(DirectoryInfo), typeof(FileInfo))]
    public class GetItemCommand : CoreCommandWithFilteredPathsBase
    {
        protected override bool ProviderSupportsShouldProcess {
            get { return false; } // this cmdlet doesn't change anything anyhow
        }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        // TODO: support for #DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            InvokeProvider.Item.Get(InternalPaths, ProviderRuntime);
        }
    }
}


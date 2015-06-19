// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute("Get", "ChildItem", DefaultParameterSetName="Items"
        /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113308"*/)]
    [OutputType(typeof(FileInfo), typeof(DirectoryInfo))]
    public class GetChildItemCommand : CoreCommandWithCredentialsBase
    {
        /* While CoreCommandWithFiltersBase provides the Path/LiteralPath parameters with their internal behavior,
         * we cannot use this class. Because some PS developer decided that the ParameterSets in this cmdlet
         * are called differently ("Items" instead of "Path"). Therefore we need to implement the same logic here
         * instead of deriving from CoreCommandWithFiltersBase
         */
        protected string[] InternalPaths;

        [Parameter(Position=0,
            ParameterSetName="Items",
            Mandatory=false,
            ValueFromPipeline=true,
            ValueFromPipelineByPropertyName=true)]
        public string[] Path
        {
            get { return InternalPaths; }
            set { InternalPaths = value; }
        }

        [Parameter(ParameterSetName="LiteralItems",
            Mandatory=true,
            ValueFromPipeline=false,
            ValueFromPipelineByPropertyName=true)]
        [Alias("PSPath")]
        public string[] LiteralPath
        {
            get { return InternalPaths; }
            set
            {
                AvoidWildcardExpansion = true;
                InternalPaths = value;
            }
        }

        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter]
        public override string Filter { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter Name { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        // TODO: #DynamicParameter support

        protected override void ProcessRecord()
        {
            var paths = InternalPaths ?? new [] { "." }; // no path means the current location
            if (Name.IsPresent)
            {
                InvokeProvider.ChildItem.GetNames(paths, ReturnContainers.ReturnMatchingContainers,
                    Recurse.IsPresent, ProviderRuntime);
            }
            else
            {
                InvokeProvider.ChildItem.Get(paths, Recurse.IsPresent, ProviderRuntime);
            }
        }
    }
}

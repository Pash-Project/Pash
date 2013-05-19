// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Move-Item
    /// 
    /// DESCRIPTION
    ///   Moves an item from one path to another. The behavior depends on the provider you have loaded, but for the Filesystem provider this means moving files or folders.
    /// 
    /// RELATED PASH COMMANDS
    ///   Get-ChildItem
    ///   New-Item
    ///   Rename-Item
    ///   Copy-Item
    ///   
    /// RELATED POSIX COMMANDS
    ///   mv
    /// </summary>
    [Cmdlet("Move", "Item", DefaultParameterSetName = "Path", SupportsShouldProcess = true)]
    public class MoveItemCommand : ProviderCommandBase
    {
        protected override void ProcessRecord()
        {
            foreach (String _path in Path)
            {
                InvokeProvider.Item.Move(_path, Destination);

                if (PassThru.ToBool()) WriteObject(Path);
            }
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter]
        public override string Filter { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        [Alias(new string[] { "PSPath" }),
        Parameter(
            ParameterSetName = "LiteralPath",
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true)]
        public string[] LiteralPath { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(
            ParameterSetName = "Path",
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }

        //protected override bool ProviderSupportsShouldProcess { get; }
    }

}

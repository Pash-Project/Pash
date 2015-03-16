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
    public class MoveItemCommand : CoreCommandWithFilteredPathsBase
    {
        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime;
            runtime.PassThru = PassThru.IsPresent;
            InvokeProvider.Item.Move(InternalPaths, Destination, runtime);
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }
    }

}

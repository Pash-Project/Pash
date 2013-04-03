// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Microsoft.PowerShell.Commands.Provider
{
    [Cmdlet("New", "Item", DefaultParameterSetName="pathSet", SupportsShouldProcess=true)]
    public class NewItemCommand : ProviderCommandBase
    {
        /// <summary>
        /// NAME
        ///   New-Item
        /// 
        /// DESCRIPTION
        ///   Creates a new item in the given provider path. The behavior depends on the provider you have loaded, but for the Filesystem provider this means making new files or folders.
        /// 
        /// RELATED PASH COMMANDS
        ///   Get-ChildItem
        ///   Move-Item
        ///   Rename-Item
        ///   Copy-Item
        ///   
        /// RELATED POSIX COMMANDS
        ///   mkdir
        /// </summary>
        protected override void ProcessRecord()
        {
            foreach (String _path in Path)
            {
                InvokeProvider.Item.New(_path, Name, ItemType, Value);
            }
        }

        /// <summary>
        /// Allows you to override already existing items. Can not override if the item's permissions disallows the operation.
        /// </summary>
        [Parameter]
        public override SwitchParameter Force { get; set; }

        /// <summary>
        /// The type of the new item. For instance, in the Filesystem provider, if it is a directory or a file.
        /// </summary>
        [Alias(new string[] { "Type" }), Parameter(ValueFromPipelineByPropertyName = true)]
        public string ItemType { get; set; }

        /// <summary>
        /// What name you want to give the new item.
        /// </summary>
        [Parameter(
            Mandatory = true, 
            ParameterSetName = "nameSet", 
            ValueFromPipelineByPropertyName = true), 
        AllowEmptyString, AllowNull]
        public string Name { get; set; }
        
        /// <summary>
        /// The path of the new item.
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = "pathSet",  
            ValueFromPipelineByPropertyName = true), 
        Parameter(
            Position = 0,
            Mandatory = false,
            ParameterSetName = "nameSet",
            ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }

        /// <summary>
        /// The contents or value of the new item.
        /// </summary>
        [Parameter(
            ValueFromPipeline = true, 
            ValueFromPipelineByPropertyName = true)]
        public object Value { get; set; }

    }
}

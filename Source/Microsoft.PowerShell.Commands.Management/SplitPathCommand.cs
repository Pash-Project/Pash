// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Currently borked, needs Pash.Engine implementation

using System;
using System.Collections.ObjectModel;
using System.Management;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Split, "Path", DefaultParameterSetName = "ParentSet")]
    public class SplitPathCommand : CoreCommandWithCredentialsBase
    {
        [Parameter(
            Position = 0,
            ParameterSetName = "ParentSet",
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true)]
        [Parameter(
            Position = 0,
            ParameterSetName = "QualifierSet",
            ValueFromPipeline = true)]
        [Parameter(
            Position = 0,
            ParameterSetName = "LeafSet",
            ValueFromPipeline = true)]
        public string[] Path { get; set; }

        [Parameter]
        public SwitchParameter Resolve { get; set; }

        [Parameter(ParameterSetName = "IsAbsoluteSet")]
        public SwitchParameter IsAbsolute { get; set; }

        [Parameter(
            ParameterSetName = "LeafSet",
            ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Leaf { get; set; }

        [ParameterAttribute(ParameterSetName = "LiteralPathSet")]
        [Alias("PSPath")]
        public string[] LiteralPath { get; set; }

        [ParameterAttribute(ParameterSetName = "NoQualifierSet")]
        public SwitchParameter NoQualifier { get; set; }

        [ParameterAttribute(ParameterSetName = "ParentSet")]
        public SwitchParameter Parent { get; set; }

        [ParameterAttribute(Position = 1, ParameterSetName = "QualifierSet")]
        public SwitchParameter Qualifier { get; set; }

        protected override void ProcessRecord()
        {
            foreach (Path path in Path)
            {
                ProcessPath(path);
            }
        }

        private void ProcessPath(Path path)
        {
            if (Leaf.IsPresent)
            {
                WritePath(path.GetChildNameOrSelfIfNoChild().RemoveDrive());
            }
            else
            {
                WritePath(path.GetParentPath(string.Empty));
            }
        }

        private void WritePath(Path path)
        {
            WriteObject(path.ToString());
        }
    }
}
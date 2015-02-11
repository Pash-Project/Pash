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
        [Parameter(
            Position = 0,
            ParameterSetName = "IsAbsoluteSet",
            ValueFromPipeline = true)]
        [Parameter(
            Position = 0,
            ParameterSetName = "NoQualifierSet",
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
            else if (IsAbsolute.IsPresent)
            {
                WriteObject(PathIsAbsolute(path));
            }
            else if (NoQualifier.IsPresent)
            {
                WritePath(path.RemoveDrive());
            }
            else if (Qualifier.IsPresent)
            {
                WritePath(GetDriveOrThrow(path));
            }
            else
            {
                WritePath(path.GetParentPath(string.Empty));
            }
        }

        private bool PathIsAbsolute(Path path)
        {
            string drive = path.GetDrive();
            return !String.IsNullOrEmpty(drive) && (drive != path.CorrectSlash);
        }


        private Path GetDriveOrThrow(Path path)
        {
            string drive = path.GetDrive();
            if (drive != null)
            {
                return drive + ":";
            }
            throw new FormatException(string.Format("Cannot parse path because path '{0}' does not have a qualifier specified.", path));
        }

        private void WritePath(Path path)
        {
            WriteObject(path.ToString());
        }
    }
}
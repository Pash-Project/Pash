// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using Pash.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Provider;

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
            foreach (Path path in GetPathsToProcess())
            {
                ProcessPath(path);
            }
        }

        private IEnumerable<Path> GetPathsToProcess()
        {
            foreach (Path path in Path)
            {
                if (Resolve.IsPresent)
                {
                    foreach (Path resolvedPath in ResolvePaths(path))
                    {
                        yield return resolvedPath;
                    }
                }
                else
                {
                    yield return path;
                }
            }
        }

        private IEnumerable<Path> ResolvePaths(Path path)
        {
            if (!WildcardPattern.ContainsWildcardCharacters(path))
            {
                if (InvokeProvider.Item.Exists(path))
                {
                    yield return path;
                }
                throw new ItemNotFoundException();
            }

            CmdletProvider provider;
            foreach (Path resolvedPath in InvokeProvider.ChildItem.Globber.GetGlobbedProviderPaths(path, ProviderRuntime, out provider))
            {
                yield return System.IO.Path.GetFullPath(resolvedPath);
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
            string drive = null;
            if (path.TryGetDriveName(out drive))
            {
                if (System.IO.Path.DirectorySeparatorChar == '\\')
                {
                    return drive != @"\";
                }
                return true;
            }
            return false;
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
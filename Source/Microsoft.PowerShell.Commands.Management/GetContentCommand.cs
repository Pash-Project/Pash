// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.IO;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute(VerbsCommon.Get, "Content", DefaultParameterSetName="Path" /*HelpUri="http://go.microsoft.com/fwlink/?LinkID=113310"*/)] 
    public class GetContentCommand : ContentCommandBase
    {
        [ParameterAttribute(ValueFromPipelineByPropertyName = true)]
        public long ReadCount { get; set; }

        [Alias("Last")]
        [ParameterAttribute(ValueFromPipelineByPropertyName = true)]
        public int Tail { get; set; }

        [Alias("First", "Head")]
        [ParameterAttribute(ValueFromPipelineByPropertyName = true)]
        public long TotalCount { get; set; }

        public GetContentCommand()
        {
            ReadCount = 1;
            TotalCount = -1;
        }

        List<object> pendingLines = new List<object>();

        protected override void ProcessRecord()
        {
            foreach (string fileName in Path)
            {
                CheckPathHasSupportedProvider(fileName);

                if (Tail > 0)
                {
                    WriteLastLinesToPipeline(fileName);
                }
                else
                {
                    WriteLinesToPipeline(fileName);
                }

                WritePendingLines();
            }
        }

        private void CheckPathHasSupportedProvider(string fileName)
        {
            PSDriveInfo drive;
            var provider = SessionState.SessionStateGlobal.GetProviderByPath(fileName, out drive) as FileSystemProvider;
            if (provider == null)
            {
                throw new NotImplementedException("Not supported by FileSystemProvider");
            }
        }

        private void WriteLastLinesToPipeline(string fileName)
        {
            foreach (string line in File.ReadLines(fileName).Reverse().Take(Tail).Reverse())
            {
                WriteLine(line);
            }
        }

        private void WriteLine(string line)
        {
            if (ReadCount > 1)
            {
                pendingLines.Add(line);

                if (pendingLines.Count >= ReadCount)
                {
                    WritePendingLines();
                }
            }
            else
            {
                WriteObject(line);
            }
        }

        private void WritePendingLines()
        {
            if ((pendingLines != null) && (pendingLines.Count > 0))
            {
                WriteObject(pendingLines.ToArray());
                pendingLines.Clear();
            }
        }

        private void WriteLinesToPipeline(string fileName)
        {
            int currentLineNumber = 1;
            if (!ReadMoreLines(currentLineNumber))
            {
                return;
            }

            foreach (string line in File.ReadLines(fileName))
            {
                WriteLine(line);

                currentLineNumber++;
                if (!ReadMoreLines(currentLineNumber))
                {
                    return;
                }
            }
        }

        private bool ReadMoreLines(int currentLineNumber)
        {
            return (TotalCount == -1) || (currentLineNumber <= TotalCount);
        }
    }
}

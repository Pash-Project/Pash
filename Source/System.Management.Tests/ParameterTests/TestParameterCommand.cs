// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using NUnit.Framework;

namespace System.Management.Tests.ParameterTests
{
    [Cmdlet("Test", "Parameter")]
    public sealed class TestParameterCommand : PSCmdlet
    {
        [Alias(new string[] { "PSPath", "Path" })]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "File")]
        public string FilePath { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Variable")]
        public string Variable { get; set; }

        [Parameter]
        [Alias(new string[] { "FullName", "fn" })]
        public string Name;

        [Parameter]
        public SwitchParameter Recurse;

        // ValueFromPipeline need public getter so this should be skipped
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "TheseShouldBeSkipped")]
        public int Length { private get; set; }

        // All properties require a public setter so this should be skipped
        [Parameter(ParameterSetName = "TheseShouldBeSkipped")]
        public int Height { get; private set; }

        // Fields must be public so this should be skipped
        [Parameter]
        private int Age;
        // To avoid generating warning CS0414
        void Ignore() { this.Age++; }

        internal static CmdletInfo CreateCmdletInfo()
        {
            return new CmdletInfo("Test-Parameter", typeof(TestParameterCommand), "", null, null);
        }
    }
}

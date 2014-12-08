// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using NUnit.Framework;

namespace System.Management.Tests.ParameterTests
{
    [Cmdlet("Test", "Parameter", DefaultParameterSetName = "File")]
    public sealed class TestParameterCommand : PSCmdlet
    {
        [Alias(new string[] { "PSPath", "Path" })]
        [Parameter(Mandatory = true, Position = 3, ParameterSetName = "File")]
        public string FilePath { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Variable")]
        public string Variable { get; set; }

        [Parameter(ParameterSetName = "Variable")]
        public SwitchParameter ConstVar;

        [Parameter(Position=1)]
        [Alias(new string[] { "FullName", "fn", "identity" })]
        public string Name;

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter]
        public SwitchParameter Recurse;

        [Parameter(ParameterSetName = "Other")]
        public double[] FavoriteNumbers { get; set; }

        [Parameter(ParameterSetName = "Other")]
        public Exception[] FavoriteExceptions { get; set; }

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
            return new CmdletInfo("Test-Parameter", typeof(TestParameterCommand), "");
        }
    }
}

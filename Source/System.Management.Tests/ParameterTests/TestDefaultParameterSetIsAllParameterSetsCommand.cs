// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;

namespace System.Management.Tests.ParameterTests
{
    [Cmdlet("Test", "DefaultParameterSetIsAllParameterSets", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
    public class TestDefaultParameterSetIsAllParameterSetsCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Filter { get; set; }
        
        internal static CmdletInfo CreateCmdletInfo()
        {
            return new CmdletInfo("Test-DefaultParameterSetIsAllParameterSets", typeof(TestDefaultParameterSetIsAllParameterSetsCommand), "");
        }
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;

namespace System.Management.Tests.ParameterTests
{
    [Cmdlet("Test", "OutputType")]
    [OutputType(typeof(string))]
    [OutputType(typeof(bool))]
    public sealed class TestOutputTypeCommand : PSCmdlet
    {
    }
}

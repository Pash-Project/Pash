using System;
using NUnit.Framework;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class Conversion_Tests_6 : ReferenceTestBase
    {
        [TestCase("[bool]1", true)]
        [TestCase("[bool]-1", true)]
        [TestCase("[bool]0", false)]
        [TestCase("[bool][char]0", false, Explicit = true)]
        [TestCase("[bool]0.0D", false)]
        [TestCase("[bool]0.1D", true)]
        [TestCase("[bool]0.0", false)]
        [TestCase("[bool]0.1", true)]
        [TestCase("[bool]$null", false)]
        [TestCase("[bool]$notExisting", false)]
        [TestCase("[bool]@()", false)]
        [TestCase("[bool]@(1)", true)]
        [TestCase("[bool]@(0)", false)]
        [TestCase("[bool]@($false)", false)]
        [TestCase("[bool]@{}", true)]
        [TestCase("[bool]@{a='b'}", true)]
        [TestCase("[bool]''", false)]
        [TestCase("[bool]'false'", true)]
        [TestCase("[bool][Boolean]$true", true)]
        [TestCase("[bool][Boolean]$false", false)]
        [TestCase("[bool](new-object System.Management.Automation.SwitchParameter $true)", true)]
        [TestCase("[bool](new-object System.Management.Automation.SwitchParameter $false)", false)]
        public void ConvertToBool_Spec_6_2(string cmd, bool expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

    }
}


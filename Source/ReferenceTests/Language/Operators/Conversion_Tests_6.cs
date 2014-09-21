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
        [TestCase("[bool][char]0", false)]
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

        [TestCase("[char]$null", '\u0000')]
        [TestCase("[char]97", 'a')]
        [TestCase("[char]9731", '☃')]
        [TestCase("[char][byte]97", 'a')]
        [TestCase("[char][System.Int16]97", 'a')]
        [TestCase("[char]97L", 'a')]
        [TestCase("[char]'x'", 'x')]
        public void ConvertToChar_Spec_6_3(string cmd, char expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [ExpectedException]
        [TestCase("[char]$true")]
        [TestCase("[char]$false")]
        [TestCase("[char]0.5")]
        [TestCase("[char]1.0")]
        [TestCase("[char][float]0.5")]
        [TestCase("[char][float]1.0")]
        [TestCase("[char]0.5d")]
        [TestCase("[char]1.0d")]
        [TestCase("[char]-1")]
        [TestCase("[char]65536")]
        [TestCase("[char]-1L")]
        [TestCase("[char]65536L")]
        [TestCase("[char]''")]
        [TestCase("[char]'ab'")]
        [TestCase("[char](1,2)")]
        public void ConvertToChar_Spec_Errors_6_3(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[string]$null", "")]
        [TestCase("[string][char]9731", "☃")]
        [TestCase("[string]1", "1")]
        [TestCase("[string]1.5", "1.5")]
        [TestCase("[string]-1", "-1")]
        [TestCase("[string]1.100d", "1.100")]
        [TestCase("[string][double]::NaN", "NaN")]
        [TestCase("[string][double]::PositiveInfinity", "Infinity")]
        [TestCase("[string][double]::NegativeInfinity", "-Infinity")]
        [TestCase("$OFS='x';[string](1..5)", "1x2x3x4x5")]
        [TestCase("[string](1..5)", "1 2 3 4 5")]
        [TestCase("$OFS=7;[string](1,2,(3,4),5)", "1727System.Object[]75")]
        [TestCase("[string](1,2,(3,4),5)", "1 2 System.Object[] 5")]
        [TestCase("[string][System.IO.FileAttributes]7", "ReadOnly, Hidden, System")]
        [TestCase("[string]{$a = 5}", "$a = 5", Explicit=true, Reason="Script block conversion is still missing")]
        [TestCase("[string]{ Get-ChildItem foo; $i++ }", " Get-ChildItem foo; $i++ ", Explicit = true, Reason = "Script block conversion is still missing")]
        [TestCase("$OFS=1.2;[string][System.Linq.Enumerable]::Range(1, 5)", "11,221,231,241,25"), SetCulture("de-DE")]
        [TestCase("[string][System.Linq.Enumerable]::Range(1, 5)", "1 2 3 4 5")]
        public void ConvertToString_Spec_6_8(string cmd, string expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }
    }
}


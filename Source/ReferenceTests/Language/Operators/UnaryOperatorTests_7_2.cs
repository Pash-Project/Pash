using System;
using NUnit.Framework;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class UnaryOperatorTests_7_2 : ReferenceTestBase
    {

        [TestCase("-not $true", false)]
        [TestCase("! $true", false)]
        [TestCase("-not $false", true)]
        [TestCase("! $false", true)]
        [TestCase("-not -not $false", false)]
        [TestCase("! -not $false", false)]
        [TestCase("-not ! $false", false)]
        [TestCase("! ! $false", false)]
        [TestCase("-not 0", true)]
        [TestCase("! 0", true)]
        [TestCase("-not 1.23", false)]
        [TestCase("! 1.23", false)]
        [TestCase("-not \"xyz\"", false)]
        [TestCase("!\"xyz\"", false)]
        [TestCase("-not \"\"", true)]
        [TestCase("!\"\"", true)]
        public void LogicalNOT_Spec_7_2_2(string cmd, bool result)
        {
            ExecuteAndCompareTypedResult(cmd, result);
        }
    }
}


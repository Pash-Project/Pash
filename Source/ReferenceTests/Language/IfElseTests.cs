using System;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class IfElseTests : ReferenceTestBase
    {
        [Test]
        public void NewlinesCanSeparateIfAndElse()
        {
            var cmd = NewlineJoin("if ($false) { 1 }", "else { 2 }");
            ExecuteAndCompareTypedResult(cmd, 2);
        }

        [Test]
        public void NewlinesCanSeparateIfAndElseif()
        {
            var cmd = NewlineJoin("if ($false) { 1 }", "elseif ($true) { 2 }", "else { 3 }");
            ExecuteAndCompareTypedResult(cmd, 2);
        }
    }
}


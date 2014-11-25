using System;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class StringTests : ReferenceTestBase
    {
        [TestCase("\"`\"`\"\"", "\"\"")]
        [TestCase("\"foo`nbar\"", "foo\nbar")]
        [TestCase("\"foo`rbar\"", "foo\rbar")]
        [TestCase("\"foo`tbar\"", "foo\tbar")]
        [TestCase("\"foo`abar\"", "foo\abar")]
        [TestCase("\"foo`fbar\"", "foo\fbar")]
        [TestCase("\"foo`vbar\"", "foo\vbar")]
        [TestCase("\"foo`'bar\"", "foo'bar")]
        [TestCase("\"foo`\"bar\"", "foo\"bar")] // escaped quote
        [TestCase("\"foo\"\"bar\"", "foo\"bar")] // double quotes is one quote
        [TestCase("\"foo``tbar\"", "foo`tbar")] // double backticks is an escaped backtick
        [TestCase("\"foo`$bar\"", "foo$bar")] // variable shouldn't be resolved
        [TestCase("$bar=\"``t\"; \"foo`t$bar\"", "foo\t`t")] // the variable's resolved `t shouldn't be resovled a second time
        [TestCase("'foo''bar'", "foo'bar", Explicit = true, Reason = "Issue #225")]
        [TestCase("'foo`nbar'", "foo`nbar")] // in single quoted strings only ' can be escaped
        [TestCase("'foo`rbar'", "foo`rbar")]
        [TestCase("'foo``bar'", "foo``bar")]
        [TestCase("'foo`tbar'", "foo`tbar")]
        [TestCase("'foo`\"bar'", "foo`\"bar")]
        [TestCase("'foo\\nbar'", "foo\\nbar")] // just make sure that backslash escaping does *not* work
        [TestCase("\"foo\\nbar\"", "foo\\nbar")]
        [TestCase("\"foo\\rbar\"", "foo\\rbar")]
        [TestCase("\"foo\\tbar\"", "foo\\tbar")]
        public void StringWithAccentEscaped(string psStr, string expected)
        {
            var res = ReferenceHost.Execute(psStr);
            Assert.AreEqual(NewlineJoin(expected), res);
        }

        [TestCase("$x", "2")]
        [TestCase("$global:x", "1")] // scope qualifier works
        [TestCase("$foo:x", "")] // invalid drive qualifier doesn't throw
        [TestCase("$y", "")] // unset variable doesn't throw
        public void StringWithVariableWorks(string varAccess, string expected)
        {
            var stringPrefix = "foo ";
            var res = ReferenceHost.Execute("$x = 1; & { $x = 2; \"" + stringPrefix + varAccess + "\"; }");
            Assert.That(res, Is.EqualTo(NewlineJoin(stringPrefix + expected)));
        }
    }
}


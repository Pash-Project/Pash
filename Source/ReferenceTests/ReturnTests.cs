using System;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class ReturnTests : ReferenceTestBase
    {
        [Test]
        public void ReturnWritesToPipeline()
        {
            var expected = NewlineJoin("foobar");
            var result = ReferenceHost.Execute("return 'foobar'");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReturnEndsScriptBlockAndWritesToPipeline()
        {
            var expected = NewlineJoin("foo", "bar", "bla" );
            var command = NewlineJoin(
                "'foo'",
                "& { return 'bar'; 'baz' }",
                "'bla'"
            );
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReturnEndsFunctionAndWritesToPipeline()
        {
            var expected = NewlineJoin("foo", "bar", "bla");
            var command = NewlineJoin(
                "function testfun { 'foo'; return 'bar'; 'baz' }",
                "testfun",
                "'bla'"
            );
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReturnEndsScriptAndWritesToPipeline()
        {
            var expected = NewlineJoin("foo", "bar", "bla");
            var scriptname = CreateScript("'foo'; return 'bar'; 'baz'");
            var command = NewlineJoin(String.Format("& '{0}'", scriptname), "'bla'");
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [TestCase("5", "3 + 2")]
        [TestCase("abcd", "& { return 'abcd' }")]
        [TestCase("TESTVALUE", "$testvar")]
        public void ReturnEvaluatesExpression(string returnValue, string returnExpression)
        {
            var expected = NewlineJoin("foo", returnValue, "bla");
            var command = NewlineJoin(
                "& { $testvar = 'TESTVALUE'; 'foo'; return " + returnExpression + "; 'baz' }",
                "'bla'"
            );
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }
    }
}


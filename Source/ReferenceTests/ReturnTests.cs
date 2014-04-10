using System;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class ReturnTests : ReferenceTestBase
    {
        [TearDown]
        public void Cleanup()
        {
            RemoveCreatedScripts();
        }

        [Test]
        public void ReturnWritesToPipeline()
        {
            var expected = "foobar" + Environment.NewLine;
            var result = ReferenceHost.Execute("return 'foobar'");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReturnEndsScriptBlockAndWritesToPipeline()
        {
            var expected = NewlineJoin(new [] { "foo", "bar", "bla" });
            var command = NewlineJoin(new [] {
                "'foo'",
                "& { return 'bar'; 'baz' }",
                "'bla'"
            });
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReturnEndsFunctionAndWritesToPipeline()
        {
            var expected = NewlineJoin(new [] { "foo", "bar", "bla" });
            var command = NewlineJoin(new [] {
                "function testfun { 'foo'; return 'bar'; 'baz' }",
                "testfun",
                "'bla'"
            });
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ReturnEndsScriptAndWritesToPipeline()
        {
            var expected = NewlineJoin(new [] { "foo", "bar", "bla" });
            var scriptname = CreateScript("'foo'; return 'bar'; 'baz'");
            var command = NewlineJoin(new [] {
                String.Format("& '{0}'", scriptname),
                "'bla'"
            });
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }

        [TestCase("5", "3 + 2")]
        [TestCase("abcd", "& { return 'abcd' }")]
        [TestCase("TESTVALUE", "$testvar")]
        public void ReturnEvaluatesExpression(string returnValue, string returnExpression)
        {
            var expected = NewlineJoin(new [] { "foo", returnValue, "bla" });
            var command = NewlineJoin(new [] {
                "& { $testvar = 'TESTVALUE'; 'foo'; return " + returnExpression + "; 'baz' }",
                "'bla'"
            });
            var result = ReferenceHost.Execute(command);
            Assert.AreEqual(expected, result);
        }
    }
}


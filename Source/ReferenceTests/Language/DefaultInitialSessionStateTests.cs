using System;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace ReferenceTests
{
    [TestFixture]
    public class DefaultInitialSessionStateTests : ReferenceTestBase
    {

        [TestCase("$true", true)]
        [TestCase("$false", false)]
        [TestCase("$null", null)]
        [TestCase("$?", true)]
        public void DefaultVariableIsDefined(string name, object value)
        {
            ExecuteAndCompareTypedResult(name, value);
        }

        [Test]
        public void DefaultErrorVariableIsDefined()
        {
            ExecuteAndCompareTypedResult("$Error");
        }

        [TestCase("Prompt")]
        [TestCase("gci")]
        [TestCase("dir")]
        [TestCase("1 | % { $_ }")]
        [TestCase("1 | foreach { $_ }")]
        [TestCase("cd .")]
        [TestCase("1 | ft")]
        [TestCase("1 | fl")]
        [TestCase("gcm")]
        [TestCase("gsnp")]
        [TestCase("1 | select")]
        public void DefaultAliasOrFunctionIsDefined(string cmd)
        {
            // TODO: These tests should be better, e.g. by using the providers for functions and aliases
            // However, these aren't implemented, yet, so we use this test
            Assert.DoesNotThrow(delegate {
                ReferenceHost.Execute(cmd);
            });
        }
    }
}


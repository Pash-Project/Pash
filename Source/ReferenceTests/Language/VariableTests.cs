using System;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class VariableTests : ReferenceTestBase
    {

        [TestCase("$x")] // normal
        [TestCase("$global:x")] // scope qualified
        [TestCase("$foo:x")] // drive qualified
        [TestCase("$:x")] // strange
        public void UnknownVariableIsSimplyNull(string name)
        {
            var res = ReferenceHost.RawExecute(name);
            Assert.That(res, Is.Empty);
        }

    }
}


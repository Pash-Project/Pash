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
        [TestCase("$:x", Ignore = true)] // PS isn't too harsh when it comes to colons in the name. Pash needs to relax
        public void UnknownVariableIsSimplyNull(string name)
        {
            var res = ReferenceHost.RawExecute(name);
            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0], Is.Null);
        }

        [Test]
        public void SettingAVariableOnlyModifiesTheObject()
        {
            var cmd = "$x = 1; $var = Get-Variable x; $var.Value; $x = 2; $var.Value";
            ExecuteAndCompareTypedResult(cmd, 1, 2);
        }

        [Test]
        public void SetVariableValueViaObject()
        {
            var cmd = "$x = 1; $var = Get-Variable x; $var.Value = 2; $x";
            ExecuteAndCompareTypedResult(cmd, 2);
        }

    }
}


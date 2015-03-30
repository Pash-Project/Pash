using System;
using NUnit.Framework;
using System.Security;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class ConvertToSecureStringTests : ReferenceTestBase
    {
        [Test]
        public void CanConvertPlaintextToSecureString()
        {
            var val = "ASuperSecureString";
            var res = ReferenceHost.RawExecute("ConvertTo-SecureString -AsPlainText -Force '" + val + "'");
            Assert.That(res.Count, Is.EqualTo(1));
            var secureStr = res[0].BaseObject as SecureString;
            Assert.That(secureStr, Is.Not.Null, "Result is not a SecureString");
            var decoded = TestUtil.DecodeSecureString(secureStr);
            Assert.That(decoded, Is.EqualTo(val));
        }

    }
}


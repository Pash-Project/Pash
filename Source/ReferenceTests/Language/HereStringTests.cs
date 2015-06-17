using System;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class HereStringTests : ReferenceTestBase
    {
        [Test] // @"...'@ throws
        public void MixExpandableVerbatimSytnax()
        {
            Assert.Catch<ParseException>(delegate
            {
                ReferenceHost.Execute("@\"\r\nfoo bar\r\n'@");
            });
        }

        [TestCase("@\"\r\nfoo bar\r\n\"@", "foo bar")]
        [TestCase("@\"\r\nfoo\r\nbar\r\n\"@", "foo\r\nbar")]
        [TestCase("@\"\r\n$foo\r\n\"@", "1")]
        public void ExpandableHereString(string psStr, string expected)
        {
            var res = ReferenceHost.Execute("$foo = 1; " + psStr);
            Assert.AreEqual(NewlineJoin(expected), res);
        }

        [TestCase("@'\r\nfoo bar\r\n'@", "foo bar")]
        [TestCase("@'\r\nfoo\r\nbar\r\n'@", "foo\r\nbar")]
        [TestCase("@\'\r\n$foo\r\n\'@", "$foo")]
        public void VerbatimHereString(string psStr, string expected)
        {
            var res = ReferenceHost.Execute("$foo = 1; " + psStr);
            Assert.AreEqual(NewlineJoin(expected), res);
        }
    }
}

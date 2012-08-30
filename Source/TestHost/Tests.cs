using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void AddIntegers()
        {
            Assert.AreEqual("3\r\n", TestHost.Execute("1 + 2"));
        }

        [Test]
        public void ConcatStringInteger()
        {
            Assert.AreEqual("xxx1\r\n", TestHost.Execute("'xxx' + 1"));
        }

        [Test]
        public void VerbatimString()
        {
            Assert.AreEqual("xxx\r\n", TestHost.Execute("'xxx'"));
        }

        [Test]
        public void WriteOutputString()
        {
            Assert.AreEqual("xxx\r\n", TestHost.Execute("Write-Output 'xxx'"));
        }

        [Test]
        public void WriteHost()
        {
            Assert.AreEqual("xxx\r\n", TestHost.Execute("Write-Host 'xxx'"));
        }
    }
}

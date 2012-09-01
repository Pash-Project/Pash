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

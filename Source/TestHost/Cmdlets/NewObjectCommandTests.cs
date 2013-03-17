using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class NewObjectCommandTests
    {
        [Test]
        public void ATest()
        {
            Assert.AreEqual("xxx" + Environment.NewLine, TestHost.Execute("New-Object string 'xxx'"));
        }
    }
}

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
        public void CanCreateTypeWithNoParameters()
        {
            Assert.AreEqual("0.0" + Environment.NewLine, TestHost.Execute("New-Object System.Version"));
        }
    }
}

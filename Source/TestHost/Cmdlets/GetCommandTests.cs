using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class GetCommandTests
    {
        [Test]
        public void ATest()
        {
            var results = TestHost.Execute("Get-Command");

            StringAssert.Contains("Get-Command", results);
            Assert.GreaterOrEqual(results.Split('\n').Length, 10);
        }
    }
}

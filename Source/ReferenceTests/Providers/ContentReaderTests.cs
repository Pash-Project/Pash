using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Providers
{
    [TestFixture]
    public class ContentReaderTests : ReferenceTestBaseWithTestModule
    {
        [Test]
        public void ReadFileWithTwoLines()
        {
            string fileName = CreateFile(NewlineJoin("first", "second"), ".txt");
            string result = ReferenceHost.Execute("Test-ContentReader -path " + fileName);

            Assert.AreEqual(NewlineJoin("first", "second"), result);
        }
    }
}

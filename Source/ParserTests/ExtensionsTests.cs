using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ParserTests
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void GenerateTest()
        {
           CollectionAssert.AreEqual(new[]{1,2,3}, Extensions.Enumerable._.Generate(1, i => i + 1, 3).ToArray());
        }
    }
}

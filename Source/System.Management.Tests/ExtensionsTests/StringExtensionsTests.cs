using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Management.Tests.ExtensionsTests
{
    [TestFixture]
    class StringExtensionsTests
    {
        [Test]
        public void ATest()
        {
            var result = new List<string>(new[] { "a", "b" }).JoinString("-");
            Assert.AreEqual("a-b", result);
        }

        [Test]
        public void BTest()
        {
            var result = new List<object>(new[] { "a", "b" }).JoinString("-");
            Assert.AreEqual("a-b", result);
        }

        [Test]
        public void CTest()
        {
            var result = new[] { "a", "b" }.JoinString("-");
            Assert.AreEqual("a-b", result);
        }

        [Test]
        public void DTest()
        {
            var result = new object[] { "a", "b" }.JoinString("-");
            Assert.AreEqual("a-b", result);
        }
    }
}

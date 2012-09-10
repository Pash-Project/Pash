using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    class LiteralsTests
    {
        ////  7.7.1 Addition
        ////      Description:
        ////      
        ////          The result of the addition operator + is the sum of the values designated by the two operands after the usual arithmetic conversions (§6.15) have been applied.
        ////      
        ////          This operator is left associative.
        ////      
        [Test(Description = "from the spec"), Ignore("NYI")]
        public void AdditionExamples()
        {
            ////          12 + -10L               # long result 2
            Assert.AreEqual("2\r\n", TestHost.Execute("12 + -10L"));

            ////          -10.300D + 12           # decimal result 1.700
            Assert.AreEqual("1.700\r\n", TestHost.Execute("-10.300D + 12"));

            ////          10.6 + 12               # double result 22.6
            Assert.AreEqual("22.6\r\n", TestHost.Execute("10.6 + 12"));

            ////          12 + "0xabc"            # int result 2760
            Assert.AreEqual("2760\r\n", TestHost.Execute("12 + \"0xabc\""));
        }

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
    }
}

// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost
{
    [TestFixture]
    class IntegerLiterals
    {
        [Test]
        [TestCase("0", "System.Int32")]
        [TestCase("2147483647", "System.Int32")] // int.MaxValue
        [TestCase("2147483648", "System.Int64")] // int.MaxValue + 1
        [TestCase("9223372036854775807", "System.Int64")] // long.MaxValue
        [TestCase("9223372036854775808", "System.Decimal")] // long.MaxValue + 1
        [TestCase("79228162514264337593543950335", "System.Decimal")] // decimal.MaxValue
        [TestCase("79228162514264337593543950336", "System.Double")] // decimal.MaxValue + 1
        [TestCase("1kb", "System.Int32")]
        [TestCase("1mb", "System.Int32")]
        [TestCase("1gb", "System.Int32")]
        [TestCase("1tb", "System.Int64")]
        [TestCase("1pb", "System.Int64")]
        public void SimpleIntegerLiteralShouldBeOfType(string literal, string expectedType)
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType()", literal));
            Assert.AreEqual(expectedType + Environment.NewLine, result);
        }

        [Test]
        public void SimpleIntegerLiteralVeryLargeNumberShouldBeDouble()
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType()", new string('9', 200)));
            Assert.AreEqual("System.Double" + Environment.NewLine, result);
        }

        [Test]
        public void SimpleIntegerLiteralVeryVeryVeryLargeNumberShouldThrow()
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType()", new string('9', 310)));
            StringAssert.Contains("Exception", result);
        }

        [Test, Combinatorial]
        public void LongIntegerLiteralShouldBeInt64(
            [Values("0", "2147483647", "2147483648", "9223372036854775807")]
            string literal,
            [Values("l", "L")]
            string typeSuffix)
        {
            var result = TestHost.Execute(true, string.Format("({0}{1}).GetType()", literal, typeSuffix));
            Assert.AreEqual("System.Int64" + Environment.NewLine, result);
        }

        [Test]
        [TestCase("l")]
        [TestCase("L")]
        public void LongIntegerLiteralLargeNumberShouldThrow(string typeSuffix)
        {
            var result = TestHost.Execute(true, string.Format("(9223372036854775808{0}).GetType()", typeSuffix));
            StringAssert.Contains("Exception", result);
        }

        [Test]
        [TestCase("-1", "System.Int32")]
        [TestCase("-2147483647", "System.Int32")] // int.MinValue + 1
        [TestCase("-2147483648", "System.Int32")] // int.MinValue
        [TestCase("-9223372036854775807", "System.Int64")] // long.MaxValue + 1
        [TestCase("-9223372036854775808", "System.Int64")] // long.MaxValue
        public void NegativeIntegerLiteralShouldBeOfType(string literal, string expectedType)
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType()", literal));
            Assert.AreEqual(expectedType + Environment.NewLine, result);
        }

        [TestCase("1kb", "1024")]
        [TestCase("1mb", "1048576")]
        [TestCase("1gb", "1073741824")]
        [TestCase("1tb", "1099511627776")]
        [TestCase("1pb", "1125899906842624")]
        [TestCase("9876543210kb", "10113580247040")]
        public void IntegerWithNumericMultiplier(string literal, string expectedValue)
        {
            var result = TestHost.Execute(true, string.Format("{0}", literal));
            Assert.AreEqual(expectedValue + Environment.NewLine, result);
        }
    }
}

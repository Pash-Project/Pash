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
        [TestCase("0", typeof(System.Int32))]
        [TestCase("2147483647", typeof(System.Int32))] // int.MaxValue
        [TestCase("2147483648", typeof(System.Int64))] // int.MaxValue + 1
        [TestCase("9223372036854775807", typeof(System.Int64))] // long.MaxValue
        [TestCase("9223372036854775808", typeof(System.Decimal))] // long.MaxValue + 1
        [TestCase("79228162514264337593543950335", typeof(System.Decimal))] // decimal.MaxValue
        [TestCase("79228162514264337593543950336", typeof(System.Double))] // decimal.MaxValue + 1
        [TestCase("1kb", typeof(System.Int32))]
        [TestCase("1mb", typeof(System.Int32))]
        [TestCase("1gb", typeof(System.Int32))]
        [TestCase("100gb", typeof(System.Int64))]
        [TestCase("1tb",typeof( System.Int64))]
        [TestCase("1pb", typeof(System.Int64))]
        public void SimpleIntegerLiteralShouldBeOfType(string literal, Type expectedType)
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType().Name", literal));
            Assert.AreEqual(expectedType.Name + Environment.NewLine, result);
        }

        [Test]
        public void SimpleIntegerLiteralVeryLargeNumberShouldBeDouble()
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType().Name", new string('9', 200)));
            Assert.AreEqual(typeof(System.Double).Name + Environment.NewLine, result);
        }

        [Test]
        public void SimpleIntegerLiteralVeryVeryVeryLargeNumberShouldThrow()
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType().Name", new string('9', 310)));
            StringAssert.Contains("Exception", result);
        }

        [Test, Combinatorial]
        public void LongIntegerLiteralShouldBeInt64(
            [Values("0", "2147483647", "2147483648", "9223372036854775807")]
            string literal,
            [Values("l", "L")]
            string typeSuffix)
        {
            var result = TestHost.Execute(true, string.Format("({0}{1}).GetType().Name", literal, typeSuffix));
            Assert.AreEqual(typeof(System.Int64).Name + Environment.NewLine, result);
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
        [TestCase("-1", typeof(System.Int32))]
        [TestCase("-2147483647", typeof(System.Int32))] // int.MinValue + 1
        [TestCase("-2147483648", typeof(System.Int32))] // int.MinValue
        [TestCase("-2147483649", typeof(System.Int64))] // int.MinValue - 1
        [TestCase("-9223372036854775807", typeof(System.Int64))] // long.MaxValue + 1
        [TestCase("-9223372036854775808", typeof(System.Int64))] // long.MaxValue
        public void NegativeIntegerLiteralShouldBeOfType(string literal, Type expectedType)
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType().Name", literal));
            Assert.AreEqual(expectedType.Name + Environment.NewLine, result);
        }

        [TestCase("1kb", "1024")]
        [TestCase("1mb", "1048576")]
        [TestCase("1gb", "1073741824")]
        [TestCase("100gb", "107374182400")]
        [TestCase("1tb", "1099511627776")]
        [TestCase("1pb", "1125899906842624")]
        [TestCase("9876543210kb", "10113580247040")]
        [TestCase("12lkb", "12288")]
        public void IntegerWithNumericMultiplier(string literal, string expectedValue)
        {
            var result = TestHost.Execute(true, string.Format("{0}", literal));
            Assert.AreEqual(expectedValue + Environment.NewLine, result);
        }
    }
}

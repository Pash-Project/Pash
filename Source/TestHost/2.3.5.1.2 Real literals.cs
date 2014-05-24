// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost
{
    [TestFixture]
    [SetCulture("en-US")]
    class RealLiterals
    {
        [Test]
        [TestCase("1.2", typeof(System.Double))]
        //[TestCase("1.", typeof(System.Double)) Throws parse exception but works with Microsoft's PowerShell
        [TestCase("-4.5", typeof(System.Double))]
        [TestCase(".4", typeof(System.Double))]
        [TestCase("3.45e3", typeof(System.Double))]
        //[TestCase("3.e3", typeof(System.Double))] Works with Microsoft's PowerShell.
        [TestCase(".45e35", typeof(System.Double))]
        [TestCase("2.45e35", typeof(System.Double))]
        [TestCase("32.2e+3", typeof(System.Double))]
        //[TestCase("32.e+12", typeof(System.Double))] Works with Microsoft's PowerShell.
        [TestCase("123.456e-2", typeof(System.Double))]
        [TestCase("123.456e-231", typeof(System.Double))]
        [TestCase("123.456E-231", typeof(System.Double))]
        [TestCase("1.1mb", typeof(System.Double))]
        [TestCase("1.2MB", typeof(System.Double))]
        [TestCase("1.1kb", typeof(System.Double))]
        [TestCase("1.1gb", typeof(System.Double))]
        [TestCase("1.1tb", typeof(System.Double))]
        [TestCase("1.1pb", typeof(System.Double))]
        public void SimpleRealLiteralShouldBeOfType(string literal, Type expectedType)
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType().Name", literal));
            Assert.AreEqual(expectedType.Name + Environment.NewLine, result);
        }

        [Test]
        [Ignore("Throws parse exception but works with Microsoft's PowerShell")]
        public void SimpleRealLiteralWithDotButNoFollowingNumber()
        {
            var result = TestHost.Execute(true, "1.");
            Assert.AreEqual("1." + Environment.NewLine, result);
        }

        /// <summary>
        /// PowerShell actually throws a System.Management.Automation.RuntimeException:
        /// 
        /// PS C:\> 2.2e500
        /// At line:1 char:8
        /// + 2.2e500
        /// +        ~
        /// Bad numeric constant: 2.2e500.
        ///     + CategoryInfo          : ParserError: (:) [], ParentContainsErrorRecordException
        ///     + FullyQualifiedErrorId : BadNumericConstant
        /// 
        /// PS C:\> $error[0].gettype()
        /// 
        /// IsPublic IsSerial Name            BaseType
        /// -------- -------- ----            --------
        /// True     True     ParseException  System.Management.Automation.RuntimeException
        /// </summary>
        [Test]
        public void NumberTooBigForDouble()
        {
            var result = TestHost.Execute(true, "2.2E+500");
            StringAssert.Contains("The real literal 2.2E+500 is too large.", result);
        }

        [Test]
        [TestCase("1.2d", typeof(System.Decimal))]
        //[TestCase("1.d", typeof(System.Decimal))] Throws parse exception but works with Microsoft's PowerShell
        [TestCase("-4.5D", typeof(System.Decimal))]
        [TestCase(".4d", typeof(System.Decimal))]
        [TestCase("3.45e3d", typeof(System.Decimal))]
        //[TestCase("3.e3d", typeof(System.Decimal))] Works with Microsoft's PowerShell.
        [TestCase("32.2e+3D", typeof(System.Decimal))]
        //[TestCase("32.e+12d", typeof(System.Decimal))] Works with Microsoft's PowerShell.
        [TestCase("123.456e-2d", typeof(System.Decimal))]
        [TestCase("123.456e-231d", typeof(System.Decimal))]
        [TestCase("123.456E-231d", typeof(System.Decimal))]
        [TestCase("1.1dmb", typeof(System.Decimal))]
        [TestCase("1.2dMB", typeof(System.Decimal))]
        [TestCase("1.1Dkb", typeof(System.Decimal))]
        [TestCase("1.1DGB", typeof(System.Decimal))]
        [TestCase("1.1dtb", typeof(System.Decimal))]
        [TestCase("1.1dpb", typeof(System.Decimal))]
        public void SimpleDecimalRealLiteralShouldBeOfType(string literal, Type expectedType)
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType().Name", literal));
            Assert.AreEqual(expectedType.Name + Environment.NewLine, result);
        }

        [Test]
        [TestCase(".45e35d", "Bad numeric constant: .45e35d.")]
        [TestCase("2.45e35D", "Bad numeric constant: 2.45e35D.")]
        public void InvalidDecimalRealLiterals(string literal, string expectedResult)
        {
            var result = TestHost.Execute(true, literal);
            StringAssert.Contains(expectedResult, result);
        }
    }
}

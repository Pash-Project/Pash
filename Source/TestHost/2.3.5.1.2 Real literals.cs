// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost
{
    [TestFixture]
    class RealLiterals
    {
        [Test]
        [TestCase("1.2", "System.Double")]
        //[TestCase("1.", "System.Double") Throws parse exception but works with Microsoft's PowerShell
        [TestCase("-4.5", "System.Double")]
        [TestCase(".4", "System.Double")]
        [TestCase("3.45e3", "System.Double")]
        //[TestCase("3.e3", "System.Double")] Works with Microsoft's PowerShell.
        [TestCase(".45e35", "System.Double")]
        [TestCase("2.45e35", "System.Double")]
        [TestCase("32.2e+3", "System.Double")]
        //[TestCase("32.e+12", "System.Double")] Works with Microsoft's PowerShell.
        [TestCase("123.456e-2", "System.Double")]
        [TestCase("123.456e-231", "System.Double")]
        [TestCase("123.456E-231", "System.Double")]
        //[TestCase("32.e+12", "System.Double")] Works with Microsoft's PowerShell.
        public void SimpleRealLiteralShouldBeOfType(string literal, string expectedType)
        {
            var result = TestHost.Execute(true, string.Format("({0}).GetType()", literal));
            Assert.AreEqual(expectedType + Environment.NewLine, result);
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
    }
}

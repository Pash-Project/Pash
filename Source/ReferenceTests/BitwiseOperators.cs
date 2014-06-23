// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    class BitwiseOperators : ReferenceTestBase
    {
        [Test]
        [TestCase("0x0F0F -band 0xFE", "14", Description = "# int with value 0xE")]
        [TestCase("0x0F0F -band 0xFEL", "14", Description = "# long with value 0xE")]
        [TestCase("0x0F0F -band 14.6", "15", Description = "# long with value 0xF")]
        [TestCase("0x0F0F -bor 0xFE", "4095", Description = "# int with value 0xFFF")]
        [TestCase("0x0F0F -bor 0xFEL", "4095", Description = "# long with value 0xFFF")]
        [TestCase("0x0F0F -bor 14.40D", "3855", Description = "# long with value 0xF0F")]
        [TestCase("0x0F0F -bxor 0xFE", "4081", Description = "# int with value 0xFF1")]
        [TestCase("0x0F0F -bxor 0xFEL", "4081", Description = "# long with value 0xFF1")]
        [TestCase("0x0F0F -bxor 14.40D", "3841", Description = "# long with value 0xF01")]
        [TestCase("0x0F0F -bxor 14.6", "3840", Description = "# long with value 0xF00")]
        public void BitwiseOperation(string input, string expectedResult)
        {
            var result = ReferenceHost.Execute(input);
            Assert.AreEqual(expectedResult + Environment.NewLine, result);
        }

        [Test]
        public void BitwiseOperationInsideMethodCall()
        {
            string result = ReferenceHost.Execute(
@"$type = [System.Object]
$type.GetMethods([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Public) | Foreach-Object { $_.Name }
"
                );

            StringAssert.Contains("GetHashCode" + Environment.NewLine, result);
        }
    }
}

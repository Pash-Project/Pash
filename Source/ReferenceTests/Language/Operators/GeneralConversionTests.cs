using System;
using NUnit.Framework;
using System.Runtime.Serialization.Formatters;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class GeneralConversionTests : ReferenceTestBase
    {
        [TestCase("[string]$notExisting", "")]
        [TestCase("[string]$null", "")]
        [TestCase("[int]$notExisting", 0)]
        [TestCase("[int]$null", 0)]
        [TestCase("[bool]$notExisting", false)]
        [TestCase("[bool]$null", false)]
        [TestCase("[float]$notExisting", 0.0f)]
        [TestCase("[float]$null", 0.0f)]
        [TestCase("[PSObject]$null", null)]
        public void NullIsCorrectlyConverted(string cmd, object expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [TestCase("[string]5", "5")]
        [TestCase("[int]'5'", 5)]
        [TestCase("[int]2.7", 3)]
        [TestCase("[bool]1", true)]
        public void SimpleConversionWorks(string cmd, object expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [TestCase("[int][string]5", 5)]
        [TestCase("[Byte][char]'5'", (byte) '5')]
        [TestCase("[float][int]2.7", 3.0f)]
        [TestCase("[int][bool]3", 1)]
        public void DoubleConversionWorks(string cmd, object expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void CanConvertHexStringToInt()
        {
            ExecuteAndCompareTypedResult("[int]'0x1a'", (int)26);
        }

        [TestCase("'-0x1a'")]
        [TestCase("'+0x1a'")]
        public void ConvertSignedHexStringToIntThrows(string signedHexStr)
        {
            // TODO: check for correct exception type
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute("[int]" + signedHexStr);
            });
        }

        [Test]
        public void ConversionToArrayWorks()
        {
            ExecuteAndCompareTypedResult("([int[]]4.7).GetType()", typeof(int[]));
        }

        [TestCase("[void]'a'")]
        [TestCase("[void]1")]
        [TestCase("[void][version]'1.0'")]
        [TestCase("[void]$null")]
        public void ConvertToVoidNotWrittenToPipeline(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd, new object[0]);
        }
    }
}


using System;
using NUnit.Framework;
using System.Runtime.Serialization.Formatters;

namespace ReferenceTests
{
    [TestFixture]
    public class ConversionTests : ReferenceTestBase
    {
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
        public void ConversionToArrayWorks()
        {
            ExecuteAndCompareTypedResult("([int[]]4.7).GetType()", typeof(int[]));
        }

    }
}


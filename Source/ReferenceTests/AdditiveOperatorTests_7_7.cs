using System;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class AdditiveOperatorTests_7_7 : ReferenceTestBase
    {
        [TestCase("12 + -10L", (long) 2)] // 12 + -10L  # long result 2
        [TestCase("10.6 + 12", (double) 22.6)] // 10.6 + 12 # double result 22.6
        [TestCase("12 + \"0xabc\"", (int) 2760)] // 12 + "0xabc" # int result 2760
        public void Addition_Spec_7_7_1(string cmd, object expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test] // -10.300D + 12 # decimal result 1.700
        public void Addition_Spec_7_7_1_decimal()
        {
            // decimals aren't constante expressions, we need a seperate test
            ExecuteAndCompareTypedResult("-10.300D + 12", (decimal)1.7m);
        }

        [Test]
        public void AdditionOverflowAutomaticConversion()
        {
            int max = int.MaxValue;
            long max64 = max;
            var res = ReferenceHost.Execute(max.ToString() + " + 5");
            Assert.AreEqual(NewlineJoin((max64 + 5).ToString()), res);
        }

        [TestCase("\"red\" + \"blue\"", "redblue")] //"red" + "blue" # "redblue"
        [TestCase("\"red\" + \"123\"", "red123")] //"red" + "123" # "red123"
        [TestCase("\"red\" + 123", "red123")] // "red" + 123  # "red123"
        [TestCase("\"red\" + 123.456e+5", "red12345600")] // "red" + 123.456e+5 # "red12345600"
        [TestCase("\"red\" + (20, 30, 40)", "red20 30 40")] // "red" + (20,30,40) # "red20 30 40"
        public void StringConcatenation_Spec_7_7_2(string cmd, string expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [TestCase("12 - -10L", (long) 22)]
        [TestCase("10.5 - 12", (double) -1.5)]
        [TestCase("12 - \"0xabc\"", (int) -2736)]
        public void Subtraction_Spec_7_7_5(string cmd, object expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test] // -10.300D - 12 # decimal result -22.300
        public void Subtraction_Spec_7_7_5_decimal()
        {
            // decimals aren't constante expressions, we need a seperate test
            ExecuteAndCompareTypedResult("-10.300D - 12", (decimal) -22.3m);
        }
    }
}


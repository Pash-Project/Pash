using System;
using NUnit.Framework;
using System.Collections;

namespace ReferenceTests
{
    [TestFixture]
    public class AdditiveOperatorTests_7_7 : ReferenceTestBase
    {
        [TestCase("12 + -10L", (long) 2)] // 12 + -10L  # long result 2
        [TestCase("10.6 + 12", (double) 22.6)] // 10.6 + 12 # double result 22.6
        [TestCase("12 + \"0xabc\"", (int) 2760)] // 12 + "0xabc" # int result 2760
        [TestCase("12 + \"-0xabc\"", (int) -2736)]
        [TestCase("12 + \"+0xabc\"", (int) 2760)]
        [TestCase("111111111111111111 + 0", 111111111111111111)]
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

        [TestCase("[int]::MaxValue + 1", typeof(long), Explicit = true,
         Reason = "PS behaves different to its own specification as the result here is double")]
        [TestCase("[int]::MinValue - 1", typeof(long), Explicit = true,
         Reason = "PS behaves different to its own specification as the result here is double")]
        [TestCase("[long]::MaxValue + 1", typeof(double))]
        [TestCase("[long]::MinValue - 1", typeof(double))]
        public void OverflowIsCorrectlyCasted(string cmd, Type resultType)
        {
            ExecuteAndCompareType(cmd, resultType);
        }

        [TestCase("0 - \"4294967296\"", typeof(long))]
        [TestCase("4294967296 + \"9223372036854775808\"", typeof(decimal))] // second is long.MaxValue + 1
        public void ConversionWithOverflowWorksIfOneOperandIsTyped(string cmd, Type result)
        {
            ExecuteAndCompareType(cmd, result);
        }

        [Test]
        public void SignedHexValueOverflowIsDecimal()
        {
            // thex hex string is long::minvalue
            ExecuteAndCompareType("0 - \"-0x8000000000000000\"", typeof(decimal));
        }

        [TestCase("2+2", 2.0d)]
        [TestCase("5-1", 2.0d)]
        public void AdditiveArgumentExpressionWorks(string argExp, object expected)
        {
            var cmd = String.Format("[Math]::Sqrt({0})", argExp);
            ExecuteAndCompareTypedResult(cmd, expected);
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

        [TestCase("$a + \"red\"", new object[] { 10, 20, "red" })]
        [TestCase("$a + 12.5,$true", new object[] { 10, 20, 12.5, true })]
        [TestCase("$a + (new-object 'System.Double[,]' 2,3)", new object[] { 10, 20, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 })]
        [TestCase("(new-object 'System.Double[,]' 2,3) + $a", new object[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10, 20 })]
        public void ArrayConcatenation_Spec_7_7_3(string cmd, object[] expected)
        {
            cmd = "$a = new-object System.Int32[] 2; $a[0] = 10; $a[1] = 20;" + cmd;
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void HashtableConcatenation_Spec_7_7_4()
        {
            var cmd = NewlineJoin(
                "$h1 = @{ FirstName = \"James\"; LastName = \"Anderson\" }",
                "$h2 = @{ Dept = \"Personnel\" }",
                "$h3 = $h1 + $h2",
                "$h3"
                );
            var expected = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            expected["FirstName"] = "James";
            expected["LastName"] = "Anderson";
            expected["Dept"] = "Personnel";
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void HashtableConcatenationFailsWithSameKey()
        {
            var cmd = NewlineJoin(
                "$h1 = @{ FirstName = \"James\"; LastName = \"Anderson\" }",
                "$h2 = @{ Dept = \"Personnel\"; firstname = \"John\" }",
                "$h3 = $h1 + $h2;",
                "$h3"
                );
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute(cmd);
            });
        }

        [TestCase("12 - -10L", (long) 22)]
        [TestCase("10.5 - 12", (double) -1.5)]
        [TestCase("12 - \"0xabc\"", (int) -2736)]
        [TestCase("12 - \"+0xabc\"", (int) -2736)]
        [TestCase("12 - \"-0xabc\"", (int) 2760)]
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


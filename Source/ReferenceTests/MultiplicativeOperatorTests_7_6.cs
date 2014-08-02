using System;
using NUnit.Framework;

namespace ReferenceTests
{
    [TestFixture]
    public class MultiplicativeOperatorTests_7_6 : ReferenceTestBase
    {
        [TestCase("12 * -10L", (long) -120)]
        [TestCase("10.5 * 13", (double) 136.5)]
        [TestCase("12 * \"0xabc\"", (int) 32976)]
        public void Multiplication_Spec_7_6_1(string cmd, object result)
        {
            ExecuteAndCompareTypedResult(cmd, result);
        }

        [Test]
        public void Multiplication_Spec_7_6_1_decimal()
        {
            ExecuteAndCompareTypedResult("-10.300D * 12", (decimal) -123.600m);
        }

        /*
        string replication 7.6.2
        "red" * "3" # string replicated 3 times
        "red" * 4 # string replicated 4 times
        "red" * 0 # results in an empty string
        "red" * 2.3450D # string replicated twice
        "red" * 2.7 # string replicated 3 times
*/
        public void StringReplication_Spec_7_6_2(string cmd, string result)
        {
            ExecuteAndCompareTypedResult(cmd, result);
        }

        /*

        division 7.6.4
        10/-10          # int result -1.2
        12/-10          # double result -1.2
        12/-10D         # decimal result 1.2
        12/10.6         # double result 1.13207547169811
        12/"0xabc"      # double result 0.00436681222707424
*/
        public void Division_Spec_7_6_4(string cmd, object result)
        {
            ExecuteAndCompareTypedResult(cmd, result);
        }
        /*
        remainder 7.6.5
        10 % 3                      # int result 1
        10.0 % 0.3                  # double result 0.1
        10.00D % "0x4"              # decimal result 2.00
        */
        public void Remainder_Spec_7_6_5(string cmd, object result)
        {
            ExecuteAndCompareTypedResult(cmd, result);
        }
    }
}


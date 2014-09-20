using System;
using NUnit.Framework;
using System.Collections;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class FormatOperatorTests_7_5 : ReferenceTestBase
    {
        [Test, SetCulture("en-US")]
        [TestCase("\"{2} <= {0} + {1}`n\" -f $i,$j,($i+$j)", "22 <= 10 + 12\n")]
        [TestCase("\">{0,3}<\" -f 5", ">  5<")]
        [TestCase("\">{0,-3}<\" -f 5", ">5  <")]
        [TestCase("\">{0,3:000}<\" -f 5", ">005<")]
        [TestCase("\">{0,5:0.00}<\" -f 5.0", "> 5.00<")]
        [TestCase("\">{0:C}<\" -f     1234567.888", ">$1,234,567.89<")]
        [TestCase("\">{0:C}<\" -f     -1234.56", ">($1,234.56)<")]
        [TestCase("\">{0,12:e2}<\" -f 123.456e2", ">   1.23e+004<")]
        [TestCase("\">{0,-12:p}<\" -f -0.252", ">-25.20 %    <")]
        [TestCase("$format = \">{0:x8}<\"; $format -f 123455", ">0001e23f<")]
        public void Format_Spec_7_5(string cmd, string expected)
        {
            cmd = "$i = 10; $j = 12; " + cmd;
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, SetCulture("de-DE")]
        [TestCase("'>{0}' -F 5", ">5")] // case-insensitive
        [TestCase("':{0}'-f5", ":5")] // Doesn't need whitespace
        [TestCase("'*{0}'\u2013f5", "*5")] // Can use U+2013 as dash
        [TestCase("'*{0}'\u2014f5", "*5")] // Can use U+2014 as dash
        [TestCase("'*{0}'\u2015f5", "*5")] // Can use U+2015 as dash
        [TestCase("'*{0}'-f\r5", "*5")] // Allows newline after operator
        [TestCase("'*{0}'-f\n5", "*5")] // Allows newline after operator
        [TestCase("'*{0}'-f\r\n5", "*5")] // Allows newline after operator
        [TestCase("5-f7", "5")] // Result type is string and left operand will be converted to string
        [TestCase("{{0}+1}-f7", "7+1", Explicit = true, Reason = "Script blocks cannot be converted to strings yet")] // Abusing type conversion rules to string
        [TestCase("1..5-f7", "1 2 3 4 5")] // Abusing type conversion rules to string
        [TestCase("'{0}-{1}-{2}'-f0..2", "0-1-2")] // Allows a range on the other side
        [TestCase("'{0,10:N6} {1,10:N6}' -F 3,5", "  3,000000   5,000000")]
        [TestCase("'{0,10:N6} {1,10:N6}' -F [Math]::Sqrt(9),5", "  3,000000   5,000000", Explicit = true, Reason = "For some reason the values on the right are apparently converted to string before invoking the format operator")]
        public void FormatOperatorTests(string cmd, string expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }
    }
}


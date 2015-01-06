using System;
using NUnit.Framework;
using System.Globalization;
using System.Xml;
using System.Text.RegularExpressions;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class Conversion_Tests_6 : ReferenceTestBase
    {
        [TestCase("[bool]1", true)]
        [TestCase("[bool]-1", true)]
        [TestCase("[bool]0", false)]
        [TestCase("[bool][char]0", false)]
        [TestCase("[bool]0.0D", false)]
        [TestCase("[bool]0.1D", true)]
        [TestCase("[bool]0.0", false)]
        [TestCase("[bool]0.1", true)]
        [TestCase("[bool]$null", false)]
        [TestCase("[bool]$notExisting", false)]
        [TestCase("[bool]@()", false)]
        [TestCase("[bool]@(1)", true)]
        [TestCase("[bool]@(0)", false)]
        [TestCase("[bool]@($false)", false)]
        [TestCase("[bool]@{}", true)]
        [TestCase("[bool]@{a='b'}", true)]
        [TestCase("[bool]''", false)]
        [TestCase("[bool]'false'", true)]
        [TestCase("[bool][Boolean]$true", true)]
        [TestCase("[bool][Boolean]$false", false)]
        [TestCase("[bool](new-object System.Management.Automation.SwitchParameter $true)", true)]
        [TestCase("[bool](new-object System.Management.Automation.SwitchParameter $false)", false)]
        public void ConvertToBool_Spec_6_2(string cmd, bool expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [TestCase("[char]$null", '\u0000')]
        [TestCase("[char]97", 'a')]
        [TestCase("[char]9731", '☃')]
        [TestCase("[char][byte]97", 'a')]
        [TestCase("[char][System.Int16]97", 'a')]
        [TestCase("[char]97L", 'a')]
        [TestCase("[char]'x'", 'x')]
        public void ConvertToChar_Spec_6_3(string cmd, char expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [ExpectedException]
        [TestCase("[char]$true")]
        [TestCase("[char]$false")]
        [TestCase("[char]0.5")]
        [TestCase("[char]1.0")]
        [TestCase("[char][float]0.5")]
        [TestCase("[char][float]1.0")]
        [TestCase("[char]0.5d")]
        [TestCase("[char]1.0d")]
        [TestCase("[char]-1")]
        [TestCase("[char]65536")]
        [TestCase("[char]-1L")]
        [TestCase("[char]65536L")]
        [TestCase("[char]''")]
        [TestCase("[char]'ab'")]
        [TestCase("[char](1,2)")]
        public void ConvertToChar_Spec_Errors_6_3(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[byte]$false", (byte)0)]
        [TestCase("[byte]$true", (byte)1)]
        [TestCase("[byte][char]42", (byte)42)]
        [TestCase("[byte]0", (byte)0)]
        [TestCase("[byte]6", (byte)6)]
        [TestCase("[byte]255", (byte)255)]
        [TestCase("[byte]0.0", (byte)0)]
        [TestCase("[byte]5.5", (byte)6)]
        [TestCase("[byte]255.4", (byte)255)]
        [TestCase("[byte][float]0.0", (byte)0)]
        [TestCase("[byte][float]5.5", (byte)6)]
        [TestCase("[byte][float]255.4", (byte)255)]
        [TestCase("[byte]0.0d", (byte)0)]
        [TestCase("[byte]5.5d", (byte)6)]
        [TestCase("[byte]255.4d", (byte)255)]
        [TestCase("[byte]$null", (byte)0)]
        [TestCase("[byte]''", (byte)0)]
        [TestCase("[byte]'0.0'", (byte)0, Explicit = true, Reason = "Wrong culture handling while parsing")]
        [TestCase("[byte]'5.5'", (byte)6, Explicit = true, Reason = "Wrong culture handling while parsing")]
        [TestCase("[byte]'255.4'", (byte)255, Explicit = true, Reason = "Wrong culture handling while parsing")]
        public void ConvertToByte_Spec_6_4(string cmd, byte expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, SetCulture("de-DE"), Explicit("Wrong culture handling while parsing")]
        public void ConvertToByteFromFPStringUsesInvariantCulture()
        {
            ExecuteAndCompareTypedResult("[byte]'5.5'", (byte)6);
            ExecuteAndCompareTypedResult("[byte]'5,5'", (byte)55);
        }

        [ExpectedException]
        [TestCase("[byte][char]256")]
        [TestCase("[byte]256")]
        [TestCase("[byte]255.5")]
        [TestCase("[byte][float]255.5")]
        [TestCase("[byte]255.5d")]
        [TestCase("[byte]255.5d")]
        [TestCase("[byte]'255.5'")]
        [TestCase("[byte]'Foo'")]
        public void ConvertToByte_Spec_Errors_6_4(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[int]$false", 0)]
        [TestCase("[int]$true", 1)]
        [TestCase("[int][char]4242", 4242)]
        [TestCase("[int]0", 0)]
        [TestCase("[int]6", 6)]
        [TestCase("[int]2147483647", 2147483647)]
        [TestCase("[int]0.0", 0)]
        [TestCase("[int]5.5", 6)]
        [TestCase("[int]2147483647.4", 2147483647)]
        [TestCase("[int][float]0.0", 0)]
        [TestCase("[int][float]5.5", 6)]
        [TestCase("[int]0.0d", 0)]
        [TestCase("[int]5.5d", 6)]
        [TestCase("[int]2147483647.4d", 2147483647)]
        [TestCase("[int]$null", 0)]
        [TestCase("[int]''", 0)]
        [TestCase("[int]'0.0'", 0, Explicit = true, Reason = "Wrong culture handling while parsing")]
        [TestCase("[int]'5.5'", 6, Explicit = true, Reason = "Wrong culture handling while parsing")]
        [TestCase("[int]'2147483647.4'", 2147483647, Explicit = true, Reason = "Wrong culture handling while parsing")]
        public void ConvertToInt_Spec_6_4(string cmd, int expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, SetCulture("de-DE"), Explicit("Wrong culture handling while parsing")]
        public void ConvertToIntFromFPStringUsesInvariantCulture()
        {
            ExecuteAndCompareTypedResult("[int]'5.5'", 6);
            ExecuteAndCompareTypedResult("[int]'5,5'", 55);
        }

        [ExpectedException]
        [TestCase("[int]2147483648")]
        [TestCase("[int]2147483647.5")]
        [TestCase("[int][float]2147483648")]
        [TestCase("[int]2147483647.5d")]
        [TestCase("[int]'2147483647.5'")]
        [TestCase("[int]'Foo'")]
        public void ConvertToInt_Spec_Errors_6_4(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[long]$false", 0)]
        [TestCase("[long]$true", 1)]
        [TestCase("[long][char]4242", 4242)]
        [TestCase("[long]0", 0)]
        [TestCase("[long]6", 6)]
        [TestCase("[long]9223372036854775807", 9223372036854775807)]
        [TestCase("[long]0.0", 0)]
        [TestCase("[long]5.5", 6)]
        [TestCase("[long][float]0.0", 0)]
        [TestCase("[long][float]5.5", 6)]
        [TestCase("[long]0.0d", 0)]
        [TestCase("[long]5.5d", 6)]
        [TestCase("[long]9223372036854775807.4d", 9223372036854775807)]
        [TestCase("[long]$null", 0)]
        [TestCase("[long]''", 0)]
        [TestCase("[long]'0.0'", 0, Explicit = true, Reason = "Wrong culture handling while parsing")]
        [TestCase("[long]'5.5'", 6, Explicit = true, Reason = "Wrong culture handling while parsing")]
        [TestCase("[long]'9223372036854775807.4'", 9223372036854775807, Explicit = true, Reason = "Wrong culture handling while parsing")]
        public void ConvertToLong_Spec_6_4(string cmd, long expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, SetCulture("de-DE"), Explicit]
        public void ConvertToLongFromFPStringUsesInvariantCulture()
        {
            ExecuteAndCompareTypedResult("[long]'5.5'", 6);
            ExecuteAndCompareTypedResult("[long]'5,5'", 55);
        }

        [ExpectedException]
        [TestCase("[long]9223372036854775808d")]
        [TestCase("[long]9223372036854775808.0")]
        [TestCase("[long][float]9223372036854775808")]
        [TestCase("[long]9223372036854775807.5d")]
        [TestCase("[long]'9223372036854775807.5'")]
        [TestCase("[long]'Foo'")]
        public void ConvertToLong_Spec_Errors_6_4(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[float]$false", 0f)]
        [TestCase("[float]$true", 1f)]
        [TestCase("[float][char]9731", 9731f, Explicit = true, Reason = "Conversion from char fails")]
        [TestCase("[float]42", 42f)]
        [TestCase("[float]16777216", 16777216f)]
        [TestCase("[float]$null", 0f)]
        [TestCase("[float]'0.0'", 0f)]
        [TestCase("[float]'0.5'", 0.5f)]
        [TestCase("[float]'16777216'", 16777216f)]
        [TestCase("[float]'16777216.0'", 16777216f)]
        [TestCase("[float]'-Infinity'", float.NegativeInfinity)]
        [TestCase("[float]'Infinity'", float.PositiveInfinity)]
        [TestCase("[float]'NaN'", float.NaN)]
        public void ConvertToFloat_Spec_6_5(string cmd, float expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [ExpectedException]
        [TestCase("[float]'-infinity'")]
        [TestCase("[float]'infinity'")]
        [TestCase("[float]'Nan'")]
        [TestCase("[float]'xyz'")]
        public void ConvertToFloat_Spec_Erors_6_5(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[double]$false", 0)]
        [TestCase("[double]$true", 1)]
        [TestCase("[double][char]9731", 9731, Explicit = true, Reason = "Conversion from char fails")]
        [TestCase("[double]42", 42)]
        [TestCase("[double]9007199254740992", 9007199254740992)]
        [TestCase("[double]$null", 0)]
        [TestCase("[double]'0.0'", 0)]
        [TestCase("[double]'0.5'", 0.5)]
        [TestCase("[double]'9007199254740992'", 9007199254740992)]
        [TestCase("[double]'9007199254740992.0'", 9007199254740992)]
        [TestCase("[double]'-Infinity'", double.NegativeInfinity)]
        [TestCase("[double]'Infinity'", double.PositiveInfinity)]
        [TestCase("[double]'NaN'", double.NaN)]
        public void ConvertToDouble_Spec_6_5(string cmd, double expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [ExpectedException]
        [TestCase("[double]'-infinity'")]
        [TestCase("[double]'infinity'")]
        [TestCase("[double]'Nan'")]
        [TestCase("[double]'xyz'")]
        public void ConvertToDouble_Spec_Erors_6_5(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[decimal]$false", "0")]
        [TestCase("[decimal]$true", "1")]
        [TestCase("[decimal][char]9731", "9731", Explicit = true, Reason = "Conversion from char fails")]
        [TestCase("[decimal]42", "42")]
        [TestCase("[decimal]9007199254740992", "9007199254740992")]
        [TestCase("[decimal]9223372036854775807", "9223372036854775807")]
        [TestCase("[decimal]$null", "0")]
        [TestCase("[decimal]'0.0'", "0")]
        [TestCase("[decimal]'0.5'", "0.5")]
        [TestCase("[decimal]'0.500'", "0.5")]
        [TestCase("[decimal]'9007199254740992'", "9007199254740992")]
        [TestCase("[decimal]'9223372036854775807.0'", "9223372036854775807")]
        public void ConvertToDecimal_Spec_6_6(string cmd, string expected)
        {
            ExecuteAndCompareTypedResult(cmd, decimal.Parse(expected, CultureInfo.InvariantCulture));
        }

        [ExpectedException]
        [TestCase("[decimal]'xyz'")]
        public void ConvertToDecimal_Spec_Erors_6_6(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd);
        }

        [TestCase("[string]$null", "")]
        [TestCase("[string][char]9731", "☃")]
        [TestCase("[string]1", "1")]
        [TestCase("[string]1.5", "1.5")]
        [TestCase("[string]-1", "-1")]
        [TestCase("[string]1.100d", "1.100")]
        [TestCase("[string][double]::NaN", "NaN")]
        [TestCase("[string][double]::PositiveInfinity", "Infinity")]
        [TestCase("[string][double]::NegativeInfinity", "-Infinity")]
        [TestCase("$OFS='x';[string](1..5)", "1x2x3x4x5")]
        [TestCase("[string](1..5)", "1 2 3 4 5")]
        [TestCase("$OFS=7;[string](1,2,(3,4),5)", "1727System.Object[]75")]
        [TestCase("[string](1,2,(3,4),5)", "1 2 System.Object[] 5")]
        [TestCase("[string][System.IO.FileAttributes]7", "ReadOnly, Hidden, System")]
        [TestCase("[string]{$a = 5}", "$a = 5", Explicit=true, Reason="Script block conversion is still missing")]
        [TestCase("[string]{ Get-ChildItem foo; $i++ }", " Get-ChildItem foo; $i++ ", Explicit = true, Reason = "Script block conversion is still missing")]
        [TestCase("$OFS=1.2;[string][System.Linq.Enumerable]::Range(1, 5)", "11,221,231,241,25"), SetCulture("de-DE")]
        [TestCase("[string][System.Linq.Enumerable]::Range(1, 5)", "1 2 3 4 5")]
        public void ConvertToString_Spec_6_8(string cmd, string expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void ConvertToStringFromHashtableDoesntEnumerate()
        {
            var cmd = "$OFS='|'; $h = @{foo='bar'; bar='baz'}; [string]$h";
            ExecuteAndCompareTypedResult(cmd, "System.Collections.Hashtable");
        }

        [Test]
        public void ConvertToStringFromArrayWithNull()
        {
            ExecuteAndCompareTypedResult("$OFS='|';[string]@(1,'f',$null,3)", "1|f||3");
        }

        [Test, ExpectedException, Explicit]
        public void ConvertToArray_Spec_6_9_TargetTypeMayNotBeMultidimensional()
        {
            ReferenceHost.RawExecute("[int[,]]5");
        }

        [Test]
        public void ConvertToArray_Spec_6_9_NullIsRetainedAsIs()
        {
            ExecuteAndCompareTypedResult("[int[]]$null", new object[] { null });
        }

        [Test]
        public void ConvertToArray_Spec_6_9_ScalarEndsUpInNewArray()
        {
            // TODO: can this be tested easier with RawExecute and via NUnit directly? I couldn't figure out how.
            ExecuteAndCompareTypedResult("([int[]]5).Length", 1);
            ExecuteAndCompareTypedResult("([int[]]5)[0]", 5);
        }

        [Test]
        public void ConvertToArray_Spec_6_9_ScalarEndsUpInNewArrayWithConversion()
        {
            // TODO: can this be tested easier with RawExecute and via NUnit directly? I couldn't figure out how.
            ExecuteAndCompareTypedResult("([int[]]'5').Length", 1);
            ExecuteAndCompareTypedResult("([int[]]'5')[0]", 5);
        }

        [Test]
        public void ConvertToArray_Spec_6_9_OneDimensionalArrayElementsGetConverted()
        {
            ExecuteAndCompareTypedResult("[double[]](1..5)", 1.0, 2.0, 3.0, 4.0, 5.0);
        }

        [Test, Explicit("This one fails in PowerShell, contrary to the spec")]
        public void ConvertToArray_Spec_6_9_MultiDimensionalArrayGetsFlattened()
        {
            ExecuteAndCompareTypedResult("$a = New-Object 'int[,]' 2,2; $a[0,0] = 10; $a[0,1] = 20; $a[1,0] = 30; $a[1,1] = 40; [int[]]$a", 10, 20, 30, 40);
        }

        [Test, Explicit("This one fails in PowerShell, contrary to the spec")]
        public void ConvertToArray_Spec_6_9_MultiDimensionalArrayGetsFlattenedAndElementsConverted()
        {
            ExecuteAndCompareTypedResult("$a = New-Object 'int[,]' 2,2; $a[0,0] = 10; $a[0,1] = 20; $a[1,0] = 30; $a[1,1] = 40; [string[]]$a", "10", "20", "30", "40");
        }

        [Test]
        public void ConvertToArray_Spec_6_9_StringToCharArrayWorks()
        {
            ExecuteAndCompareTypedResult("[char[]]'abc\u9731'", 'a', 'b', 'c', '\u9731');
        }

        [Test]
        public void ConvertToArray_Spec_6_9_OtherEnumerableType()
        {
            ExecuteAndCompareTypedResult("[string[]][System.Linq.Enumerable]::Range(1,5)", "1", "2", "3", "4", "5");
        }

        [Test]
        public void ConvertToXml_Spec_6_10_StringToXmlWorks()
        {
            var results = ReferenceHost.RawExecute("[xml]'<foo bar=\"baz\"></foo>'");
            Assert.AreEqual(1, results.Count);
            var result = results[0];
            Assert.AreEqual(typeof(XmlDocument), result.BaseObject.GetType());
        }

        [Test, Explicit("Seemingly PowerShell doesn't parse the scriptblock and thus doesn't care about syntax errors here.")]
        public void ConvertToXml_Spec_6_10_ScriptblockToXmlWorksByConvertingToStringFirst()
        {
            var results = ReferenceHost.RawExecute("[xml]{<foo bar='baz'></foo>}");
            Assert.AreEqual(1, results.Count);
            var result = results[0];
            Assert.AreEqual(typeof(XmlDocument), result.BaseObject.GetType());
        }

        [TestCase("[regex]'abc*'")]
        [TestCase("[regex]5")]
        public void ConvertToRegex_Spec_6_11(string cmd)
        {
            var results = ReferenceHost.RawExecute(cmd);
            Assert.AreEqual(1, results.Count);
            var result = results[0];
            Assert.AreEqual(typeof(Regex), result.BaseObject.GetType());
        }

        [Test, SetCulture("de-DE")]
        public void ConvertToRegex_Spec_6_11_ConversionToStringUsesInvariantCulture()
        {
            var results = ReferenceHost.RawExecute("[regex]1.2");
            Assert.AreEqual(1, results.Count);
            var result = results[0];
            Assert.AreEqual(typeof(Regex), result.BaseObject.GetType());
            var regex = (Regex)result.BaseObject;
            Assert.True(regex.IsMatch("1*2"));
        }

        [TestCase("[System.ConsoleColor]'Red'", ConsoleColor.Red)]
        [TestCase("[System.ConsoleColor]'12'", ConsoleColor.Red)]
        [TestCase("[System.ConsoleColor]'Red, Green'", ConsoleColor.Yellow)]
        [TestCase("[System.ConsoleColor]'Red,Green'", ConsoleColor.Yellow)]
        public void ConvertToEnum_Spec_6_13(string cmd, Enum expected)
        {
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, Combinatorial]
        public void ConvertToByteFromString_Spec_6_16(
            [Values(false, true)] bool leadingSpace,
            [Values(false, true)] bool trailingSpace,
            [Values("", "+", "0x")] string prefix,
            [Values(false, true)] bool hexadecimal)
        {
            string cmd = "[byte]'" +
                (leadingSpace ? "  " : "") +
                prefix + "42" +
                (trailingSpace ? " " : "") + "'";
            byte expected = prefix == "0x" ? (byte)0x42 : (byte)42;
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, Combinatorial]
        public void ConvertToIntFromString_Spec_6_16(
            [Values(false, true)] bool leadingSpace,
            [Values(false, true)] bool trailingSpace,
            [Values("", "+", "-", "0x")] string prefix,
            [Values(false, true)] bool hexadecimal)
        {
            string cmd = "[int]'" +
                (leadingSpace ? "  " : "") +
                prefix + "4242" +
                (trailingSpace ? " " : "") + "'";
            int expected = prefix == "0x" ? 0x4242 : 4242;
            if (prefix == "-") { expected *= -1; }
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, Combinatorial]
        public void ConvertToLongFromString_Spec_6_16(
            [Values(false, true)] bool leadingSpace,
            [Values(false, true)] bool trailingSpace,
            [Values("", "+", "-", "0x")] string prefix,
            [Values(false, true)] bool hexadecimal)
        {
            string cmd = "[long]'" +
                (leadingSpace ? "  " : "") +
                prefix + "4242424242424242" +
                (trailingSpace ? " " : "") + "'";
            long expected = prefix == "0x" ? 0x4242424242424242 : 4242424242424242;
            if (prefix == "-") { expected *= -1; }
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, Combinatorial]
        public void ConvertToFloatFromString_Spec_6_16(
            [Values(false, true)] bool leadingSpace,
            [Values(false, true)] bool trailingSpace,
            [Values("", "+", "-")] string prefix)
        {
            string cmd = "[float]'" +
                (leadingSpace ? "  " : "") +
                prefix + "0.75" +
                (trailingSpace ? " " : "") + "'";
            float expected = 0.75f;
            if (prefix == "-") { expected *= -1; }
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, Combinatorial]
        public void ConvertToDoubleFromString_Spec_6_16(
            [Values(false, true)] bool leadingSpace,
            [Values(false, true)] bool trailingSpace,
            [Values("", "+", "-")] string prefix)
        {
            string cmd = "[double]'" +
                (leadingSpace ? "  " : "") +
                prefix + "0.75" +
                (trailingSpace ? " " : "") + "'";
            double expected = 0.75;
            if (prefix == "-") { expected *= -1; }
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test, Combinatorial]
        public void ConvertToDecimalFromString_Spec_6_16(
            [Values(false, true)] bool leadingSpace,
            [Values(false, true)] bool trailingSpace,
            [Values("", "+", "-")] string prefix)
        {
            string cmd = "[decimal]'" +
                (leadingSpace ? "  " : "") +
                prefix + "0.75" +
                (trailingSpace ? " " : "") + "'";
            decimal expected = 0.75m;
            if (prefix == "-") { expected *= -1; }
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void ConvertUsingConstructor_Spec_6_18()
        {
            ExecuteAndCompareTypedResult("[version]'1.0'", new Version("1.0"));
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace TestHost
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TrueTest()
        {
            StringAssert.AreEqualIgnoringCase("True" + Environment.NewLine, TestHost.Execute("$true"));
        }

        [Test]
        public void IfTrueTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("if ($true) { 'xxx' }"));
        }

        [Test]
        public void IfFalseTest()
        {
            StringAssert.AreEqualIgnoringCase("yyy" + Environment.NewLine, TestHost.Execute("if ($false) { 'xxx' } else { 'yyy' }"));

        }
        [Test]
        public void IfTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("if (1 -eq 1) { 'xxx' }"));
            StringAssert.AreEqualIgnoringCase("", TestHost.Execute("if (1 -eq 2) { 'xxx' }"));
        }

        [Test]
        public void ElseifTest()
        {
            StringAssert.AreEqualIgnoringCase("yyy" + Environment.NewLine, TestHost.Execute("if (1 -eq 2) { 'xxx' } elseif (1 -eq 1) { 'yyy' }"));
        }

        [Test]
        public void ElseTest()
        {
            StringAssert.AreEqualIgnoringCase("yyy" + Environment.NewLine, TestHost.Execute("if (1 -eq 2) { 'xxx' } else { 'yyy' }"));
        }

        [Test]
        public void ComparisonTest()
        {
            StringAssert.AreEqualIgnoringCase("True" + Environment.NewLine, TestHost.Execute("1 -eq 1"));
        }

        [Test]
        public void ElementAccessTest()
        {
            StringAssert.AreEqualIgnoringCase("b" + Environment.NewLine, TestHost.Execute("'abc'[1]"));
        }

        [Test]
        public void ExecuteScriptTest()
        {
            string scriptPath = Path.GetTempFileName();
            scriptPath += ".ps1";
            File.WriteAllText(scriptPath, "'xxx'");

            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("& " + scriptPath));
        }

        [Test]
        public void AmpersandInvocationTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("& 'Write-Host' 'xxx'"));
        }

        [Test]
        public void FunctionTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("function f { 'xxx' } ; f"));
        }

        [Test(Description = "Issue#14")]
        public void TwoCommandsTest()
        {
            StringAssert.AreEqualIgnoringCase("a" + Environment.NewLine + "b" + Environment.NewLine, TestHost.Execute("'a' ; 'b'"));
        }

        [Test]
        public void SemicolonOnlyTest()
        {
            StringAssert.AreEqualIgnoringCase("", TestHost.Execute(";"));
        }

        [Test]
        public void SemicolonTerminatedTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("'xxx';"));
        }

        [Test]
        public void VariableTest()
        {
            StringAssert.AreEqualIgnoringCase("variable:\\" + Environment.NewLine, TestHost.Execute("Set-Location variable:", "$PWD"));
        }

        [Test]
        public void WriteVariableTest()
        {
            StringAssert.AreEqualIgnoringCase("$x = y" + Environment.NewLine + "y" + Environment.NewLine, TestHost.Execute("$x = 'y'", "$x"));
        }

        [Test, Explicit("correct behavior")]
        public void WriteVariableTestCorrect()
        {
            StringAssert.AreEqualIgnoringCase(Environment.NewLine, TestHost.Execute("$x = 'y'"));
        }

        [Test]
        public void PipelineTest()
        {
            Assert.AreEqual("xxx", TestHost.Execute("'xxx' | Write-Host -NoNewline"));
        }

        [Test]
        public void PipelineTest2()
        {
            Assert.AreEqual("xxx" + Environment.NewLine, TestHost.Execute("'xxx' | Write-Host"));
        }

        [Test]
        public void WriteOutputString()
        {
            Assert.AreEqual("xxx" + Environment.NewLine, TestHost.Execute("Write-Output 'xxx'"));
        }

        [Test]
        public void WriteHost()
        {
            Assert.AreEqual("xxx" + Environment.NewLine, TestHost.Execute("Write-Host 'xxx'"));
        }

        [Test]
        public void WriteHostNothing()
        {
            Assert.AreEqual(Environment.NewLine, TestHost.Execute("Write-Host"));
        }

        [Test]
        public void Ranges()
        {
            //// 7.4 Range operator
            //// Examples:
            //// 
            ////     1..10              # ascending range 1..10
            {
                var result = TestHost.Execute("1..10");

                var expected =
@"1
2
3
4
5
6
7
8
9
10
";

                Assert.AreEqual(expected, result);
            }

            //CollectionAssert.AreEqual(new[] { 3, 2, 1 }, (int[])TestHost.Execute("3..1"));

            //////    -500..-495          # descending range -500..-495
            //CollectionAssert.AreEqual(new[] { -500, -499, -498, -497, -496, -495 }, (int[])TestHost.Execute("-500..-495"));

            //////     16..16             # seqeunce of 1
            //CollectionAssert.AreEqual(new[] { 16 }, (int[])TestHost.Execute("16..16"));

            ////     
            ////     $x = 1.5
            ////     $x..5.40D          # ascending range 2..5
            ////     
            ////     $true..3           # ascending range 1..3
            ////     -2..$null          # ascending range -2..0
            ////    "0xf".."0xa"        # descending range 15..10           
        }

        [Test, Explicit("bug")]
        public void JaggedArrayTest()
        {
            // This should make a 2-element array, where the 2nd element is itself an array.
            var result = TestHost.Execute("$x = 1,2; (3,$x).Count");

            Assert.AreEqual(2, result);
        }

        [Test, Description("https://github.com/JayBazuzi/Pash2/issues/6")]
        public void UnrecognizedCommandBug()
        {
            // notice typo
            var result = TestHost.Execute(true, "Get-ChlidItem");

            Assert.AreEqual("Command 'Get-ChlidItem' not found.", result);
        }
    }
}

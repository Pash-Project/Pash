using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace TestHost
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void ExecuteScriptTest()
        {
            string scriptPath = Path.GetTempFileName();
            scriptPath += ".ps1";
            File.WriteAllText(scriptPath, "'xxx'");

            StringAssert.AreEqualIgnoringCase("xxx\r\n", TestHost.Execute("& " + scriptPath));
        }

        [Test]
        public void AmpersandInvocationTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx\r\n", TestHost.Execute("& 'Write-Host' 'xxx'"));
        }

        [Test]
        public void FunctionTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx\r\n", TestHost.Execute("function f { 'xxx' } ; f"));
        }

        [Test(Description = "Issue#14")]
        public void TwoCommandsTest()
        {
            StringAssert.AreEqualIgnoringCase("a\r\nb\r\n", TestHost.Execute("'a' ; 'b'"));
        }

        [Test]
        public void SemicolonOnlyTest()
        {
            StringAssert.AreEqualIgnoringCase("", TestHost.Execute(";"));
        }

        [Test]
        public void SemicolonTerminatedTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx\r\n", TestHost.Execute("'xxx';"));
        }

        [Test]
        public void VariableTest()
        {
            StringAssert.AreEqualIgnoringCase("variable:\\\r\n", TestHost.Execute("Set-Location variable:", "$PWD"));
        }

        [Test]
        public void WriteVariableTest()
        {
            StringAssert.AreEqualIgnoringCase("$x = y\r\ny\r\n", TestHost.Execute("$x = 'y'", "$x"));
        }

        [Test, Ignore("correct behavior")]
        public void WriteVariableTestCorrect()
        {
            StringAssert.AreEqualIgnoringCase("\r\n", TestHost.Execute("$x = 'y'"));
        }

        [Test]
        public void PipelineTest()
        {
            Assert.AreEqual("xxx", TestHost.Execute("'xxx' | Write-Host -NoNewline"));
        }

        [Test]
        public void PipelineTest2()
        {
            Assert.AreEqual("xxx\r\n", TestHost.Execute("'xxx' | Write-Host"));
        }

        [Test]
        public void WriteOutputString()
        {
            Assert.AreEqual("xxx\r\n", TestHost.Execute("Write-Output 'xxx'"));
        }

        [Test]
        public void WriteHost()
        {
            Assert.AreEqual("xxx\r\n", TestHost.Execute("Write-Host 'xxx'"));
        }

        [Test]
        public void WriteHostNothing()
        {
            Assert.AreEqual("\r\n", TestHost.Execute("Write-Host"));
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

        [Test, Ignore("bug")]
        public void JaggedArrayTest()
        {
            // This should make a 2-element array, where the 2nd element is itself an array.
            var result = TestHost.Execute("$x = 1,2; (3,$x).Count");

            Assert.AreEqual(2, result);
        }

    }
}

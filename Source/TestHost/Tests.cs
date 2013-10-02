// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
        public void GetHostName()
        {
            StringAssert.AreEqualIgnoringCase("TestHost" + Environment.NewLine, TestHost.Execute("(Get-Host).Name"));
        }

        [Test]
        public void RootPathTest()
        {
            StringAssert.AreEqualIgnoringCase(Path.GetPathRoot(Environment.CurrentDirectory) + Environment.NewLine, TestHost.Execute("Set-Location / ; Get-Location"));
        }

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
        public void IfEqualTest()
        {
            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("if (1 -eq 1) { 'xxx' }"));
            StringAssert.AreEqualIgnoringCase("", TestHost.Execute("if (1 -eq 2) { 'xxx' }"));
        }

        [Test]
        public void IfNotEqualTest()
        {
            StringAssert.AreEqualIgnoringCase("", TestHost.Execute("if (1 -ne 1) { 'xxx' }"));
            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("if (1 -ne 2) { 'xxx' }"));
        }

        [Test]
        public void AnotherIfTest()
        {
            var result = TestHost.Execute(
                @"$x = ""hi""",
                @"if ($x.Length -ne 2) { write-host ""xxx"" }",
                @"if ($x.Length -ne 3) { write-host ""yyy"" }"
                );

            StringAssert.AreEqualIgnoringCase("yyy" + Environment.NewLine, result);
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

        [TestCase(@"1 -eq 1", "True")]
        [TestCase(@"1 -eq 2", "False")]
        [TestCase(@"'abc' -eq 'abc'", "True")]
        [TestCase(@"'abc' -eq 'ghi'", "False")]
        [TestCase(@"$true -eq $true", "True")]
        [TestCase(@"$true -eq $false", "False")]
        [TestCase(@"$test = $null; $test -eq $null", "True")]
        [TestCase(@"'abc' -eq $null", "False")]
        [TestCase(@"'abc' -eq 'ABC'", "True")]
        public void ComparisonTest(string input, string expected)
        {
            string result = TestHost.Execute(input);

            StringAssert.AreEqualIgnoringCase(expected + Environment.NewLine, result);
        }

        [Test]
        public void ElementAccessTest()
        {
            StringAssert.AreEqualIgnoringCase("b" + Environment.NewLine, TestHost.Execute("'abc'[1]"));
        }

        [Test]
        public void ExecuteScriptTest()
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            scriptPath += ".ps1";
            File.WriteAllText(scriptPath, "'xxx'");

            StringAssert.AreEqualIgnoringCase("xxx" + Environment.NewLine, TestHost.Execute("& " + scriptPath));

            File.Delete(scriptPath);
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
            System.Management.Path path = "";

            var expectedPath = "Variable:" + path.CorrectSlash;
            var actualPath = TestHost.Execute("Set-Location variable:", "Get-Location").Trim();

            StringAssert.AreEqualIgnoringCase(expectedPath, actualPath);
        }

        [Test]
        public void WriteVariableTest()
        {
            StringAssert.AreEqualIgnoringCase("y" + Environment.NewLine, TestHost.Execute("$x = 'y'", "$x"));
        }

        [Test]
        public void WriteVariableTestCorrect()
        {
            StringAssert.AreEqualIgnoringCase("", TestHost.Execute("$x = 'y'"));
        }

        [Test]
        public void WriteVariableTestCorrect2()
        {
            StringAssert.AreEqualIgnoringCase("y" + Environment.NewLine, TestHost.Execute("($x = 'y')"));
        }

        [Test]
        [TestCase(@"$a = $b = 0				# value not written to pipeline", "")]
        [TestCase(@"$a = ($b = 0)			# value not written to pipeline", "")]
        [TestCase(@"($a = ($b = 0))		# pipeline gets 0", "0")]
        public void TopLevelSideEffects(string input, string expected)
        {
            var result = TestHost.Execute(input);
            Assert.AreEqual(expected, result.Replace(Environment.NewLine, ""));
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

                var expected = Enumerable.Range(1, 10).JoinString(Environment.NewLine) + Environment.NewLine;

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

        [Test]
        public void SequenceInPipeline()
        {
            //// 7.4 Range operator
            //// Examples:
            //// 
            ////     1..10              # ascending range 1..10
            {
                var result = TestHost.Execute("1..10 | Write-Host");

                var expected = Enumerable.Range(1, 10).JoinString(Environment.NewLine) + Environment.NewLine;

                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void JaggedArrayTest()
        {
            // This should make a 2-element array, where the 2nd element is itself an array.
            var result = TestHost.Execute("$x = 1,2; (3,$x).Count");

            Assert.AreEqual(2 + Environment.NewLine, result);
        }

        [Test, Description("https://github.com/Pash-Project/Pash/issues/6")]
        public void UnrecognizedCommandBug()
        {
            // notice typo
            var result = TestHost.ExecuteWithZeroErrors("Get-ChlidItem");

            Assert.AreEqual("Exception: Command 'Get-ChlidItem' not found.", result);
        }

        [Test]
        public void GetChildItemFromRootDefaultProviderShouldReturnSomething()
        {
            var result = TestHost.ExecuteWithZeroErrors("Get-ChildItem /");

            Assert.Greater(result.Length, 0);
        }

        [Test]
        public void WriteHostArray()
        {
            var result = TestHost.ExecuteWithZeroErrors("Write-Host 1,aaa,$true");

            Assert.AreEqual(
                "1 aaa True" + Environment.NewLine,
                result
                );
        }

        [Test]
        public void OutDefaultArray()
        {
            var result = TestHost.ExecuteWithZeroErrors("1,$true");

            Assert.AreEqual(
                "1" + Environment.NewLine + "True" + Environment.NewLine,
                result
                );
        }

        [Test]
        public void ArrayIndex()
        {
            var result = TestHost.ExecuteWithZeroErrors("(10..12)[1]");

            Assert.AreEqual(
                "11" + Environment.NewLine,
                result
            );
        }

        [Test]
        [TestCase(@"$x++ ; $x", "2")]
        [TestCase(@"$x++ ; $x++ ; $x", "3")]
        [TestCase(@"$y = $x++ ; $y", "1")]

        [TestCase(@"++$x ; $x", "2")]
        [TestCase(@"(++$x)", "2")]
        [TestCase(@"$y = ++$x ; $y", "2")]
        public void IncrementDecrement(string input, string expected)
        {
            var result = TestHost.Execute(
                "$x = 1",
                input
                );

            Assert.AreEqual(
                expected + Environment.NewLine,
                result
                );
        }

        [TestFixture]
        class SortObjectTests
        {
            [Test]
            public void IntArray()
            {
                var result = TestHost.ExecuteWithZeroErrors("2,4,3 | Sort-Object");

                Assert.AreEqual(
                    new[] { 2, 3, 4 }.JoinString(Environment.NewLine) + Environment.NewLine,
                    result
                );
            }

            [Test]
            public void Singleton()
            {
                var result = TestHost.ExecuteWithZeroErrors("1 | Sort-Object");

                Assert.AreEqual(
                    "1" + Environment.NewLine,
                    result
                );
            }

            [Test]
            public void Null()
            {
                var result = TestHost.ExecuteWithZeroErrors("$null | Sort-Object");

                Assert.AreEqual(
                    "",
                    result
                );
            }

            [Test]
            public void ByProperty()
            {
                var result = TestHost.Execute("Get-ChildItem | Sort-Object Name");
            }

            [Test]
            public void ByPropertyLowercase()
            {
                var result = TestHost.Execute("Get-ChildItem | Sort-Object name");
            }
        }

        [Test]
        public void ScriptBlock()
        {
            var result = TestHost.Execute("& { 1 }");

            Assert.AreEqual("1" + Environment.NewLine, result);
        }

        [Test]
        public void Return()
        {
            var result = TestHost.Execute("& { return; 1 } ; 2");

            Assert.AreEqual("2" + Environment.NewLine, result);
        }

        [Test]
        public void EnumParameter()
        {
            var result = TestHost.Execute("[Text.RegularExpressions.Regex]::IsMatch('FOO', 'foo', [Text.RegularExpressions.RegexOptions] 'IgnoreCase' )");

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void For()
        {
            var result = TestHost.Execute("for ($i = 0; $i -ile 10; $i++) { $i }");

            Assert.AreEqual(Enumerable.Range(0, 11).JoinString(Environment.NewLine) + Environment.NewLine, result);
        }

        [Test]
        public void ForEach()
        {
            string result = TestHost.Execute("foreach ($i in (0..10)) { $i }");

            Assert.AreEqual(Enumerable.Range(0, 11).JoinString(Environment.NewLine) + Environment.NewLine, result);
        }

        [Test]
        public void ForEachCharacterInString()
        {
            string result = TestHost.Execute("foreach ($char in 'abc') { $char }");

            Assert.AreEqual(string.Format("a{0}b{0}c{0}", Environment.NewLine), result);
        }

        [Test]
        public void ExpressionsAsParameters()
        {
            var result = TestHost.Execute(
                "$x = 5",
                "Write-Host $x"
                );

            Assert.AreEqual("5" + Environment.NewLine, result);
        }

        [Test]
        public void ForeachObjectCmdlet()
        {
            var result = TestHost.Execute(
                "1,2,3 | ForEach-Object { $_ }"
                );

            Assert.AreEqual(
                "1" + Environment.NewLine +
                "2" + Environment.NewLine +
                "3" + Environment.NewLine,
                result
                );
        }

        [Test]
        [Explicit]
        [Description("`ForEach-Object` can take many script blocks. The first one runs **once** before everything else; the last one runs **once** after everything else; the rest run once for each input")]
        // I changed `ForEachObjectCommand.Process` from `ScriptBlock[]` to `ScriptBlock`, to work around a limitation in `CommandProcessor.BindArgument()`. Once that is fixed, this test can be reenabled.
        public void ForeachObjectCmdletManyScriptBlocks()
        {
            var result = TestHost.Execute(
                "1..3 | % { 'a' + $_ } { 'b' + $_ } { 'c' + $_ } { 'd' + $_ } { 'e' + $_ }"
                );

            Assert.AreEqual(
                "a" + Environment.NewLine +
                "b1" + Environment.NewLine +
                "c1" + Environment.NewLine +
                "d1" + Environment.NewLine +
                "b2" + Environment.NewLine +
                "c2" + Environment.NewLine +
                "d2" + Environment.NewLine +
                "b3" + Environment.NewLine +
                "c3" + Environment.NewLine +
                "d3" + Environment.NewLine +
                "e" + Environment.NewLine,
                result
                );
        }

        [TestCase(@"8 -ge 8", "True")]
        [TestCase(@"8 -ge 7", "True")]
        [TestCase(@"8 -ge 9", "False")]
        public void GreaterOrEqualToTest(string input, string expected)
        {
            string result = TestHost.Execute(input);

            StringAssert.AreEqualIgnoringCase(expected + Environment.NewLine, result);
        }

        [Test]
        public void HashTableIntegerIndexer()
        {
            string result = TestHost.Execute(
                "$test = new-object System.Collections.Hashtable",
                "$test.Add(0, 'value')",
                "$test[0]");

            StringAssert.AreEqualIgnoringCase("value" + Environment.NewLine, result);
        }

        [Test]
        public void HashTableStringIndexer()
        {
            string result = TestHost.Execute(
                "$test = new-object System.Collections.Hashtable",
                "$test.Add('name', 'value')",
                "$test['name']"
               );

            StringAssert.AreEqualIgnoringCase("value" + Environment.NewLine, result);
        }

        [Test]
        public void Exit()
        {
            string result = TestHost.Execute(
                @"& { Write-Host 'before'; exit; Write-Host 'after' }"
            );

            StringAssert.AreEqualIgnoringCase("before" + Environment.NewLine, result);
        }

        [TestFixture]
        class TryCatchTests
        {
            [Test]
            public void TryBlockStatementIsExecuted()
            {
                string result = TestHost.Execute(@"try { Write-Host 'try' } catch { }");

                StringAssert.AreEqualIgnoringCase("try" + Environment.NewLine, result);
            }

            [Test]
            public void TryBlockThrowsExceptionAndSingleStatementInCatchBlockIsExecuted()
            {
                string result = TestHost.Execute(@"try { $null.GetType() } catch { Write-Host 'catch' }");

                StringAssert.AreEqualIgnoringCase("catch" + Environment.NewLine, result);
            }

            [Test]
            public void TryBlockReturnsAndSingleStatementInCatchBlockIsNotExecuted()
            {
                string result = TestHost.Execute(@"try { Write-Host 'exiting'; return } catch { Write-Host 'catch' }");

                StringAssert.AreEqualIgnoringCase("exiting" + Environment.NewLine, result);
            }

            [Test]
            public void TryBlockThrowsExceptionAndErrorRecordAvailableInCatchBlock()
            {
                string result = TestHost.Execute(@"try { 'abc'.Substring(-1) } catch { Write-Host $_.GetType() }");

                StringAssert.AreEqualIgnoringCase("System.Management.Automation.ErrorRecord" + Environment.NewLine, result);
            }
        }

        [TestCase(@"$i = 1; $i += 10; Write-Host $i", "11")]
        [TestCase(@"$x = 'a'; $x += 'b'; Write-Host $x", "ab")]
        public void AssignmentByAdditionOperator(string input, string expected)
        {
            string result = TestHost.Execute(input);

            StringAssert.AreEqualIgnoringCase(expected + Environment.NewLine, result);
        }
        
        [TestCase(@"$i = 11; $i -= 10; Write-Host $i", "1")]
        public void AssignmentBySubtractionOperator(string input, string expected)
        {
            string result = TestHost.Execute(input);

            StringAssert.AreEqualIgnoringCase(expected + Environment.NewLine, result);
        }

        [TestCase(@"$i = 2; $i *= 10; Write-Host $i", "20")]
        public void AssignmentByMultiplicationOperator(string input, string expected)
        {
            string result = TestHost.Execute(input);

            StringAssert.AreEqualIgnoringCase(expected + Environment.NewLine, result);
        }

        [TestCase(@"$i = 6; $i /= 2; Write-Host $i", "3")]
        public void AssignmentByDivisionOperator(string input, string expected)
        {
            string result = TestHost.Execute(input);

            StringAssert.AreEqualIgnoringCase(expected + Environment.NewLine, result);
        }
    }
}

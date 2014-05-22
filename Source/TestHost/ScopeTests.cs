using NUnit.Framework;
using System;
using System.IO;

namespace TestHost
{
    [TestFixture]
    public class ScopeTests
    {
        public ScopeTests()
        {
        }

        [TearDown]
        public void RemoveScriptFile()
        {
            File.Delete(GetScriptFileName());
        }

        private string formatLines(string[] lines)
        {
            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }

        private string GetScriptFileName()
        {
            string directory = Path.GetDirectoryName(typeof(ScopeTests).Assembly.Location);
            return Path.Combine(directory, "ScopeTests.ps1");
        }

        private string CreateScript(string script)
        {
            string fileName = GetScriptFileName();
            File.WriteAllText(fileName, script);

            return fileName;
        }

        [Test]
        public void ScriptBlockVariableTest()
        {
            string statement = "$a=0; Write-Host $a; & { $a = 10; Write-Host $a }; Write-Host $a;";
            string result = TestHost.Execute(statement);
            Assert.AreEqual(formatLines(new string[] {"0", "10", "0"}), result);
        }

        [Test]
        public void DotSourceScriptBlockVariableTest()
        {
            string statement = "$a=0; Write-Host $a; . { $a = 10; Write-Host $a }; Write-Host $a;";
            string result = TestHost.Execute(statement);
            Assert.AreEqual(formatLines(new string[] {"0", "10", "10"}), result);
        }

        //the following test corresponds to the VariableAccessTest, but with user input instead of using the internals
        [TestCase("x", "f")] //correct x is fetched in general (from function scope)
        [TestCase("local:x", "")] //local scope has no variable x
        [TestCase("script:x", "")] //sx is private in the script scope
        [TestCase("global:x", "g")]
        [TestCase("y", "l")] //the overridden y in the local scope
        [TestCase("local:y", "l")] //also the local one, but explicitly
        [TestCase("script:y", "s")]
        [TestCase("global:y", "g")]
        [TestCase("z", "s")] //ignores the private z in function scope
        public void ComplexVariableScopeTest(string varname, string expected)
        {
            string script = CreateScript(formatLines(new string[] {
                "$private:x = \"s\";",
                "$y=\"s\";",
                "$z=\"s\";",
                "function foo",
                "{",
                "  $x=\"f\";",
                "  $y=\"f\";",
                "  $private:z=\"f\";",
                "  & {",
                "      $y=\"l\";",
                String.Format("Write-Host ${0};", varname),
                "  };",
                "};",
                "foo"
            }));

            string statement = formatLines(new string[] {
                "$x=\"g\";",
                "$y=\"g\";",
                String.Format("& '{0}';", script)
            });
            string result = TestHost.Execute(statement);
            Assert.AreEqual(expected + Environment.NewLine, result);
        }

        [Test]
        public void ExecuteScriptBlockInNewScopeTest()
        {
            string statement = "$a=0; Write-Host $a; & { $a = 10; Write-Host $a }; Write-Host $a;";
            string result = TestHost.Execute(statement);
            Assert.AreEqual(formatLines(new string[] {"0", "10", "0"}), result);
        }

        [Test]
        public void FunctionScopeTest()
        {
            string script = CreateScript(formatLines(new string[] {
                "function foo { Write-Host 'sfoo'; };",
                "function bar { Write-Host 'sbar'; };",
                "function private:baz { Write-Host 'sbaz'; };",
                " & {",
                "   function bar { Write-Host 'lbar'; };",
                "   bar; script:bar; global:bar;", //prints "lbar", "sbar", "gbar"
                "   baz;", //prints "gbaz", because script baz is private
                "   foo;", //"prints sfoo"
                " };",
                "foo; bar; baz;" //prints "sfoo", "sbar", "sbaz"
            }));
            var statement = formatLines(new string[] {
                "function foo { Write-Host 'gfoo'; };",
                "function bar { Write-Host 'gbar'; };",
                "function baz { Write-Host 'gbaz'; };",
                String.Format("& '{0}';", script),
                "foo; bar; baz;" //prints "gfoo", "gbar", "gbaz"
            });
            string expected = formatLines(new string[] {
                "lbar", "sbar", "gbar",
                "gbaz",
                "sfoo",
                "sfoo", "sbar", "sbaz",
                "gfoo", "gbar", "gbaz"
            });
            Assert.AreEqual(expected, TestHost.Execute(statement));
        }

        [Test]
        public void FunctionOverwriteScopeTest()
        {
            string script = CreateScript(formatLines(new string[] {
                "function global:foo { Write-Host 'sfoo'; };",
                "function bar { Write-Host 'sbar'; };",
                "bar;", //prints "sbar"
                " & {",
                "  function script:bar { Write-Host 'lbar'; };", //ovewrites script's bar function
                "};",
                "bar;" //prints "lbar" now
            }));
            var statement = formatLines(new string[] {
                "function foo { Write-Host 'gfoo'; };",
                "function bar { Write-Host 'gbar'; };",
                "foo; bar;", //prints "gfoo", "gbar"
                String.Format("& '{0}';", script),
                "foo; bar;", //prints "sfoo", "gbar"
            });
            string expected = formatLines(new string[] {
                "gfoo", "gbar",
                "sbar",
                "lbar",
                "sfoo", "gbar"
            });
            Assert.AreEqual(expected, TestHost.Execute(statement));
        }
    }
}


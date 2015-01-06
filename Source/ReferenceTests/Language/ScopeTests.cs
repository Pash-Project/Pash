using NUnit.Framework;
using System;
using System.IO;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class ScopeTests : ReferenceTestBase
    {
        [Test]
        public void ScriptBlockVariableTest()
        {
            string statement = "$a=0; $a; & { $a = 10; $a }; $a;";
            string result = ReferenceHost.Execute(statement);
            Assert.AreEqual(NewlineJoin("0", "10", "0"), result);
        }

        [Test]
        public void DotSourceScriptBlockVariableTest()
        {
            string statement = "$a=0; $a; . { $a = 10; $a }; $a;";
            string result = ReferenceHost.Execute(statement);
            Assert.AreEqual(NewlineJoin("0", "10", "10"), result);
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
            string script = CreateFile(NewlineJoin(
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
                "      $" + varname + ";",
                "  };",
                "};",
                "foo"
            ), "ps1");

            string statement = NewlineJoin(
                "$x=\"g\";",
                "$y=\"g\";",
                String.Format("& '{0}';", script)
            );
            string result = ReferenceHost.Execute(statement);
            Assert.AreEqual(NewlineJoin(expected), result);
        }

        [Test]
        public void FunctionScopeTest()
        {
            string script = CreateFile(NewlineJoin(
                "function foo { 'sfoo'; };",
                "function bar { 'sbar'; };",
                "function private:baz { 'sbaz'; };",
                "& {",
                "   function bar { 'lbar'; };",
                "   bar; script:bar; global:bar;", //prints "lbar", "sbar", "gbar"
                "   baz;", //prints "gbaz", because script baz is private
                "   foo;", //"prints sfoo"
                " };",
                "foo; bar; baz;" //prints "sfoo", "sbar", "sbaz"
            ), "ps1");
            var statement = NewlineJoin(
                "function foo { 'gfoo'; };",
                "function bar { 'gbar'; };",
                "function baz { 'gbaz'; };",
                String.Format("& '{0}';", script),
                "foo; bar; baz;" //prints "gfoo", "gbar", "gbaz"
            );
            string expected = NewlineJoin(
                "lbar", "sbar", "gbar",
                "gbaz",
                "sfoo",
                "sfoo", "sbar", "sbaz",
                "gfoo", "gbar", "gbaz"
            );
            Assert.AreEqual(expected, ReferenceHost.Execute(statement));
        }

        [Test]
        public void FunctionOverwriteScopeTest()
        {
            string script = CreateFile(NewlineJoin(
                "function global:foo {'sfoo'; };",
                "function bar { 'sbar'; };",
                "bar;", //prints "sbar"
                " & {",
                "  function script:bar { 'lbar'; };", //ovewrites script's bar function
                "};",
                "bar;" //prints "lbar" now
            ), "ps1");
            var statement = NewlineJoin(
                "function foo { 'gfoo'; };",
                "function bar { 'gbar'; };",
                "foo; bar;", //prints "gfoo", "gbar"
                String.Format("& '{0}';", script),
                "foo; bar;" //prints "sfoo", "gbar"
            );
            string expected = NewlineJoin(
                "gfoo", "gbar",
                "sbar",
                "lbar",
                "sfoo", "gbar"
            );
            Assert.AreEqual(expected, ReferenceHost.Execute(statement));
        }
    }
}


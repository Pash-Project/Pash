using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.IO;

namespace ReferenceTests.Commands
{
    class ImportModuleTests : ReferenceTestBase
    {
        [Test]
        public void CanImportScriptModule()
        {
            var module = CreateFile("function foo {'works'}", "psm1");
            var cmd = "Import-Module '" + module + "'; foo";
            var expected = NewlineJoin("works");
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(expected));
        }

        [Test]
        public void ImportModuleReturnsCorrectObject()
        {
            var module = CreateFile("function foo {'works'}", "psm1");
            var cmd = "Import-Module '" + module + "' -PassThru";
            var res = ReferenceHost.RawExecute(cmd);
            Assert.That(res.Count, Is.EqualTo(1));
            var mod = res[0].BaseObject as PSModuleInfo;
            Assert.That(mod, Is.Not.Null);
            Assert.That(mod.ExportedFunctions.Keys, Contains.Item("foo"));
            Assert.That(mod.Name, Is.EqualTo(Path.GetFileNameWithoutExtension(module)));
            Assert.That(mod.Path, Is.EqualTo(module));
        }

        [Test]
        public void ImportingAModuleTwiceReturnsTheSameReferenceAndIsNotLoadedTwice()
        {
            var module = CreateFile(NewlineJoin(
                "$x = 1",
                "function foo {$x}",
                "Export-ModuleMember -Variable x -Function foo"
            ), "psm1");
            var cmd = NewlineJoin(
                "$m1 = Import-Module '" + module + "' -PassThru;",
                "$x; foo;", // make sure we get the correct values
                "$x = 2; $x; foo", // modify the value, check that the module internal value is modified
                "$m2 = Import-Module '" + module + "' -PassThru;",
                "$x; foo", // make sure it's still the modified value
                "[object]::ReferenceEquals($m1, $m2)" // check that we returned indeed the same module object
            );
            ExecuteAndCompareTypedResult(cmd, 1, 1, 2, 2, 2, 2, true);
        }

        [Test]
        [Ignore("How can PS throw and MethodInvocationException from inside a cmdlet?")]
        public void WrongScriptModuleExtensionThrows()
        {
            var module = CreateFile("function foo {'fail'}", "psm3");
            Assert.Throws<MethodInvocationException>(delegate {
                ReferenceHost.Execute("Import-Module '" + module + "';");
            });
        }

        [TestCase("")]
        [TestCase("-Scope 'Global'")]
        [TestCase("-Global")]
        [TestCase("-Global:$false")] // strange, but PS behavior
        public void CanImportScriptModuleFromFunctionToGlobalScope(string scopeParam)
        {
            var module = CreateFile("function testfun {'globalWorks'}", "psm1");
            var cmd = NewlineJoin(
                "function doImport {",
                "  Import-Module '" + module + "' " + scopeParam + ";",
                "  testfun;",
                "}",
                "doImport;"
                );
            var expected = NewlineJoin("globalWorks");
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(expected));
            ReferenceHost.RawExecuteInLastRunspace("testfun");
            Assert.That(ReferenceHost.LastResults, Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase("-Scope 'Global'")]
        [TestCase("-Global")]
        [TestCase("-Global:$false")] // strange, but PS behavior
        public void CanImportScriptModuleFromScriptToGlobalScope(string scopeParam)
        {
            var module = CreateFile("function testfun {'globalWorks'}", "psm1");
            var script = CreateFile( "Import-Module '" + module + "' " + scopeParam + ";", "ps1");
            var cmd = "& '" + script + "'; testfun";
            var expected = NewlineJoin("globalWorks");
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(expected));
            ReferenceHost.RawExecuteInLastRunspace("testfun");
            Assert.That(ReferenceHost.LastResults, Is.EqualTo(expected));
        }

        [Test]
        public void CanImportScriptModuleFromFunctionOnlyToLocalScope()
        {
            var module = CreateFile("function testfun {'localWorks'}", "psm1");
            var cmd = NewlineJoin(
                "function doImport {",
                "  Import-Module '" + module + "' -Scope 'Local';",
                "  testfun;",
                "}",
                "doImport;"
            );
            var expected = NewlineJoin("localWorks");
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(expected));
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("testfun");
            });
        }

        [Test]
        public void CanImportScriptModuleFromScriptOnlyToLocalScope()
        {
            var module = CreateFile("function testfun {'localWorks'}", "psm1");
            var script = CreateFile("Import-Module '" + module + "' -Scope 'local'; testfun", "ps1");
            var cmd = "& '" + script + "';";
            var expected = NewlineJoin("localWorks");
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(expected));
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("testfun");
            });
        }

        [Test]
        public void ModuleScopeDerivesFromGlobalScopeOnly()
        {
            var module = CreateFile(NewlineJoin(
                "$x = 'm';",
                "function foo {",
                "  $x = 'mf';",
                "  \"x = $x\";",
                "  \"script:x = \" + $script:x;",
                "  \"global:x = \" + $global:x;",
                "  \"gv 1 = \" + (Get-Variable -Scope 1 x).Value;",
                "  \"gv 2 = \" + (Get-Variable -Scope 2 x).Value;",
                "};"
             ), "psm1");
            var script = CreateFile(NewlineJoin(
                "$x = 's';",
                "Import-Module '" + module + "';",
                "& { $x = 'sf'; foo }"
            ), "ps1");
            var statement = NewlineJoin(
                "$x = 'g';",
                "& { ",
                "  $x = 'gf';",
                "  & '" + script + "';",
                "}"
            );
            var expected = NewlineJoin(
                "x = mf",
                "script:x = m",
                "global:x = g",
                "gv 1 = m",
                "gv 2 = g"
            );
            Assert.That(ReferenceHost.Execute(statement), Is.EqualTo(expected));
        }

        // TODO: tests for modules importing modules to check scope derivance and execution
        // TODO: test that checks what happens if the script module returns a value -> shouldn't
        // TODO: test that checks the behavior if both the Global and the Scope parameter are set -> exception
        // TODO: test if module exports a variable that overwrites an existing one. which PSVariable *object* will be used?
        // TODO: what if another module with the same name is loaded -> two modules, different path
    }
}

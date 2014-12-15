using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.IO;
using TestPSSnapIn;

namespace ReferenceTests.Commands
{
    public class ImportModuleTests : ModuleCommandTestBase
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
        public void CanImportAssemblyModule()
        {
            var cmd = "Import-Module '" + BinaryTestModule + "'; " + CmdletName(typeof(TestCommand));
            ExecuteAndCompareTypedResult(cmd, TestCommand.OutputString);
        }

        [Test]
        public void CanImportManifestModule()
        {
            var module = CreateFile(CreateManifest(null, "Me", "FooComp", "1.0"), "psd1");
            var cmd = "$m = Import-Module '" + module + "' -PassThru;"
                + "$m.Author; $m.CompanyName; $m.ModuleType; $m.Version";
            ExecuteAndCompareTypedResult(cmd, "Me", "FooComp", ModuleType.Manifest, new Version("1.0"));
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
            var scriptRootVar = mod.SessionState.PSVariable.Get("PSScriptRoot");
            Assert.That(scriptRootVar, Is.Not.Null);
            Assert.That(scriptRootVar.Value, Is.EqualTo(Path.GetDirectoryName(module)));
        }

        [Test]
        public void ImportedScriptModuleKnowsScriptRoot()
        {
            var module = CreateFile("function foo { $PSScriptRoot }", "psm1");
            var cmd = "Import-Module '" + module + "'; foo";
            ExecuteAndCompareTypedResult(cmd, Path.GetDirectoryName(module));
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
                "function foo { return 3; }; foo", // overwrite foo
                "$m2 = Import-Module '" + module + "' -PassThru;",
                "$x; foo", // make sure it's still the modified value and that foo got re-imported
                "[object]::ReferenceEquals($m1, $m2)" // check that we returned indeed the same module object
            );
            ExecuteAndCompareTypedResult(cmd, 1, 1, 2, 2, 3, 2, 2, true);
        }

        [Test]
        public void WrongScriptModuleExtensionThrows()
        {
            var module = CreateFile("function foo {'fail'}", "psm3");
            Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute("Import-Module '" + module + "';");
            });
        }

        [Test]
        public void ModuleExtensionCheckIsCaseInvariant()
        {
            var module = CreateFile("function foo {'works'}", "PSm1");
            ExecuteAndCompareTypedResult("Import-Module '" + module + "'; foo", "works");
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

        [Test]
        public void ImportingAnInvalidManifestFails()
        {
            var module = CreateFile(NewlineJoin(CreateManifest(null, "Me", "FooComp", "1.0") + "'ab'"), "psd1");
            Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute("Import-Module '" + module + "'");
            });
        }

        [Test]
        public void ImportingAManifestWithRootModuleAndModuleToProcessFails()
        {
            var manifest = new Dictionary<string, string>() {
               { "RootModule", "foo" }, { "ModuleToProcess", "bar" }, { "ModuleVersion", "1.0"} };
            var module = CreateFile(NewlineJoin(CreateManifest(manifest)), "psd1");
            Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute("Import-Module '" + module + "'");
            });
        }

        [Test]
        [Timeout(3000)]
        public void ImportingAManifestWithItselfAsRootModule()
        {
            var path = Path.Combine(Path.GetTempPath(), "manifTest.psd1");
            File.WriteAllText(path, CreateManifest(path, "Test", "Foo", "1.0"));
            AddCleanupFile(path);
            var e = Assert.Throws<CmdletInvocationException>(delegate {
                ReferenceHost.Execute("Import-Module '" + path + "'");
            });
            //Assert.That(e.ErrorRecord.FullyQualifiedErrorId.Equals("foo"));
        }

        [Test]
        public void NestedManifestAddInformationAndFirstManifestRestrictExportsFromTargetModuleIfNotEmpty()
        {
            var scriptModule = CreateFile(NewlineJoin(
                "function foo {'foo'}",
                "function bar {'bar'}",
                "$x = 1",
                "$y = 2",
                "New-Alias baz bar",
                "Export-ModuleMember -Function foo -Var *"
            ), "psm1");
            var nestedManifest = CreateFile(CreateManifest(scriptModule, "The Guy", null, "1.0", "@()", "y"), "psd1");
            var manifest = CreateFile(CreateManifest(nestedManifest, "Me", "MyComp", "1.0", "bar", "x"), "psd1");
            var res = ReferenceHost.RawExecute("Import-Module '" + manifest + "' -PassThru; $x; $y");
            Assert.That(res.Count, Is.EqualTo(3));
            Assert.That(res[1], Is.Null); // nestedManifest only allows y to be exported
            Assert.That(res[2].BaseObject, Is.EqualTo(2)); // all 3 modules export it
            var module = res[0].BaseObject as PSModuleInfo;
            Assert.That(module.Author, Is.EqualTo("The Guy")); // was overwritten by nestedManifest
            Assert.That(module.CompanyName, Is.EqualTo("MyComp")); // still original from manifest
            Assert.That(module.ModuleType, Is.EqualTo(ModuleType.Script)); // as the last module was a script
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo"); // not imported due to no function exports in manifest
            });
        }

        /*
         * -Global or -Scope global: Import module and items to global session state
         * no options: import module to current modules session state table, import items to module scope
         * -scope local: import module to current modules session state table, import items to *local scope*
         */

        // TODO: module import overwrites existing module or variable
        // TODO: -global: export to global session state, not current module
        // TODO: make sure that only the most nested manifest defines the exports, but only if the manifest export list is defined
        // TODO: test that a manifest hashtable must not include an unknown member
        // TODO: test that manifest needs a version
        // TODO: test that manifest module.Path is path of nested RootModule
        // TODO: test with get-module that modules are actually always in the sessionstate of the current module (or global)
        // TODO: test that reimporting doesn't reload the module, but re-imports the exported module members (to local or global sessionstate)
        // TODO: test that modules are only loaded by path if a slash is in the name
        // TODO: tests for modules importing modules to check scope derivance and execution
        // TODO: test that checks what happens if the script module returns a value -> shouldn't
        // TODO: test that checks the behavior if both the Global and the Scope parameter are set -> exception
        // TODO: test if module exports a variable that overwrites an existing one. which PSVariable *object* will be used?
        // TODO: what if another module with the same name is loaded -> two modules, different path
    }
}

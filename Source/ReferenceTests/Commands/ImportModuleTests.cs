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
        public void ImportingAModuleTwiceReturnsTheSameReferenceButIsLoadedTwice()
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
            var e = Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute("Import-Module '" + module + "';");
            });
            Assert.That(e.Errors[0].Exception is InvalidOperationException);
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
            var e = Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute("Import-Module '" + module + "'");
            });
            Assert.That(e.Errors[0].Exception is ArgumentException);
        }

        [Test]
        public void ImportingAManifestWithRootModuleAndModuleToProcessFails()
        {
            var manifest = new Dictionary<string, string>() {
               { "RootModule", "foo" }, { "ModuleToProcess", "bar" }, { "ModuleVersion", "1.0"} };
            var module = CreateFile(NewlineJoin(CreateManifest(manifest)), "psd1");
            var e = Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute("Import-Module '" + module + "'");
            });
            Assert.That(e.Errors[0].Exception is InvalidOperationException);
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
            Assert.That(e.ErrorRecord.Exception is InvalidOperationException);
        }

        [Test]
        public void NestedManifestsAddInfos()
        {
            var scriptModule = CreateFile("function foo {'foo'}", "psm1");
            var nestedManifest = CreateFile(CreateManifest(scriptModule, "The Guy", null, "1.1"), "psd1");
            var manifest = CreateFile(CreateManifest(nestedManifest, "Me", "MyComp", "1.0"), "psd1");
            var res = ReferenceHost.RawExecute("Import-Module '" + manifest + "' -PassThru;");
            Assert.That(res.Count, Is.EqualTo(1));
            var module = res[0].BaseObject as PSModuleInfo;
            Assert.That(module.Author, Is.EqualTo("The Guy")); // was overwritten by nestedManifest
            Assert.That(module.CompanyName, Is.EqualTo("MyComp")); // still original from manifest
            Assert.That(module.ModuleType, Is.EqualTo(ModuleType.Script)); // as the last module was a script
            Assert.That(module.Version, Is.EqualTo(new Version("1.0"))); // not overwritten by nested manifest
        }

        [Test]
        public void OneNestedManifestRestrictsExportsFromTargetModule()
        {
            var scriptModule = CreateFile(NewlineJoin(
                "function foo {'foo'}",
                "function bar {'bar'}",
                "$x = 1",
                "$y = 2",
                "Export-ModuleMember -Function foo -Var *"
                ), "psm1");
            var nestedManifest = CreateFile(CreateManifest(scriptModule, null, null, "1.0", "@()", "y"), "psd1");
            var manifest = CreateFile(CreateManifest(nestedManifest, null, null, "1.0", null, "@()"), "psd1");
            // nestedManifest only allows y to be exported. restriction of toplevel mainfest has no influence anymore
            ExecuteAndCompareTypedResult("Import-Module '" + manifest + "'; $x; $y", null, 2);
            Assert.Throws<CommandNotFoundException>(delegate
            {
                ReferenceHost.RawExecuteInLastRunspace("foo"); // not imported due to no function exports in manifest
            });
        }


        [Test]
        public void ManifestWithoutRestrictionsCounts()
        {
            var scriptModule = CreateFile(NewlineJoin(
                "function foo {'foo'}",
                "function bar {'bar'}",
                "$x = 1",
                "$y = 2",
                "Export-ModuleMember -Function foo -Var *"
                ), "psm1");
            var uselessManifest = CreateFile(CreateManifest(scriptModule, "", "", "1.0"), "psd1");
            var manifest = CreateFile(CreateManifest(uselessManifest, null, null, "1.0", "@()", "@()"), "psd1");
            // useless manifest doesn't restrict any export, so the next one will do it
            ExecuteAndCompareTypedResult("Import-Module '" + manifest + "'; $x; $y; foo", 1, 2, "foo");
        }

        [Test]
        public void ManifestOnlyRestrictsExportsIfNotEmpty()
        {
            var scriptModule = CreateFile(NewlineJoin(
                "function foo {'foo'}",
                "function bar {'bar'}",
                "$x = 1",
                "$y = 2",
                "Export-ModuleMember -Function foo -Var *"
                ), "psm1");
            var nestedManifest = CreateFile(CreateManifest(scriptModule, null, null, "1.0", null, "@()"), "psd1");
            var manifest = CreateFile(CreateManifest(nestedManifest, null, null, "1.0", "@()", null), "psd1");
            // nestedManifest function restriction is null, so not modified. nestedManifest doesn't allow any variable to be exported.
            ExecuteAndCompareTypedResult("Import-Module '" + manifest + "'; foo; $x; $y", "foo", null, null);
        }

        // TODO: test that a manifest hashtable must not include an unknown member
        // TODO: test that manifest needs a version
        // TODO: test that manifest module.Path is path of nested RootModule
        // TODO: test with get-module that modules are actually always in the sessionstate of the current module (or global)
        // TODO: test that modules are only loaded by path if a slash is in the name
        // TODO: tests for modules importing modules to check scope derivance and execution
        // TODO: test that checks what happens if the script module returns a value -> shouldn't
        // TODO: test that checks the behavior if both the Global and the Scope parameter are set -> exception
        // TODO: what if another module with the same name is loaded -> two modules, different path. but what is really loaded with nested stuff?
    }
}

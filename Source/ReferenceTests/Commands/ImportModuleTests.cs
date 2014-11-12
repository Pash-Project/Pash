using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

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
        public void CanImportScriptModuleToGlobalScope(string scopeParam)
        {
            var module = CreateFile("function test {'globalWorks'}", "psm1");
            var cmd = NewlineJoin(
                "function doImport {",
                "  Import-Module '" + module + "' " + scopeParam + ";",
                "  test",
                "}",
                "doImport;"
            );
            var expected = NewlineJoin("globalWorks");
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(expected));
            ReferenceHost.RawExecuteInLastRunspace("test");
            Assert.That(ReferenceHost.LastResults, Is.EqualTo(expected));
        }

        public void CanImportScriptModuleToLocalScope(string scopeParam)
        {
            var module = CreateFile("function test {'localWorks'}", "psm1");
            var cmd = NewlineJoin(
                "function doImport {",
                "  Import-Module '" + module + "' -Scope 'Local';",
                "  test",
                "}",
                "doImport;"
            );
            var expected = NewlineJoin("localWorks");
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(expected));
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("test");
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
                "  \"script:x = $script:x\";",
                "  \"global:x = $global:x\";",
                "  \"gv 1 = $((Get-Variable -Scope 1 x).Value)\";",
                "  \"gv 2 = $((Get-Variable -Scope 2 x).Value)\";",
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
    }
}

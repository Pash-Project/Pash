using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Management.Automation;
using System.IO;
using TestPSSnapIn;

namespace ReferenceTests.Commands
{
    class RemoveModuleTests : ModuleCommandTestBase
    {
        private string _tempDir = Path.GetTempPath();

        [Test]
        public void ModuleCanBeRemoved()
        {
            var module = CreateTestModule();
            var moduleName = Path.GetFileNameWithoutExtension(module);
            var cmd = "Import-Module '" + module + "'; foo; Remove-Module '" + moduleName + "'";
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar")));
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo;");
            });
        }

        [Test]
        public void ModuleCanBeRemovedBySubScope()
        {
            var module = CreateTestModule();
            var moduleName = Path.GetFileNameWithoutExtension(module);
            var rmModuleScript = CreateFile("& { Remove-Module '" + moduleName + "'; };", "ps1");
            var cmd = "Import-Module '" + module + "'; foo; & '" + rmModuleScript + "'";
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar")));
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo;");
            });
        }

        [Test]
        public void ModuleFunctionToBeRemovedCanBeHidden()
        {
            // This test equals ModuleCanBeRemovedBySubScope, but the script also defines a foo function
            // Note that the Remove-Module cmdlet won't find the exported function, but won't remove the scoped
            // foo { 'baz' } either, at it is not from the module.
            var module = CreateTestModule();
            var moduleName = Path.GetFileNameWithoutExtension(module);
            var rmModuleScript = CreateFile(NewlineJoin(
                "function foo { 'baz' };",
                "& {",
                "  Remove-Module '" + moduleName + "';",
                "  foo;",
                "};"
            ), "ps1");
            var cmd = "Import-Module '" + module + "'; foo; & '" + rmModuleScript + "'";
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar", "baz")));
            ReferenceHost.RawExecuteInLastRunspace("foo"); // exported function still exists
            Assert.That(ReferenceHost.LastResults, Is.EqualTo(NewlineJoin("bar")));
        }

        [TestCase("teSt*")]
        [TestCase("test,test2")]
        public void RemoveModuleCanRemoveMultiple(string rmArg)
        {
            var module1 = CreateTestModule();
            var module2 = CreateTest2Module();
            var cmd = NewlineJoin(
                "Import-Module '" + module1 + "'",
                "Import-Module '" + module2 + "'",
                "foo; bar",
                "Remove-Module " + rmArg
                );
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar", "baz")));
            // check that both are removed
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo;");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("bar;");
            });
        }

        [Test]
        public void RemoveModuleWithSameNameRemovesBoth()
        {
            var module1 = CreateTestModule();
            var module2 = CreateAdvancedTestModuleInOtherDir();
            var cmd = NewlineJoin(
                "Import-Module '" + module1 + "'",
                "Import-Module '" + module2 + "'",
                "foo; bla",
                "Remove-Module 'test'"
                );
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar", "blub")));
            // check that both are removed
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo;");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("bla;");
            });
        }

        [Test]
        public void RemoveModuleByPSModuleInfo()
        {
            var module1 = CreateTestModule();
            var module2 = CreateAdvancedTestModuleInOtherDir();
            var cmd = NewlineJoin(
                "Import-Module '" + module1 + "'",
                "$m2 = Import-Module '" + module2 + "' -PassThru",
                "foo; bla",
                "Remove-Module -ModuleInfo $m2",
                "foo"
                );
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar", "blub", "bar")));
            // check that 2nd is removed
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("bla;");
            });
        }

        [Test]
        public void RemoveModuleRemovesAllMembers()
        {
            var module = CreateAdvancedTestModuleInOtherDir();
            var cmd = NewlineJoin(
                "Import-Module '" + module + "'",
                "bla; $x; blub;",
                "Remove-Module 'test'",
                "$x"
            );
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("blub", "module", "blub", null)));
            // check that function and alias are removed
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("bla");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("blub");
            });
        }

        [Test]
        public void RemoveModuleIsAlsoRemovedFromGlobalScope()
        {
            var module = CreateTestModule();
            var script = CreateFile("foo; Import-Module '" + module + "' -scope 'local'; foo; Remove-Module 'test'", "ps1");
            var cmd = NewlineJoin(
                "Import-Module '" + module + "'",
                "foo",
                "& '" + script + "'"
            );
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar", "bar", "bar")));
            // check that remove-module also removed the module from global space
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo");
            });
        }

        [Test]
        public void RemoveAssemblyModuleRemovesCmdlets()
        {
            var moduleName = Path.GetFileNameWithoutExtension(BinaryTestModule);
            var script = CreateFile("Remove-Module '" + moduleName + "';", "ps1");
            var cmd = NewlineJoin(
                "Import-Module '" + BinaryTestModule + "'",
                CmdletName(typeof(TestCommand)),
                "& '" + script + "'"
            );
            ExecuteAndCompareTypedResult(cmd, TestCommand.OutputString);
            // check that remove-module also removed the cmdlets from global scope
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("Test-Command");
            });
        }

        // TODO: test nested module removal

        private string CreateTestModule()
        {
            var filePath = Path.Combine(_tempDir, "test.psm1");
            File.WriteAllText(filePath, "function foo { 'bar'; };");
            AddCleanupFile(filePath);
            return filePath;
        }

        private string CreateTest2Module()
        {
            var filePath = Path.Combine(_tempDir, "test2.psm1");
            File.WriteAllText(filePath, "function bar { 'baz'; };");
            AddCleanupFile(filePath);
            return filePath;
        }

        private string CreateAdvancedTestModuleInOtherDir()
        {
            var modDir = Path.Combine(_tempDir, "testModule");
            Directory.CreateDirectory(modDir);
            AddCleanupDir(modDir);
            var filePath = Path.Combine(modDir, "test.psm1");
            File.WriteAllText(filePath, NewlineJoin(
                "function bla { 'blub'; };",
                "$x = 'module';",
                "New-Alias blub bla;",
                "Export-ModuleMember -Variable x -Function bla -Alias blub;"
            ));
            AddCleanupFile(filePath);
            return filePath;
        }
    }
}

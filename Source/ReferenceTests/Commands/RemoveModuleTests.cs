using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests.Commands
{
    class RemoveModuleTests : ReferenceTestBase
    {
        [Test]
        public void ModuleCanBeRemoved()
        {
            var module = CreateFile("function foo { 'bar'; };", "psm1");
            var moduleName = System.IO.Path.GetFileNameWithoutExtension(module);
            var cmd = "Import-Module '" + module + "'; foo; Remove-Module '" + moduleName + "'";
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar")));
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo;");
            });
        }

        [Test]
        public void ModuleCanBeRemovedBySubScope()
        {
            var module = CreateFile("function foo { 'bar'; };", "psm1");
            var moduleName = System.IO.Path.GetFileNameWithoutExtension(module);
            var rmModuleScript = CreateFile("& { Remove-Module '" + moduleName + "'; };", "ps1");
            var cmd = "Import-Module '" + module + "'; foo; & '" + rmModuleScript + "'";
            Assert.That(ReferenceHost.Execute(cmd), Is.EqualTo(NewlineJoin("bar")));
            Assert.Throws<CommandNotFoundException>(delegate
                                                    {
                ReferenceHost.RawExecuteInLastRunspace("foo;");
            });
        }

        [Test]
        public void ModuleFunctionToBeRemovedCanBeHidden()
        {
            // This test equals ModuleCanBeRemovedBySubScope, but the script also defines a foo function
            // Note that the Remove-Module cmdlet won't find the exported function, but won't remove the scoped
            // foo { 'baz' } either, at it is not from the module.
            var module = CreateFile("function foo { 'bar'; };", "psm1");
            var moduleName = System.IO.Path.GetFileNameWithoutExtension(module);
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

        // TODO: test with a script importing the same module as the global, and removing it -> removed from global also
        // TODO: test that shows that exported members of all types are removed
    }
}

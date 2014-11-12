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
    }
}

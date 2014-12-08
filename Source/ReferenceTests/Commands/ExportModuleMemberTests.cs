using System;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests
{
    [TestFixture]
    public class ExportModuleMemberTests : ReferenceTestBase
    {
        private string _testModule = NewlineJoin(
            "function foo { 'foo' };",
            "function foobar { 'foobar' };",
            "function bar { 'bar' };",
            "$x = 1",
            "$y = 2",
            "$xy = 12",
            "New-Alias lorem bar",
            "New-Alias loremipsum bar",
            "New-Alias ipsum bar"
        );

        [Test]
        public void ExportModuleMemberByPattern()
        {
            var module = CreateFile(_testModule + "Export-ModuleMember -Function Foo* -Variable X* -Alias loRem*", "psm1");
            var cmd = NewlineJoin(
                "Import-Module '" + module + "';",
                "foo; foobar;",
                "$x; $xy",
                "lorem; loremipsum;",
                "$y" // shouldn't be exported
                );
            ExecuteAndCompareTypedResult(cmd, "foo", "foobar", 1, 12, "bar", "bar", null);
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("bar");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("ipsum");
            });
        }


        [Test]
        public void ExportModuleMemberByArray()
        {
            var module = CreateFile(_testModule + "Export-ModuleMember -Function Foo,Bar -Variable X,Y -Alias loRem,Ipsum", "psm1");
            var cmd = NewlineJoin(
                "Import-Module '" + module + "';",
                "foo; bar;",
                "$x; $y",
                "lorem; ipsum;",
                "$xy" // shouldn't be exported
                );
            ExecuteAndCompareTypedResult(cmd, "foo", "bar", 1, 2, "bar", "bar", null);
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foobar");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("loremipsum");
            });
        }

        [Test]
        public void ExportModuleMemberVariablePreventsExportingFunctions()
        {
            var module = CreateFile(_testModule + "Export-ModuleMember -Variable x", "psm1");
            var cmd = NewlineJoin(
                "Import-Module '" + module + "';",
                "$x; $y; $xy"
                );
            ExecuteAndCompareTypedResult(cmd, 1, null, null);
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foobar");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("bar");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("loremipsum");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("lorem");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("ipsum");
            });
        }

        [Test]
        public void ExportModuleMemberNothingPreventsExportingFunctions()
        {
            var module = CreateFile(_testModule, "psm1");
            var cmd = NewlineJoin(
                "Import-Module '" + module + "';",
                "$x; $y; $xy"
                );
            ExecuteAndCompareTypedResult(cmd, null, null, null);
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foobar");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("foo");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("bar");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("loremipsum");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("lorem");
            });
            Assert.Throws<CommandNotFoundException>(delegate {
                ReferenceHost.RawExecuteInLastRunspace("ipsum");
            });
        }

        [Test]
        public void ExportModuleMemberOutsideModuleThrows()
        {
            Assert.Throws<InvalidOperationException>(delegate {
                ReferenceHost.Execute("Export-ModuleMember");
            });
        }

        // TODO: test a module with cmdlets to export
    }
}


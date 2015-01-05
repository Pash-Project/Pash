using System;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests.Commands
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
            "New-Alias lorem foo",
            "New-Alias loremipsum foobar",
            "New-Alias ipsum bar"
        );

        [Test]
        public void ExportModuleMemberWithStar()
        {
            var module = CreateFile(_testModule + "Export-ModuleMember -Function * -Variable * -Alias *", "psm1");
            var cmd = NewlineJoin(
                "Import-Module '" + module + "';",
                "foo; foobar; bar;",
                "$x; $y; $xy",
                "lorem; loremipsum; ipsum"
                );
            ExecuteAndCompareTypedResult(cmd, "foo", "foobar", "bar", 1, 2, 12, "foo", "foobar", "bar");
        }

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
            ExecuteAndCompareTypedResult(cmd, "foo", "foobar", 1, 12, "foo", "foobar", null);
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
            ExecuteAndCompareTypedResult(cmd, "foo", "bar", 1, 2, "foo", "bar", null);
            Assert.Throws<CommandNotFoundException>(delegate
            {
                ReferenceHost.RawExecuteInLastRunspace("foobar");
            });
            Assert.Throws<CommandNotFoundException>(delegate
            {
                ReferenceHost.RawExecuteInLastRunspace("loremipsum");
            });
        }


        [Test]
        public void ExportModuleAliasWithoutFunctionDoesntResolve()
        {
            var module = CreateFile(_testModule + "Export-ModuleMember -Alias lorem", "psm1");
            Assert.Throws<CommandNotFoundException>(delegate
            {
                ReferenceHost.Execute("Import-Module '" + module + "'; lorem");
            });
        }

        [Test]
        public void ExportModuleMembersThatDontExistDoesntThrow()
        {
            var module = CreateFile(_testModule + "Export-ModuleMember -Function foo,bla,blub", "psm1");
            ExecuteAndCompareTypedResult("Import-Module '" + module + "'; foo", "foo");
        }

        [Test]
        public void ExportModuleMemberAddsToListWhenUsedMultipleTimes()
        {
            var module = CreateFile(_testModule + NewlineJoin(
                "Export-ModuleMember -Variable x",
                "Export-ModuleMember -Function foo",
                "Export-ModuleMember -Alias lorem"
            ), "psm1");
            var cmd = NewlineJoin(
                "Import-Module '" + module + "';",
                "$x; foo; lorem"
            );
            ExecuteAndCompareTypedResult(cmd, 1, "foo", "foo");
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
            var module = CreateFile(_testModule + "Export-ModuleMember", "psm1");
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
            Assert.Throws<CmdletInvocationException>(delegate {
                ReferenceHost.Execute("Export-ModuleMember");
            });
        }

        // TODO: test a module with cmdlets to export
    }
}


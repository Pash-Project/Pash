using System;
using TestPSSnapIn;
using NUnit.Framework;
using System.Reflection;
using System.Management.Automation;

namespace TestHost
{
    [TestFixture]
    public class SnapInTests
    {
        private string _assemblyLoadCommand;
        private string _testCmdletName;

        private string AddSnapInCommand
        {
            get
            {
                if (String.IsNullOrEmpty(_assemblyLoadCommand))
                {
                    var assembly = Assembly.GetAssembly(typeof(PashTestSnapIn));
                    _assemblyLoadCommand = String.Format("Add-PSSnapIn '{0}'", assembly.Location);
                }
                return _assemblyLoadCommand;
            }
        }

        private string TestCmdletName
        {
            get
            {
                if (String.IsNullOrEmpty(_testCmdletName))
                {
                    var attribute = System.Attribute.GetCustomAttribute(typeof(TestCommand), typeof(CmdletAttribute))
                            as CmdletAttribute;
                    _testCmdletName = attribute.FullName;
                }
                return _testCmdletName;
            }
        }

        [Test]
        public void AddSnapInFromFileTest()
        {
            string[] defSnapins = TestHost.Execute("Get-PSSnapIn | ForEach-Object { $_.Name }").Split((string[])null,
                StringSplitOptions.RemoveEmptyEntries);
            string[] statements = new string[] {
                AddSnapInCommand,
                "Get-PSSnapIn | ForEach-Object { $_.Name }"
            };
            var snapin = new PashTestSnapIn();
            var output = TestHost.Execute(statements).Split((string[]) null,
                StringSplitOptions.RemoveEmptyEntries);
            Assert.True(output.Length == defSnapins.Length + 1);
            bool found = false;
            foreach (var loaded in output)
            {
                if (loaded.Equals(snapin.Name))
                {
                    found = true;
                    continue;
                }
                defSnapins.ShouldContain(loaded);
            }
            Assert.True(found, "SnapIn was not added");
        }

        [Test]
        public void SnapInCommandTest()
        {
            string[] statements = new string[] {
                AddSnapInCommand,
                TestCmdletName
            };
            var output = TestHost.Execute(statements);
            Assert.AreEqual(TestCommand.OutputString + Environment.NewLine, output, "Command wasn't added properly");
        }

        [Test]
        public void RemoveSnapInTest()
        {
            string[] defSnapins = TestHost.Execute("Get-PSSnapIn").Split((string[]) null,
                StringSplitOptions.RemoveEmptyEntries);
            string[] statements = new string[] {
                AddSnapInCommand,
                String.Format("Remove-PSSnapIn '{0}'", new PashTestSnapIn().Name),
                "Get-PSSnapIn"
            };
            var afterRemoval = TestHost.Execute(statements).Split((string[])null,
                StringSplitOptions.RemoveEmptyEntries);
            Assert.True(afterRemoval.Length == defSnapins.Length, "Removal doesn't work correctly");
            foreach (var loaded in defSnapins)
            {
                afterRemoval.ShouldContain(loaded);
            }
        }

        [Test]
        public void RemoveSnapInCmdletTest()
        {
            string[] statements = new string[] {
                AddSnapInCommand,
                String.Format("Remove-PSSnapIn '{0}'", new PashTestSnapIn().Name),
                TestCmdletName
            };
            var output = TestHost.ExecuteWithZeroErrors(statements);
            StringAssert.Contains("CommandNotFoundException", output);
        }
    }
}


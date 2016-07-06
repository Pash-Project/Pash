// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.ServiceProcess;
using System.Text;
using Microsoft.PowerShell.Commands;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class GetCommandTests : ReferenceTestBase
    {
        [Test]
        public void ATest()
        {
            var results = ReferenceHost.Execute("Get-Command");

            StringAssert.Contains("Get-Command", results);
            Assert.GreaterOrEqual(results.Split('\n').Length, 10);
        }

        [Test]
        public void OutputTypeForClearVariableCommand()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$c = Get-Command | ? { $_.Name -eq 'Clear-Variable' }",
                "$name = $c.OutputType[0].Name",
                "$typeName = $c.OutputType[0].Type.FullName",
                "\"name=$name, type=$typeName\""
            });

            Assert.AreEqual("name=System.Management.Automation.PSVariable, type=System.Management.Automation.PSVariable" + Environment.NewLine, result);
        }

        [Test]
        [TestCase("Clear-Variable", new [] { typeof(PSVariable) })]
        [TestCase("Add-PSSnapin", new[] { typeof(PSSnapInInfo) })]
        [TestCase("Add-Type", new[] { typeof(Type) })]
        [TestCase("Convert-Path", new[] { typeof(string) })]
        [TestCase("ConvertTo-Csv", new[] { typeof(string) })]
        [TestCase("Get-ChildItem", new[] { typeof(FileInfo), typeof(DirectoryInfo) })]
        [TestCase("Get-Command", new[] { typeof(AliasInfo), typeof(ApplicationInfo), typeof(FunctionInfo), typeof(CmdletInfo), typeof(ExternalScriptInfo), typeof(FilterInfo), typeof(WorkflowInfo), typeof(String), typeof(PSObject) })]
        [TestCase("Get-Content", new[] { typeof(byte), typeof(string) })]
        [TestCase("Get-Date", new[] { typeof(string), typeof(DateTime) })]
        [TestCase("Get-History", new[] { typeof(HistoryInfo) })]
        [TestCase("Get-Host", new[] { typeof(PSHost) })]
        [TestCase("Get-Item", new[] { typeof(Boolean), typeof(String), typeof(FileInfo), typeof(DirectoryInfo), typeof(FileInfo) })]
        [TestCase("Get-Location", new[] { typeof(PathInfo), typeof(PathInfoStack) })]
        [TestCase("Get-Member", new[] { typeof(MemberDefinition) })]
        [TestCase("Get-Module", new[] { typeof(PSModuleInfo) })]
        [TestCase("Get-Process", new[] { typeof(ProcessModule), typeof(FileVersionInfo), typeof(Process) })]
        [TestCase("Get-PSDrive", new[] { typeof(PSDriveInfo) })]
        [TestCase("Get-PSProvider", new[] { typeof(ProviderInfo) })]
        [TestCase("Get-PSSnapin", new[] { typeof(PSSnapInInfo) })]
        [TestCase("Get-Random", new[] { typeof(int), typeof(long), typeof(double) })]
        [TestCase("Get-Service", new[] { typeof(ServiceController) })]
        [TestCase("Get-Variable", new[] { typeof(PSVariable) })]
        [TestCase("Import-Module", new[] { typeof(PSModuleInfo) })]
        [TestCase("Join-Path", new[] { typeof(string) })]
        [TestCase("New-Alias", new[] { typeof(AliasInfo) })]
        [TestCase("New-Item", new[] { typeof(string), typeof(FileInfo) })]
        [TestCase("Restart-Service", new[] { typeof(ServiceController) })]
        [TestCase("Resume-Service", new[] { typeof(ServiceController) })]
        [TestCase("Set-Alias", new[] { typeof(AliasInfo) })]
        [TestCase("Set-Location", new[] { typeof(PathInfo), typeof(PathInfoStack) })]
        [TestCase("Set-Variable", new[] { typeof(PSVariable) })]
        [TestCase("Split-Path", new[] { typeof(string), typeof(bool) })]
        [TestCase("Start-Service", new[] { typeof(ServiceController) })]
        [TestCase("Stop-Process", new[] { typeof(Process) })]
        [TestCase("Stop-Service", new[] { typeof(ServiceController) })]
        [TestCase("Suspend-Service", new[] { typeof(ServiceController) })]
        [TestCase("Test-Path", new[] { typeof(bool) })]
        [TestCase("Select-String", new[] { typeof(MatchInfo), typeof(bool) })]
        public void OutputTypesForCommand(string commandName, Type[] expectedOutputTypes)
        {
            string command = string.Format("(Get-Command {0}).OutputType", commandName);
            var results = ReferenceHost.RawExecute(command);
            Type[] outputTypes = results.Select(item => item.BaseObject)
                .OfType<PSTypeName>()
                .Select(typeName => typeName.Type)
                .ToArray();

            CollectionAssert.AreEquivalent(expectedOutputTypes, outputTypes);
        }

        [Test]
        public void FindCommandByName()
        {
            string result = ReferenceHost.Execute("(Get-Command Clear-Variable).Name");

            Assert.AreEqual("Clear-Variable" + Environment.NewLine, result);
        }

        [Test]
        public void FindCommandByNameUsingNameParameter()
        {
            string result = ReferenceHost.Execute("(Get-Command -Name Clear-Variable).Name");

            Assert.AreEqual("Clear-Variable" + Environment.NewLine, result);
        }

        [Test]
        public void FindCommandsByTwoNames()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$commands = Get-Command Clear-Variable,Get-Variable",
                "$first = $commands[0].Name",
                "$second = $commands[1].Name",
                "\"$first $second\""
            });

            Assert.AreEqual("Clear-Variable Get-Variable" + Environment.NewLine, result);
        }

        [Test]
        public void FindCommandByWildcardName()
        {
            string result = ReferenceHost.Execute("(Get-Command Clear-Var*).Name");

            Assert.AreEqual("Clear-Variable" + Environment.NewLine, result);
        }
    }
}

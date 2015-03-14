// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

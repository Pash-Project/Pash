// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class ClearVariableTests : ReferenceTestBase
    {
        [Test]
        public void ByName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "Clear-Variable foo",
                "$foo -eq $null"
            });

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void NameUsingNamedParametersAndAbbreviatedCommandName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'bar'",
                "clv -name foo",
                "$foo -eq $null"
            });

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void MultipleNames()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$a = 'abc'",
                "$b = 'abc'",
                "Clear-Variable a,b",
                "($a -eq $null).ToString() + \", \" + ($b -eq $null).ToString()"
            });

            Assert.AreEqual("True, True" + Environment.NewLine, result);
        }

        [Test]
        public void PassThru()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'abc'",
                "$output = Clear-Variable foo -passthru",
                "'Name=' + $output.Name + ', Value is null=' + ($output.Value -eq $null).ToString() + ', Type=' + $output.GetType().Name"
            });

            Assert.AreEqual("Name=foo, Value is null=True, Type=PSVariable" + Environment.NewLine, result);
        }

        [Test]
        public void PassThruTwoVariablesCleared()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$foo = 'abc'",
                "$bar = 'abc'",
                "$v = Clear-Variable foo,bar -passthru",
                "'Names=' + $v[0].Name + ',' + $v[1].Name + ' Values are null=' + ($v[0].Value -eq $null).ToString() + ',' + ($v[1].Value -eq $null).ToString() + ' Type=' + $v.GetType().Name + ' ' + $v[0].GetType().Name"
            });

            Assert.AreEqual("Names=foo,bar Values are null=True,True Type=Object[] PSVariable" + Environment.NewLine, result);
        }
    }
}

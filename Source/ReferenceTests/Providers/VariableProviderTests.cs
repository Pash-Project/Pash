// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Providers
{
    [TestFixture]
    class VariableProviderTests : ReferenceTestBase
    {
        [Test]
        public void ListAllVariables()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$VariableProviderTestsVariable = 'abc'",
                "Get-ChildItem variable: | ? { $_.Name -eq 'VariableProviderTestsVariable' } | % { $_.Value }"
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void ListAllVariablesUsingBackslashInDriveName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$VariableProviderTestsVariable = 'abc'",
                String.Format(@"Get-ChildItem variable:{0} | ? {{ $_.Name -eq 'VariableProviderTestsVariable' }} | % {{ $_.Value }}", Path.DirectorySeparatorChar)
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void ListSingleVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$VariableProviderTestsVariable = 'abc'",
                @"Get-ChildItem variable:VariableProviderTestsVariable | % { $_.Value }"
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void ListSingleVariableWithBackslashInDriveName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$VariableProviderTestsVariable = 'abc'",
                String.Format(@"Get-ChildItem variable:{0}VariableProviderTestsVariable | % {{ $_.Value }}", Path.DirectorySeparatorChar)
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void GetContentForVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$VariableProviderTestsVariable = 'abc'",
                @"Get-Content variable:VariableProviderTestsVariable"
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void SetContentForVariable()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$VariableProviderTestsVariable = 'abc'",
                @"Set-Content variable:VariableProviderTestsVariable 'test'",
                "$VariableProviderTestsVariable"
            });

            Assert.AreEqual("test" + Environment.NewLine, result);
        }

        [Test]
        public void SetContentForVariableUsingTwoItems()
        {
            string result = ReferenceHost.Execute(new string[] {
                "$VariableProviderTestsVariable = 'abc'",
                @"Set-Content variable:VariableProviderTestsVariable 'test1','test2'",
                "$type = $VariableProviderTestsVariable.GetType().Name",
                "$first = $VariableProviderTestsVariable[0]",
                "$second = $VariableProviderTestsVariable[1]",
                "\"$type - $first - $second\""
            });

            Assert.AreEqual("Object[] - test1 - test2" + Environment.NewLine, result);
        }
    }
}

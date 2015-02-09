// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Providers
{
    [TestFixture]
    class FunctionProviderTests : ReferenceTestBase
    {
        [Test]
        public void ListAllFunctions()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function testfunc {}",
                "Get-ChildItem function: | ? { $_.Name -eq 'testfunc' } | % { $_.Name }"
            });

            Assert.AreEqual("testfunc" + Environment.NewLine, result);
        }

        [Test]
        public void ListAllFunctionsUsingBackslashInDriveName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function testfunc {}",
                String.Format(@"Get-ChildItem function:{0} | ? {{ $_.Name -eq 'testfunc' }} | % {{ $_.Name }}", Path.DirectorySeparatorChar)
            });

            Assert.AreEqual("testfunc" + Environment.NewLine, result);
        }

        [Test]
        public void ListSingleFunction()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function testfunc {}",
                @"Get-ChildItem function:testfunc | % { $_.Name }"
            });

            Assert.AreEqual("testfunc" + Environment.NewLine, result);
        }

        [Test]
        public void ListSingleFunctionWithBackslashInDriveName()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function testfunc {}",
                String.Format(@"Get-ChildItem function:{0}testfunc | % {{ $_.Name }}", Path.DirectorySeparatorChar)
            });

            Assert.AreEqual("testfunc" + Environment.NewLine, result);
        }

        [Test, Explicit("ScriptBlock.ToString() should return the function body text")]
        public void GetContentForFunction()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function testfunc { Write-Host 'test' }",
                @"(Get-Content function:testfunc).ToString()"
            });

            Assert.AreEqual(" Write-Host 'test' " + Environment.NewLine, result);
        }

        [Test]
        public void GetContentForFunctionIsScriptBlock()
        {
            string result = ReferenceHost.Execute(new string[] {
                "function testfunc {}",
                @"(Get-Content function:testfunc).GetType().FullName"
            });

            Assert.AreEqual("System.Management.Automation.ScriptBlock" + Environment.NewLine, result);
        }
    }
}

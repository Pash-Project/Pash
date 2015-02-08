// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class ClearContentTests : ReferenceTestBase
    {
        [Test]
        public void StringValueOverwritesExistingFile()
        {
            string fileName = CreateFile("test", ".txt");

            ReferenceHost.Execute("Clear-Content -path " + fileName);

            string result = File.ReadAllText(fileName);
            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void NoNamedParameters()
        {
            string fileName = CreateFile("test", ".txt");

            string result = ReferenceHost.Execute(new string[] {
                "Clear-Content " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void TwoFiles()
        {
            string fileName1 = CreateFile("1", ".txt");
            string fileName2 = CreateFile("2", ".txt");

            ReferenceHost.Execute("Clear-Content " + fileName1 + "," + fileName2);

            string input1Text = File.ReadAllText(fileName1);
            string input2Text = File.ReadAllText(fileName2);
            Assert.AreEqual(String.Empty, input1Text);
            Assert.AreEqual(String.Empty, input2Text);
        }

        [Test, Explicit("Pash throws a CmdletInvocationException")]
        public void FileDoesNotExistThrowsItemNotFoundException()
        {
            var ex = Assert.Throws<ExecutionWithErrorsException>(delegate {
                ReferenceHost.Execute("Clear-Content FileDoesNotExistItemNotFoundExceptionThrown.txt");
            });

            ErrorRecord error = ex.Errors[0];
            Assert.IsInstanceOf<ItemNotFoundException>(error.Exception);
            Assert.AreEqual(ErrorCategory.ObjectNotFound, error.CategoryInfo.Category);
            Assert.AreEqual("PathNotFound,Microsoft.PowerShell.Commands.ClearContentCommand", error.FullyQualifiedErrorId);
        }

        [Test]
        public void FileDoesNotExistThrowsException()
        {
            try
            {
                ReferenceHost.Execute("Clear-Content FileDoesNotExistItemNotFoundExceptionThrown.txt");
                Assert.Fail("Exception should be thrown.");
            }
            catch (Exception)
            {
            }
        }
    }
}

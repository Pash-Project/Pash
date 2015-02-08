// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class AddContentTests : ReferenceTestBase
    {
        private string GenerateTempFile(string content)
        {
            return CreateFile(content, ".txt");
        }

        [Test]
        public void StringValueDoesNotOverwriteExistingFile()
        {
            string fileName = GenerateTempFile("test" + Environment.NewLine);

            string result = ReferenceHost.Execute(new string[] {
                "Add-Content -value 'abc' -path " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual(NewlineJoin("test", "abc"), result);
        }

        [Test]
        public void NoNamedParameters()
        {
            string fileName = GenerateTempFile("test" + Environment.NewLine);

            string result = ReferenceHost.Execute(new string[] {
                "Add-Content " + fileName + " 'abc'",
                "Get-Content " + fileName
            });

            Assert.AreEqual(NewlineJoin("test", "abc"), result);
        }

        [Test]
        public void ValueTakenFromPipeline()
        {
            string fileName = GenerateTempFile("test" + Environment.NewLine);

            string result = ReferenceHost.Execute(new string[] {
                "'abc' | Add-Content " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual(NewlineJoin("test", "abc"), result);
        }

        [Test]
        public void TwoFiles()
        {
            string fileName1 = GenerateTempFile("1" + Environment.NewLine);
            string fileName2 = GenerateTempFile("2" + Environment.NewLine);

            ReferenceHost.Execute("'abc' | Add-Content " + fileName1 + "," + fileName2);

            string input1Text = File.ReadAllText(fileName1);
            string input2Text = File.ReadAllText(fileName2);
            Assert.AreEqual(NewlineJoin("1", "abc"), input1Text);
            Assert.AreEqual(NewlineJoin("2", "abc"), input2Text);
        }

        [Test]
        public void ArrayOfValues()
        {
            string fileName = GenerateTempFile("test" + Environment.NewLine);

            string result = ReferenceHost.Execute(new string[] {
                "$a = 'abc','def'",
                "Add-Content -value $a -path " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual(NewlineJoin("test", "abc", "def"), result);
        }
    }
}
